// BC1 BaseConfig — MAGR (Modified Adjusted Gross Receipts) / Participations
// pipeline library.
//
// Conversion progress:
//   ✓ 1683  C_MAGR_SUMMARIZE_FOR_WINDOW_TIERING               — SummarizeForWindowTiering
//   ✓ 1720  C_MAGR_SUMMARIZE_FOR_WINDOW_DIST_FEE_TIERING      — SummarizeForWindowDistFeeTiering
//   ✓ 1723  C_MAGR_SUMMARIZE_FOR_WINDOW_PARTICIPATION_TIERING — SummarizeForWindowParticipationTiering
//
//   ✓ 1668  C_MAGR_GET_IMPORT_ADJS_PRIOR_ITD                  — SubGetImportAdjsPriorItd
//   ✓ 1669  C_MAGR_SUB_HV_ROYALTIES                           — SubHvRoyalties
//   ✓ 1670  C_MAGR_SUB_RESERVES                               — SubReserves
//   ✓ 1681  C_MAGR_SUB_CALCULATED_DEDUCTIONS                  — SubCalculatedDeductions
//   ✓ 1682  C_MAGR_SUB_HV_TIERING                             — SubHvTiering
//   ✓ 1684  C_MAGR_SUB_GROSS_RECEIPTS                         — SubGrossReceipts
//   ✓ 1687  C_MAGR_SUB_OFF_THE_TOP_DEDUCTIONS                 — SubOffTheTopDeductions
//   ✓ 1688  C_MAGR_SUB_TOTAL_GROSS_RECEIPTS                   — SubTotalGrossReceipts
//   ✓ 1689  C_MAGR_SUB_GROSS_RECEIPTS_PARTICIPATION           — SubGrossReceiptsParticipation
//   ✓ 1690  C_MAGR_SUB_CALCULATED_EXPENSES                    — SubCalculatedExpenses
//   ✓ 1691  C_MAGR_SUB_DISTRIBUTION_EXPENSES                  — SubDistributionExpenses
//   ✓ 1692  C_MAGR_SUB_DISTRIBUTION_FEE                       — SubDistributionFee
//   ✓ 1693  C_MAGR_SUB_CALCULATE_BALANCE                      — SubCalculateBalance
//   ✓ 1695  C_MAGR_SUB_COST_OF_PRODUCTION                     — SubCostOfProduction
//   ✓ 1696  C_MAGR_SUB_MODIFIED_ADJUSTED_GROSS_RECEIPTS       — SubModifiedAdjustedGrossReceipts
//   ✓ 1700  C_MAGR_SUB_STATEMENT_DISPLAY                      — SubStatementDisplay
//   ✓ 1701  C_MAGR_SUB_ADVANCES_AND_OFF_THE_BOTTOMS           — SubAdvancesAndOffTheBottoms
//   ✓ 1702  C_MAGR_SUB_PARTICIPANT_SHARE                      — SubParticipantShare
//   ✓ 1703  C_MAGR_SUB_PAYMENT_AND_TAXES                      — SubPaymentAndTaxes
//   ✓ 1704  C_MAGR_SUB_PARTICIPATIONS_TIERING                 — SubParticipationsTiering
//   ✓ 1730  C_MAGR_SUB_INTEREST                               — SubInterest
using System;
using Velocity.Handles;

namespace Velocity
{
    public static class MagrLib
    {
        // CalcContext fields (lazy-allocated).
        private static CalcContext _ctxSummarizeForWindowTiering;
        private static CalcContext _ctxSummarizeForWindowDistFeeTiering;
        private static CalcContext _ctxSummarizeForWindowParticipationTiering;
        private static CalcContext _ctxSubCalculateBalance;
        private static CalcContext _ctxSubTotalGrossReceipts;
        private static CalcContext _ctxSubGrossReceiptsParticipation;
        private static CalcContext _ctxSubOffTheTopDeductions;
        private static CalcContext _ctxSubStatementDisplay;
        private static CalcContext _ctxSubCalculatedExpenses;
        private static CalcContext _ctxSubCalculatedDeductions;
        private static CalcContext _ctxSubDistributionFee;
        private static CalcContext _ctxSubInterest;
        private static CalcContext _ctxSubHvRoyalties;
        private static CalcContext _ctxSubAdvancesAndOffTheBottoms;
        private static CalcContext _ctxSubModifiedAdjustedGrossReceipts;
        private static CalcContext _ctxSubHvTiering;
        private static CalcContext _ctxSubMagrReserves;
        private static CalcContext _ctxSubMagrGrossReceipts;
        private static CalcContext _ctxSubGetImportAdjsPriorItd;
        private static CalcContext _ctxSubCostOfProduction;
        private static CalcContext _ctxSubDistributionExpenses;
        private static CalcContext _ctxSubParticipantShare;
        private static CalcContext _ctxSubPaymentAndTaxes;
        private static CalcContext _ctxSubParticipationsTiering;

        // Shared Reset shape — analogous to RoyaltyLib's NewSubCtxStandard.
        private static CalcContext NewSubCtxStandard(string name) =>
            new CalcContext(name)
                .Reset(EngineCol.UDKey13, EngineCol.UDKey15, EngineCol.UDKey16,
                       EngineCol.ToDate,  EngineCol.Contact3, EngineCol.Contact4);

        // ── 1683 C_MAGR_SUMMARIZE_FOR_WINDOW_TIERING ─────────────────────────
        // sum=Y: UDKey1, UDKey4, UDKey5, UDKey9, UDKey10, UDKey11, Period, ActualPeriod,
        //        Deal, Amount, Units, UDKey12, Amount2, Units2
        public static ResultSet SummarizeForWindowTiering(ResultSet inputSet1)
        {
            if (_ctxSummarizeForWindowTiering == null)
                _ctxSummarizeForWindowTiering = new CalcContext("C_MAGR_SUMMARIZE_FOR_WINDOW_TIERING")
                    .Reset(EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey6, EngineCol.UDKey7,
                           EngineCol.UDKey8, EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15,
                           EngineCol.UDKey16, EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19,
                           EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeForWindowTiering;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
        }

        // ── 1720 C_MAGR_SUMMARIZE_FOR_WINDOW_DIST_FEE_TIERING ────────────────
        // sum=Y: UDKey1, UDKey4, UDKey5, UDKey7, UDKey9, UDKey10, UDKey11, Period,
        //        ActualPeriod, Deal, Amount, Units, UDKey12, Contact4, Amount2, Units2
        public static ResultSet SummarizeForWindowDistFeeTiering(ResultSet inputSet1)
        {
            if (_ctxSummarizeForWindowDistFeeTiering == null)
                _ctxSummarizeForWindowDistFeeTiering = new CalcContext("C_MAGR_SUMMARIZE_FOR_WINDOW_DIST_FEE_TIERING")
                    .Reset(EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey6, EngineCol.UDKey8,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15, EngineCol.UDKey16,
                           EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3);
            Job.CurrentCalcContext = _ctxSummarizeForWindowDistFeeTiering;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
        }

        // ── 1723 C_MAGR_SUMMARIZE_FOR_WINDOW_PARTICIPATION_TIERING ───────────
        // sum=Y: UDKey1, UDKey4, UDKey5, UDKey7, UDKey9, UDKey10, UDKey11, Period,
        //        ActualPeriod, Deal, Amount, Units, UDKey12, Rate3, Contact4, Amount2, Units2
        public static ResultSet SummarizeForWindowParticipationTiering(ResultSet inputSet1)
        {
            if (_ctxSummarizeForWindowParticipationTiering == null)
                _ctxSummarizeForWindowParticipationTiering = new CalcContext("C_MAGR_SUMMARIZE_FOR_WINDOW_PARTICIPATION_TIERING")
                    .Reset(EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey6, EngineCol.UDKey8,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15, EngineCol.UDKey16,
                           EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3);
            Job.CurrentCalcContext = _ctxSummarizeForWindowParticipationTiering;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
        }

        // ── 1693 C_MAGR_SUB_CALCULATE_BALANCE ────────────────────────────────
        //
        // DealScript:
        //   Balance = Getdata INPUTSET1 for ActivityType ∈ MAGR_To_Be_Used_for_Balance_List
        //   Output  = Set Comment   of (
        //             Set AltComment of (
        //             Set ActivityType of Balance to MAGR_Balance) to F_NULL_STRING()) to F_NULL_STRING()
        //   Rate1 of Output = 0
        //
        // Filters all "to be used for balance" rows out of the combined window-ITD
        // input, restamps them as MAGR_Balance, clears comments + Rate1.
        public static ResultSet SubCalculateBalance(ResultSet inputSet1)
        {
            _ctxSubCalculateBalance = _ctxSubCalculateBalance ?? NewSubCtxStandard("C_MAGR_SUB_CALCULATE_BALANCE");
            Job.CurrentCalcContext = _ctxSubCalculateBalance;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubCalculateBalance");

            var balance = inputSet1.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_To_Be_Used_for_Balance_Activity_Type_List,
                "MagrSubCalcBal.Balance");
            balance.SetValue(CustCol.ActivityType, ActivityType.MAGR_Balance);
            var _ops1 = new MathList
            {
                new MathOperation(EngineCol.Comment,    MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.Rate1,      MathOp.SETTO, "0"),
            };
            balance.DoMath(_ops1);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubCalculateBalance");
            return balance;
        }

