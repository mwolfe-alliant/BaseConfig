// BC1 BaseConfig — Royalty Statement entry point.
// Source: bc1_master c_calc 1542 = C_ROY_MAIN_ROYALTY_STATEMENT.
// CTG entry-class name: RoyaltyStatementEllen.  Template group: "Royalty - Statement".
//
// Statement variant of RoyaltyAccrual.  Differences:
//   - PriorStmtITD aliases to PriorITD (no separate prior-statement load).
//   - RoyEarned Current = (Current - prior, RecoupmentGroup-stamped, RemoveZeros)
//     and RoyEarnedITD = (PriorRoyEarnedITD ∪ RoyEarnedCurrentOutput) so prior
//     RecoupmentGroup tags are preserved.
//   - Adds SubRoyaltiesDue, SubPaymentDueAndTaxes, SubAPEntries, SubStatementDisplay.
//   - No GLEntries (that's accrual-only).
using System;
using System.Reflection;
using Velocity.Handles;

namespace Velocity
{
    public class RoyaltyStatementEllen
    {
        private static CalcContext _ctxRoyaltyStatement;

        public static ResultSet Run() => new RoyaltyStatementEllen().Main();

        public static string CtgName =>
            (typeof(RoyaltyStatementEllen)
                .GetMethod(nameof(Main))
                ?.GetCustomAttribute<CalcTemplateGroupAttribute>()
                ?.Name)
            ?? throw new InvalidOperationException("CalcTemplateGroupAttribute missing from RoyaltyStatement.Main");

        [CalcTemplateGroup("Royalty - Statement - Ellen")]
        public ResultSet Main()
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxRoyaltyStatement = _ctxRoyaltyStatement ?? new CalcContext("C_ROY_MAIN_ROYALTY_STATEMENT");
            Job.CurrentCalcContext = _ctxRoyaltyStatement;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  RoyaltyStatement");

