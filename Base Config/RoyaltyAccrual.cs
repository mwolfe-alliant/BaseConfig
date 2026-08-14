// BC1 BaseConfig — Royalty Accrual entry point.
// Source: bc1_master c_calc 1541 = C_ROY_MAIN_ROYALTY_ACCRUAL.
// CTG entry-class name: RoyaltyAccrual.  Template group: "Royalty - Accrual".
//
// DealScript pipeline (top-down):
//   PriorStmtITD       <- GetData CalcResult, ActivityType ∈ Prior_ITD_List, Period = F_FIRST_PERIOD_OF_PRIOR_INTERVAL
//   <ImportedTrx, PriorITD, PriorUpToWindowITD, AdjITD, WindowPeriod>
//                      <- RoyaltyLib.SubGetImportAdjsPriorITD()
//   <AmountOverCapITD, DedAndReturnCapITD, SalesITD>
//                      <- RoyaltyLib.SubDeductionCap(ImportedTrx)
//   RevenueDeductions  <- RoyaltyLib.SubRevenueDeduction(ImportedTrx)
//   NetSalesITD        <- RoyaltyLib.SubNetSales(SalesITD, DedAndReturnCapITD, RevenueDeductions)
//   TieredNetSales     <- RoyaltyLib.SubTiering(NetSalesITD, PriorITD, WindowPeriod)
//   <NetSalesWithRateMethod, RoyEarned, AdjToRoyEarnedITD>
//                      <- RoyaltyLib.SubRoyaltiesEarned(TieredNetSales, AdjITD, WindowPeriod)
//   PriorSalesRoyITD   = -1 * Prior rows (filtered by GL_Export list)
//   PriorSalesRoyWindowITD  filter PriorUpToWindowITD likewise
//   SalesAndRoyITD     = Combine of all sales/roy parts (ITD)
//   SalesAndRoyCurrent = Current variant (TransType=Current)
//   PriorStmtRoyEarnedITD  filter PriorStmtITD for ActivityType=Royalties_Earned
//   StmtRoyEarned      = (RoyEarnedITD - PriorStmtRoyEarnedITD), RecoupmentGroup <- ContractUDF.RecoupmentGroup
//   RoyEarnedITDOutput = Combine(PriorStmtRoyEarnedITD, NonZeroStmtRoyEarned), TransType=ITD
//   RoyEarnedCurrentOutput = (RoyEarnedITDOutput - PriorRoyEarnedITD), TransType=Current
//   <BonusPaymentITDOutput, BonusPaymentCurrentOutput>
//                      <- RoyaltyLib.SubBonusPayment(NonRoyEarnedITDOutput, PriorITD)
//   <ReservesLiquidatedOutput, AdjAndReserveBalancesOutput>
//                      <- RoyaltyLib.SubReserves(RoyEarnedITDOutput, PriorUpToWindowITD, PriorITD, WindowPeriod, AdjITD)
//   RecoupmentOutput   <- RoyaltyLib.SubAdvances(RoyEarnedITDOutput, ReservesLiquidatedOutput, AdjITD, PriorITD, PriorStmtITD)
//   MGOutput           <- RoyaltyLib.SubGuarantees(RoyEarnedITDOutput, ReservesLiquidatedOutput, AdjITD, PriorITD)
//   GIOutput           <- RoyaltyLib.SubGuaranteeInstallments(RoyEarnedITDOutput, ReservesLiquidatedOutput, AdjITD, PriorITD)
//   Finals             = Combine(RoyEarnedCurrent, NonRoyEarnedCurrent, RoyEarnedITD, NonRoyEarnedITD,
//                                AdjAndReserveBalances, ReservesLiquidated, Recoupment, MG, GI,
//                                BonusPaymentITD, BonusPaymentCurrent)
//   NonZeros           <- CommonLib.RemoveZeros(Finals)
//   GLOutput           <- RoyaltyLib.SubGLEntries(NonZeros)
//   Output             = Combine(NonZeros, WindowPeriod, GLOutput)
using System;
using System.Reflection;
using Velocity.Handles;

namespace Velocity
{
    public class RoyaltyAccrual
    {
        private static CalcContext _ctxRoyaltyAccrual;

        public static ResultSet Run() => new RoyaltyAccrual().Main();

        public static string CtgName =>
            (typeof(RoyaltyAccrual)
                .GetMethod(nameof(Main))
                ?.GetCustomAttribute<CalcTemplateGroupAttribute>()
                ?.Name)
            ?? throw new InvalidOperationException("CalcTemplateGroupAttribute missing from RoyaltyAccrual.Main");