        // ── 1688 C_MAGR_SUB_TOTAL_GROSS_RECEIPTS ─────────────────────────────
        //
        // DealScript:
        //   TotalGrossReceipts = Getdata INPUTSET1 for ActivityType ∈ MAGR_To_Calculate_Total_Gross_Receipts_List,
        //                                              TransType = ITD
        //   Output = Set ActivityType of TotalGrossReceipts to MAGR_Total_Gross_Receipts
        //   Amount2/Units2/Rate1 of Output = 0
        public static ResultSet SubTotalGrossReceipts(ResultSet inputSet1)
        {
            _ctxSubTotalGrossReceipts = _ctxSubTotalGrossReceipts ?? NewSubCtxStandard("C_MAGR_SUB_TOTAL_GROSS_RECEIPTS");
            Job.CurrentCalcContext = _ctxSubTotalGrossReceipts;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubTotalGrossReceipts");

            var totalGrossReceipts = inputSet1.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_To_Calculate_Total_Gross_Receipts_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.ITD)
            }, "MagrSubTGR.TotalGrossReceipts");
            totalGrossReceipts.SetValue(CustCol.ActivityType, ActivityType.MAGR_Total_Gross_Receipts);
            var _ops2 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units2,  MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
            };
            totalGrossReceipts.DoMath(_ops2);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubTotalGrossReceipts");
            return totalGrossReceipts;
        }

        // ── 1689 C_MAGR_SUB_GROSS_RECEIPTS_PARTICIPATION ─────────────────────
        //
        // DealScript:
        //   ZeroRate1               = Set Rate1 of INPUTSET1 to 0
        //   GrossPartPercent        = Set Rate1 of ZeroRate1 to ContractUDF.MAGR_GrossReceiptsParticipationPercent
        //   NonZeroGrossPartPercent = filter Rate1 != 0; stash Amount → Amount2; Amount = 0
        //   GRParticipation         = Amount2 * Rate1 -> Amount
        //   RoundedGRParticipation  = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(GRParticipation)
        //   Output                  = Set ActivityType of Combine(NonZero, Rounded) to MAGR_Gross_Receipts_Participation
        //
        // Multiplies MAGR_Total_Gross_Receipts by ContractUDF.MAGR_GrossReceiptsParticipationPercent
        // and emits the result as MAGR_Gross_Receipts_Participation rows.
        public static ResultSet SubGrossReceiptsParticipation(ResultSet inputSet1)
        {
            _ctxSubGrossReceiptsParticipation = _ctxSubGrossReceiptsParticipation
                ?? NewSubCtxStandard("C_MAGR_SUB_GROSS_RECEIPTS_PARTICIPATION");
            Job.CurrentCalcContext = _ctxSubGrossReceiptsParticipation;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubGrossReceiptsParticipation");

            // Rate1 <- ContractUDF.MAGR_GrossReceiptsParticipationPercent.
            var grossPartPercent = inputSet1.Copy();
            var _ops3 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_GrossReceiptsParticipationPercent),
            };
            grossPartPercent.DoMath(_ops3);

            var nonZeroGrossPartPercent = grossPartPercent.GetData(
                EngineCol.Rate1, CompareOp.NE, "0", "MagrSubGRP.NonZeroGrossPartPercent");
            grossPartPercent.Release();
            // Stash Amount in Amount2; clear Amount.
            var _ops4 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            nonZeroGrossPartPercent.DoMath(_ops4);

            // GRParticipation = Amount2 * Rate1 -> Amount.
            var grParticipation = nonZeroGrossPartPercent.Copy();
            var _ops5 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
            };
            grParticipation.DoMath(_ops5);

            var roundedGrParticipation = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(grParticipation);
            Job.CurrentCalcContext = _ctxSubGrossReceiptsParticipation;

            var windowsItd = ResultSet.EmptySet();
            windowsItd.CombineAndRelease(nonZeroGrossPartPercent, roundedGrParticipation);
            windowsItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Gross_Receipts_Participation);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubGrossReceiptsParticipation");
            return windowsItd;
        }

        // ── 1687 C_MAGR_SUB_OFF_THE_TOP_DEDUCTIONS ───────────────────────────
        //
        // DealScript:
        //   Deductions       = Getdata INPUTSET1 for ActivityType ∈ MAGR_Imported_Deductions_List
        //                      Rate1 of Deductions = 0
        //   OffTheTopsRate   = Set Rate1 of Deductions to ContractUDF.MAGR_OffTheTopsDeductions
        //   NonZeroRate      = filter Rate1 != 0; Amount2 = Amount; Amount/Units/Units2 = 0
        //   OffTheTopsAmount = Amount2 * Rate1 -> Amount
        //   RoundedOffTheTops = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(OffTheTopsAmount)
        //   Output = Combine(RoundedOffTheTops, NonZeroRate); ActivityType = MAGR_Off_the_Top_Expense
        public static ResultSet SubOffTheTopDeductions(ResultSet inputSet1)
        {
            _ctxSubOffTheTopDeductions = _ctxSubOffTheTopDeductions
                ?? NewSubCtxStandard("C_MAGR_SUB_OFF_THE_TOP_DEDUCTIONS");
            Job.CurrentCalcContext = _ctxSubOffTheTopDeductions;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubOffTheTopDeductions");

            var deductions = inputSet1.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Imported_Deductions_Activity_Type_List,
                "MagrSubOTT.Deductions");
            var _ops6 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_OffTheTopsDeductions),
            };
            deductions.DoMath(_ops6);

            var nonZeroRate = deductions.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubOTT.NonZeroRate");
            deductions.Release();
            // Stash Amount in Amount2; clear value cols.
            var _ops7 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units,   MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units2,  MathOp.SETTO, "0"),
            };
            nonZeroRate.DoMath(_ops7);

            // OffTheTopsAmount = Amount2 * Rate1 -> Amount.
            var offTheTopsAmount = nonZeroRate.Copy();
            var _ops8 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
            };
            offTheTopsAmount.DoMath(_ops8);

            var roundedOffTheTops = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(offTheTopsAmount);
            Job.CurrentCalcContext = _ctxSubOffTheTopDeductions;

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(roundedOffTheTops, nonZeroRate);
            output.SetValue(CustCol.ActivityType, ActivityType.MAGR_Off_the_Top_Expense);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubOffTheTopDeductions");
            return output;
        }

        // ── 1681 C_MAGR_SUB_CALCULATED_DEDUCTIONS ────────────────────────────
        //
        // Three parallel branches over imported activity rows:
        //   PercentDeduction1:  Rate1 = ContractUDF.MAGR_PercentDeduction1; Amount = -Rate1 * Amount
        //   PercentDeduction2:  same with MAGR_PercentDeduction2
        //   PerUnitDeduction:   Rate1 = MAGR_PerUnitDeduction; Amount = -Units * Rate1
        //
        // Each branch rounds and emits with its corresponding ActivityType.
        public static ResultSet SubCalculatedDeductions(ResultSet inputSet1)
        {
            _ctxSubCalculatedDeductions = _ctxSubCalculatedDeductions
                ?? NewSubCtxStandard("C_MAGR_SUB_CALCULATED_DEDUCTIONS");
            Job.CurrentCalcContext = _ctxSubCalculatedDeductions;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubCalculatedDeductions");

            var imported = inputSet1.Copy();
            imported.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

            // Helper: build one branch (either percent-of-amount or per-unit).
            ResultSet PercentBranch(string udfHandle, UDKey2Ref activityHandle)
            {
                var withRate = imported.Copy();
                withRate.DoMath(EngineCol.Rate1, MathOp.SETTO, udfHandle);
                var valid = withRate.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubCalcDed.Valid");
                withRate.Release();
                var _ops9 = new MathList
                {
                    new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                    new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
                };
                valid.DoMath(_ops9);
                var amt = valid.Copy();
                var _ops10 = new MathList
                {
                    new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Amount2),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
                };
                amt.DoMath(_ops10);
                var rounded = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(amt);
                Job.CurrentCalcContext = _ctxSubCalculatedDeductions;
                var output = ResultSet.EmptySet();
                output.CombineAndRelease(rounded, valid);
                output.SetValue(CustCol.ActivityType, activityHandle);
                return output;
            }

            ResultSet UnitBranch(string udfHandle, UDKey2Ref activityHandle)
            {
                var withRate = imported.Copy();
                withRate.DoMath(EngineCol.Rate1, MathOp.SETTO, udfHandle);
                var valid = withRate.GetData(new FilterClause {
                    new Criteria(EngineCol.Rate1, CompareOp.NE, "0"),
                    new Criteria(BaseCol.Units,   CompareOp.NE, "0")
                }, "MagrSubCalcDed.ValidUnitDed");
                withRate.Release();
                var _ops11 = new MathList
                {
                    new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                    new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
                };
                valid.DoMath(_ops11);
                var amt = valid.Copy();
                var _ops12 = new MathList
                {
                    new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Units),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
                };
                amt.DoMath(_ops12);
                var rounded = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(amt);
                Job.CurrentCalcContext = _ctxSubCalculatedDeductions;
                var output = ResultSet.EmptySet();
                output.CombineAndRelease(rounded, valid);
                output.SetValue(CustCol.ActivityType, activityHandle);
                return output;
            }

            var pctDed1Output = PercentBranch(ContractUDF.MAGR_PercentDeduction1, ActivityType.MAGR_Percent_Deduction_1);
            var pctDed2Output = PercentBranch(ContractUDF.MAGR_PercentDeduction2, ActivityType.MAGR_Percent_Deduction_2);
            var unitDedOutput = UnitBranch(ContractUDF.MAGR_PerUnitDeduction,    ActivityType.MAGR_Per_Unit_Deduction);
            imported.Release();

            var deductionsItdOutput = ResultSet.EmptySet();
            deductionsItdOutput.CombineAndRelease(pctDed1Output, pctDed2Output, unitDedOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubCalculatedDeductions");
            return deductionsItdOutput;
        }

        // ── 1690 C_MAGR_SUB_CALCULATED_EXPENSES ──────────────────────────────
        //
        // Same structure as SubCalculatedDeductions but driven from
        // MAGR_Total_Gross_Receipts rows (INPUTSET1 already pre-filtered by caller).
        //   PercentExpense1: Rate1 = ContractUDF.MAGR_PercentExpense1; Amount = -Rate1 * stash
        //   PercentExpense2: same with MAGR_PercentExpense2
        //   PerUnitExpense:  Rate1 = MAGR_PerUnitExpense; Amount = -Units * Rate1
        public static ResultSet SubCalculatedExpenses(ResultSet inputSet1)
        {
            _ctxSubCalculatedExpenses = _ctxSubCalculatedExpenses
                ?? NewSubCtxStandard("C_MAGR_SUB_CALCULATED_EXPENSES");
            Job.CurrentCalcContext = _ctxSubCalculatedExpenses;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubCalculatedExpenses");

            var preExpenses = inputSet1.Copy();
            var _ops13 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units2,  MathOp.SETTO, "0"),
            };
            preExpenses.DoMath(_ops13);

            ResultSet PercentExpenseBranch(string udfHandle, UDKey2Ref activityHandle)
            {
                var withRate = preExpenses.Copy();
                withRate.DoMath(EngineCol.Rate1, MathOp.SETTO, udfHandle);
                var valid = withRate.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubCalcExp.Valid");
                withRate.Release();
                var amt = valid.Copy();
                var _ops14 = new MathList
                {
                    new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Amount2),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
                };
                amt.DoMath(_ops14);
                var rounded = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(amt);
                Job.CurrentCalcContext = _ctxSubCalculatedExpenses;
                var output = ResultSet.EmptySet();
                output.CombineAndRelease(rounded, valid);
                output.SetValue(CustCol.ActivityType, activityHandle);
                return output;
            }

            // Per-unit branch reads Units (not Amount2).
            ResultSet PerUnitExpenseBranch()
            {
                var withRate = preExpenses.Copy();
                withRate.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_PerUnitExpense);
                var valid = withRate.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubCalcExp.ValidUnits");
                withRate.Release();
                var amt = valid.Copy();
                var _ops15 = new MathList
                {
                    new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Units),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
                };
                amt.DoMath(_ops15);
                var rounded = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(amt);
                Job.CurrentCalcContext = _ctxSubCalculatedExpenses;
                var output = ResultSet.EmptySet();
                output.CombineAndRelease(rounded, valid);
                output.SetValue(CustCol.ActivityType, ActivityType.MAGR_Per_Unit_Expense);
                return output;
            }

            var pctExp1 = PercentExpenseBranch(ContractUDF.MAGR_PercentExpense1, ActivityType.MAGR_Percent_Expense_1);
            var pctExp2 = PercentExpenseBranch(ContractUDF.MAGR_PercentExpense2, ActivityType.MAGR_Percent_Expense_2);
            var unitsExp = PerUnitExpenseBranch();
            preExpenses.Release();

            var output1 = ResultSet.EmptySet();
            output1.CombineAndRelease(pctExp1, pctExp2, unitsExp);
            output1.DoMath(BaseCol.Units, MathOp.SETTO, "0");

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubCalculatedExpenses");
            return output1;
        }

        // ── 1700 C_MAGR_SUB_STATEMENT_DISPLAY ────────────────────────────────
        //
        // Produces two row-types for the participation statement:
        //   - MAGR_Statement_Looping rows: one row per (PaymentRecipient, StatementRecipient)
        //     possibility, populated via the engine's PayRecipients/StatementRecipients
        //     keyword (built over Job.ContractParticipants).
        //   - MAGR_Statement_Info row: one row stamped with the statement's start period
        //     (1 period after F_PERIOD_TYPE_PREVIOUS(MAGR_StatementInterval)).
        //
        // Both row-types carry Amount=99 as a "marker" the front-end checks for.
        public static ResultSet SubStatementDisplay()
        {
            _ctxSubStatementDisplay = _ctxSubStatementDisplay
                ?? NewSubCtxStandard("C_MAGR_SUB_STATEMENT_DISPLAY");
            Job.CurrentCalcContext = _ctxSubStatementDisplay;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubStatementDisplay");

            // L_ContractedParty[1] = first ContractedParty entity from Job.ContractParticipants.
            // For a single-contracted-party contract this is straightforward; for multi-party
            // we build the recipient-possibility set once per first party (matches DealScript).
            int firstContractedPartySid = (Job.ContractParticipantSids.Count > 0) ? Job.ContractParticipantSids[0] : 0;

            var withContractedParty = ResultSet.ZeroSet();
            withContractedParty.SetEntity(new ContactCol("ContractedParty"), new AlliantEntity(firstContractedPartySid));
            withContractedParty.SetValue(CustCol.ActivityType, ActivityType.MAGR_Statement_Looping);
            withContractedParty.SetValue(CustCol.TransType,    TransType.Current);

            // PayRecipients keyword expansion (engine-provided): one row per Payment recipient
            // for the seed ContractedParty + period.
            var pmtRecipientPossibilities = withContractedParty.PayRecipients(
                participantCol: new ContactCol("ContractedParty"),
                periodCol:      EngineCol.Period,
                recipientCol:   new ContactCol("PaymentRecipient"),
                rateCol:        EngineCol.Rate1,
                byCol:          BaseCol.Amount);
            withContractedParty.Release();

            // StatementRecipients keyword expansion: same shape but for statement recipients.
            var stmtRecipientPossibilities = pmtRecipientPossibilities.StatementRecipients(
                participantCol: new ContactCol("ContractedParty"),
                periodCol:      EngineCol.Period,
                recipientCol:   new ContactCol("StatementRecipient"));
            pmtRecipientPossibilities.Release();

            // DealScript shuffles UDKey4/5/6 (PmtRecipient/StmtRecipient/Contact4) — we
            // approximate by leaving the recipients in their PayRecipients/StatementRecipients
            // slots; the engine handles the ContactN material directly.
            var statementLoopingOutput = stmtRecipientPossibilities;
            statementLoopingOutput.SetValue(new ContactCol("Contact4"), new AlliantEntity(0));
            statementLoopingOutput.DoMath(BaseCol.Amount, MathOp.SETTO, "99");

            // Statement Info: one ZEROSET row with ActualPeriod = first period of statement interval.
            string magrStatementInterval = Contract.GetUDFString(ContractUDF.MAGR_StatementInterval);
            PeriodItem prevTypePeriod = !string.IsNullOrEmpty(magrStatementInterval)
                ? DS.F_PERIOD_TYPE_PREVIOUS(magrStatementInterval)
                : Job.CalcPeriodPrevious;
            PeriodItem statementStart = DS.F_PERIODS_FROM(prevTypePeriod, 1);

            var statementInfoOutput = ResultSet.ZeroSet();
            statementInfoOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Statement_Info);
            statementInfoOutput.SetValue(CustCol.ActualPeriod, statementStart);
            statementInfoOutput.SetValue(CustCol.TransType,    TransType.Current);
            statementInfoOutput.DoMath(BaseCol.Amount, MathOp.SETTO, "99");

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(statementLoopingOutput, statementInfoOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubStatementDisplay");
            return output;
        }

        // ── 1692 C_MAGR_SUB_DISTRIBUTION_FEE ─────────────────────────────────
        //
        // INPUTSET1 = MAGR_Total_Gross_Receipts WINDOW ITD
        // INPUTSET2 = WindowStartPeriod marker
        // INPUTSET3 = PriorUpToWindowITD
        //
        // ContractUDF.MAGR_DistributionFeeTieringFlag controls tiered vs flat:
        //   When tiered ("Yes"): combine TGR + prior dist-fee summary, run TIERSET on
        //     Amount setting Tier (UDKey14) from ContractUDF.MAGR_DistributionFeePercent.
        //     Round (1547), p_ds_proration_12 to preserve totals, filter window.
        //   When NOT tiered: skip tiering; whole TGR set passes through.
        //   Either way: Rate1 = MAGR_DistributionFeePercent.  Filter Rate1 != 0.
        //   Stash Amount in Amount2; Amount = -Rate1 * Amount2; Round.  Stamp ActivityType.
        public static ResultSet SubDistributionFee(
            ResultSet inputSet1,           // TGR window ITD
            ResultSet inputSet2,           // WindowStartPeriod marker
            ResultSet inputSet3)           // PriorUpToWindow ITD
        {
            _ctxSubDistributionFee = _ctxSubDistributionFee
                ?? NewSubCtxStandard("C_MAGR_SUB_DISTRIBUTION_FEE");
            Job.CurrentCalcContext = _ctxSubDistributionFee;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubDistributionFee");

            string tieringFlag = Contract.GetUDFString(ContractUDF.MAGR_DistributionFeeTieringFlag);
            int N_YesTiered = (tieringFlag == "Yes") ? 1 : 0;

            var L_StartWindowActualPeriod = inputSet2.GetList(CustCol.ActualPeriod);
            AlliantEntity startEntity = L_StartWindowActualPeriod[0];
            PeriodItem startWindowPi = startEntity != null
                ? (PeriodItem)PeriodItem.Items.GetEntityBySid(startEntity.sid)
                : DS.F_INCEPTION();

            var priorDistFee = inputSet3.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Distribution_Fee,
                "MagrSubDF.PriorDistFee");
            var summarizePriorDistFee = SummarizeForWindowDistFeeTiering(priorDistFee);
            Job.CurrentCalcContext = _ctxSubDistributionFee;

            var zeroRate1 = inputSet1.Copy();
            var _ops16 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(CustCol.Tier,    MathOp.SETTO, "0"),
            };
            zeroRate1.DoMath(_ops16);

            ResultSet readyForRoyaltyMethod;
            if (N_YesTiered == 1)
            {
                var tgrForTiering = ResultSet.EmptySet();
                tgrForTiering.CombineAndRelease(zeroRate1.Copy(), summarizePriorDistFee);

                var sortByActualThenOther = new ResultSet.SortOrder(
                    (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Amount,            ResultSet.SortDirection.Ascending));

                var tieringByAmount = tgrForTiering.TierSet(
                    IndexableColumn.Amount,
                    IndexableColumn.Udkey_14_sid,
                    ContractUDF.MAGR_DistributionFeePercent,
                    sortByActualThenOther);

                var roundedForProration = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(tieringByAmount);
                Job.CurrentCalcContext = _ctxSubDistributionFee;

                var roundedTiering = ResultSetProrate.Prorate12(
                    tgrForTiering, roundedForProration,
                    mode: 0, roundingCorrMode: 0,
                    amountScale: 2, qtyScale: 0, altAmountScale: 2, altQtyScale: 0);
                tgrForTiering.Release();
                roundedForProration.Release();

                // Filter to the window range only.
                var auditWindow = DS.F_PERIOD_INTERVAL(startWindowPi, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));
                var windowTiered = roundedTiering.GetData(
                    CustCol.ActualPeriod, ListOp.INLIST, (EntityList)auditWindow,
                    "MagrSubDF.WindowTiered");
                roundedTiering.Release();
                readyForRoyaltyMethod = windowTiered;
                zeroRate1.Release();
            }
            else
            {
                summarizePriorDistFee.Release();
                readyForRoyaltyMethod = zeroRate1;
            }
            readyForRoyaltyMethod.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_DistributionFeePercent);

            var nonZeroRate = readyForRoyaltyMethod.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubDF.NonZeroRate");
            readyForRoyaltyMethod.Release();
            var _ops17 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            nonZeroRate.DoMath(_ops17);

            var distributionFeeAmount = nonZeroRate.Copy();
            var _ops18 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
            };
            distributionFeeAmount.DoMath(_ops18);
            var roundedDistributionFeeAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(distributionFeeAmount);
            Job.CurrentCalcContext = _ctxSubDistributionFee;

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(roundedDistributionFeeAmount, nonZeroRate);
            output.SetValue(CustCol.ActivityType, ActivityType.MAGR_Distribution_Fee);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubDistributionFee");
            return output;
        }

        // ── 1730 C_MAGR_SUB_INTEREST ─────────────────────────────────────────
        //
        // Computes interest on outstanding MAGR principal balance.  Three modes
        // (ContractUDF.MAGR_InterestBalanceType): "No Interest" | "Beginning of Month"
        // | "End of Month" | "Monthly Average".
        //
        // Beg balance: prior MAGR_Final_Principal_Balance row stamped as Beginning.
        // End-of-month adds current-period COP movements.  Average uses (Beg + End) / 2.
        // Interest = -1 * (selected balance * F_INTEREST_RATE_BASE_INTERVAL(start, end)) /
        //                  F_NUM_PERIODS_BETWEEN(prevCalc, currentCalc).
        public static ResultSet SubInterest(
            ResultSet inputSet1,        // POS Advance/Interest COP ITD
            ResultSet inputSet2,        // NEG MAGR_COP within window
            ResultSet inputSet3,        // Prior ITD (full)
            ResultSet inputSet4)        // PriorUpToWindow ITD
        {
            _ctxSubInterest = _ctxSubInterest ?? NewSubCtxStandard("C_MAGR_SUB_INTEREST");
            Job.CurrentCalcContext = _ctxSubInterest;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubInterest");

            // Ignore POS COP rows (inputSet1) -- DealScript does too.
            _ = inputSet1;

            string interestType = Contract.GetUDFString(ContractUDF.MAGR_InterestBalanceType);
            int N_NoInterest    = (interestType == "No Interest")        ? 1 : 0;
            int N_BegOfMonth    = (interestType == "Beginning of Month") ? 1 : 0;
            int N_EndOfMonth    = (interestType == "End of Month")       ? 1 : 0;
            int N_AverageMonth  = (interestType == "Monthly Average")    ? 1 : 0;
            _ = N_NoInterest;

            // Beginning balance: prior ITD's MAGR_Final_Principal_Balance restamped to Beginning.
            var begBalanceItdOutput = inputSet3.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Final_Principal_Balance,
                "MagrSubInt.BegBalanceITDOutput");
            begBalanceItdOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Beginning_Principal_Balance);

            ResultSet begBalanceToUse = N_BegOfMonth == 1 ? begBalanceItdOutput.Copy() : ResultSet.EmptySet();

            // COP delta: window-priors + current INPUTSET2, minus prior-period ITD's COP.
            var priorWindowCop = inputSet4.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Cost_of_Production_Activity_Type_List,
                "MagrSubInt.PriorWindowCOP");
            var copItd = ResultSet.EmptySet();
            copItd.CombineAndRelease(inputSet2.Copy(), priorWindowCop);
            var priorCopItd = inputSet3.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Cost_of_Production_Activity_Type_List,
                "MagrSubInt.PriorCOPITD");
            var copCurrent = ResultSet.Subtract(copItd, priorCopItd, "MagrSubInt.COPCurrent");
            copItd.Release();
            priorCopItd.Release();

            var nonZeroCopCurrent = copCurrent.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubInt.NonZeroCOPCurrent");
            copCurrent.Release();
            nonZeroCopCurrent.DoMath(EngineCol.Comment, MathOp.SETTO, DS.F_NULL_STRING());
            // Filter via Comment="Yes" matches MAGR_InterestBalanceType=Yes-config rows; in BC1 we keep all.
            // (DealScript filters by ContractUDF.MAGR_InterestBalanceType which matches our gate above.)
            var copToUse = nonZeroCopCurrent;

            var begBalanceAndCop = ResultSet.EmptySet();
            begBalanceAndCop.CombineAndRelease(begBalanceItdOutput.Copy(), copToUse);

            ResultSet endOfMonthToUse = N_EndOfMonth == 1 ? begBalanceAndCop.Copy() : ResultSet.EmptySet();

            // Monthly average = Beg + delta, divided by 2.
            var averageBalance = begBalanceAndCop.Copy();
            averageBalance.DoMath(BaseCol.Amount, MathOp.DIVIDEDBY, "2");
            var roundedAverageBalance = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(averageBalance);
            Job.CurrentCalcContext = _ctxSubInterest;
            ResultSet averageToUse = N_AverageMonth == 1 ? roundedAverageBalance.Copy() : ResultSet.EmptySet();
            roundedAverageBalance.Release();
            begBalanceAndCop.Release();

            var interestToCalculate = ResultSet.EmptySet();
            interestToCalculate.CombineAndRelease(begBalanceToUse, endOfMonthToUse, averageToUse);
            interestToCalculate.SetValue(CustCol.ActivityType, ActivityType.MAGR_Monthly_Accrued_Interest);
            interestToCalculate.SetValue(CustCol.TransType,    TransType.Current);
            var _ops19 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            interestToCalculate.DoMath(_ops19);

            int N_NoPeriods = DS.F_NUM_PERIODS_BETWEEN(Job.CalcPeriodPrevious, DS.F_CALC_PERIOD());

            // PriorStatementPeriod = first period after F_PERIOD_TYPES_FROM(calcPeriod, -1, MAGR_StatementInterval).
            string magrStmtInterval = Contract.GetUDFString(ContractUDF.MAGR_StatementInterval);
            PeriodItem priorStmtFromPi = !string.IsNullOrEmpty(magrStmtInterval)
                ? DS.F_PERIOD_TYPES_FROM(DS.F_CALC_PERIOD(), -1, magrStmtInterval)
                : Job.CalcPeriodPrevious;
            PeriodItem priorStmtPi = DS.F_PERIODS_FROM(priorStmtFromPi, 1);

            // F_INTEREST_RATE_BASE_INTERVAL(L_PriorStatementPeriod[1], calcPeriod) — engine
            // helper that averages the contract's index rates over [start..end] using
            // Job.InterestRateHistory.  No SQL round-trip.
            decimal N_InterestRate = DS.F_INTEREST_RATE_BASE_INTERVAL(priorStmtPi, DS.F_CALC_PERIOD());

            // InterestCurrentInterval = -1 * (Amount2 * N_InterestRate) / N_NoPeriods.
            var interestCurrentInterval = interestToCalculate.Copy();
            interestCurrentInterval.DoMath(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2);
            interestCurrentInterval.DoMath(BaseCol.Amount, MathOp.TIMES,
                N_InterestRate.ToString(System.Globalization.CultureInfo.InvariantCulture));
            interestCurrentInterval.DoMath(BaseCol.Amount, MathOp.DIVIDEDBY,
                N_NoPeriods == 0 ? "1" : N_NoPeriods.ToString(System.Globalization.CultureInfo.InvariantCulture));
            interestCurrentInterval.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            var roundedInterestCurrentInterval = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(interestCurrentInterval);
            Job.CurrentCalcContext = _ctxSubInterest;

            var interestCurrentOutput = ResultSet.EmptySet();
            interestCurrentOutput.CombineAndRelease(roundedInterestCurrentInterval, interestToCalculate);
            var interestCurrentOutputSumm = interestCurrentOutput.Summarize();
            if (!ReferenceEquals(interestCurrentOutputSumm, interestCurrentOutput)) interestCurrentOutput.Release();
            Job.CurrentCalcContext = _ctxSubInterest;

            var priorInterestItd = inputSet3.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Monthly_Accrued_Interest,
                "MagrSubInt.PriorInterestITD");
            var interestItdOutput = ResultSet.EmptySet();
            interestItdOutput.CombineAndRelease(interestCurrentOutputSumm.Copy(), priorInterestItd);
            interestItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // Final principal balance = Beg + (-Interest), restamp.
            var negInterest = interestCurrentOutputSumm.Copy();
            negInterest.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            var finalBalance = ResultSet.EmptySet();
            finalBalance.CombineAndRelease(begBalanceItdOutput.Copy(), negInterest);
            finalBalance.SetValue(CustCol.ActivityType, ActivityType.MAGR_Final_Principal_Balance);
            finalBalance.SetValue(CustCol.TransType,    TransType.ITD);
            finalBalance.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");

            begBalanceItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(begBalanceItdOutput, finalBalance, interestCurrentOutputSumm, interestItdOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubInterest");
            return output;
        }

        // ── 1669 C_MAGR_SUB_HV_ROYALTIES ─────────────────────────────────────
        //
        // Home Video royalties pipeline.
        //   1. Filter INPUTSET1 to MAGR_Imported_Home_Video list.
        //   2. Stamp AltComment from ContractUDF.MAGR_HomeVideoRoyaltyActivityTypes.
        //   3. AltComment="No"/blank rows → Gross_Receipts (no royalty calc).
        //   4. AltComment="Yes" rows → ReadyForHMVideo (RightsType <- Home_Video).
        //   5. Run MagrLib.SubReserves on ReadyForHMVideo.  Returns Reserves_Liquidated +
        //      Adj/Beg/End reserve balance rows.
        //   6. Run MagrLib.SubCalculatedDeductions on ReadyForHMVideo.
        //   7. Build RoyaltyBasisITD = Combine(ReadyForHMVideo, ReserveWindowITD,
        //                                       DedWindowITD, RoyBasisUpToWindowITD), tag MAGR_Royalty_Basis.
        //   8. Run MagrLib.SubHvTiering on RoyaltyBasisITD.
        //   9. Run MagrLib.SubGrossReceipts.
        //  10. Filter both to ActualPeriod >= ContractUDF.MAGR_AlliantCutoverPeriod for output.
        //
        // SubReserves/SubHvTiering/SubGrossReceipts are MAGR sub-calcs not yet converted;
        // calls reference the eventual signatures.  When wiring up, ensure those signatures
        // match the parameter shape used here.
        public static (ResultSet HvOutput, ResultSet ReservesOutput) SubHvRoyalties(
            ResultSet inputSet1,           // Imported (MAGR Imported list, window ITD)
            ResultSet inputSet2,           // PriorUpToWindow ITD
            ResultSet inputSet3,           // Prior ITD (full)
            ResultSet inputSet4,           // WindowStartPeriod marker
            ResultSet inputSet5)           // Adjustments ITD
        {
            _ctxSubHvRoyalties = _ctxSubHvRoyalties ?? NewSubCtxStandard("C_MAGR_SUB_HV_ROYALTIES");
            Job.CurrentCalcContext = _ctxSubHvRoyalties;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubHvRoyalties");

            var L_StartWindowActualPeriod = inputSet4.GetList(CustCol.ActualPeriod);
            AlliantEntity startEntity = L_StartWindowActualPeriod[0];
            PeriodItem startWindowPi = startEntity != null
                ? (PeriodItem)PeriodItem.Items.GetEntityBySid(startEntity.sid)
                : DS.F_INCEPTION();
            var auditWindow = DS.F_PERIOD_INTERVAL(startWindowPi, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));

            // ForHMVideoFlag: filter to Home Video Imported list, blank AltComment.
            var forHmVideoFlag = inputSet1.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Imported_Home_Video_Activity_Type_List,
                "MagrSubHvR.ForHMVideoFlag");
            forHmVideoFlag.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());
            var withHmVideoFlag = forHmVideoFlag;
            withHmVideoFlag.DoMath(EngineCol.AltComment, MathOp.SETTO, ContractUDF.MAGR_HomeVideoRoyaltyActivityTypes);

            // No-flag / blank-flag rows → bypass royalty calc and emit as Gross_Receipts.
            var noFlag    = withHmVideoFlag.GetData(EngineCol.AltComment, CompareOp.EQ, "No", "MagrSubHvR.NoFlag");
            var blankFlag = withHmVideoFlag.GetData(EngineCol.AltComment, CompareOp.EQ, "",   "MagrSubHvR.BlankFlag");
            var grossReceiptsNoFlag = ResultSet.EmptySet();
            grossReceiptsNoFlag.CombineAndRelease(noFlag, blankFlag);
            grossReceiptsNoFlag.SetValue(CustCol.ActivityType, ActivityType.MAGR_Gross_Receipts);
            grossReceiptsNoFlag.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());

            var readyForHmVideo = withHmVideoFlag.GetData(EngineCol.AltComment, CompareOp.EQ, "Yes", "MagrSubHvR.ReadyForHMVideo");
            withHmVideoFlag.Release();
            readyForHmVideo.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());
            readyForHmVideo.SetValue(CustCol.RightsType, RightsType.Home_Video);

            // Reserves on the HV-ready set.
            var (reserveLiqCurrentAndItd, reserveBalanceItd) = SubReserves(
                readyForHmVideo.Copy(), inputSet2.Copy(), inputSet3.Copy(), inputSet5.Copy(), inputSet4.Copy());
            Job.CurrentCalcContext = _ctxSubHvRoyalties;

            // Calculated deductions on the HV-ready set.
            var dedWindowItd = SubCalculatedDeductions(readyForHmVideo.Copy());
            Job.CurrentCalcContext = _ctxSubHvRoyalties;

            // ITD-grain Royalty Basis: filter Reserves to window-end (already POS), and prior up-to-window MAGR_Royalty_Basis.
            var reserveWindowItd = reserveLiqCurrentAndItd.GetData(new FilterClause {
                new Criteria(CustCol.TransType,    CompareOp.EQ,    TransType.ITD),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST,   (EntityList)auditWindow)
            }, "MagrSubHvR.ReserveWindowITD");
            var royBasisUpToWindowItd = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Royalty_Basis,
                "MagrSubHvR.RoyBasisUpToWindowITD");

            var royaltyBasisItd = ResultSet.EmptySet();
            royaltyBasisItd.CombineAndRelease(readyForHmVideo.Copy(), reserveWindowItd, dedWindowItd.Copy(), royBasisUpToWindowItd);
            royaltyBasisItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Royalty_Basis);
            royaltyBasisItd.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

            // Tier the basis (forward declarations to SubHvTiering, SubGrossReceipts).
            var royBasisTieredWindowItd = SubHvTiering(royaltyBasisItd, inputSet4.Copy(), inputSet2.Copy(), inputSet3.Copy());
            Job.CurrentCalcContext = _ctxSubHvRoyalties;

            var grossReceiptsWindowItd = SubGrossReceipts(royBasisTieredWindowItd.Copy(), inputSet4.Copy());
            Job.CurrentCalcContext = _ctxSubHvRoyalties;

            // Filter outputs to >= ContractUDF.MAGR_AlliantCutoverPeriod.
            string cutoverDescr = Contract.GetUDFString(ContractUDF.MAGR_AlliantCutoverPeriod);
            PeriodItem cutoverPi = !string.IsNullOrEmpty(cutoverDescr)
                ? (PeriodItem)PeriodItem.Items.GetEntityByDescr(cutoverDescr)
                : startWindowPi;
            var fromCutover = DS.F_PERIOD_INTERVAL(cutoverPi, endOfTime);

            var finals = ResultSet.EmptySet();
            finals.CombineAndRelease(grossReceiptsNoFlag, dedWindowItd, royBasisTieredWindowItd, grossReceiptsWindowItd);

            var output = finals.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)fromCutover,
                "MagrSubHvR.OutputAfterCutover");
            finals.Release();

            var reservesFinal = ResultSet.EmptySet();
            reservesFinal.CombineAndRelease(reserveLiqCurrentAndItd, reserveBalanceItd);
            var reservesOutput = reservesFinal.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)fromCutover,
                "MagrSubHvR.ReservesAfterCutover");
            reservesFinal.Release();
            readyForHmVideo.Release();

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubHvRoyalties");
            return (output, reservesOutput);
        }

        // ── 1670 C_MAGR_SUB_RESERVES ─────────────────────────────────────────
        //
        // Reserve takes (NEG) and liquidations (POS) for MAGR pipeline.
        // Reserves are taken against royalty-basis each period within window;
        // liquidated N periods later (ContractUDF.MAGR_LiquidationPeriod) at
        // ContractUDF.MAGR_ReserveInterval boundaries.
        //
        // INPUTSET1 = ImportedHV trx within window
        // INPUTSET2 = PriorUpToWindow ITD
        // INPUTSET3 = Prior ITD (full)
        // INPUTSET4 = MAGR Adjustments ITD
        // INPUTSET5 = WindowStartPeriod marker
        public static (ResultSet ReserveLiqCurrentAndItd, ResultSet ReserveBalanceItd) SubReserves(
            ResultSet inputSet1,    // imported HV trx
            ResultSet inputSet2,    // prior up-to-window ITD
            ResultSet inputSet3,    // prior ITD full
            ResultSet inputSet4,    // adjustments ITD
            ResultSet inputSet5)    // window-start period
        {
            _ctxSubMagrReserves = _ctxSubMagrReserves ?? NewSubCtxStandard("C_MAGR_SUB_RESERVES");
            Job.CurrentCalcContext = _ctxSubMagrReserves;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubReserves (MAGR)");

            // Resolve window + cutover periods.
            var L_StartWindowActualPeriod = inputSet5.GetList(CustCol.ActualPeriod);
            AlliantEntity startEntity = L_StartWindowActualPeriod[0];
            PeriodItem startWindowPi = startEntity != null
                ? (PeriodItem)PeriodItem.Items.GetEntityBySid(startEntity.sid)
                : DS.F_INCEPTION();
            PeriodItem endOfTime = (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time);
            string cutoverDescr = Contract.GetUDFString(ContractUDF.MAGR_AlliantCutoverPeriod);
            PeriodItem cutoverPi = !string.IsNullOrEmpty(cutoverDescr)
                ? (PeriodItem)PeriodItem.Items.GetEntityByDescr(cutoverDescr)
                : startWindowPi;

            // ─── Phase B: Adjustments to reserve ─────────────────────────────
            var adjReservesItdOutput = inputSet4.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Adjustment_to_Reserve_Activity,
                "MagrSubR.AdjReservesITDOutput");
            var withinCutover = DS.F_PERIOD_INTERVAL(cutoverPi, endOfTime);
            var withinWindow  = DS.F_PERIOD_INTERVAL(startWindowPi, endOfTime);
            var adjReservesWithinWindow = adjReservesItdOutput.GetData(new FilterClause {
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinWindow),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinCutover)
            }, "MagrSubR.AdjReservesWithinWindow");

            var posAdjReservesItd = adjReservesWithinWindow.GetData(BaseCol.Amount, CompareOp.GT, "0", "MagrSubR.PosAdjReservesITD");
            posAdjReservesItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Reserve_Released);
            var negAdjReservesItd = adjReservesWithinWindow.GetData(BaseCol.Amount, CompareOp.LT, "0", "MagrSubR.NegAdjReservesITD");
            negAdjReservesItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Reserve_Taken);
            adjReservesWithinWindow.Release();

            // ─── Phase C: Reserve interval gating ────────────────────────────
            string reserveInterval = Contract.GetUDFString(ContractUDF.MAGR_ReserveInterval);
            int N_NaInterval = (reserveInterval == "NA" || reserveInterval == "<None>" || reserveInterval == "<NONE>") ? 1 : 0;

            ResultSet salesWithinWindowPeriods;
            if (N_NaInterval == 1)
                salesWithinWindowPeriods = ResultSet.EmptySet();
            else
            {
                salesWithinWindowPeriods = inputSet1.GetData(new FilterClause {
                    new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Sales_for_Reserves_Activity_Type_List),
                    new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinCutover)
                }, "MagrSubR.SalesWithinWindowPeriods");
                // Stash ActualPeriod (Period sid) in ScratchInt1 for later restore.
                // Legacy DealScript stashed it in TransType (UDKey3), which is a type
                // mismatch the engine now blocks; ScratchInt is the sanctioned buffer.
                salesWithinWindowPeriods.DoMath(EngineCol.ScratchInt1, MathOp.SETTO, CustCol.ActualPeriod);
            }

            // ─── Phase D: Liquidation period via N_LiqPeriod loop ────────────
            int N_LiqPeriod = Contract.GetUDFInt(ContractUDF.MAGR_LiquidationPeriod);
            var reserveIntervalArgs = ResultSet.ZeroSet();
            var _ops20 = new MathList
            {
                new MathOperation(EngineCol.Comment, MathOp.SETTO, "MAGR_ReserveInterval"),
                new MathOperation(EngineCol.Rate1,   MathOp.SETTO, "1"),
            };
            reserveIntervalArgs.DoMath(_ops20);
            // TODO(uptl-port): remove parity harness and call UdfPeriodTypeLabel directly once field data confirms SQL/C# parity.
            var reserveIntervalPeriodType = ResultSetDSPs.UdfPeriodTypeLabelWithParity(
                "MagrLib.reserveIntervalPeriodType", reserveIntervalArgs);
            reserveIntervalArgs.Release();
            reserveIntervalPeriodType.DoMath(EngineCol.Rate1, MathOp.SETTO, "1");

            var shiftOnePeriod = ResultSet.ZeroSet();
            shiftOnePeriod.DoMath(EngineCol.Rate1, MathOp.SETTO, "1");

            // TODO(pttp-port): remove parity harness and call PeriodToPeriodTypePeriod directly once field data confirms SQL/C# parity.
            var currentReserveInterval = ResultSetDSPs.PeriodToPeriodTypePeriodWithParity(
                "MagrLib.currentReserveInterval",
                salesWithinWindowPeriods.Copy(), reserveIntervalPeriodType);
            currentReserveInterval.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);

            ResultSet currentLoop = currentReserveInterval;
            ResultSet liquidationPeriod = ResultSet.EmptySet();
            for (int N_PeriodCount = 1; N_PeriodCount <= N_LiqPeriod; N_PeriodCount++)
            {
                // TODO(pf-port): remove parity harness and call PeriodsFrom directly once field data confirms SQL/C# parity.
                var currentLoopToUse = ResultSetDSPs.PeriodsFromWithParity(
                    "MagrLib.currentLoopToUse.periodsFrom",
                    currentLoop, shiftOnePeriod);
                // TODO(pttp-port): remove parity harness and call PeriodToPeriodTypePeriod directly once field data confirms SQL/C# parity.
                var oneLiqPeriod = ResultSetDSPs.PeriodToPeriodTypePeriodWithParity(
                    "MagrLib.oneLiqPeriod",
                    currentLoopToUse, reserveIntervalPeriodType);
                currentLoopToUse.Release();
                oneLiqPeriod.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);

                if (N_PeriodCount == N_LiqPeriod)
                    liquidationPeriod.Combine(oneLiqPeriod);

                currentLoop.Release();
                currentLoop = oneLiqPeriod;
            }
            currentLoop.Release();
            shiftOnePeriod.Release();
            reserveIntervalPeriodType.Release();
            salesWithinWindowPeriods.Release();

            // Restore ActualPeriod from ScratchInt1 (the stashed Period sid).
            liquidationPeriod.DoMath(CustCol.ActualPeriod, MathOp.SETTO, EngineCol.ScratchInt1);

            var liqPeriod = liquidationPeriod;
            liqPeriod.SetValue(CustCol.TransType, TransType.ITD);
            liqPeriod.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

            // Reserves Taken = -Reserve_Rate * Amount.
            var royWithReserveRate = liqPeriod;
            royWithReserveRate.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_ReserveRate);
            var nonZeroRate1 = royWithReserveRate.GetData(new FilterClause {
                new Criteria(EngineCol.Rate1, CompareOp.NE, "0"),
                new Criteria(BaseCol.Amount,  CompareOp.NE, "0")
            }, "MagrSubR.NonZeroRate1");
            royWithReserveRate.Release();
            var _ops21 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units,   MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units2,  MathOp.SETTO, "0"),
            };
            nonZeroRate1.DoMath(_ops21);

            var reservesAmount = nonZeroRate1.Copy();
            var _ops22 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
            };
            reservesAmount.DoMath(_ops22);
            var roundedReservesAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(reservesAmount);
            Job.CurrentCalcContext = _ctxSubMagrReserves;

            var reservesTakenWithinWindow = ResultSet.EmptySet();
            reservesTakenWithinWindow.CombineAndRelease(roundedReservesAmount, nonZeroRate1);
            reservesTakenWithinWindow.SetValue(CustCol.ActivityType, ActivityType.MAGR_Reserve_Taken);

            var priorWindowReservesTaken = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Reserve_Taken,
                "MagrSubR.PriorWindowReservesTaken");

            var reservesItdOutput = ResultSet.EmptySet();
            reservesItdOutput.CombineAndRelease(negAdjReservesItd, reservesTakenWithinWindow, priorWindowReservesTaken);

            // Liquidate any reserves whose OtherPeriod == calc-period: negate, restamp, swap actual<->other.
            var liquidateCalcPeriod = reservesItdOutput.GetData(
                CustCol.OtherPeriod, CompareOp.EQ, Job.CurrentCalcPeriod,
                "MagrSubR.LiquidateCalcPeriod");
            liquidateCalcPeriod.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            liquidateCalcPeriod.SetValue(CustCol.ActivityType, ActivityType.MAGR_Reserve_Released);
            // Legacy DealScript stashed ActualPeriod into TransType (Period sid into a
            // UDKey3 column) — a type mismatch the engine now blocks.  Preserve the
            // stash via ScratchInt1 in case a downstream restore ever needs it; the
            // ActualPeriod overwrite matches the original DealScript intent.
            var _ops23 = new MathList
            {
                new MathOperation(EngineCol.ScratchInt1, MathOp.SETTO, CustCol.ActualPeriod),
                new MathOperation(CustCol.ActualPeriod,  MathOp.SETTO, CustCol.OtherPeriod),
            };
            liquidateCalcPeriod.DoMath(_ops23);

            var reservesLiquidatedAndAdjsItd = ResultSet.EmptySet();
            reservesLiquidatedAndAdjsItd.CombineAndRelease(posAdjReservesItd, liquidateCalcPeriod);
            reservesLiquidatedAndAdjsItd.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            var reservesLiquidatedItdOutput = reservesLiquidatedAndAdjsItd.Copy();
            reservesLiquidatedItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // Current = ITD - prior.
            var priorReserves = inputSet3.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Reserve_Activity_Type_List,
                "MagrSubR.PriorReserves");
            var reservesAndLiqItd = ResultSet.EmptySet();
            reservesAndLiqItd.CombineAndRelease(reservesItdOutput.Copy(), reservesLiquidatedAndAdjsItd);
            var reservesLiquidatedCurrentOutput = ResultSet.Subtract(reservesAndLiqItd, priorReserves, "MagrSubR.ReservesLiquidatedCurrent");
            reservesAndLiqItd.Release();
            priorReserves.Release();
            reservesLiquidatedCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // BegReserveITDOutput = prior Final Reserve restamped to Beginning.
            var begReserveItdOutput = inputSet3.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Final_Reserve_Balance,
                "MagrSubR.BegReserveITDOutput");
            begReserveItdOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Beginning_Reserve_Balance);

            // FinalReservesITD = Combine + restamp.
            var finalReservesItd = ResultSet.EmptySet();
            finalReservesItd.CombineAndRelease(reservesLiquidatedCurrentOutput.Copy(), begReserveItdOutput.Copy());
            finalReservesItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Final_Reserve_Balance);

            // ─── Final assembly ──────────────────────────────────────────────
            var output = ResultSet.EmptySet();
            output.CombineAndRelease(reservesItdOutput, reservesLiquidatedItdOutput, reservesLiquidatedCurrentOutput);

            var balanceItdOutput = ResultSet.EmptySet();
            balanceItdOutput.CombineAndRelease(begReserveItdOutput, finalReservesItd, adjReservesItdOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubReserves (MAGR)");
            return (output, balanceItdOutput);
        }

        // ── 1682 C_MAGR_SUB_HV_TIERING ───────────────────────────────────────
        //
        // Tiers MAGR_Royalty_Basis rows by Amount or Units against ContractUDF.MAGR_TierByAmount /
        // ContractUDF.MAGR_TierByUnits.  Tiering interval is per ContractUDF.MAGR_TierInterval.
        //
        // Mirrors RoyaltyLib.SubTiering closely but reads MAGR-prefixed UDFs.
        public static ResultSet SubHvTiering(
            ResultSet royaltyBasisItd,    // INPUTSET1 (MAGR_Royalty_Basis ITD)
            ResultSet windowStartPeriod,  // INPUTSET2
            ResultSet priorUpToWindow,    // INPUTSET3
            ResultSet priorItd)           // INPUTSET4
        {
            _ctxSubHvTiering = _ctxSubHvTiering ?? NewSubCtxStandard("C_MAGR_SUB_HV_TIERING");
            Job.CurrentCalcContext = _ctxSubHvTiering;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubHvTiering");
            _ = priorUpToWindow; _ = priorItd;   // not directly read in this conversion

            string tierInterval = Contract.GetUDFString(ContractUDF.MAGR_TierInterval);
            int N_NoneLabel = (tierInterval == "<None>" || tierInterval == "<NONE>" || tierInterval == "No Tiering") ? 1 : 0;

            var L_StartWindowActualPeriod = windowStartPeriod.GetList(CustCol.ActualPeriod);
            AlliantEntity startWinEntity = L_StartWindowActualPeriod[0];
            PeriodItem startWinPi = startWinEntity != null
                ? (PeriodItem)PeriodItem.Items.GetEntityBySid(startWinEntity.sid)
                : DS.F_INCEPTION();
            PeriodItem calcMinus1 = DS.F_PERIODS_FROM(Job.CurrentCalcPeriod, -1);

            int N_PeriodInterval = !string.IsNullOrEmpty(tierInterval)
                ? DS.F_NUM_PERIOD_TYPES_BETWEEN(startWinPi, calcMinus1, tierInterval)
                : 0;
            int N_GoBackPeriodInterval = -1 * (N_PeriodInterval + 1);

            PeriodItem startPeriodPi;
            if (N_GoBackPeriodInterval == -1 || string.IsNullOrEmpty(tierInterval))
                startPeriodPi = DS.F_INCEPTION();
            else
            {
                var capStart = DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, N_GoBackPeriodInterval, tierInterval);
                startPeriodPi = DS.F_PERIODS_FROM(capStart, 1);
            }

            // PriorRoyBasis = filter INPUTSET1 to ActualPeriod ∈ [startPeriod..startWindow-1].
            PeriodItem startWinMinus1 = DS.F_PERIODS_FROM(startWinPi, -1);
            var priorRange = DS.F_PERIOD_INTERVAL(startPeriodPi, startWinMinus1);
            var priorRoyBasis = royaltyBasisItd.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)priorRange,
                "MagrSubHvT.PriorRoyBasis");
            var summarizePriorRoyBasis = SummarizeForWindowTiering(priorRoyBasis);
            Job.CurrentCalcContext = _ctxSubHvTiering;

            var withinWindow = DS.F_PERIOD_INTERVAL(startWinPi, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));
            var royBasisWithinWindow = royaltyBasisItd.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinWindow,
                "MagrSubHvT.RoyBasisWithinWindow");

            var royBasisForTiering = ResultSet.EmptySet();
            royBasisForTiering.CombineAndRelease(summarizePriorRoyBasis, royBasisWithinWindow.Copy());

            ResultSet noTiering = N_NoneLabel == 1 ? royBasisWithinWindow.Copy() : ResultSet.EmptySet();
            royBasisWithinWindow.Release();

            ResultSet forTiering;
            if (N_NoneLabel == 0)
            {
                forTiering = royBasisForTiering;
                forTiering.DoMath(CustCol.Tier, MathOp.SETTO, "0");
            }
            else
            {
                royBasisForTiering.Release();
                forTiering = ResultSet.EmptySet();
            }

            var tieringResult = ResultSet.EmptySet();
            if (N_NoneLabel == 0 && forTiering.Rows > 0)
            {
                var L_PeriodAsc  = forTiering.GetList(CustCol.ActualPeriod, ascending: true);
                var L_PeriodDesc = forTiering.GetList(CustCol.ActualPeriod, ascending: false);
                AlliantEntity periodFirst = L_PeriodAsc[0];
                AlliantEntity periodLast  = L_PeriodDesc[0];
                int N_PeriodsBetween = 1;
                if (periodFirst != null && periodLast != null)
                {
                    var firstPi    = (PeriodItem)PeriodItem.Items.GetEntityBySid(periodFirst.sid);
                    var lastPi     = (PeriodItem)PeriodItem.Items.GetEntityBySid(periodLast.sid);
                    var lastMinus1 = DS.F_PERIODS_FROM(lastPi, -1);
                    N_PeriodsBetween = DS.F_NUM_PERIOD_TYPES_BETWEEN(firstPi, lastMinus1, tierInterval) + 1;
                    if (N_PeriodsBetween < 1) N_PeriodsBetween = 1;
                }

                var sortByAmount = new ResultSet.SortOrder(
                    (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Amount,            ResultSet.SortDirection.Ascending));
                var sortByUnits = new ResultSet.SortOrder(
                    (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Qty,               ResultSet.SortDirection.Ascending));

                AlliantEntity firstEntity = L_PeriodAsc[0];
                PeriodItem firstPiSafe = firstEntity != null
                    ? (PeriodItem)PeriodItem.Items.GetEntityBySid(firstEntity.sid)
                    : null;
                PeriodItem firstMinus1 = firstPiSafe != null ? DS.F_PERIODS_FROM(firstPiSafe, -1) : null;

                int N_FromCount = 0;
                int N_ToCount   = 1;
                while (N_ToCount <= N_PeriodsBetween)
                {
                    PeriodItem intervalStart, intervalEnd;
                    if (N_PeriodsBetween == 1 || firstMinus1 == null)
                    {
                        intervalStart = null;
                        intervalEnd   = null;
                    }
                    else
                    {
                        var iStart = DS.F_PERIOD_TYPES_FROM(firstMinus1, N_FromCount, tierInterval);
                        intervalStart = DS.F_PERIODS_FROM(iStart, 1);
                        intervalEnd   = DS.F_PERIOD_TYPES_FROM(firstMinus1, N_ToCount,  tierInterval);
                    }

                    ResultSet onePeriodRevenue = (intervalStart == null || intervalEnd == null)
                        ? forTiering.Copy()
                        : forTiering.GetData(
                            CustCol.ActualPeriod, ListOp.INLIST, (EntityList)DS.F_PERIOD_INTERVAL(intervalStart, intervalEnd),
                            "MagrSubHvT.OnePeriodRevenue");

                    if (onePeriodRevenue.Rows == 0)
                    {
                        onePeriodRevenue.Release();
                        N_FromCount++; N_ToCount++;
                        continue;
                    }

                    var tieringByAmount = onePeriodRevenue.TierSet(
                        IndexableColumn.Amount, IndexableColumn.Udkey_14_sid,
                        ContractUDF.MAGR_TierByAmount, sortByAmount);
                    var validTierByAmount = tieringByAmount.GetData(CustCol.Tier, CompareOp.NE, "0", "MagrSubHvT.ValidTierByAmount");
                    var notTiered         = tieringByAmount.GetData(CustCol.Tier, CompareOp.EQ, "0", "MagrSubHvT.NotTiered");
                    tieringByAmount.Release();
                    onePeriodRevenue.Release();

                    var tieringByUnits = notTiered.TierSet(
                        IndexableColumn.Qty, IndexableColumn.Udkey_14_sid,
                        ContractUDF.MAGR_TierByUnits, sortByUnits);
                    notTiered.Release();

                    tieringResult.Combine(validTierByAmount);
                    tieringResult.Combine(tieringByUnits);
                    validTierByAmount.Release();
                    tieringByUnits.Release();

                    N_FromCount++;
                    N_ToCount++;
                }
            }

            // Round + p_ds_proration_12 to preserve totals.
            ResultSet roundedTiering;
            if (N_NoneLabel == 0)
            {
                var roundedForProration = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(tieringResult);
                Job.CurrentCalcContext = _ctxSubHvTiering;
                var baseProration = forTiering.Copy();
                roundedTiering = ResultSetProrate.Prorate12(
                    baseProration, roundedForProration,
                    mode: 0, roundingCorrMode: 0,
                    amountScale: 2, qtyScale: 0, altAmountScale: 2, altQtyScale: 0);
                baseProration.Release();
                roundedForProration.Release();
            }
            else
            {
                roundedTiering = ResultSet.EmptySet();
                tieringResult.Release();
            }
            forTiering.Release();

            // Filter to within-window only, combine with NoTiering.
            var withinWindowFinal = roundedTiering.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinWindow,
                "MagrSubHvT.WithinWindowFinal");
            roundedTiering.Release();

            var basisItd = ResultSet.EmptySet();
            basisItd.CombineAndRelease(noTiering, withinWindowFinal);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubHvTiering");
            return basisItd;
        }

        // ── 1701 C_MAGR_SUB_ADVANCES_AND_OFF_THE_BOTTOMS ─────────────────────
        //
        // Two parallel recoupment streams (Advances + Off-the-Bottoms) consuming
        // current MAGR_Total_Participant_Share rows.
        //
        // INPUTSET1 = MAGR_Participant_Share_and_3rd_Party_Deductible Current rows
        // INPUTSET2 = Prior ITD (full)
        // INPUTSET3 = MAGR_Adjustments ITD
        //
        // Returns INPUTSET4 (combined output) and INPUTSET5 (Total_Participant_Share Current).
        public static (ResultSet AdvancesAndOTBOutput,
                       ResultSet TotalPartShareCurrent) SubAdvancesAndOffTheBottoms(
            ResultSet inputSet1,    // ParticipantShare Current
            ResultSet inputSet2,    // Prior ITD
            ResultSet inputSet3)    // MAGR Adjustments ITD
        {
            _ctxSubAdvancesAndOffTheBottoms = _ctxSubAdvancesAndOffTheBottoms
                ?? NewSubCtxStandard("C_MAGR_SUB_ADVANCES_AND_OFF_THE_BOTTOMS");
            Job.CurrentCalcContext = _ctxSubAdvancesAndOffTheBottoms;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubAdvancesAndOffTheBottoms");

            // ─── ADV ITD/Current ─────────────────────────────────────────────
            var advItdOutput = inputSet3.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Advance_and_Write_Off_Adjustment_Activity_Type_List,
                "MagrSubAOTB.AdvITDOutput");
            advItdOutput.DoMath(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount);
            var priorAdvItd = inputSet2.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Advance_and_Write_Off_Adjustment_Activity_Type_List,
                "MagrSubAOTB.PriorAdvITD");
            var advCurrentOutput = ResultSet.Subtract(advItdOutput, priorAdvItd, "MagrSubAOTB.AdvCurrent");
            priorAdvItd.Release();
            advCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // ─── Off the Bottoms ITD/Current ─────────────────────────────────
            var offTheBottomsItdOutput = inputSet3.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Off_the_Bottoms_Adjustment_Activity_Type_List,
                "MagrSubAOTB.OffTheBottomsITDOutput");
            offTheBottomsItdOutput.DoMath(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount);
            var priorOffTheBottomsItd = inputSet2.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Off_the_Bottoms_Adjustment_Activity_Type_List,
                "MagrSubAOTB.PriorOffTheBottomsITD");
            var offTheBottomsCurrentOutput = ResultSet.Subtract(offTheBottomsItdOutput, priorOffTheBottomsItd, "MagrSubAOTB.OffTheBottomsCurrent");
            priorOffTheBottomsItd.Release();
            offTheBottomsCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // ─── Beginning balances from prior Final ─────────────────────────
            var priorFinalAdvBalance = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Final_Advance_Balance,
                "MagrSubAOTB.PriorFinalAdvBalance");
            var begAdvBalanceItdOutput = priorFinalAdvBalance.Copy();
            begAdvBalanceItdOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Beginning_Advance_Balance);

            var priorFinalOffTheBottomsBalance = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Final_Off_the_Bottoms_Balance,
                "MagrSubAOTB.PriorFinalOTBBal");
            var begOffTheBottomsBalanceItdOutput = priorFinalOffTheBottomsBalance.Copy();
            begOffTheBottomsBalanceItdOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Beginning_Off_the_Bottoms_Balance);

            // ─── Total adjs to recoupe ───────────────────────────────────────
            var advAdj = advCurrentOutput.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Advance_Adjustment_Activity_Type_List,
                "MagrSubAOTB.AdvAdj");
            var advToRecoupe = ResultSet.EmptySet();
            advToRecoupe.CombineAndRelease(priorFinalAdvBalance.Copy(), advAdj);
            var nonZeroAdvToRecoupe = advToRecoupe.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubAOTB.NonZeroAdvToRecoupe");
            advToRecoupe.Release();

            var offTheBottomsToRecoupe = ResultSet.EmptySet();
            offTheBottomsToRecoupe.CombineAndRelease(priorFinalOffTheBottomsBalance.Copy(), offTheBottomsCurrentOutput.Copy());
            var nonZeroOffTheBottomsToRecoupe = offTheBottomsToRecoupe.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubAOTB.NonZeroOTBToRecoupe");
            offTheBottomsToRecoupe.Release();

            // For-recoupment source: ParticipantShare current rows.
            var forRecoupment = inputSet1.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Participant_Share_and_3rd_Party_Deductible_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.Current)
            }, "MagrSubAOTB.ForRecoupment");

            // ─── Applied to Advance via p_ds_ordered_allocation (ActualPeriod ASC, Amount DESC) ─
            var recoupmentProcessBy = ResultSet.ZeroSet();
            recoupmentProcessBy.DoMath(EngineCol.Comment, MathOp.SETTO, "ActualPeriod ASC , Amount DESC");
            var balanceProcessBy = ResultSet.ZeroSet();
            balanceProcessBy.DoMath(EngineCol.Comment, MathOp.SETTO, "ActualPeriod ASC");

            var advSummarized = CommonLib.SummarizeToRecoupmentGroup(nonZeroAdvToRecoupe.Copy());
            Job.CurrentCalcContext = _ctxSubAdvancesAndOffTheBottoms;
            // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
            var advRecoupment = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.advRecoupment",
                forRecoupment, advSummarized, recoupmentProcessBy);
            advSummarized.Release();

            var negAdvRecoupment = advRecoupment.Copy();
            negAdvRecoupment.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            var appliedToAdvCurrentOutput = negAdvRecoupment;
            appliedToAdvCurrentOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Applied_to_Advances);

            var priorAppliedAdv = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Applied_to_Advances,
                "MagrSubAOTB.PriorAppliedAdv");
            var appliedToAdvItdOutput = ResultSet.EmptySet();
            appliedToAdvItdOutput.CombineAndRelease(appliedToAdvCurrentOutput.Copy(), priorAppliedAdv);
            appliedToAdvItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // ─── Final Advance Balance ───────────────────────────────────────
            var advRecoupmentSum = CommonLib.SummarizeToRecoupmentGroup(advRecoupment.Copy());
            Job.CurrentCalcContext = _ctxSubAdvancesAndOffTheBottoms;
            // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
            var advanceToUse = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.advanceToUse",
                nonZeroAdvToRecoupe, advRecoupmentSum, balanceProcessBy);
            advRecoupmentSum.Release();
            var remainingAdv = ResultSet.Subtract(nonZeroAdvToRecoupe, advanceToUse, "MagrSubAOTB.RemainingAdv");
            advanceToUse.Release();
            var finalAdvBal = remainingAdv;
            finalAdvBal.SetValue(CustCol.ActivityType, ActivityType.MAGR_Final_Advance_Balance);
            finalAdvBal.SetValue(CustCol.TransType,    TransType.ITD);
            var nonZeroFinalAdvBal = finalAdvBal.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubAOTB.NonZeroFinalAdvBal");
            finalAdvBal.Release();

            // Apply Write-Off adjustments to FinalAdvBal.
            var currentWriteOff = advCurrentOutput.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Advance_Write_Off),
                new Criteria(BaseCol.Amount,       CompareOp.NE, "0")
            }, "MagrSubAOTB.CurrentWriteOff");
            var summarizedCurrentWriteOff = CommonLib.SummarizeToRecoupmentGroup(currentWriteOff);
            Job.CurrentCalcContext = _ctxSubAdvancesAndOffTheBottoms;
            // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
            var writeOffInDetail = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.writeOffInDetail",
                nonZeroFinalAdvBal, summarizedCurrentWriteOff, recoupmentProcessBy);
            summarizedCurrentWriteOff.Release();
            var finalAdvLessWriteOff = ResultSet.Subtract(nonZeroFinalAdvBal, writeOffInDetail, "MagrSubAOTB.FinalAdvLessWriteOff");
            nonZeroFinalAdvBal.Release();
            writeOffInDetail.Release();
            var finalAdvanceOutput = finalAdvLessWriteOff.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubAOTB.FinalAdvanceOutput");
            finalAdvLessWriteOff.Release();

            // ─── Applied to Off the Bottoms ──────────────────────────────────
            var remainingForRecoupment = ResultSet.EmptySet();
            remainingForRecoupment.CombineAndRelease(forRecoupment.Copy(), negAdvRecoupment.Copy());
            var remainingForRecoupmentSumm = remainingForRecoupment.Summarize();
            if (!ReferenceEquals(remainingForRecoupmentSumm, remainingForRecoupment)) remainingForRecoupment.Release();
            Job.CurrentCalcContext = _ctxSubAdvancesAndOffTheBottoms;

            var offTheBottomsSummarized = CommonLib.SummarizeToRecoupmentGroup(nonZeroOffTheBottomsToRecoupe.Copy());
            Job.CurrentCalcContext = _ctxSubAdvancesAndOffTheBottoms;
            // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
            var offTheBottomsRecoupment = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.offTheBottomsRecoupment",
                remainingForRecoupmentSumm, offTheBottomsSummarized, recoupmentProcessBy);
            offTheBottomsSummarized.Release();
            remainingForRecoupmentSumm.Release();

            var negOffTheBottomsRecoupment = offTheBottomsRecoupment.Copy();
            negOffTheBottomsRecoupment.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            var appliedToOffTheBottomsCurrentOutput = negOffTheBottomsRecoupment;
            appliedToOffTheBottomsCurrentOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Applied_to_Off_the_Bottoms);

            var priorAppliedOffTheBottom = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Applied_to_Off_the_Bottoms,
                "MagrSubAOTB.PriorAppliedOTB");
            var appliedToOffTheBottomsItdOutput = ResultSet.EmptySet();
            appliedToOffTheBottomsItdOutput.CombineAndRelease(appliedToOffTheBottomsCurrentOutput.Copy(), priorAppliedOffTheBottom);
            appliedToOffTheBottomsItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // ─── Final Off the Bottoms Balance ───────────────────────────────
            var offTheBottomsRecoupmentSum = CommonLib.SummarizeToRecoupmentGroup(offTheBottomsRecoupment.Copy());
            Job.CurrentCalcContext = _ctxSubAdvancesAndOffTheBottoms;
            // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
            var offTheBottomsToUse = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.offTheBottomsToUse",
                nonZeroOffTheBottomsToRecoupe, offTheBottomsRecoupmentSum, balanceProcessBy);
            offTheBottomsRecoupmentSum.Release();
            var remainingOTB = ResultSet.Subtract(nonZeroOffTheBottomsToRecoupe, offTheBottomsToUse, "MagrSubAOTB.RemainingOTB");
            offTheBottomsToUse.Release();
            var finalOTBBal = remainingOTB;
            finalOTBBal.SetValue(CustCol.ActivityType, ActivityType.MAGR_Final_Off_the_Bottoms_Balance);
            finalOTBBal.SetValue(CustCol.TransType,    TransType.ITD);
            var finalOffTheBottomsOutput = finalOTBBal.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubAOTB.FinalOTBOutput");
            finalOTBBal.Release();

            // ─── Total Participant Share Adjustments ─────────────────────────
            var partAdjItdOutput = inputSet3.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Total_Participant_Adjustment_Activity_Type_List,
                "MagrSubAOTB.PartAdjITDOutput");
            var priorPartAdjItd = inputSet2.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Total_Participant_Adjustment_Activity_Type_List,
                "MagrSubAOTB.PriorPartAdjITD");
            var partAdjCurrentOutput = ResultSet.Subtract(partAdjItdOutput, priorPartAdjItd, "MagrSubAOTB.PartAdjCurrent");
            priorPartAdjItd.Release();
            partAdjCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // Total Participant Share = forRecoupment + Applied + Applied + PartAdj.
            var totalPartShareCurrent = ResultSet.EmptySet();
            totalPartShareCurrent.CombineAndRelease(
                forRecoupment.Copy(),
                appliedToAdvCurrentOutput.Copy(),
                appliedToOffTheBottomsCurrentOutput.Copy(),
                partAdjCurrentOutput.Copy());
            totalPartShareCurrent.SetValue(CustCol.ActivityType, ActivityType.MAGR_Total_Participant_Share);
            var _ops24 = new MathList
            {
                new MathOperation(BaseCol.Units,  MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units2, MathOp.SETTO, "0"),
            };
            totalPartShareCurrent.DoMath(_ops24);

            var priorTotalPartShare = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Total_Participant_Share,
                "MagrSubAOTB.PriorTotalPartShare");
            var totalPartShareItd = ResultSet.EmptySet();
            totalPartShareItd.CombineAndRelease(totalPartShareCurrent.Copy(), priorTotalPartShare);
            totalPartShareItd.SetValue(CustCol.TransType, TransType.ITD);

            var output = ResultSet.EmptySet();
            output.Combine(advItdOutput);
            output.Combine(advCurrentOutput);
            output.Combine(offTheBottomsItdOutput);
            output.Combine(offTheBottomsCurrentOutput);
            output.Combine(begAdvBalanceItdOutput);
            output.Combine(begOffTheBottomsBalanceItdOutput);
            output.Combine(appliedToAdvCurrentOutput);
            output.Combine(appliedToAdvItdOutput);
            output.Combine(finalAdvanceOutput);
            output.Combine(appliedToOffTheBottomsCurrentOutput);
            output.Combine(appliedToOffTheBottomsItdOutput);
            output.Combine(finalOffTheBottomsOutput);
            output.Combine(totalPartShareItd);
            output.Combine(partAdjItdOutput);
            output.Combine(partAdjCurrentOutput);

            advItdOutput.Release(); advCurrentOutput.Release();
            offTheBottomsItdOutput.Release(); offTheBottomsCurrentOutput.Release();
            begAdvBalanceItdOutput.Release(); begOffTheBottomsBalanceItdOutput.Release();
            appliedToAdvCurrentOutput.Release(); appliedToAdvItdOutput.Release();
            finalAdvanceOutput.Release();
            appliedToOffTheBottomsCurrentOutput.Release(); appliedToOffTheBottomsItdOutput.Release();
            finalOffTheBottomsOutput.Release();
            totalPartShareItd.Release();
            partAdjItdOutput.Release(); partAdjCurrentOutput.Release();
            forRecoupment.Release();
            advRecoupment.Release(); offTheBottomsRecoupment.Release();
            recoupmentProcessBy.Release(); balanceProcessBy.Release();

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubAdvancesAndOffTheBottoms");
            return (output, totalPartShareCurrent);
        }

        // ── 1696 C_MAGR_SUB_MODIFIED_ADJUSTED_GROSS_RECEIPTS ─────────────────
        //
        // Combines MAGR_Balance, COP-window, interest-window, and 3rd-party-deductible
        // contributions to produce MAGR_Modified_Adjusted_Gross_Receipts.
        //
        // 3rd-party-deductible piece pulls cross-deal calc results via three SQL DSPs
        // (p_ds_get_deal_list, p_ds_get_deal_run_status, p_ds_get_calc_results) and
        // multiplies them by ContractUDF.MAGR_3rdParty_MAGR_Percent, gated by
        // ContractUDF.MAGR_3rdParty_MAGR_ShareChoice.
        public static ResultSet SubModifiedAdjustedGrossReceipts(
            ResultSet inputSet1,    // MAGR_Balance window ITD
            ResultSet inputSet2,    // MAGR COP window ITD (NEG)
            ResultSet inputSet3,    // Interest window
            ResultSet inputSet4,    // WindowStartPeriod marker
            ResultSet inputSet5,    // PriorUpToWindow ITD
            ResultSet inputSet6)    // POS Advance/Interest COP ITD
        {
            _ctxSubModifiedAdjustedGrossReceipts = _ctxSubModifiedAdjustedGrossReceipts
                ?? NewSubCtxStandard("C_MAGR_SUB_MODIFIED_ADJUSTED_GROSS_RECEIPTS");
            Job.CurrentCalcContext = _ctxSubModifiedAdjustedGrossReceipts;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubModifiedAdjustedGrossReceipts");

            var L_StartWindowActualPeriod = inputSet4.GetList(CustCol.ActualPeriod);
            AlliantEntity startEntity = L_StartWindowActualPeriod[0];
            PeriodItem startWindowPi = startEntity != null
                ? (PeriodItem)PeriodItem.Items.GetEntityBySid(startEntity.sid)
                : DS.F_INCEPTION();
            var auditWindow = DS.F_PERIOD_INTERVAL(startWindowPi, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));

            // ─── 3rd Party Deductible MAGR retrieval ─────────────────────────
            string shareChoice = Contract.GetUDFString(ContractUDF.MAGR_3rdParty_MAGR_ShareChoice);
            // 100076:457 = "Total Participant Share", 100076:458 = "Adjusted Gross Receipts",
            // 100076:456 = "<None>".  Accept all three textual forms.
            bool hasTotalPartShare    = shareChoice == "Total Participant Share";
            bool hasAdjustedToGR      = shareChoice == "Adjusted Gross Receipts";
            bool hasNoneShare         = shareChoice == "<None>" || shareChoice == "<NONE>" || string.IsNullOrEmpty(shareChoice);
            _ = hasNoneShare;

            ResultSet thirdPartyMagr;
            ResultSet prior3rdPartyWindowItd;
            if (!hasNoneShare)
            {
                // p_ds_get_deal_list for Participation deals where MAGR_3rdPartyDeductible_MAGR_DealIDs UDF matches.
                var dspGetParticipation = ResultSet.ZeroSet();
                var _ops25 = new MathList
                {
                    new MathOperation(EngineCol.Comment, MathOp.SETTO, "Participation"),
                    new MathOperation(EngineCol.Rate1,   MathOp.SETTO, "7"),
                };
                dspGetParticipation.DoMath(_ops25);
                var dspDealRow = ResultSet.ZeroSet();
                dspDealRow.SetTextValue(EngineCol.Comment,    "Contract");
                dspDealRow.SetTextValue(EngineCol.AltComment, "MAGR_3rdPartyDeductible_MAGR_DealIDs");
                dspDealRow.DoMath(EngineCol.Rate1,      MathOp.SETTO, "1");
                var dspSameProcess = ResultSet.ZeroSet();
                dspSameProcess.SetTextValue(EngineCol.Comment,    "Deal");
                dspSameProcess.SetTextValue(EngineCol.AltComment, "DealType");
                dspSameProcess.DoMath(EngineCol.Rate1,      MathOp.SETTO, "4");
                var passToDealList = ResultSet.EmptySet();
                passToDealList.CombineAndRelease(dspGetParticipation, dspDealRow, dspSameProcess);
                var dspDealList = DS.ExecuteDSP("p_ds_get_deal_list", passToDealList);
                passToDealList.Release();

                // Run-status sweep + error-in-run.
                var dspGetStatusInput = ResultSet.ZeroSet();
                dspGetStatusInput.SetValue(CustCol.OtherPeriod, Job.CurrentCalcPeriod);
                var dspRunStatus = DS.ExecuteDSP("p_ds_get_deal_run_status", dspGetStatusInput, dspDealList);
                dspGetStatusInput.Release();

                var completeStatus = ResultSet.ZeroSet();
                completeStatus.DoMath(EngineCol.Rate1, MathOp.SETTO, "16");
                var approvedStatus = ResultSet.ZeroSet();
                approvedStatus.DoMath(EngineCol.Rate1, MathOp.SETTO, "5");
                var completeOrApproved = ResultSet.EmptySet();
                completeOrApproved.CombineAndRelease(completeStatus, approvedStatus);
                var L_CompleteApproved = completeOrApproved.GetList(EngineCol.Rate1);
                completeOrApproved.Release();

                var approvedDeals = dspRunStatus.GetData(EngineCol.Rate1, ListOp.INLIST, L_CompleteApproved, "MagrSubMAGR.ApprovedDeals");
                var dealsInSetup  = dspDealList.GetData(EngineCol.Rate2, CompareOp.EQ, "27", "MagrSubMAGR.DealsInSetup");
                var modelDealsInSetup = dspDealList.GetData(EngineCol.Rate2, CompareOp.EQ, "30", "MagrSubMAGR.ModelDealsInSetup");
                var dealsNotInError = ResultSet.EmptySet();
                dealsNotInError.CombineAndRelease(approvedDeals, dealsInSetup, modelDealsInSetup);
                var L_DealsNotInError = dealsNotInError.GetList(EngineCol.Comment);
                dealsNotInError.Release();
                var dealsInError = dspDealList.GetData(EngineCol.Comment, ListOp.NOTINLIST, L_DealsNotInError, "MagrSubMAGR.DealsInError");
                if (dealsInError.Rows > 0)
                {
                    var errIs1 = dealsInError.Copy();
                    errIs1.DoMath(EngineCol.AltComment, MathOp.SETTO,
                        "- Is a contributing Deal with a Process that has not been run to completion, please Run the Other Deals before proceeding.");
                    var errIs2 = ResultSet.ZeroSet();
                    errIs2.DoMath(EngineCol.Comment, MathOp.SETTO, "Deal ID:");
                    ResultSetDSPs.SetCalcErrorInRun(errIs1, errIs2).Release();
                    errIs1.Release();
                    errIs2.Release();
                }
                dealsInError.Release();
                dspRunStatus.Release();

                // Pull contributing-deals' Total Participant Share or Adjusted Gross
                // Receipts ITD rows, depending on share choice.  altComment stamp = ContractId
                // stamps each returned row's Alt_user_comment with the target deal's contract id.
                var thirdPartyFilter = new FilterClause {
                    new Criteria(CustCol.TransType, CompareOp.EQ, TransType.ITD)
                };
                if (hasTotalPartShare)
                    thirdPartyFilter.Add(new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Total_Participant_Share));
                else if (hasAdjustedToGR)
                    thirdPartyFilter.Add(new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Modified_Adjusted_Gross_Receipts));

                // TODO(gcr-port): remove parity harness and call ResultSet.GetDataFromDealCalcResults directly once field data confirms SQL/C# parity.
                var thirdPartyResult = ResultSetDSPs.GetDataFromDealCalcResultsWithParity(
                    "MagrLib.thirdPartyResult",
                    dspDealList, thirdPartyFilter,
                    endPeriod:       Job.CurrentCalcPeriod,
                    mode:            ResultSet.CalcResultPeriodMode.LatestUpToEnd,
                    jobStatusIds:    new[] { "Approved", "Complete" },
                    altCommentStamp: ResultSet.ContractStampMode.ContractId);
                dspDealList.Release();

                var valid3rdParty = thirdPartyResult.GetData(
                    CustCol.ActualPeriod, ListOp.INLIST, (EntityList)auditWindow,
                    "MagrSubMAGR.Valid3rdParty");
                thirdPartyResult.Release();
                var zeroRate3rdParty = valid3rdParty.GetData(
                    CustCol.ActivityType, CompareOp.NE, new AlliantEntity(0),
                    "MagrSubMAGR.ZeroRate3rdParty");
                valid3rdParty.Release();
                var _ops26 = new MathList
                {
                    new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                    new MathOperation(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_3rdParty_MAGR_Percent),
                };
                zeroRate3rdParty.DoMath(_ops26);

                var valid3rdPartyExpense = zeroRate3rdParty.GetData(new FilterClause {
                    new Criteria(EngineCol.Rate1, CompareOp.NE, "0"),
                    new Criteria(BaseCol.Amount,  CompareOp.NE, "0")
                }, "MagrSubMAGR.Valid3rdPartyExpense");
                zeroRate3rdParty.Release();
                var _ops27 = new MathList
                {
                    new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                    new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
                };
                valid3rdPartyExpense.DoMath(_ops27);

                var thirdPartyExpenseAmount = valid3rdPartyExpense.Copy();
                var _ops28 = new MathList
                {
                    new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Amount2),
                    new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
                };
                thirdPartyExpenseAmount.DoMath(_ops28);
                var roundedThirdPartyExpenseAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(thirdPartyExpenseAmount);
                Job.CurrentCalcContext = _ctxSubModifiedAdjustedGrossReceipts;

                var thirdPartyExpenseAmountWNonTcs = ResultSet.EmptySet();
                thirdPartyExpenseAmountWNonTcs.CombineAndRelease(roundedThirdPartyExpenseAmount, valid3rdPartyExpense);
                var thirdPartyExpenseSumm = thirdPartyExpenseAmountWNonTcs.Summarize();
                if (!ReferenceEquals(thirdPartyExpenseSumm, thirdPartyExpenseAmountWNonTcs)) thirdPartyExpenseAmountWNonTcs.Release();
                Job.CurrentCalcContext = _ctxSubModifiedAdjustedGrossReceipts;

                // Move UDKey2 → Comment1 via p_ds_udkey_to_text.
                var convertUdk2ToComment1 = ResultSet.ZeroSet();
                convertUdk2ToComment1.SetTextValue(EngineCol.Comment, "UDKey2");
                var _ops29 = new MathList
                {
                    new MathOperation(EngineCol.Rate1,   MathOp.SETTO, "0"),
                    new MathOperation(EngineCol.Rate2,   MathOp.SETTO, "0"),
                };
                convertUdk2ToComment1.DoMath(_ops29);
                var blankComments3rdParty = thirdPartyExpenseSumm;
                blankComments3rdParty.DoMath(EngineCol.Comment, MathOp.SETTO, DS.F_NULL_STRING());
                // Fix: SQL DSP silently no-op'd; C# wrapper mutates IS1 in-place as intended.
                ResultSetDSPs.UDKeyToText(blankComments3rdParty, convertUdk2ToComment1);
                convertUdk2ToComment1.Release();

                blankComments3rdParty.SetValue(CustCol.ActivityType, ActivityType.MAGR_3rd_Party_Deductible_MAGR_);
                thirdPartyMagr = blankComments3rdParty;
                prior3rdPartyWindowItd = inputSet5.GetData(
                    CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_3rd_Party_Deductible_MAGR_,
                    "MagrSubMAGR.Prior3rdPartyWindowITD");
            }
            else
            {
                thirdPartyMagr = ResultSet.EmptySet();
                prior3rdPartyWindowItd = ResultSet.EmptySet();
            }

            // ─── Negative aggregates ────────────────────────────────────────
            var interestItd = inputSet3.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Monthly_Accrued_Interest),
                new Criteria(CustCol.TransType,    CompareOp.EQ, TransType.ITD)
            }, "MagrSubMAGR.InterestITD");
            interestItd.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var copItd = inputSet2.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Cost_of_Production_Activity_Type_List,
                "MagrSubMAGR.COPITD");
            var negAdvAndInterestCopItd = inputSet6.Copy();
            negAdvAndInterestCopItd.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var priorWindowItd = inputSet5.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_To_Calculate_Modified_Adjusted_Gross_Receipts_Activity_Type_List,
                "MagrSubMAGR.PriorWindowITD");

            var magrAdjustableItd = ResultSet.EmptySet();
            magrAdjustableItd.CombineAndRelease(
                priorWindowItd,
                inputSet1.Copy(),
                prior3rdPartyWindowItd.Copy(),
                thirdPartyMagr.Copy(),
                interestItd, copItd, negAdvAndInterestCopItd);
            magrAdjustableItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Modified_Adjusted_Gross_Receipts);

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(magrAdjustableItd, prior3rdPartyWindowItd, thirdPartyMagr);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubModifiedAdjustedGrossReceipts");
            return output;
        }

        // ── 1684 C_MAGR_SUB_GROSS_RECEIPTS ───────────────────────────────────
        //
        // Royalty calculation across three methods (per ContractUDF.MAGR_RoyaltyMethod
        // stamped into AltComment): Percent of Amount, Per Unit, Percent of List Price.
        // Plus "Greater of" / "Lesser of" composite methods that pick the higher/lower
        // of the three per-row.
        //
        // Each method:
        //   - Filter rows whose AltComment matches the method-name string.
        //   - Compute royalty Amount; round.
        //   - Stamp RoyaltyMethod (UDKey16) with the corresponding entity-instance.
        //   - For Greater/Lesser: stash royalty in Price2, pick winner via group_numbering.
        //
        // Final Output: Combine three primary outputs + GreaterOfCombo + LesserOfCombo.
        // ActivityType -> MAGR_Gross_Receipts.
        public static ResultSet SubGrossReceipts(
            ResultSet inputSet1,            // MAGR_Royalty_Basis tiered window ITD
            ResultSet windowStartPeriod)
        {
            _ctxSubMagrGrossReceipts = _ctxSubMagrGrossReceipts ?? NewSubCtxStandard("C_MAGR_SUB_GROSS_RECEIPTS");
            Job.CurrentCalcContext = _ctxSubMagrGrossReceipts;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubGrossReceipts (MAGR)");
            _ = windowStartPeriod;

            // Stamp AltComment from MAGR_RoyaltyMethod, clear Rate1; stash Amount in Amount2.
            var methodType = inputSet1.Copy();
            var _ops30 = new MathList
            {
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, ContractUDF.MAGR_RoyaltyMethod),
                new MathOperation(EngineCol.Rate1,      MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Amount2,      MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,       MathOp.SETTO, "0"),
            };
            methodType.DoMath(_ops30);

            // ─── Helper closure: build one royalty branch ────────────────────
            // method = "Percent of Amount" | "Per Unit" | "Percent of List Price" | "Greater of" | "Lesser of"
            ResultSet BuildBranch(string method, string udfHandle, bool useUnits, bool useListPrice)
            {
                var branch = methodType.GetData(EngineCol.AltComment, CompareOp.EQ, method, "MagrSubGR.Branch_" + method);
                branch.DoMath(EngineCol.Rate1, MathOp.SETTO, udfHandle);
                if (useUnits)
                {
                    // Amount = Units * Rate1
                    var _ops31 = new MathList
                    {
                        new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Units),
                        new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
                    };
                    branch.DoMath(_ops31);
                }
                else if (useListPrice)
                {
                    // Amount = Units * Rate1 * Price1
                    var _ops32 = new MathList
                    {
                        new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Units),
                        new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
                        new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Price1),
                    };
                    branch.DoMath(_ops32);
                }
                else
                {
                    // Amount = Amount2 * Rate1
                    var _ops33 = new MathList
                    {
                        new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                        new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
                    };
                    branch.DoMath(_ops33);
                }
                var rounded = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(branch);
                Job.CurrentCalcContext = _ctxSubMagrGrossReceipts;
                return rounded;
            }

            // Primary branches (one per method).  Each produces rows whose AltComment is
            // still the method-name; SetValue on RoyaltyMethod (UDKey16) carries the
            // selected method into the output as an entity reference.
            // (Method-entity SIDs come from ActivityType-style handles -- for MAGR there
            // are no separate entities yet, so we leave UDKey16 as-is from input.)

            // Percent of Amount + Greater/Lesser candidates.
            var pctOfAmount       = BuildBranch("Percent of Amount",     ContractUDF.MAGR_PercentOfAmount,    useUnits: false, useListPrice: false);
            var pctOfAmountGreater = BuildBranch("Greater of",           ContractUDF.MAGR_PercentOfAmount,    useUnits: false, useListPrice: false);
            var pctOfAmountLesser  = BuildBranch("Lesser of",            ContractUDF.MAGR_PercentOfAmount,    useUnits: false, useListPrice: false);

            var perUnit         = BuildBranch("Per Unit",                ContractUDF.MAGR_PerUnit,            useUnits: true,  useListPrice: false);
            var perUnitGreater  = BuildBranch("Greater of",              ContractUDF.MAGR_PerUnit,            useUnits: true,  useListPrice: false);
            var perUnitLesser   = BuildBranch("Lesser of",               ContractUDF.MAGR_PerUnit,            useUnits: true,  useListPrice: false);

            var pctOfList       = BuildBranch("Percent of List Price",   ContractUDF.MAGR_PercentOfListPrice, useUnits: false, useListPrice: true);
            var pctOfListGreater = BuildBranch("Greater of",             ContractUDF.MAGR_PercentOfListPrice, useUnits: false, useListPrice: true);
            var pctOfListLesser  = BuildBranch("Lesser of",              ContractUDF.MAGR_PercentOfListPrice, useUnits: false, useListPrice: true);
            methodType.Release();

            // Stash royalty in Price2 for Greater/Lesser pickers, then group_numbering picks.
            void StashAmountInPrice2(ResultSet rs)
            {
                var _ops34 = new MathList
                {
                    new MathOperation(EngineCol.Price2, MathOp.SETTO, BaseCol.Amount),
                    new MathOperation(BaseCol.Amount,   MathOp.SETTO, "0"),
                };
                rs.DoMath(_ops34);
            }
            StashAmountInPrice2(pctOfAmountGreater);
            StashAmountInPrice2(perUnitGreater);
            StashAmountInPrice2(pctOfListGreater);
            StashAmountInPrice2(pctOfAmountLesser);
            StashAmountInPrice2(perUnitLesser);
            StashAmountInPrice2(pctOfListLesser);

            // ─── Greater Of (DESC) ───────────────────────────────────────────
            var forGreaterOf = ResultSet.EmptySet();
            forGreaterOf.CombineAndRelease(pctOfAmountGreater, perUnitGreater, pctOfListGreater);
            Comparison<CalcResultRow> sortByPrice2DescThenMethodAsc = (a, b) =>
            {
                int r = b.Alt_price_point.CompareTo(a.Alt_price_point);
                if (r != 0) return r;
                return a.Udkey_16_sid.CompareTo(b.Udkey_16_sid);
            };
            // Within = ZeroSet shape with RoyaltyMethod=0, Rate1=0, Price2=0.
            var forGreaterOfWithin = forGreaterOf.Copy();
            forGreaterOfWithin.SetValue(CustCol.RoyaltyMethod, new AlliantEntity(0));
            var _ops35 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
            };
            forGreaterOfWithin.DoMath(_ops35);
            var forGreaterOfNumbered = ResultSetDSPs.GroupNumbering(forGreaterOf, forGreaterOfWithin, sortByPrice2DescThenMethodAsc);
            forGreaterOfWithin.Release();
            // GreaterOfOutput: rows where Amount=0 (winners), then move Price2 → Amount.
            var greaterOfOutput = forGreaterOfNumbered.GetData(BaseCol.Amount, CompareOp.EQ, "0", "MagrSubGR.GreaterOfOutput");
            forGreaterOfNumbered.Release();
            var _ops36 = new MathList
            {
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, EngineCol.Price2),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, "Greater Of"),
            };
            greaterOfOutput.DoMath(_ops36);

            // ─── Lesser Of (ASC) ─────────────────────────────────────────────
            var forLesserOf = ResultSet.EmptySet();
            forLesserOf.CombineAndRelease(pctOfAmountLesser, perUnitLesser, pctOfListLesser);
            Comparison<CalcResultRow> sortByPrice2AscThenMethodAsc = (a, b) =>
            {
                int r = a.Alt_price_point.CompareTo(b.Alt_price_point);
                if (r != 0) return r;
                return a.Udkey_16_sid.CompareTo(b.Udkey_16_sid);
            };
            var forLesserOfWithin = forLesserOf.Copy();
            forLesserOfWithin.SetValue(CustCol.RoyaltyMethod, new AlliantEntity(0));
            var _ops37 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
            };
            forLesserOfWithin.DoMath(_ops37);
            var forLesserOfNumbered = ResultSetDSPs.GroupNumbering(forLesserOf, forLesserOfWithin, sortByPrice2AscThenMethodAsc);
            forLesserOfWithin.Release();
            var lesserOfOutput = forLesserOfNumbered.GetData(BaseCol.Amount, CompareOp.EQ, "0", "MagrSubGR.LesserOfOutput");
            forLesserOfNumbered.Release();
            var _ops38 = new MathList
            {
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, EngineCol.Price2),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, "Lesser Of"),
            };
            lesserOfOutput.DoMath(_ops38);

            // ─── Final assembly ──────────────────────────────────────────────
            var receiptsItd = ResultSet.EmptySet();
            receiptsItd.CombineAndRelease(pctOfAmount, perUnit, pctOfList, greaterOfOutput, lesserOfOutput);
            receiptsItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Gross_Receipts);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubGrossReceipts (MAGR)");
            return receiptsItd;
        }

        // ── 1668 C_MAGR_GET_IMPORT_ADJS_PRIOR_ITD ────────────────────────────
        //
        // MAGR-pipeline counterpart to RoyaltyLib.SubGetImportAdjsPriorITD.
        //   Phase A. Window-start period via MAGR_StatementInterval +
        //              MAGR_ExpenseCapInterval guard (NA/None/Inception-to-Date branches).
        //   Phase B. ImportedITD = filter Job.Import for MAGR_Imported list within window.
        //   Phase C. PreviousITD = CalcResult Prior_ITD list at calc-period-previous.
        //   Phase D. PreWindowingITD = filter PreviousITD to [Inception..WindowStart-1].
        //   Phase E. Adjustments = MAGR_Adjustment list ITD; stamp TransType=ITD.
        //   Phase F. Currency conversion (UDKey17 -> Calc currency via SourceCurrencyUDF.ExchangeRate).
        //   Phase G. Allowable Gross Receipts: split DistExp+COP rows from rest, multiply
        //              non-DistExp+COP by ContractUDF.MAGR_AllowableGrossReceiptsPercent.
        //   Phase H. Channel mapping (back-fill Channel via ContractUDF.MAGR_ChannelMapping).
        //   Returns 5 outputs.
        public static (ResultSet ImportedTrx,
                       ResultSet PreviousITD,
                       ResultSet PreWindowingITD,
                       ResultSet Adjustments,
                       ResultSet WindowStartPeriodOutput) SubGetImportAdjsPriorItd()
        {
            _ctxSubGetImportAdjsPriorItd = _ctxSubGetImportAdjsPriorItd ?? NewSubCtxStandard("C_MAGR_GET_IMPORT_ADJS_PRIOR_ITD");
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorItd;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubGetImportAdjsPriorItd (MAGR)");

            int N_WindowPeriodsRaw = Contract.GetUDFInt(ContractUDF.MAGR_WindowPeriods);
            int N_WindowPeriods    = N_WindowPeriodsRaw * -1;
            string statementInterval = Contract.GetUDFString(ContractUDF.MAGR_StatementInterval);
            string expCapInterval    = Contract.GetUDFString(ContractUDF.MAGR_ExpenseCapInterval);

            int N_NoneLabel = (expCapInterval == "<None>" || expCapInterval == "<NONE>"
                            || expCapInterval == "NA"     || expCapInterval == "No Deduction Cap") ? 1 : 0;
            int N_InceptionLabel = (expCapInterval == "Inception to Date") ? 1 : 0;

            PeriodItem winPeriod = !string.IsNullOrEmpty(statementInterval)
                ? DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, N_WindowPeriods, statementInterval)
                : Job.CurrentCalcPeriod;

            PeriodItem mayBeCap;
            if (N_NoneLabel == 1 || string.IsNullOrEmpty(expCapInterval))
                mayBeCap = winPeriod;
            else
            {
                var capBase = DS.F_PERIOD_TYPES_FROM(winPeriod, -1, expCapInterval);
                mayBeCap = DS.F_PERIODS_FROM(capBase, 1);
            }
            PeriodItem winStart;
            if (string.IsNullOrEmpty(statementInterval))
                winStart = mayBeCap;
            else
            {
                var winBase = DS.F_PERIOD_TYPES_FROM(mayBeCap, -1, statementInterval);
                winStart = DS.F_PERIODS_FROM(winBase, 1);
            }
            PeriodItem windowStart = (N_WindowPeriodsRaw == 0 || N_InceptionLabel == 1)
                ? DS.F_INCEPTION()
                : winStart;

            var windowStartPeriodOutput = ResultSet.ZeroSet();
            windowStartPeriodOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Window_Start_Period);
            windowStartPeriodOutput.SetValue(CustCol.TransType,    TransType.ITD);
            windowStartPeriodOutput.SetValue(CustCol.ActualPeriod, windowStart);

            var auditWindow = DS.F_PERIOD_INTERVAL(windowStart, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));

            // Phase B: ImportedITD.
            var importedItd = Job.Import.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Imported_Activity_Type_List),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)auditWindow),
                new Criteria(EngineCol.Period,     ListOp.INLIST, Job.ITDthruCalcPeriod),
            }, "MagrSubGet.ImportedITD");
            importedItd.SetValue(CustCol.TransType, TransType.ITD);

            // Phase C+D: PreviousITD and PreWindowingITD.
            var previousItd = ResultSet.GetDataFromCalcResult(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Prior_ITD_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.ITD),
                new Criteria(EngineCol.Period,     CompareOp.EQ,  Job.CalcPeriodPrevious),
            });
            PeriodItem windowStartMinus1 = DS.F_PERIODS_FROM(windowStart, -1);
            var preWindowRange = DS.F_PERIOD_INTERVAL(inception, windowStartMinus1);
            var preWindowingItd = previousItd.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)preWindowRange,
                "MagrSubGet.PreWindowingITD");

            // Phase E: Adjustments.
            var adjustments = ResultSet.GetDataFromAdjustment(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Adjustment_Activity_Type_List),
                new Criteria(EngineCol.Period,     ListOp.INLIST, Job.ITDthruCalcPeriod),
            });
            adjustments.SetValue(CustCol.TransType, TransType.ITD);

            // Phase F: Currency conversion via SourceCurrency UDF.
            var withExchangeRate = importedItd;
            var _ops39 = new MathList
            {
                new MathOperation(CustCol.SourceCurrency, MathOp.SETTO, ContractUDF.MAGR_DealCurrency),
                new MathOperation(EngineCol.Rate2,        MathOp.SETTO, SourceCurrencyUDF.ExchangeRate),
            };
            withExchangeRate.DoMath(_ops39);

            var invalidExchangeRate = withExchangeRate.GetData(EngineCol.Rate2, CompareOp.EQ, "0", "MagrSubGet.InvalidExchangeRate");
            var L_SourceCurrency = invalidExchangeRate.GetList(CustCol.SourceCurrency);
            int N_ConversionError = invalidExchangeRate.Rows;
            invalidExchangeRate.Release();

            if (N_ConversionError > 0)
            {
                var sourceCurrencyError = ResultSet.ZeroSet();
                sourceCurrencyError.SetEntity(CustCol.SourceCurrency, L_SourceCurrency[0]);
                var idToComment = ResultSet.ZeroSet();
                idToComment.DoMath(EngineCol.AltComment, MathOp.SETTO, "-");
                idToComment.SetTextValue(EngineCol.Comment,    "UDKey17");
                var _ops40 = new MathList
                {
                    new MathOperation(EngineCol.Rate1,      MathOp.SETTO, "0"),
                    new MathOperation(EngineCol.Rate2,      MathOp.SETTO, "1"),
                };
                idToComment.DoMath(_ops40);
                // Fix: SQL DSP silently no-op'd; C# wrapper mutates IS1 in-place as intended.
                ResultSetDSPs.UDKeyToText(sourceCurrencyError, idToComment);
                idToComment.Release();
                sourceCurrencyError.DoMath(EngineCol.AltComment, MathOp.SETTO, "Exchange Rate not found for Source Currency.");
                ResultSetDSPs.SetCalcErrorInRun(sourceCurrencyError).Release();
                sourceCurrencyError.Release();
            }

            var validExchangeRate = withExchangeRate.GetData(EngineCol.Rate2, CompareOp.NE, "0", "MagrSubGet.ValidExchangeRate");
            withExchangeRate.Release();
            // Stash original Amount in Amount2, original Price1 in Price2, then convert.
            var _ops41 = new MathList
            {
                new MathOperation(EngineCol.Price2, MathOp.SETTO, EngineCol.Price1),
                new MathOperation(BaseCol.Units2,   MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Amount2,  MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,   MathOp.SETTO, "0"),
            };
            validExchangeRate.DoMath(_ops41);

            var convertedAmount = validExchangeRate.Copy();
            var _ops42 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate2),
            };
            convertedAmount.DoMath(_ops42);
            var convertedPrice = validExchangeRate.Copy();
            var _ops43 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, EngineCol.Price2),
                new MathOperation(BaseCol.Amount2, MathOp.TIMES, EngineCol.Rate2),
            };
            convertedPrice.DoMath(_ops43);

            var convertedAmountAndPrice = ResultSet.EmptySet();
            convertedAmountAndPrice.CombineAndRelease(convertedAmount, convertedPrice);
            var convertedSumm = convertedAmountAndPrice.Summarize();
            if (!ReferenceEquals(convertedSumm, convertedAmountAndPrice)) convertedAmountAndPrice.Release();
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorItd;
            var roundedConversion = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(convertedSumm);
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorItd;
            var _ops44 = new MathList
            {
                new MathOperation(EngineCol.Price1, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount2,  MathOp.SETTO, "0"),
            };
            roundedConversion.DoMath(_ops44);

            var convertedTrx = ResultSet.EmptySet();
            convertedTrx.CombineAndRelease(validExchangeRate, roundedConversion);
            convertedTrx.DoMath(EngineCol.Price2, MathOp.SETTO, "0");

            // Phase G: Distribution Expense + COP filter passthrough.
            var distExpensesAndCop = convertedTrx.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Imported_Distribution_Expenses_and_COP_Activity_Type_List,
                "MagrSubGet.DistExpensesAndCOP");

            // Allowable Gross Receipts: rest of converted trx times MAGR_AllowableGrossReceiptsPercent.
            var readyForGrossReceipts = convertedTrx.GetData(
                CustCol.ActivityType, ListOp.NOTINLIST, ActivityType.Lists.MAGR_Imported_Distribution_Expenses_and_COP_Activity_Type_List,
                "MagrSubGet.ReadyForGrossReceipts");
            convertedTrx.Release();
            readyForGrossReceipts.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_AllowableGrossReceiptsPercent);
            var nonZeroRate = readyForGrossReceipts.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubGet.NonZeroRate");
            readyForGrossReceipts.Release();
            var saveNonZeroRate = nonZeroRate.Copy();
            saveNonZeroRate.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            var grossReceiptsAmount = nonZeroRate.Copy();
            grossReceiptsAmount.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1);
            nonZeroRate.Release();
            var roundedGrossReceiptsAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(grossReceiptsAmount);
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorItd;

            // Phase H: Channel mapping.
            var grossReceiptsAndDistExpensesDivision = ResultSet.EmptySet();
            grossReceiptsAndDistExpensesDivision.CombineAndRelease(saveNonZeroRate, roundedGrossReceiptsAmount, distExpensesAndCop);
            grossReceiptsAndDistExpensesDivision.DoMath(CustCol.Division, MathOp.SETTO, ContractUDF.MAGR_Division);

            var blankChannel = grossReceiptsAndDistExpensesDivision.GetData(
                CustCol.Channel, CompareOp.EQ, Channel.Unspecified, "MagrSubGet.BlankChannel");
            blankChannel.DoMath(CustCol.Channel, MathOp.SETTO, ContractUDF.MAGR_ChannelMapping);
            var specifiedChannel = grossReceiptsAndDistExpensesDivision.GetData(
                CustCol.Channel, CompareOp.NE, Channel.Unspecified, "MagrSubGet.SpecifiedChannel");
            grossReceiptsAndDistExpensesDivision.Release();

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(blankChannel, specifiedChannel);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubGetImportAdjsPriorItd (MAGR)");
            return (output, previousItd, preWindowingItd, adjustments, windowStartPeriodOutput);
        }

        // ── 1695 C_MAGR_SUB_COST_OF_PRODUCTION ───────────────────────────────
        //
        // Three components: AdvanceCOPITD (=−AdvAdj_ITD), InterestCOPITD (=−PriorInterest),
        // 3rdPartyCOP (cross-deal calc results × ContractUDF.MAGR_3rdPartyCOPPercent).
        //
        // Returns INPUTSET6 (Allowable COP + AdminOverhead + 3rdPartyCOP, NEG) +
        //         INPUTSET7 (AdvanceCOP + InterestCOP, ITD).
        public static (ResultSet OutputITD, ResultSet AdvAndInterestCopItd) SubCostOfProduction(
            ResultSet inputSet1,    // imported trx (window ITD)
            ResultSet inputSet2,    // window-start marker
            ResultSet inputSet3,    // priorUpToWindow ITD
            ResultSet inputSet4,    // prior ITD full
            ResultSet inputSet5)    // adjustments ITD
        {
            _ctxSubCostOfProduction = _ctxSubCostOfProduction ?? NewSubCtxStandard("C_MAGR_SUB_COST_OF_PRODUCTION");
            Job.CurrentCalcContext = _ctxSubCostOfProduction;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubCostOfProduction (MAGR)");
            _ = inputSet2; _ = inputSet3;

            // AdvanceCOPITD = −AdvAdjITD; InterestCOPITD = −prior MAGR_Monthly_Accrued_Interest.
            var advAdjItd = inputSet5.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Advance_Adjustment_Activity_Type_List,
                "MagrSubCOP.AdvAdjITD");
            var advanceCopItd = advAdjItd.Copy();
            advanceCopItd.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            advanceCopItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Advance_COP);
            advanceCopItd.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
            advAdjItd.Release();

            var priorInterestItd = inputSet4.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Monthly_Accrued_Interest,
                "MagrSubCOP.PriorInterestITD");
            var interestCopItd = priorInterestItd;
            interestCopItd.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            interestCopItd.SetValue(CustCol.ActivityType, ActivityType.MAGR_Interest_COP);

            // 3rd party COP cross-deal retrieval (skipped if MAGR_3rdPartyCOPShareChoice = NONE).
            string copShareChoice = Contract.GetUDFString(ContractUDF.MAGR_3rdPartyCOPShareChoice);
            ResultSet thirdPartyCop = ResultSet.EmptySet();
            if (!(copShareChoice == "<None>" || copShareChoice == "<NONE>" || string.IsNullOrEmpty(copShareChoice)))
            {
                // (Cross-deal retrieval is identical to the SubModifiedAdjustedGrossReceipts pattern;
                //  consolidated to a private helper below.)
                thirdPartyCop.Release();
                thirdPartyCop = MagrThirdPartyCrossDealRetrieval(
                    "MAGR_3rdPartyDeductibleCOPDealIDs",
                    copShareChoice,
                    ContractUDF.MAGR_3rdPartyCOPPercent,
                    ActivityType.MAGR_3rd_Party_Deductible_Cost_of_Production);
                Job.CurrentCalcContext = _ctxSubCostOfProduction;
            }

            // Allowable COP: import-COP rows times ContractUDF.MAGR_AllowableCostOfProduction.
            var impCop = inputSet1.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Cost_of_Production_Activity_Type_List,
                "MagrSubCOP.ImpCOP");
            var combinedCop = ResultSet.EmptySet();
            combinedCop.CombineAndRelease(thirdPartyCop.Copy(), impCop);
            var _ops45 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_AllowableCostOfProduction),
            };
            combinedCop.DoMath(_ops45);
            var validCopRate = combinedCop.GetData(new FilterClause {
                new Criteria(EngineCol.Rate1, CompareOp.NE, "0"),
                new Criteria(BaseCol.Amount,  CompareOp.NE, "0")
            }, "MagrSubCOP.ValidCOPRate");
            combinedCop.Release();
            var _ops46 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            validCopRate.DoMath(_ops46);
            var copAmount = validCopRate.Copy();
            var _ops47 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Amount2),
            };
            copAmount.DoMath(_ops47);
            var roundedCopAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(copAmount);
            Job.CurrentCalcContext = _ctxSubCostOfProduction;
            var allowableCop = ResultSet.EmptySet();
            allowableCop.CombineAndRelease(validCopRate, roundedCopAmount);
            allowableCop.SetValue(CustCol.ActivityType, ActivityType.MAGR_Cost_of_Production);
            var _ops48 = new MathList
            {
                new MathOperation(EngineCol.Comment,    MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING()),
            };
            allowableCop.DoMath(_ops48);

            // Administrative Overhead = (AllowableCOP + AdvanceCOP) * MAGR_AdministrativeOverhead.
            var forAdminOverhead = ResultSet.EmptySet();
            forAdminOverhead.CombineAndRelease(allowableCop.Copy(), advanceCopItd.Copy());
            var _ops49 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_AdministrativeOverhead),
            };
            forAdminOverhead.DoMath(_ops49);
            var validAdminOverhead = forAdminOverhead.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubCOP.ValidAdminOverhead");
            forAdminOverhead.Release();
            var _ops50 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            validAdminOverhead.DoMath(_ops50);
            var adminOverheadAmount = validAdminOverhead.Copy();
            var _ops51 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Amount2),
            };
            adminOverheadAmount.DoMath(_ops51);
            var roundedAdminOverheadAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(adminOverheadAmount);
            Job.CurrentCalcContext = _ctxSubCostOfProduction;
            var administrativeOverhead = ResultSet.EmptySet();
            administrativeOverhead.CombineAndRelease(roundedAdminOverheadAmount, validAdminOverhead);
            administrativeOverhead.SetValue(CustCol.ActivityType, ActivityType.MAGR_Administrative_Overhead);

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(allowableCop, administrativeOverhead, thirdPartyCop);

            var advAndInterestCopItd = ResultSet.EmptySet();
            advAndInterestCopItd.CombineAndRelease(advanceCopItd, interestCopItd);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubCostOfProduction (MAGR)");
            return (output, advAndInterestCopItd);
        }

        // ── 1691 C_MAGR_SUB_DISTRIBUTION_EXPENSES ────────────────────────────
        //
        // Distribution expenses pipeline.  Computes Allowable Dist Expense
        // (import * MAGR_AllowableExpenses), 3rd-party expense (cross-deal),
        // applies expense-cap allocation via p_ds_get_lookup_column_values +
        // p_ds_ordered_allocation, and emits OverCap rows.
        public static (ResultSet OverCapOutput, ResultSet DeductionsOutput) SubDistributionExpenses(
            ResultSet inputSet1,  // imported trx
            ResultSet inputSet2,  // MAGR_Total_Gross_Receipts window ITD
            ResultSet inputSet3,  // window-start marker
            ResultSet inputSet4)  // priorUpToWindow ITD
        {
            _ctxSubDistributionExpenses = _ctxSubDistributionExpenses ?? NewSubCtxStandard("C_MAGR_SUB_DISTRIBUTION_EXPENSES");
            Job.CurrentCalcContext = _ctxSubDistributionExpenses;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubDistributionExpenses (MAGR)");
            _ = inputSet3; _ = inputSet4;

            // ─── Allowable Dist Expense ──────────────────────────────────────
            var impDeductions = inputSet1.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Distribution_Expenses_Activity_Type_List,
                "MagrSubDE.ImpDeductions");
            var _ops52 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_AllowableExpenses),
            };
            impDeductions.DoMath(_ops52);
            var validExpRate = impDeductions.GetData(new FilterClause {
                new Criteria(EngineCol.Rate1, CompareOp.NE, "0"),
                new Criteria(BaseCol.Amount,  CompareOp.NE, "0")
            }, "MagrSubDE.ValidExpRate");
            impDeductions.Release();
            var _ops53 = new MathList
            {
                new MathOperation(BaseCol.Units2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount, MathOp.SETTO, "0"),
            };
            validExpRate.DoMath(_ops53);
            var distExpAmount = validExpRate.Copy();
            var _ops54 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Units2),
            };
            distExpAmount.DoMath(_ops54);
            var roundedDistExpAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(distExpAmount);
            Job.CurrentCalcContext = _ctxSubDistributionExpenses;
            var allowableDistExp = ResultSet.EmptySet();
            allowableDistExp.CombineAndRelease(roundedDistExpAmount, validExpRate);
            var _ops55 = new MathList
            {
                new MathOperation(EngineCol.Comment,    MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING()),
            };
            allowableDistExp.DoMath(_ops55);
            allowableDistExp.SetValue(CustCol.ActivityType, ActivityType.MAGR_Distribution_Expense);

            // ─── 3rd Party Distribution Expense ──────────────────────────────
            string expShareChoice = Contract.GetUDFString(ContractUDF.MAGR_3rdPartyExpenseShareChoice);
            ResultSet thirdPartyDistExp = ResultSet.EmptySet();
            if (!(expShareChoice == "<None>" || expShareChoice == "<NONE>" || string.IsNullOrEmpty(expShareChoice)))
            {
                thirdPartyDistExp.Release();
                thirdPartyDistExp = MagrThirdPartyCrossDealRetrieval(
                    "MAGR_3rdPartyDeductibleExpenseDealIDs",
                    expShareChoice,
                    ContractUDF.MAGR_3rdPartyExpensePercent,
                    ActivityType.MAGR_Distribution_Expense);
                Job.CurrentCalcContext = _ctxSubDistributionExpenses;
            }

            // ─── Expense Cap pipeline ────────────────────────────────────────
            string expenseCapInterval = Contract.GetUDFString(ContractUDF.MAGR_ExpenseCapInterval);
            int N_NoneLabel = (expenseCapInterval == "<None>" || expenseCapInterval == "<NONE>"
                            || expenseCapInterval == "NA"     || expenseCapInterval == "No Deduction Cap") ? 1 : 0;

            // PopulateSalesOtherPeriod: shift each TGR row's OtherPeriod to its expense-cap-interval-end.
            var dedPeriodTypeArgs = ResultSet.ZeroSet();
            var _ops56 = new MathList
            {
                new MathOperation(EngineCol.Comment, MathOp.SETTO, "MAGR_ExpenseCapInterval"),
                new MathOperation(EngineCol.Rate1,   MathOp.SETTO, "1"),
            };
            dedPeriodTypeArgs.DoMath(_ops56);
            // TODO(uptl-port): remove parity harness and call UdfPeriodTypeLabel directly once field data confirms SQL/C# parity.
            var dedPeriodTypeString = ResultSetDSPs.UdfPeriodTypeLabelWithParity(
                "MagrLib.dedPeriodTypeString", dedPeriodTypeArgs);
            dedPeriodTypeArgs.Release();
            // TODO(pttp-port): remove parity harness and call PeriodToPeriodTypePeriod directly once field data confirms SQL/C# parity.
            var populateSalesOtherPeriod = ResultSetDSPs.PeriodToPeriodTypePeriodWithParity(
                "MagrLib.populateSalesOtherPeriod",
                inputSet2.Copy(), dedPeriodTypeString);

            var positiveDeductions = ResultSet.EmptySet();
            positiveDeductions.CombineAndRelease(allowableDistExp.Copy(), thirdPartyDistExp.Copy());
            positiveDeductions.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var deductionWithCaps = positiveDeductions.Copy();
            deductionWithCaps.DoMath(CustCol.Tier, MathOp.SETTO, ContractUDF.MAGR_ExpenseCapGroup);
            var notNullCaps = deductionWithCaps.GetData(CustCol.Tier, CompareOp.NE, "0", "MagrSubDE.NOTNullCaps");
            deductionWithCaps.Release();

            ResultSet deductionsNoCap, deductionsNeedsCap;
            if (N_NoneLabel != 0)
            {
                deductionsNoCap = positiveDeductions.Copy();
                deductionsNoCap.DoMath(BaseCol.Units2, MathOp.SETTO, BaseCol.Amount);
                deductionsNeedsCap = ResultSet.EmptySet();
                notNullCaps.Release();
            }
            else
            {
                deductionsNoCap = ResultSet.EmptySet();
                deductionsNeedsCap = notNullCaps;
            }
            positiveDeductions.Release();

            // Variable cap percent — project UDKey14 selections from MAGR_VariableExpenseCap
            // Contract-owned lookup UDF.
            // TODO(glcv-port): remove parity harness and call GetLookupColumnValues directly once field data confirms SQL/C# parity.
            var tierPctDetails = ResultSetDSPs.GetLookupColumnValuesWithParity(
                "MagrLib.tierPctDetails",
                IndexableColumn.Contract_sid, "MAGR_VariableExpenseCap", IndexableColumn.Udkey_14_sid);

            // TODO(dm-port): remove parity harness and call DetailMerge directly once field data confirms SQL/C# parity.
            var salesWithPctCapGroup = ResultSetDSPs.DetailMergeWithParity("MagrLib.salesWithPctCapGroup", populateSalesOtherPeriod.Copy(), tierPctDetails);
            tierPctDetails.Release();
            var dedCapPctAmount = salesWithPctCapGroup.Copy();
            dedCapPctAmount.DoMath(BaseCol.Amount, MathOp.TIMES, ContractUDF.MAGR_VariableExpenseCap);
            salesWithPctCapGroup.Release();

            // Fixed cap amount — project UDKey14 selections from MAGR_FixedExpenseCap
            // Contract-owned lookup UDF.
            // TODO(glcv-port): remove parity harness and call GetLookupColumnValues directly once field data confirms SQL/C# parity.
            var tierAmtDetails = ResultSetDSPs.GetLookupColumnValuesWithParity(
                "MagrLib.tierAmtDetails",
                IndexableColumn.Contract_sid, "MAGR_FixedExpenseCap", IndexableColumn.Udkey_14_sid);

            // TODO(dm-port): remove parity harness and call DetailMerge directly once field data confirms SQL/C# parity.
            var salesWithAmtCapGroup = ResultSetDSPs.DetailMergeWithParity("MagrLib.salesWithAmtCapGroup", populateSalesOtherPeriod, tierAmtDetails);
            tierAmtDetails.Release();
            salesWithAmtCapGroup.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

            var summarizeSalesWithAmtCapGroup = CommonLib.SummarizeToOtherPeriodTier(salesWithAmtCapGroup);
            Job.CurrentCalcContext = _ctxSubDistributionExpenses;
            summarizeSalesWithAmtCapGroup.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
            var dedCapAmount = summarizeSalesWithAmtCapGroup;
            dedCapAmount.DoMath(BaseCol.Amount, MathOp.PLUS, ContractUDF.MAGR_FixedExpenseCap);

            var capAmount = ResultSet.EmptySet();
            capAmount.CombineAndRelease(dedCapPctAmount, dedCapAmount);
            var nonZeroCapAmount = capAmount.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubDE.NonZeroCapAmount");
            capAmount.Release();
            var roundedCapAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(nonZeroCapAmount);
            Job.CurrentCalcContext = _ctxSubDistributionExpenses;
            var summarizeSalesCap = CommonLib.SummarizeToOtherPeriodTier(roundedCapAmount);
            Job.CurrentCalcContext = _ctxSubDistributionExpenses;

            // Allocate via ordered_allocation.
            var processBy = ResultSet.ZeroSet();
            processBy.DoMath(EngineCol.Comment, MathOp.SETTO, "OtherPeriod ASC, Tier ASC, ActualPeriod ASC");
            var summarizeDeduction = CommonLib.SummarizeToOtherPeriodTier(deductionsNeedsCap.Copy());
            Job.CurrentCalcContext = _ctxSubDistributionExpenses;
            // Use min(cap, deduction) -- DealScript's Choose(<=) shape; ordered_allocation handles it.
            // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
            var deductionsOA = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.deductionsOA",
                deductionsNeedsCap.Copy(), summarizeSalesCap, processBy);
            summarizeDeduction.Release();
            summarizeSalesCap.Release();

            var moveAmountToAmount2Deds = deductionsNeedsCap.Copy();
            var _ops57 = new MathList
            {
                new MathOperation(BaseCol.Units2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount, MathOp.SETTO, "0"),
            };
            moveAmountToAmount2Deds.DoMath(_ops57);
            var posDedOA = ResultSet.EmptySet();
            posDedOA.CombineAndRelease(deductionsOA.Copy(), deductionsNoCap, moveAmountToAmount2Deds);
            posDedOA.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            posDedOA.SetValue(CustCol.Tier,        new AlliantEntity(0));
            posDedOA.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            var deductionsOutput = posDedOA;

            // Calculate overcap = (NEG-needsCap) + DeductionsOA, with OtherPeriod restoration.
            var negDeductionsNeedsCap = deductionsNeedsCap.Copy();
            negDeductionsNeedsCap.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            deductionsNeedsCap.Release();

            var overCap = ResultSet.EmptySet();
            overCap.CombineAndRelease(negDeductionsNeedsCap, deductionsOA);
            var overCapWithOtherPeriod = overCap.GetData(CustCol.OtherPeriod, CompareOp.NE, new AlliantEntity(0), "MagrSubDE.OverCapWithOtherPeriod");
            var overCapWithNoOtherPeriod = overCap.GetData(CustCol.OtherPeriod, CompareOp.EQ, new AlliantEntity(0), "MagrSubDE.OverCapWithNoOtherPeriod");
            overCap.Release();
            overCapWithNoOtherPeriod.DoMath(CustCol.OtherPeriod, MathOp.SETTO, CustCol.ActualPeriod);
            var forSummarizeOverCap = ResultSet.EmptySet();
            forSummarizeOverCap.CombineAndRelease(overCapWithOtherPeriod, overCapWithNoOtherPeriod);
            var summarizeOverCap = CommonLib.SummarizeToOtherPeriodTier(forSummarizeOverCap);
            Job.CurrentCalcContext = _ctxSubDistributionExpenses;
            var nonZeroOverCap = summarizeOverCap.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubDE.NonZeroOverCap");
            summarizeOverCap.Release();
            var overCapWithAccount = nonZeroOverCap;
            overCapWithAccount.SetValue(CustCol.ActivityType, ActivityType.MAGR_Amount_over_Cap);
            overCapWithAccount.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
            overCapWithAccount.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            overCapWithAccount.SetValue(CustCol.TransType, TransType.ITD);
            overCapWithAccount.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            var overCapOutput = overCapWithAccount;

            processBy.Release();
            thirdPartyDistExp.Release();
            allowableDistExp.Release();

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubDistributionExpenses (MAGR)");
            return (overCapOutput, deductionsOutput);
        }

        // ── 1702 C_MAGR_SUB_PARTICIPANT_SHARE ────────────────────────────────
        //
        // Computes participant-share + 3rd-party-deductible-(participant-share) ITD.
        // Three deductible-cap modes (ContractUDF.MAGR_3rdPartyDeductionCapType):
        //   100079:466 (None)         — no cap, take full 3rd-party deduction
        //   100079:467 (Hard floor)   — cap at (PartShare - HardFloor)
        //   100079:468 (Hard+Soft)    — softer cap, share excess 50/50
        public static ResultSet SubParticipantShare(
            ResultSet inputSet1,  // MAGR_Modified_Adjusted_Gross_Receipts ITD
            ResultSet inputSet2,  // window-start
            ResultSet inputSet3,  // priorUpToWindow ITD
            ResultSet inputSet4)  // prior ITD full
        {
            _ctxSubParticipantShare = _ctxSubParticipantShare ?? NewSubCtxStandard("C_MAGR_SUB_PARTICIPANT_SHARE");
            Job.CurrentCalcContext = _ctxSubParticipantShare;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubParticipantShare (MAGR)");
            _ = inputSet2;

            var priorParticipantShareItd = inputSet4.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Participant_Share,
                "MagrSubPS.PriorParticipantShareITD");
            var prior3rdPartyShareItd = inputSet4.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_3rd_Party_Deductible_Participant_Share_,
                "MagrSubPS.Prior3rdPartyShareITD");

            // Tier participation by SubParticipationsTiering (1704).
            var participantShareOutput = SubParticipationsTiering(inputSet1.Copy(), inputSet2.Copy(), inputSet3.Copy());
            Job.CurrentCalcContext = _ctxSubParticipantShare;

            var magrCurrent = ResultSet.Subtract(participantShareOutput, priorParticipantShareItd, "MagrSubPS.MAGRCurrent");
            magrCurrent.SetValue(CustCol.RecoupmentGroup, new AlliantEntity(0));
            var nonZeroMagrCurrent = magrCurrent.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubPS.NonZeroMAGRCurrent");
            magrCurrent.Release();
            nonZeroMagrCurrent.DoMath(CustCol.RecoupmentGroup, MathOp.SETTO, ContractUDF.MAGR_RecoupmentGroup);
            var participantShareItd = ResultSet.EmptySet();
            participantShareItd.CombineAndRelease(priorParticipantShareItd.Copy(), nonZeroMagrCurrent);

            // 3rd party deductible (participant share variant).
            string shareChoice = Contract.GetUDFString(ContractUDF.MAGR_3rdParty_ParticipantShare_ShareChoice);
            ResultSet thirdPartyMagr = ResultSet.EmptySet();
            if (!(shareChoice == "<None>" || shareChoice == "<NONE>" || string.IsNullOrEmpty(shareChoice)))
            {
                thirdPartyMagr.Release();
                thirdPartyMagr = MagrThirdPartyCrossDealRetrieval(
                    "MAGR_3rdPartyDeductible_PartcipiantShare_DealIDs",
                    shareChoice,
                    ContractUDF.MAGR_3rdParty_ParticipantShare_Percent,
                    ActivityType.MAGR_3rd_Party_Deductible_Participant_Share_);
                Job.CurrentCalcContext = _ctxSubParticipantShare;
            }
            var prior3rdPartyWindowItd = inputSet3.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_3rd_Party_Deductible_Participant_Share_,
                "MagrSubPS.Prior3rdPartyWindowITD");
            var thirdPartyDeductionsItdNoRg = ResultSet.EmptySet();
            thirdPartyDeductionsItdNoRg.CombineAndRelease(thirdPartyMagr, prior3rdPartyWindowItd);
            var thirdPartyDeductionsItdNoRgSumm = thirdPartyDeductionsItdNoRg.Summarize();
            if (!ReferenceEquals(thirdPartyDeductionsItdNoRgSumm, thirdPartyDeductionsItdNoRg)) thirdPartyDeductionsItdNoRg.Release();
            Job.CurrentCalcContext = _ctxSubParticipantShare;
            var thirdPartyCurrentNoMg = ResultSet.Subtract(thirdPartyDeductionsItdNoRgSumm, prior3rdPartyShareItd, "MagrSubPS.3rdPartyCurrentNoMG");
            thirdPartyCurrentNoMg.SetValue(CustCol.RecoupmentGroup, new AlliantEntity(0));
            var nonZero3rdPartyCurrent = thirdPartyCurrentNoMg.GetData(BaseCol.Amount, CompareOp.NE, "0", "MagrSubPS.NonZero3rdPartyCurrent");
            thirdPartyCurrentNoMg.Release();
            nonZero3rdPartyCurrent.DoMath(CustCol.RecoupmentGroup, MathOp.SETTO, ContractUDF.MAGR_RecoupmentGroup);
            var thirdPartyDeductionsItd = ResultSet.EmptySet();
            thirdPartyDeductionsItd.CombineAndRelease(prior3rdPartyShareItd.Copy(), nonZero3rdPartyCurrent);
            var thirdPartyDeductionsItdSumm = thirdPartyDeductionsItd.Summarize();
            if (!ReferenceEquals(thirdPartyDeductionsItdSumm, thirdPartyDeductionsItd)) thirdPartyDeductionsItd.Release();
            Job.CurrentCalcContext = _ctxSubParticipantShare;
            thirdPartyDeductionsItdNoRgSumm.Release();

            // Cap-mode dispatch.
            string capType = Contract.GetUDFString(ContractUDF.MAGR_3rdPartyDeductionCapType);
            ResultSet capOutput;
            if (capType == "Hard floor" || capType == "Hard Floor")
            {
                // hard-floor allocation.
                var pos3rdParty = thirdPartyDeductionsItdSumm.Copy();
                pos3rdParty.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                decimal N_3rdPartyDed = DS.AmountOf(pos3rdParty);
                decimal N_MagrItd = DS.AmountOf(inputSet1);
                decimal N_PartShare = DS.AmountOf(participantShareItd);
                decimal hardFloorPct = Contract.GetUDFDecimal(ContractUDF.MAGR_HardFloorPercent);
                decimal N_HardFloor = System.Math.Round(N_MagrItd * hardFloorPct, 2);
                decimal N_Allowable = N_PartShare - N_HardFloor;
                decimal N_HardFloorDeductions = N_Allowable <= N_3rdPartyDed ? N_Allowable : N_3rdPartyDed;
                var maxHardFloor = ResultSet.ZeroSet();
                maxHardFloor.DoMath(BaseCol.Amount, MathOp.SETTO, N_HardFloorDeductions.ToString(System.Globalization.CultureInfo.InvariantCulture));
                var processBy = ResultSet.ZeroSet();
                processBy.DoMath(EngineCol.Comment, MathOp.SETTO, "ActualPeriod ASC");
                // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
                var posHardFloor = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.posHardFloor",
                    pos3rdParty, maxHardFloor, processBy);
                maxHardFloor.Release();
                processBy.Release();
                posHardFloor.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                capOutput = posHardFloor;
            }
            else if (capType == "Hard and Soft floor" || capType == "Hard and Soft Floor")
            {
                var pos3rdParty = thirdPartyDeductionsItdSumm.Copy();
                pos3rdParty.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                decimal N_3rdPartyDed = DS.AmountOf(pos3rdParty);
                decimal N_MagrItd = DS.AmountOf(inputSet1);
                decimal N_PartShare = DS.AmountOf(participantShareItd);
                decimal hardFloorPct = Contract.GetUDFDecimal(ContractUDF.MAGR_HardFloorPercent);
                decimal softFloorPct = Contract.GetUDFDecimal(ContractUDF.MAGR_SoftFloorPercent);
                decimal N_HardFloor = System.Math.Round(N_MagrItd * hardFloorPct, 2);
                decimal N_DedToReachHard = N_PartShare - N_HardFloor;
                decimal N_SoftFloor = System.Math.Round(N_MagrItd * softFloorPct, 2);
                decimal N_DedToReachSoft = N_PartShare - N_SoftFloor;
                decimal N_3rdPartyLessSoft = N_3rdPartyDed - N_DedToReachSoft;
                decimal N_SharedDeductions = N_3rdPartyDed > N_DedToReachSoft ? N_3rdPartyLessSoft : 0m;
                decimal N_MaxShare = System.Math.Round(N_SharedDeductions * 0.5m, 2);
                decimal n_3rdPartyDeduction = (N_3rdPartyDed <= N_DedToReachSoft && N_3rdPartyDed <= N_DedToReachHard) ? N_3rdPartyDed : 0m;
                decimal N_DedSoftPlusMaxShare = N_DedToReachSoft + N_MaxShare;
                decimal N_PotentialDeduction = (N_3rdPartyDed <= N_DedToReachSoft && N_3rdPartyDed <= N_DedToReachHard) ? 0m : N_DedSoftPlusMaxShare;
                decimal N_3rdPartyPotentialDeduction = (N_PotentialDeduction < N_DedToReachHard && N_PotentialDeduction != 0m) ? N_PotentialDeduction : 0m;
                decimal N_3rdPartyHardFloor = (N_PotentialDeduction >= N_DedToReachHard && N_PotentialDeduction != 0m) ? N_DedToReachHard : 0m;
                decimal N_HardSoftDeductions = n_3rdPartyDeduction + N_3rdPartyPotentialDeduction + N_3rdPartyHardFloor;
                var maxHardSoft = ResultSet.ZeroSet();
                maxHardSoft.DoMath(BaseCol.Amount, MathOp.SETTO, N_HardSoftDeductions.ToString(System.Globalization.CultureInfo.InvariantCulture));
                var processBy = ResultSet.ZeroSet();
                processBy.DoMath(EngineCol.Comment, MathOp.SETTO, "ActualPeriod ASC");
                // TODO(oa-port): remove parity harness and call OrderedAllocation directly once field data confirms SQL/C# parity.
                var posHardSoft = ResultSetDSPs.OrderedAllocationWithParity("MagrLib.posHardSoft",
                    pos3rdParty, maxHardSoft, processBy);
                maxHardSoft.Release();
                processBy.Release();
                posHardSoft.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                capOutput = posHardSoft;
            }
            else
            {
                capOutput = thirdPartyDeductionsItdSumm.Copy();
            }
            thirdPartyDeductionsItdSumm.Release();

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(participantShareItd, capOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubParticipantShare (MAGR)");
            return output;
        }

        // ── 1703 C_MAGR_SUB_PAYMENT_AND_TAXES ────────────────────────────────
        //
        // Per-recipient payment + tax pipeline.  Mirrors the structure of
        // RoyaltyLib.SubPaymentDueAndTaxes but reads MAGR_-prefixed UDFs and emits
        // MAGR_-prefixed activity types.  See RoyaltyLib version for phase
        // commentary; the differences are: PartShare vs Royalties as input; payment
        // currency is MAGR_PaymentCurrency; tax UDFs use MAGR_DefaultWithholdingTaxRate
        // / MAGR_TreatyTaxRate / MAGR_VATRate.
        public static ResultSet SubPaymentAndTaxes(
            ResultSet inputSet1,    // MAGR_Total_Participant_Share Current
            ResultSet inputSet2,    // priorPeriodItd
            ResultSet inputSet3)    // adjustments ITD
        {
            _ctxSubPaymentAndTaxes = _ctxSubPaymentAndTaxes ?? NewSubCtxStandard("C_MAGR_SUB_PAYMENT_AND_TAXES");
            Job.CurrentCalcContext = _ctxSubPaymentAndTaxes;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubPaymentAndTaxes (MAGR)");

            // Adj to Payment.
            var adjToPaymentItdOutput = inputSet3.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Adjustment_to_Payment_Due,
                "MagrSubPT.AdjToPaymentITDOutput");
            var priorAdjToPaymentItd = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Adjustment_to_Payment_Due,
                "MagrSubPT.PriorAdjToPaymentITD");
            var adjToPaymentCurrentOutput = ResultSet.Subtract(adjToPaymentItdOutput, priorAdjToPaymentItd, "MagrSubPT.AdjToPaymentCurrent");
            priorAdjToPaymentItd.Release();
            adjToPaymentCurrentOutput.SetValue(CustCol.TransType, TransType.Current);
            adjToPaymentItdOutput.Release();
            adjToPaymentCurrentOutput.Release();

            // Balance brought forward.
            var balanceBfItdOutput = inputSet2.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Payment_Due_Carry_Forward,
                "MagrSubPT.BalanceBFITDOutput");
            balanceBfItdOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Payment_Due_Brought_Forward);
            balanceBfItdOutput.DoMath(EngineCol.Price2, MathOp.SETTO, BaseCol.Amount);

            int firstContractedPartySid = (Job.ContractParticipantSids.Count > 0) ? Job.ContractParticipantSids[0] : 0;

            // PartShareWithDivision: stamp Division and ContractedParty=first.
            var partShareWithDivision = inputSet1.Copy();
            partShareWithDivision.DoMath(CustCol.Division, MathOp.SETTO, ContractUDF.MAGR_Division);
            var currentPayment = partShareWithDivision;
            currentPayment.SetEntity(new ContactCol("ContractedParty"), new AlliantEntity(firstContractedPartySid));
            var _ops58 = new MathList
            {
                new MathOperation(EngineCol.Price2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units,   MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units2,  MathOp.SETTO, "0"),
            };
            currentPayment.DoMath(_ops58);

            // Payment by Recipient.
            var balanceDueByRecipientSet = currentPayment.PayRecipients(
                participantCol: new ContactCol("ContractedParty"),
                periodCol:      EngineCol.Period,
                recipientCol:   new ContactCol("PaymentRecipient"),
                rateCol:        EngineCol.Rate1,
                byCol:          BaseCol.Amount);
            currentPayment.Release();

            var currentBalanceDueAndBF = ResultSet.EmptySet();
            currentBalanceDueAndBF.CombineAndRelease(balanceBfItdOutput.Copy(), balanceDueByRecipientSet);
            currentBalanceDueAndBF.SetValue(CustCol.TransType, TransType.ITD);
            currentBalanceDueAndBF.DoMath(BaseCol.Units2, MathOp.SETTO, "0");
            // Total payment per recipient.
            currentBalanceDueAndBF.Lookup(BaseCol.Units2, MathOp.PLUS, BaseCol.Amount,
                                           new ContactCol("PaymentRecipient"), currentBalanceDueAndBF);
            // (Use Units2 as cumulative sum scratch.)

            var negativePayment = currentBalanceDueAndBF.GetData(BaseCol.Units2, CompareOp.LT, "0", "MagrSubPT.NegativePayment");
            negativePayment.SetValue(CustCol.ActivityType, ActivityType.MAGR_Payment_Due_Carry_Forward);
            var _ops59 = new MathList
            {
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, "Negative Payment at Recipient"),
                new MathOperation(BaseCol.Units2, MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, EngineCol.Price2),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
            };
            negativePayment.DoMath(_ops59);
            var negativePaymentOutput = negativePayment;

            // Positive payment paths.
            var positivePayment = currentBalanceDueAndBF.GetData(BaseCol.Units2, CompareOp.GT, "0", "MagrSubPT.PositivePayment");
            currentBalanceDueAndBF.Release();
            var _ops60 = new MathList
            {
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, ContractUDF.MAGR_DealOnHoldFlag),
                new MathOperation(BaseCol.Units2, MathOp.SETTO, "0"),
            };
            positivePayment.DoMath(_ops60);

            var dealHold = positivePayment.GetData(EngineCol.AltComment, CompareOp.EQ, "Yes", "MagrSubPT.DealHold");
            dealHold.DoMath(EngineCol.AltComment, MathOp.SETTO, "Deal on Hold");
            var notHeld = positivePayment.GetData(EngineCol.AltComment, CompareOp.EQ, "No", "MagrSubPT.NotHeld");
            positivePayment.Release();
            notHeld.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());

            var onHoldOutput = ResultSet.EmptySet();
            onHoldOutput.CombineAndRelease(dealHold);
            onHoldOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Payment_Due_Carry_Forward);
            var _ops61 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, EngineCol.Price2),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
            };
            onHoldOutput.DoMath(_ops61);

            // Currency conversion (calc → payment-recipient currency).
            var calcCurrencyMovedToSourceCurrency = ResultSet.ZeroSet();
            calcCurrencyMovedToSourceCurrency.DoMath(CustCol.SourceCurrency, MathOp.SETTO, ContractUDF.MAGR_DealCurrency);
            var L_SourceCurrency = calcCurrencyMovedToSourceCurrency.GetList(CustCol.SourceCurrency);
            calcCurrencyMovedToSourceCurrency.Release();

            var noHeldPaymentWithSourceCurrency = notHeld;
            noHeldPaymentWithSourceCurrency.SetEntity(CustCol.SourceCurrency, L_SourceCurrency[0]);
            // CalcCurrency = PaymentRecipient.MAGR_PaymentCurrency.
            // (Customer-tier: no engine helper for ContactN UDF, so DoMath via UDF lookup.)
            var _ops62 = new MathList
            {
                new MathOperation(CustCol.CalculationCurrency, MathOp.SETTO, PaymentRecipientUDF.MAGR_PaymentCurrency),
                new MathOperation(CustCol.OtherPeriod, MathOp.SETTO, CustCol.ActualPeriod),
                new MathOperation(EngineCol.Rate2, MathOp.SETTO, "0"),
            };
            noHeldPaymentWithSourceCurrency.DoMath(_ops62);

            var paymentWithRate = noHeldPaymentWithSourceCurrency;
            paymentWithRate.SetValue(CustCol.ActualPeriod, Job.CurrentCalcPeriod);
            paymentWithRate.DoMath(EngineCol.Rate2, MathOp.SETTO, SourceCurrencyUDF.ExchangeRate);

            var invalidExchangeRate = paymentWithRate.GetData(EngineCol.Rate2, CompareOp.EQ, "0", "MagrSubPT.InvalidExchangeRate");
            int N_ConversionError = invalidExchangeRate.Rows;
            invalidExchangeRate.Release();
            if (N_ConversionError > 0)
            {
                var sourceCurrencyError = ResultSet.ZeroSet();
                sourceCurrencyError.SetEntity(CustCol.SourceCurrency, L_SourceCurrency[0]);
                var idToComment = ResultSet.ZeroSet();
                // UDKeyToText reads Comment=source-UDKey-col-name, AltComment=delimiter (per DSP spec).
                idToComment.DoMath(EngineCol.AltComment, MathOp.SETTO, "-");
                idToComment.SetTextValue(EngineCol.Comment,    "UDKey17");
                var _ops63 = new MathList
                {
                    new MathOperation(EngineCol.Rate1,      MathOp.SETTO, "0"),
                    new MathOperation(EngineCol.Rate2,      MathOp.SETTO, "1"),
                };
                idToComment.DoMath(_ops63);
                // Fix: SQL DSP silently no-op'd; C# wrapper mutates IS1 in-place as intended.
                ResultSetDSPs.UDKeyToText(sourceCurrencyError, idToComment);
                idToComment.Release();
                sourceCurrencyError.DoMath(EngineCol.AltComment, MathOp.SETTO, "Exchange Rate not found for Source Currency.");
                ResultSetDSPs.SetCalcErrorInRun(sourceCurrencyError).Release();
                sourceCurrencyError.Release();
            }

            // Convert.
            paymentWithRate.DoMath(EngineCol.Price1, MathOp.SETTO, BaseCol.Amount);
            var convertedPayment = paymentWithRate;
            convertedPayment.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate2);
            var roundedConvertedPayment = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(convertedPayment);
            Job.CurrentCalcContext = _ctxSubPaymentAndTaxes;

            // Below-min check.
            var convertedPaymentWithTotal = roundedConvertedPayment;
            convertedPaymentWithTotal.Lookup(BaseCol.Amount2, MathOp.PLUS, BaseCol.Amount,
                                              new ContactCol("PaymentRecipient"), roundedConvertedPayment);
            convertedPaymentWithTotal.DoMath(BaseCol.Units, MathOp.SETTO, PaymentRecipientUDF.MAGR_PaymentMinimum);

            var belowMin = convertedPaymentWithTotal.GetData(BaseCol.Amount2, CompareOp.LT, BaseCol.Units, "MagrSubPT.BelowMin");
            // (DealScript uses Choose(...) which compares Amount2 vs Units row-by-row.  GetData with
            //  ColumnName-on-RHS isn't supported here -- fall back to filtering Amount2<0.005 as a
            //  conservative below-minimum approximation.)
            var _ops64 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Price1),
                new MathOperation(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod),
            };
            belowMin.DoMath(_ops64);
            belowMin.SetValue(CustCol.ActivityType, ActivityType.MAGR_Payment_Due_Carry_Forward);
            belowMin.DoMath(EngineCol.AltComment, MathOp.SETTO, "Payment Below Minimum");
            belowMin.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            belowMin.SetValue(CustCol.CalculationCurrency, ContractUDF.MAGR_DealCurrency);
            var _ops65 = new MathList
            {
                new MathOperation(EngineCol.Price1, MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Units2,   MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Amount2,  MathOp.SETTO, EngineCol.Price2),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
            };
            belowMin.DoMath(_ops65);
            var belowMinimumOutput = belowMin;

            var aboveMin = convertedPaymentWithTotal.GetData(BaseCol.Amount2, CompareOp.GE, BaseCol.Units, "MagrSubPT.AboveMin");
            convertedPaymentWithTotal.Release();
            aboveMin.SetValue(CustCol.RecoupmentGroup, new AlliantEntity(0));
            aboveMin.DoMath(BaseCol.Units2, MathOp.SETTO, "0");

            var paymentDueCurrentOutput = aboveMin;
            paymentDueCurrentOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Payment_Due);
            paymentDueCurrentOutput.DoMath(CustCol.CalculationCurrency, MathOp.SETTO, PaymentRecipientUDF.MAGR_PaymentCurrency);
            paymentDueCurrentOutput.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            var _ops66 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, EngineCol.Price2),
                new MathOperation(EngineCol.Price2, MathOp.SETTO, "0"),
            };
            paymentDueCurrentOutput.DoMath(_ops66);

            // ─── Tax pipeline (WH + VAT) ─────────────────────────────────────
            // 1. ForTaxes: clear Territory, AltComment, Rate1.
            // 2. Stamp Territory from Division.MAGR_CountryOfTaxResidence — this is the
            //    "tax territory" the contract operates in.
            // 3. Snapshot PaymentRecipient's country into a parallel set, mark Units2=99.
            //    Lookup by (PaymentRecipient, Territory) -- when the Division territory
            //    matches the recipient's country, Units2 ends up >0; otherwise stays 0.
            //    Rows where territory != recipient's country are "possibility tax" rows.
            //    Rows where territory == recipient's country are "possibility VAT" rows.
            // 4. WH branch: filter SubjectToWithholding=Yes; split SubjectToTreaty Yes/No.
            //    Rate1 ← Division.MAGR_TreatyTaxRate (treaty=Yes) or MAGR_DefaultWithholdingTaxRate (treaty=No).
            //    Tax = -1 * Amount * Rate1.
            // 5. VAT branch: filter SubjectToVAT=Yes; Rate1 ← Division.MAGR_VATRate; Tax = Amount * Rate1.
            var forTaxes = aboveMin.Copy();
            forTaxes.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());
            forTaxes.SetValue(CustCol.Territory, new AlliantEntity(0));
            forTaxes.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

            var aboveMinWithCountryOfResidence = forTaxes;
            aboveMinWithCountryOfResidence.DoMath(CustCol.Territory, MathOp.SETTO, DivisionUDF.MAGR_CountryOfTaxResidence);

            var pmtRecTerritory = aboveMinWithCountryOfResidence.Copy();
            pmtRecTerritory.SetValue(CustCol.Territory, new AlliantEntity(0));
            var _ops67 = new MathList
            {
                new MathOperation(CustCol.Territory, MathOp.SETTO, PaymentRecipientUDF.MAGR_CountryOfTaxResidence),
                new MathOperation(BaseCol.Units2, MathOp.SETTO, "99"),
            };
            pmtRecTerritory.DoMath(_ops67);

            // territory-eq lookup: Units2 += Lookup(Units2 by PaymentRecipient, Territory).
            aboveMinWithCountryOfResidence.Lookup(BaseCol.Units2, MathOp.PLUS, BaseCol.Units2,
                                                   pmtRecTerritory,
                                                   new ContactCol("PaymentRecipient"), CustCol.Territory);
            pmtRecTerritory.Release();

            var possibilityTax = aboveMinWithCountryOfResidence.GetData(BaseCol.Units2, CompareOp.EQ, "0", "MagrSubPT.PossibilityTax");
            var possibilityVat = aboveMinWithCountryOfResidence.GetData(BaseCol.Units2, CompareOp.NE, "0", "MagrSubPT.PossibilityVAT");
            var _ops68 = new MathList
            {
                new MathOperation(EngineCol.Comment, MathOp.SETTO, PaymentRecipientUDF.MAGR_SubjectToVATFlag),
                new MathOperation(BaseCol.Units2,    MathOp.SETTO, "0"),
            };
            possibilityVat.DoMath(_ops68);
            aboveMinWithCountryOfResidence.Release();

            // WH branch.
            var taxType = possibilityTax;
            var _ops69 = new MathList
            {
                new MathOperation(EngineCol.Comment,    MathOp.SETTO, PaymentRecipientUDF.MAGR_SubjectToWithholdingFlag),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, PaymentRecipientUDF.MAGR_SubjectToTreatyFlag),
            };
            taxType.DoMath(_ops69);

            var readyForWhTax = taxType.GetData(new FilterClause {
                new Criteria(EngineCol.Comment,    CompareOp.EQ, "Yes"),
                new Criteria(EngineCol.AltComment, CompareOp.EQ, "No")
            }, "MagrSubPT.ReadyForWHTax");
            var _ops70 = new MathList
            {
                new MathOperation(EngineCol.Comment,    MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.Rate1,      MathOp.SETTO, DivisionUDF.MAGR_DefaultWithholdingTaxRate),
            };
            readyForWhTax.DoMath(_ops70);
            var whTax = readyForWhTax;

            var readyForTreatyTax = taxType.GetData(new FilterClause {
                new Criteria(EngineCol.Comment,    CompareOp.EQ, "Yes"),
                new Criteria(EngineCol.AltComment, CompareOp.EQ, "Yes")
            }, "MagrSubPT.ReadyForTreatyTax");
            var _ops71 = new MathList
            {
                new MathOperation(EngineCol.Comment,    MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.Rate1,      MathOp.SETTO, DivisionUDF.MAGR_TreatyTaxRate),
            };
            readyForTreatyTax.DoMath(_ops71);
            var treatyTax = readyForTreatyTax;

            var taxes = ResultSet.EmptySet();
            taxes.CombineAndRelease(whTax, treatyTax);
            var nonZeroTax = taxes.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubPT.NonZeroTax");
            taxes.Release();
            var _ops72 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            nonZeroTax.DoMath(_ops72);
            var taxAmount = nonZeroTax.Copy();
            var _ops73 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
            };
            taxAmount.DoMath(_ops73);
            var roundedTaxAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(taxAmount);
            Job.CurrentCalcContext = _ctxSubPaymentAndTaxes;
            var taxOutput = ResultSet.EmptySet();
            taxOutput.CombineAndRelease(roundedTaxAmount, nonZeroTax);
            taxOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Withholding_Tax);
            taxOutput.DoMath(EngineCol.Comment, MathOp.SETTO, DS.F_NULL_STRING());

            // VAT branch.
            var readyForVat = possibilityVat.GetData(EngineCol.Comment, CompareOp.EQ, "Yes", "MagrSubPT.ReadyForVAT");
            possibilityVat.Release();
            readyForVat.DoMath(EngineCol.Comment, MathOp.SETTO, DS.F_NULL_STRING());
            var vatTax = readyForVat;
            vatTax.DoMath(EngineCol.Rate1, MathOp.SETTO, DivisionUDF.MAGR_VATRate);
            var nonZeroVat = vatTax.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubPT.NonZeroVAT");
            vatTax.Release();
            var _ops74 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            nonZeroVat.DoMath(_ops74);
            var vatAmount = nonZeroVat.Copy();
            var _ops75 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
            };
            vatAmount.DoMath(_ops75);
            var roundedVatAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(vatAmount);
            Job.CurrentCalcContext = _ctxSubPaymentAndTaxes;
            var vatOutput = ResultSet.EmptySet();
            vatOutput.CombineAndRelease(roundedVatAmount, nonZeroVat);
            vatOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_VAT);

            var finalPayment = paymentDueCurrentOutput.Copy();
            finalPayment.SetValue(CustCol.ActivityType, ActivityType.MAGR_Final_Payment_Due);
            finalPayment.DoMath(EngineCol.Price1, MathOp.SETTO, "0");

            var paymentAndTaxes = ResultSet.EmptySet();
            paymentAndTaxes.CombineAndRelease(paymentDueCurrentOutput.Copy(), finalPayment, taxOutput.Copy(), vatOutput.Copy());
            paymentAndTaxes.SetValue(CustCol.TransType, TransType.Current);
            var currentPaymentAndTaxesOutput = paymentAndTaxes;
            var _ops76 = new MathList
            {
                new MathOperation(EngineCol.Comment,    MathOp.SETTO, DS.F_NULL_STRING()),
                new MathOperation(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING()),
            };
            currentPaymentAndTaxesOutput.DoMath(_ops76);

            var priorPaymentItd = inputSet2.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.MAGR_Payment_and_Taxes_Activity_Type_List,
                "MagrSubPT.PriorPaymentITD");
            var itdPaymentAndTaxesOutput = ResultSet.EmptySet();
            itdPaymentAndTaxesOutput.CombineAndRelease(currentPaymentAndTaxesOutput.Copy(), priorPaymentItd);
            itdPaymentAndTaxesOutput.SetValue(CustCol.TransType, TransType.ITD);

            var output = ResultSet.EmptySet();
            output.Combine(balanceBfItdOutput);
            output.Combine(negativePaymentOutput);
            output.Combine(onHoldOutput);
            output.Combine(belowMinimumOutput);
            output.Combine(currentPaymentAndTaxesOutput);
            output.Combine(itdPaymentAndTaxesOutput);
            output.DoMath(BaseCol.Units, MathOp.SETTO, "0");

            balanceBfItdOutput.Release();
            negativePaymentOutput.Release();
            onHoldOutput.Release();
            belowMinimumOutput.Release();
            currentPaymentAndTaxesOutput.Release();
            itdPaymentAndTaxesOutput.Release();
            taxOutput.Release(); vatOutput.Release();

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubPaymentAndTaxes (MAGR)");
            return output;
        }

        // ── 1704 C_MAGR_SUB_PARTICIPATIONS_TIERING ───────────────────────────
        //
        // Tiers MAGR_Modified_Adjusted_Gross_Receipts rows by Amount against
        // ContractUDF.MAGR_PartcipationsTieringInterval (interval) and
        // ContractUDF.MAGR_ParticipantShare (per-tier %), filters within window,
        // applies the share %, emits MAGR_Participant_Share rows.
        //
        // Mirrors SubHvTiering closely but tiers ONLY by Amount (no per-units branch).
        public static ResultSet SubParticipationsTiering(
            ResultSet inputSet1,           // MAGR_MAGR ITD
            ResultSet inputSet2,           // window-start
            ResultSet inputSet3)           // priorUpToWindow ITD
        {
            _ctxSubParticipationsTiering = _ctxSubParticipationsTiering ?? NewSubCtxStandard("C_MAGR_SUB_PARTICIPATIONS_TIERING");
            Job.CurrentCalcContext = _ctxSubParticipationsTiering;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubParticipationsTiering (MAGR)");

            string tierInterval = Contract.GetUDFString(ContractUDF.MAGR_PartcipationsTieringInterval);
            int N_NoneLabel = (tierInterval == "<None>" || tierInterval == "<NONE>" || tierInterval == "No Tiering") ? 1 : 0;

            var L_StartWindowActualPeriod = inputSet2.GetList(CustCol.ActualPeriod);
            AlliantEntity startEntity = L_StartWindowActualPeriod[0];
            PeriodItem startWinPi = startEntity != null
                ? (PeriodItem)PeriodItem.Items.GetEntityBySid(startEntity.sid)
                : DS.F_INCEPTION();
            PeriodItem calcMinus1 = DS.F_PERIODS_FROM(Job.CurrentCalcPeriod, -1);

            int N_PeriodInterval = !string.IsNullOrEmpty(tierInterval)
                ? DS.F_NUM_PERIOD_TYPES_BETWEEN(startWinPi, calcMinus1, tierInterval)
                : 0;
            int N_GoBack = -1 * (N_PeriodInterval + 1);
            PeriodItem startPeriodPi = (N_GoBack == -1 || string.IsNullOrEmpty(tierInterval))
                ? DS.F_INCEPTION()
                : DS.F_PERIODS_FROM(DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, N_GoBack, tierInterval), 1);

            PeriodItem startWinMinus1 = DS.F_PERIODS_FROM(startWinPi, -1);
            var priorRange = DS.F_PERIOD_INTERVAL(startPeriodPi, startWinMinus1);
            var priorRoyBasis = inputSet1.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Modified_Adjusted_Gross_Receipts),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)priorRange)
            }, "MagrSubPT.PriorRoyBasis");

            var summarizePriorRoyBasis = SummarizeForWindowParticipationTiering(priorRoyBasis);
            Job.CurrentCalcContext = _ctxSubParticipationsTiering;

            var withinWindow = DS.F_PERIOD_INTERVAL(startWinPi, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));
            var royBasisWithinWindow = inputSet1.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Modified_Adjusted_Gross_Receipts),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinWindow)
            }, "MagrSubPT.RoyBasisWithinWindow");

            var royBasisForTiering = ResultSet.EmptySet();
            royBasisForTiering.CombineAndRelease(summarizePriorRoyBasis, royBasisWithinWindow.Copy());

            ResultSet noTiering = N_NoneLabel == 1 ? royBasisWithinWindow.Copy() : ResultSet.EmptySet();
            royBasisWithinWindow.Release();

            ResultSet forTiering;
            if (N_NoneLabel == 0)
            {
                forTiering = royBasisForTiering;
                forTiering.DoMath(CustCol.Tier, MathOp.SETTO, "0");
            }
            else
            {
                royBasisForTiering.Release();
                forTiering = ResultSet.EmptySet();
            }

            var tieringResult = ResultSet.EmptySet();
            if (N_NoneLabel == 0 && forTiering.Rows > 0)
            {
                var sortByAmount = new ResultSet.SortOrder(
                    (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Amount,            ResultSet.SortDirection.Ascending));
                var tieringByAmount = forTiering.TierSet(
                    IndexableColumn.Amount, IndexableColumn.Udkey_14_sid,
                    ContractUDF.MAGR_ParticipantShare, sortByAmount);
                tieringResult.Combine(tieringByAmount);
                tieringByAmount.Release();
            }

            ResultSet roundedTiering;
            if (N_NoneLabel == 0)
            {
                var roundedForProration = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(tieringResult);
                Job.CurrentCalcContext = _ctxSubParticipationsTiering;
                var baseProration = forTiering.Copy();
                roundedTiering = ResultSetProrate.Prorate12(
                    baseProration, roundedForProration,
                    mode: 0, roundingCorrMode: 0,
                    amountScale: 2, qtyScale: 0, altAmountScale: 2, altQtyScale: 0);
                baseProration.Release();
                roundedForProration.Release();
            }
            else
            {
                roundedTiering = ResultSet.EmptySet();
                tieringResult.Release();
            }
            forTiering.Release();

            var withinWindowFinal = roundedTiering.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinWindow,
                "MagrSubPT.WithinWindowFinal");
            roundedTiering.Release();

            var salesWithinInterval = ResultSet.EmptySet();
            salesWithinInterval.CombineAndRelease(noTiering, withinWindowFinal);
            var _ops77 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            salesWithinInterval.DoMath(_ops77);

            var withPercentOfSales = salesWithinInterval;
            withPercentOfSales.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.MAGR_ParticipantShare);
            var royaltyPercentOfSales = withPercentOfSales.Copy();
            var _ops78 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1),
            };
            royaltyPercentOfSales.DoMath(_ops78);
            var roundedRoyaltyPercentOfSales = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(royaltyPercentOfSales);
            Job.CurrentCalcContext = _ctxSubParticipationsTiering;

            var percentOfSalesWithTcs = ResultSet.EmptySet();
            percentOfSalesWithTcs.CombineAndRelease(withPercentOfSales, roundedRoyaltyPercentOfSales);
            var percentOfAmountOutput = percentOfSalesWithTcs.GetData(EngineCol.Rate1, CompareOp.NE, "0", "MagrSubPT.PercentOfAmountOutput");
            percentOfSalesWithTcs.Release();
            percentOfAmountOutput.SetValue(CustCol.ActivityType, ActivityType.MAGR_Participant_Share);

            var priorWindowItd = inputSet3.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Participant_Share,
                "MagrSubPT.PriorWindowITD");
            var output = ResultSet.EmptySet();
            output.CombineAndRelease(priorWindowItd, percentOfAmountOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubParticipationsTiering (MAGR)");
            return output;
        }

        // Private helper: 3rd-party cross-deal retrieval pattern shared by
        // SubCostOfProduction, SubDistributionExpenses, SubModifiedAdjustedGrossReceipts,
        // and SubParticipantShare.
        //
        // Runs p_ds_get_deal_list (Type=Participation, admin=7) filtered by the named
        // contract UDF, then p_ds_get_deal_run_status, then p_ds_get_calc_results.
        // Multiplies the returned rows by ratePctUdf and stamps ActivityType=outputAt.
        private static ResultSet MagrThirdPartyCrossDealRetrieval(
            string dealIdsUdfName,    // e.g. "MAGR_3rdPartyDeductibleCOPDealIDs"
            string shareChoice,       // resolved string from contract UDF (see below)
            string ratePctUdfHandle,  // ContractUDF.MAGR_3rdParty*Percent
            UDKey2Ref outputActivityType)
        {
            // p_ds_get_deal_list payload.
            var dspGetParticipation = ResultSet.ZeroSet();
            var _ops79 = new MathList
            {
                new MathOperation(EngineCol.Comment, MathOp.SETTO, "Participation"),
                new MathOperation(EngineCol.Rate1,   MathOp.SETTO, "7"),
            };
            dspGetParticipation.DoMath(_ops79);
            var dspDealRow = ResultSet.ZeroSet();
            dspDealRow.SetTextValue(EngineCol.Comment,    "Contract");
            dspDealRow.SetTextValue(EngineCol.AltComment, dealIdsUdfName);
            dspDealRow.DoMath(EngineCol.Rate1,      MathOp.SETTO, "1");
            var dspSameProcess = ResultSet.ZeroSet();
            dspSameProcess.SetTextValue(EngineCol.Comment,    "Deal");
            dspSameProcess.SetTextValue(EngineCol.AltComment, "DealType");
            dspSameProcess.DoMath(EngineCol.Rate1,      MathOp.SETTO, "4");
            var passToDealList = ResultSet.EmptySet();
            passToDealList.CombineAndRelease(dspGetParticipation, dspDealRow, dspSameProcess);
            var dspDealList = DS.ExecuteDSP("p_ds_get_deal_list", passToDealList);
            passToDealList.Release();

            // Run-status sweep.
            var dspGetStatusInput = ResultSet.ZeroSet();
            dspGetStatusInput.SetValue(CustCol.OtherPeriod, Job.CurrentCalcPeriod);
            var dspRunStatus = DS.ExecuteDSP("p_ds_get_deal_run_status", dspGetStatusInput, dspDealList);
            dspGetStatusInput.Release();

            var completeStatus = ResultSet.ZeroSet();
            completeStatus.DoMath(EngineCol.Rate1, MathOp.SETTO, "16");
            var approvedStatus = ResultSet.ZeroSet();
            approvedStatus.DoMath(EngineCol.Rate1, MathOp.SETTO, "5");
            var completeOrApproved = ResultSet.EmptySet();
            completeOrApproved.CombineAndRelease(completeStatus, approvedStatus);
            var L_CompleteApproved = completeOrApproved.GetList(EngineCol.Rate1);
            completeOrApproved.Release();

            var approvedDeals    = dspRunStatus.GetData(EngineCol.Rate1, ListOp.INLIST, L_CompleteApproved, "Magr3rdParty.ApprovedDeals");
            var dealsInSetup     = dspDealList.GetData(EngineCol.Rate2, CompareOp.EQ, "27", "Magr3rdParty.DealsInSetup");
            var modelDealsInSetup = dspDealList.GetData(EngineCol.Rate2, CompareOp.EQ, "30", "Magr3rdParty.ModelDealsInSetup");
            var dealsNotInError = ResultSet.EmptySet();
            dealsNotInError.CombineAndRelease(approvedDeals, dealsInSetup, modelDealsInSetup);
            var L_DealsNotInError = dealsNotInError.GetList(EngineCol.Comment);
            dealsNotInError.Release();
            var dealsInError = dspDealList.GetData(EngineCol.Comment, ListOp.NOTINLIST, L_DealsNotInError, "Magr3rdParty.DealsInError");
            if (dealsInError.Rows > 0)
            {
                var errIs1 = dealsInError.Copy();
                errIs1.DoMath(EngineCol.AltComment, MathOp.SETTO,
                    "- Is a contributing Deal with a Process that has not been run to completion, please Run the Other Deals before proceeding.");
                var errIs2 = ResultSet.ZeroSet();
                errIs2.DoMath(EngineCol.Comment, MathOp.SETTO, "Deal ID:");
                ResultSetDSPs.SetCalcErrorInRun(errIs1, errIs2).Release();
                errIs1.Release();
                errIs2.Release();
            }
            dealsInError.Release();
            dspRunStatus.Release();

            // Pull contributing-deals' Total Participant Share or Adjusted Gross
            // Receipts ITD rows depending on shareChoice.
            var thirdPartyFilter = new FilterClause {
                new Criteria(CustCol.TransType, CompareOp.EQ, TransType.ITD)
            };
            if (shareChoice == "Total Participant Share")
                thirdPartyFilter.Add(new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Total_Participant_Share));
            else if (shareChoice == "Adjusted Gross Receipts")
                thirdPartyFilter.Add(new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.MAGR_Modified_Adjusted_Gross_Receipts));

            // TODO(gcr-port): remove parity harness and call ResultSet.GetDataFromDealCalcResults directly once field data confirms SQL/C# parity.
            var thirdPartyResult = ResultSetDSPs.GetDataFromDealCalcResultsWithParity(
                "MagrLib.thirdPartyResult3rdParty",
                dspDealList, thirdPartyFilter,
                endPeriod:       Job.CurrentCalcPeriod,
                mode:            ResultSet.CalcResultPeriodMode.LatestUpToEnd,
                jobStatusIds:    new[] { "Approved", "Complete" },
                altCommentStamp: ResultSet.ContractStampMode.ContractId);
            dspDealList.Release();

            var valid3rdParty = thirdPartyResult.GetData(
                CustCol.ActivityType, CompareOp.NE, new AlliantEntity(0),
                "Magr3rdParty.Valid3rdParty");
            thirdPartyResult.Release();
            var _ops80 = new MathList
            {
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate1, MathOp.SETTO, ratePctUdfHandle),
            };
            valid3rdParty.DoMath(_ops80);

            var validRate = valid3rdParty.GetData(new FilterClause {
                new Criteria(EngineCol.Rate1, CompareOp.NE, "0"),
                new Criteria(BaseCol.Amount,  CompareOp.NE, "0")
            }, "Magr3rdParty.ValidRate");
            valid3rdParty.Release();
            var _ops81 = new MathList
            {
                new MathOperation(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount),
                new MathOperation(BaseCol.Amount,  MathOp.SETTO, "0"),
            };
            validRate.DoMath(_ops81);

            var amount = validRate.Copy();
            var _ops82 = new MathList
            {
                new MathOperation(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, BaseCol.Amount2),
                new MathOperation(BaseCol.Amount, MathOp.TIMES, "-1"),
            };
            amount.DoMath(_ops82);
            var rounded = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(amount);

            var combined = ResultSet.EmptySet();
            combined.CombineAndRelease(rounded, validRate);
            var summed = combined.Summarize();
            if (!ReferenceEquals(summed, combined)) combined.Release();
            summed.DoMath(EngineCol.Comment, MathOp.SETTO, DS.F_NULL_STRING());

            // Move UDKey2 → Comment1 via DSP.
            var convertUdk2ToComment1 = ResultSet.ZeroSet();
            convertUdk2ToComment1.SetTextValue(EngineCol.Comment, "UDKey2");
            var _ops83 = new MathList
            {
                new MathOperation(EngineCol.Rate1,   MathOp.SETTO, "0"),
                new MathOperation(EngineCol.Rate2,   MathOp.SETTO, "0"),
            };
            convertUdk2ToComment1.DoMath(_ops83);
            // Fix: SQL DSP silently no-op'd; C# wrapper mutates IS1 in-place as intended.
            ResultSetDSPs.UDKeyToText(summed, convertUdk2ToComment1);
            convertUdk2ToComment1.Release();

            summed.SetValue(CustCol.ActivityType, outputActivityType);
            return summed;
        }

    }
}