            // ─── Phase 1: Pull import + adjustment + window data ─────────────
            var (importedTrx, priorItd, priorUpToWindowItd, adjItd, windowPeriod) =
                RoyaltyLib.SubGetImportAdjsPriorITD();
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 2: Deduction cap ──────────────────────────────────────
            var (amountOverCapItd, dedAndReturnCapItd, salesItd) =
                RoyaltyLib.SubDeductionCap(importedTrx.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 3: Calculated revenue deductions ──────────────────────
            var revenueDeductions = RoyaltyLib.SubRevenueDeduction(importedTrx.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;
            importedTrx.Release();

            // ─── Phase 4: Net Sales ──────────────────────────────────────────
            var netSalesItd = RoyaltyLib.SubNetSales(salesItd.Copy(), dedAndReturnCapItd.Copy(), revenueDeductions.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 5: Tiering ────────────────────────────────────────────
            var tieredNetSales = RoyaltyLib.SubTiering(netSalesItd, priorItd.Copy(), windowPeriod.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 6: Royalties Earned ───────────────────────────────────
            var (netSalesWithRateMethod, royEarned, adjToRoyEarnedItd) =
                RoyaltyLib.SubRoyaltiesEarned(tieredNetSales, adjItd.Copy(), windowPeriod.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 7: SalesAndRoy assembly + Stmt-style RoyEarned splits ─
            var priorSalesRoyItd = priorItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.GL_Export_Activity_Type_List,
                "RoyStmt.PriorSalesRoyITD");
            priorSalesRoyItd.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var priorSalesRoyWindowItd = priorUpToWindowItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.GL_Export_Activity_Type_List,
                "RoyStmt.PriorSalesRoyWindowITD");

            var salesAndRoyItd = ResultSet.EmptySet();
            salesAndRoyItd.CombineAndRelease(
                priorSalesRoyWindowItd,
                dedAndReturnCapItd, salesItd, amountOverCapItd, revenueDeductions,
                netSalesWithRateMethod, royEarned);
            salesAndRoyItd.Combine(adjToRoyEarnedItd);

            var salesAndRoyCurrent = ResultSet.EmptySet();
            salesAndRoyCurrent.CombineAndRelease(salesAndRoyItd.Copy(), priorSalesRoyItd);
            salesAndRoyCurrent.SetValue(CustCol.TransType, TransType.Current);

            // UnspecRoyEarnedCurrent: Current rows where ActivityType=Royalties_Earned, RecoupmentGroup cleared.
            var unspecRoyEarnedCurrent = salesAndRoyCurrent.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Earned,
                "RoyStmt.UnspecRoyEarnedCurrent");
            unspecRoyEarnedCurrent.SetValue(CustCol.RecoupmentGroup, new AlliantEntity(0));

            // RoyEarnedCurrent: keep non-zero Amount, stamp RecoupmentGroup from contract UDF.
            var royEarnedCurrent = unspecRoyEarnedCurrent.GetData(
                BaseCol.Amount, CompareOp.NE, "0", "RoyStmt.RoyEarnedCurrent");
            unspecRoyEarnedCurrent.Release();
            royEarnedCurrent.DoMath(CustCol.RecoupmentGroup, MathOp.SETTO, ContractUDF.RecoupmentGroup);

            // RemoveZeros (consumes royEarnedCurrent).
            var royEarnedCurrentOutput = CommonLib.RemoveZeros(royEarnedCurrent);

            var nonRoyEarnedCurrentOutput = salesAndRoyCurrent.GetData(
                CustCol.ActivityType, CompareOp.NE, ActivityType.Royalties_Earned,
                "RoyStmt.NonRoyEarnedCurrentOutput");
            salesAndRoyCurrent.Release();

            // RoyEarnedITDOutput = Combine(PriorRoyEarnedITD, RoyEarnedCurrentOutput), TransType=ITD.
            var priorRoyEarnedItd = priorItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Earned,
                "RoyStmt.PriorRoyEarnedITD");
            var royEarnedItdOutput = ResultSet.EmptySet();
            royEarnedItdOutput.CombineAndRelease(priorRoyEarnedItd, royEarnedCurrentOutput.Copy());
            royEarnedItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            var nonRoyEarnedItdOutput = salesAndRoyItd.GetData(
                CustCol.ActivityType, CompareOp.NE, ActivityType.Royalties_Earned,
                "RoyStmt.NonRoyEarnedITDOutput");
            salesAndRoyItd.Release();

            // ─── Phase 8: Bonus Payment ──────────────────────────────────────
            var (bonusPaymentItdOutput, bonusPaymentCurrentOutput) =
                RoyaltyLib.SubBonusPayment(nonRoyEarnedItdOutput.Copy(), priorItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 9: Reserves ───────────────────────────────────────────
            var (reservesLiquidatedOutput, adjAndReserveBalancesOutput) = RoyaltyLib.SubReserves(
                royEarnedItdOutput.Copy(), priorUpToWindowItd.Copy(), priorItd.Copy(), windowPeriod.Copy(), adjItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 10: Advances (PriorStmtITD aliases to PriorITD) ───────
            var recoupmentOutput = RoyaltyLib.SubAdvances(
                royEarnedItdOutput.Copy(), reservesLiquidatedOutput.Copy(), adjItd.Copy(), priorItd.Copy(), priorItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 11: Guarantees ────────────────────────────────────────
            var mgOutput = RoyaltyLib.SubGuarantees(
                royEarnedItdOutput.Copy(), reservesLiquidatedOutput.Copy(), adjItd.Copy(), priorItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 12: Guarantee Installments ────────────────────────────
            var giOutput = RoyaltyLib.SubGuaranteeInstallments(
                royEarnedItdOutput.Copy(), reservesLiquidatedOutput.Copy(), adjItd.Copy(), priorItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 13: Royalties Due ─────────────────────────────────────
            var passToRoyDue = ResultSet.EmptySet();
            passToRoyDue.Combine(royEarnedItdOutput);
            passToRoyDue.Combine(reservesLiquidatedOutput);
            passToRoyDue.Combine(recoupmentOutput);
            passToRoyDue.Combine(giOutput);
            var (royDueOutput, adjToRoyDueItdOutput) =
                RoyaltyLib.SubRoyaltiesDue(passToRoyDue, priorItd.Copy(), adjItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;
            passToRoyDue.Release();

            // ─── Phase 14: Payment and Taxes ─────────────────────────────────
            var passToPayment = ResultSet.EmptySet();
            passToPayment.Combine(royDueOutput);
            passToPayment.Combine(mgOutput);
            passToPayment.Combine(bonusPaymentCurrentOutput);
            var paymentAndTaxesOutput = RoyaltyLib.SubPaymentDueAndTaxes(passToPayment, priorItd.Copy(), adjItd.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;
            passToPayment.Release();

            // ─── Phase 15: AP Entries ────────────────────────────────────────
            var apOutput = RoyaltyLib.SubAPEntries(paymentAndTaxesOutput.Copy());
            Job.CurrentCalcContext = _ctxRoyaltyStatement;

            // ─── Phase 16: Statement Display ─────────────────────────────────
            var passToStatement = ResultSet.EmptySet();
            passToStatement.Combine(royEarnedCurrentOutput);
            passToStatement.Combine(nonRoyEarnedCurrentOutput);
            var statementOutput = RoyaltyLib.SubStatementDisplay(passToStatement);
            Job.CurrentCalcContext = _ctxRoyaltyStatement;
            passToStatement.Release();

            // Inputs to subs are now consumed (each Sub got its own Copy()).  Release.
            priorUpToWindowItd.Release();
            priorItd.Release();
            adjItd.Release();

            // ─── Phase 17: Finals + RemoveZeros ──────────────────────────────
            var finals = ResultSet.EmptySet();
            finals.Combine(royEarnedCurrentOutput);
            finals.Combine(nonRoyEarnedCurrentOutput);
            finals.Combine(royEarnedItdOutput);
            finals.Combine(nonRoyEarnedItdOutput);
            finals.Combine(adjAndReserveBalancesOutput);
            finals.Combine(reservesLiquidatedOutput);
            finals.Combine(recoupmentOutput);
            finals.Combine(mgOutput);
            finals.Combine(royDueOutput);
            finals.Combine(giOutput);
            finals.Combine(adjToRoyDueItdOutput);
            finals.Combine(paymentAndTaxesOutput);
            finals.Combine(apOutput);
            finals.Combine(statementOutput);
            finals.Combine(bonusPaymentItdOutput);
            finals.Combine(bonusPaymentCurrentOutput);

            royEarnedCurrentOutput.Release();
            nonRoyEarnedCurrentOutput.Release();
            royEarnedItdOutput.Release();
            nonRoyEarnedItdOutput.Release();
            adjAndReserveBalancesOutput.Release();
            reservesLiquidatedOutput.Release();
            recoupmentOutput.Release();
            mgOutput.Release();
            royDueOutput.Release();
            giOutput.Release();
            adjToRoyDueItdOutput.Release();
            paymentAndTaxesOutput.Release();
            apOutput.Release();
            statementOutput.Release();
            bonusPaymentItdOutput.Release();
            bonusPaymentCurrentOutput.Release();

            var nonZeros = CommonLib.RemoveZeros(finals);

            // ─── Final assembly ──────────────────────────────────────────────
            var output = ResultSet.EmptySet();
            output.CombineAndRelease(windowPeriod, nonZeros);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  RoyaltyStatement");
            return output;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }
    }
}