        [CalcTemplateGroup("Royalty - Accrual")]
        public ResultSet Main()
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxRoyaltyAccrual = _ctxRoyaltyAccrual ?? new CalcContext("C_ROY_MAIN_ROYALTY_ACCRUAL");
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  RoyaltyAccrual");

            // ─── Prior statement ITD ─────────────────────────────────────────
            // GetData CalcResult for ActivityType ∈ Prior_ITD_Activity_Type_List,
            //                       TransType=ITD,
            //                       Period   = F_FIRST_PERIOD_OF_PRIOR_INTERVAL(StatementInterval).
            // F_FIRST_PERIOD_OF_PRIOR_INTERVAL is function 47; not exposed on DS yet —
            // approximate via F_PERIOD_TYPES_FROM(calcPeriod, -1, statementInterval).
            string statementInterval = Contract.GetUDFString(ContractUDF.StatementInterval);
            PeriodItem priorStmtFirst = !string.IsNullOrEmpty(statementInterval)
                ? DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, -1, statementInterval)
                : Job.CalcPeriodPrevious;

            var priorStmtItd = ResultSet.GetDataFromCalcResult(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Prior_ITD_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.ITD),
                new Criteria(EngineCol.Period,     CompareOp.EQ,  priorStmtFirst),
            });

            // ─── Phase 1: Pull import + adjustment + window data ─────────────
            var (importedTrx, priorItd, priorUpToWindowItd, adjItd, windowPeriod) =
                RoyaltyLib.SubGetImportAdjsPriorITD();
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 2: Deduction cap ──────────────────────────────────────
            var (amountOverCapItd, dedAndReturnCapItd, salesItd) =
                RoyaltyLib.SubDeductionCap(importedTrx.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 3: Calculated revenue deductions ──────────────────────
            var revenueDeductions = RoyaltyLib.SubRevenueDeduction(importedTrx.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;
            importedTrx.Release();

            // ─── Phase 4: Net Sales ──────────────────────────────────────────
            var netSalesItd = RoyaltyLib.SubNetSales(salesItd.Copy(), dedAndReturnCapItd.Copy(), revenueDeductions.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 5: Tiering ────────────────────────────────────────────
            var tieredNetSales = RoyaltyLib.SubTiering(netSalesItd, priorItd.Copy(), windowPeriod.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 6: Royalties Earned ───────────────────────────────────
            var (netSalesWithRateMethod, royEarned, adjToRoyEarnedItd) =
                RoyaltyLib.SubRoyaltiesEarned(tieredNetSales, adjItd.Copy(), windowPeriod.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 7: SalesAndRoyITD assembly + RoyEarned ITD/Current ────
            // PriorSalesRoyITD = -1 * (PriorITD ∩ GL_Export_List).  Used only as the
            // "prior baseline" for Current calculations below.
            var priorSalesRoyItd = priorItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.GL_Export_Activity_Type_List,
                "RoyAccrual.PriorSalesRoyITD");
            priorSalesRoyItd.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var priorSalesRoyWindowItd = priorUpToWindowItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.GL_Export_Activity_Type_List,
                "RoyAccrual.PriorSalesRoyWindowITD");

            var salesAndRoyItd = ResultSet.EmptySet();
            salesAndRoyItd.CombineAndRelease(
                priorSalesRoyWindowItd,
                dedAndReturnCapItd, salesItd, amountOverCapItd, revenueDeductions,
                netSalesWithRateMethod, royEarned);
            salesAndRoyItd.Combine(adjToRoyEarnedItd);

            var salesAndRoyCurrent = ResultSet.EmptySet();
            salesAndRoyCurrent.CombineAndRelease(salesAndRoyItd.Copy(), priorSalesRoyItd);
            salesAndRoyCurrent.SetValue(CustCol.TransType, TransType.Current);

            // RoyEarnedITDOutput: PriorStmt's RoyEarned ITD plus current RoyEarned (post-stmt-rebase).
            var priorStmtRoyEarnedItd = priorStmtItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Earned,
                "RoyAccrual.PriorStmtRoyEarnedITD");
            var royEarnedItd = salesAndRoyItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Earned,
                "RoyAccrual.RoyEarnedITD");
            var stmtRoyEarned = ResultSet.Subtract(royEarnedItd, priorStmtRoyEarnedItd, "RoyAccrual.StmtRoyEarned");
            royEarnedItd.Release();
            stmtRoyEarned.DoMath(CustCol.RecoupmentGroup, MathOp.SETTO, ContractUDF.RecoupmentGroup);

            var nonZeroStmtRoyEarned = CommonLib.RemoveZeros(stmtRoyEarned);
            // (RemoveZeros consumed stmtRoyEarned.)

            var royEarnedItdOutput = ResultSet.EmptySet();
            royEarnedItdOutput.CombineAndRelease(priorStmtRoyEarnedItd.Copy(), nonZeroStmtRoyEarned);
            royEarnedItdOutput.SetValue(CustCol.TransType, TransType.ITD);
            priorStmtRoyEarnedItd.Release();

            var nonRoyEarnedItdOutput = salesAndRoyItd.GetData(
                CustCol.ActivityType, CompareOp.NE, ActivityType.Royalties_Earned,
                "RoyAccrual.NonRoyEarnedITDOutput");
            salesAndRoyItd.Release();

            var priorRoyEarnedItd = priorItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Earned,
                "RoyAccrual.PriorRoyEarnedITD");
            var royEarnedCurrentOutput = ResultSet.Subtract(royEarnedItdOutput, priorRoyEarnedItd, "RoyAccrual.RoyEarnedCurrentOutput");
            priorRoyEarnedItd.Release();
            royEarnedCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            var nonRoyEarnedCurrentOutput = salesAndRoyCurrent.GetData(
                CustCol.ActivityType, CompareOp.NE, ActivityType.Royalties_Earned,
                "RoyAccrual.NonRoyEarnedCurrentOutput");
            salesAndRoyCurrent.Release();

            // ─── Phase 8: Bonus Payment ──────────────────────────────────────
            var (bonusPaymentItdOutput, bonusPaymentCurrentOutput) =
                RoyaltyLib.SubBonusPayment(nonRoyEarnedItdOutput.Copy(), priorItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 9: Reserves ───────────────────────────────────────────
            var (reservesLiquidatedOutput, adjAndReserveBalancesOutput) = RoyaltyLib.SubReserves(
                royEarnedItdOutput.Copy(), priorUpToWindowItd.Copy(), priorItd.Copy(), windowPeriod.Copy(), adjItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 10: Advances ──────────────────────────────────────────
            var recoupmentOutput = RoyaltyLib.SubAdvances(
                royEarnedItdOutput.Copy(), reservesLiquidatedOutput.Copy(), adjItd.Copy(), priorItd.Copy(), priorStmtItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 11: Guarantees ────────────────────────────────────────
            var mgOutput = RoyaltyLib.SubGuarantees(
                royEarnedItdOutput.Copy(), reservesLiquidatedOutput.Copy(), adjItd.Copy(), priorItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Phase 12: Guarantee Installments ────────────────────────────
            var giOutput = RoyaltyLib.SubGuaranteeInstallments(
                royEarnedItdOutput.Copy(), reservesLiquidatedOutput.Copy(), adjItd.Copy(), priorItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // Inputs to subs are now consumed (each Sub got its own Copy()).  Release.
            priorUpToWindowItd.Release();
            priorItd.Release();
            adjItd.Release();
            priorStmtItd.Release();

            // ─── Phase 13: Finals + RemoveZeros ──────────────────────────────
            var finals = ResultSet.EmptySet();
            finals.CombineAndRelease(
                royEarnedCurrentOutput, nonRoyEarnedCurrentOutput,
                royEarnedItdOutput, nonRoyEarnedItdOutput,
                adjAndReserveBalancesOutput, reservesLiquidatedOutput,
                recoupmentOutput);
            finals.Combine(mgOutput);
            finals.Combine(giOutput);
            finals.Combine(bonusPaymentItdOutput);
            finals.Combine(bonusPaymentCurrentOutput);
            mgOutput.Release();
            giOutput.Release();
            bonusPaymentItdOutput.Release();
            bonusPaymentCurrentOutput.Release();

            var nonZeros = CommonLib.RemoveZeros(finals);
            // (finals consumed.)

            // ─── Phase 14: GL Entries ────────────────────────────────────────
            var glOutput = RoyaltyLib.SubGLEntries(nonZeros.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyAccrual;

            // ─── Final assembly ──────────────────────────────────────────────
            var output = ResultSet.EmptySet();
            output.CombineAndRelease(nonZeros, windowPeriod, glOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  RoyaltyAccrual");
            return output;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }
    }
}

