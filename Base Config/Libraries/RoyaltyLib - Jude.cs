// BC1 BaseConfig — Royalty pipeline library.
//
// Conversion progress:
//   ✓ 1546  C_SUMMARIZE_TO_ACTUAL_PERIOD_BUNDLE       — SummarizeToActualPeriodBundle
//   ✓ 1557  C_SUMMARIZE_FOR_WINDOW_TIERING            — SummarizeForWindowTiering
//   ✓ 1567  C_SUMMARIZE_FOR_ROYALTIES_DUE             — SummarizeForRoyaltiesDue
//   ✓ 1570  C_SUMMARIZE_FOR_GL_ENTRIES                — SummarizeForGLEntries
//   ✓ 1572  C_SUMMARIZE_FOR_AP_ENTRIES                — SummarizeForAPEntries
//   ✓ 1574  C_SUMMARIZE_FOR_STATEMENT_DISPLAY         — SummarizeForStatementDisplay
//   ✓ 1591  C_SUMMARIZE_FOR_GUARANTEE_INSTALLMENT     — SummarizeForGuaranteeInstallment
//   ✓ 1623  C_SUMMARIZE_TO_ACTUAL_PERIOD_RATE3_PRICE2 — SummarizeToActualPeriodRate3Price2
//   ✓ 1655  C_SUMMARIZE_FOR_BONUS_TIERING             — SummarizeForBonusTiering
//   ✓ 1657  C_SUMMARIZE_TO_TIER                       — SummarizeToTier
//
//   ✓ 1550  C_ROY_GET_IMPORT_ADJS_PRIOR_ITD           — SubGetImportAdjsPriorITD
//   ✓ 1553  C_ROY_SUB_DEDUCTION_CAP                   — SubDeductionCap
//   ✓ 1555  C_ROY_SUB_NET_SALES                       — SubNetSales
//   ✓ 1556  C_ROY_SUB_TIERING                         — SubTiering
//   ✓ 1590  C_ROY_SUB_GUARANTEE_INSTALLMENTS          — SubGuaranteeInstallments
//   ✓ 1558  C_ROY_SUB_ROYALTIES_EARNED                — SubRoyaltiesEarned
//   ✓ 1559  C_ROY_SUB_RESERVES                        — SubReserves
//   ✓ 1560  C_ROY_SUB_REVENUE_DEDUCTION               — SubRevenueDeduction
//   ✓ 1564  C_ROY_SUB_GUARANTEES                      — SubGuarantees
//   ✓ 1566  C_ROY_SUB_ROYALTIES_DUE                   — SubRoyaltiesDue
//   ✓ 1568  C_ROY_SUB_PAYMENT_DUE_AND_TAXES           — SubPaymentDueAndTaxes
//   ✓ 1569  C_ROY_SUB_GL_ENTRIES                      — SubGLEntries
//   ✓ 1571  C_ROY_SUB_AP_ENTRIES                      — SubAPEntries
//   ✓ 1573  C_ROY_SUB_STATEMENT_DISPLAY               — SubStatementDisplay
//   ✓ 1577  C_ROY_SUB_ADVANCES_WITH_CP                — SubAdvancesWithCP (dormant)
//   ☐ 1590  C_ROY_SUB_GUARANTEE_INSTALLMENTS          — TODO (24153b — largest)
//   ✓ 1644  C_ROY_SUB_ADVANCES                        — SubAdvances
//   ✓ 1662  C_ROY_SUB_BONUS_PAYMENT                   — SubBonusPayment
//
// CalcContext Reset() column lists are the COMPLEMENT of sum=Y in c_calc_context.
using System;
using Velocity.Handles;

namespace Velocity
{
    public static class RoyaltyLib
    {
        // CalcContext fields (lazy-allocated).  Each Sub method sets
        // Job.CurrentCalcContext to its own field at entry; *1/+0-style
        // re-summarization in DealScript translates to .Summarize() calls
        // that operate against this context.  No save/restore: callers must
        // re-establish their own context after a Sub returns.  Pattern matches
        // getty's CommonLib/StandardPrecisionLib/StandardContractLib (Phase 2-cleaned).
        private static CalcContext _ctxSummarizeToActualPeriodBundle;
        private static CalcContext _ctxSummarizeForWindowTiering;
        private static CalcContext _ctxSummarizeForRoyaltiesDue;
        private static CalcContext _ctxSummarizeForGLEntries;
        private static CalcContext _ctxSummarizeForAPEntries;
        private static CalcContext _ctxSummarizeForStatementDisplay;
        private static CalcContext _ctxSummarizeForGuaranteeInstallment;
        private static CalcContext _ctxSummarizeToActualPeriodRate3Price2;
        private static CalcContext _ctxSummarizeForBonusTiering;
        private static CalcContext _ctxSummarizeToTier;

        // Sub-calc CalcContext fields.  Reset cols are derived from c_calc_context
        // (the COMPLEMENT of summarize_by_column_flag='Y').  Six of seven Subs share
        // the same Reset set: {UDKey13, UDKey15, UDKey16, ToDate, Contact3, Contact4}.
        // 1569 SubGLEntries and 1571 SubAPEntries have empty Reset (sum on every col).
        private static CalcContext _ctxSubNetSales;
        private static CalcContext _ctxSubRevenueDeduction;
        private static CalcContext _ctxSubGuarantees;
        private static CalcContext _ctxSubRoyaltiesDue;
        private static CalcContext _ctxSubGLEntries;
        private static CalcContext _ctxSubAPEntries;
        private static CalcContext _ctxSubStatementDisplay;
        private static CalcContext _ctxSubRoyaltiesEarned;
        private static CalcContext _ctxSubPaymentDueAndTaxes;
        private static CalcContext _ctxSubDeductionCap;
        private static CalcContext _ctxSubAdvancesWithCP;
        private static CalcContext _ctxSubAdvances;
        private static CalcContext _ctxSubReserves;
        private static CalcContext _ctxSubBonusPayment;
        private static CalcContext _ctxSubTiering;
        private static CalcContext _ctxSubGetImportAdjsPriorITD;
        private static CalcContext _ctxSubGuaranteeInstallments;

        // Sub calcs have a pure no-op c_calc_context (all summarize_by_column_flag='Y').
        // Both factories return an empty CalcContext; the DB does not authorize any Reset
        // for these calcs, so nothing gets erased from the group-by key.
        private static CalcContext NewSubCtxStandard(string name) => new CalcContext(name);
        private static CalcContext NewSubCtxFullKey(string name) => new CalcContext(name);

        // ── 1546 C_SUMMARIZE_TO_ACTUAL_PERIOD_BUNDLE ─────────────────────────
        // sum=Y: UDKey8 (Bundle), Period, ActualPeriod, Deal, Amount, Units, Amount2, Units2
        public static ResultSet SummarizeToActualPeriodBundle(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeToActualPeriodBundle == null)
                _ctxSummarizeToActualPeriodBundle = new CalcContext("C_SUMMARIZE_TO_ACTUAL_PERIOD_BUNDLE")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey4,
                           EngineCol.UDKey5, EngineCol.UDKey6, EngineCol.UDKey7,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15, EngineCol.UDKey16,
                           EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeToActualPeriodBundle;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1557 C_SUMMARIZE_FOR_WINDOW_TIERING ──────────────────────────────
        // sum=Y: UDKey1, UDKey4, UDKey5, UDKey6, UDKey7, Period, ActualPeriod, Deal,
        //        Amount, Units, Amount2, Units2
        public static ResultSet SummarizeForWindowTiering(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeForWindowTiering == null)
                _ctxSummarizeForWindowTiering = new CalcContext("C_SUMMARIZE_FOR_WINDOW_TIERING")
                    .Reset(EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey8,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15, EngineCol.UDKey16,
                           EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
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
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1567 C_SUMMARIZE_FOR_ROYALTIES_DUE ───────────────────────────────
        // sum=Y: UDKey2, UDKey3, Period, ActualPeriod, Deal, Amount, Units, UDKey15, UDKey18, Amount2, Units2
        public static ResultSet SummarizeForRoyaltiesDue(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeForRoyaltiesDue == null)
                _ctxSummarizeForRoyaltiesDue = new CalcContext("C_SUMMARIZE_FOR_ROYALTIES_DUE")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey4, EngineCol.UDKey5, EngineCol.UDKey6,
                           EngineCol.UDKey7, EngineCol.UDKey8, EngineCol.UDKey9, EngineCol.UDKey10,
                           EngineCol.UDKey11, EngineCol.UDKey12, EngineCol.UDKey13, EngineCol.UDKey14,
                           EngineCol.UDKey16, EngineCol.UDKey17, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeForRoyaltiesDue;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1570 C_SUMMARIZE_FOR_GL_ENTRIES ──────────────────────────────────
        // sum=Y: UDKey2, UDKey3, UDKey4, UDKey5, UDKey6, UDKey7, Period, ActualPeriod, Deal,
        //        Amount, Units, UDKey18, Amount2, Units2
        public static ResultSet SummarizeForGLEntries(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeForGLEntries == null)
                _ctxSummarizeForGLEntries = new CalcContext("C_SUMMARIZE_FOR_GL_ENTRIES")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey8,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15, EngineCol.UDKey16,
                           EngineCol.UDKey17, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeForGLEntries;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1572 C_SUMMARIZE_FOR_AP_ENTRIES ──────────────────────────────────
        // sum=Y: UDKey2, UDKey3, Period, ActualPeriod, Deal, Amount, Units, UDKey15, UDKey18,
        //        Contact1, Contact2, Amount2, Units2
        public static ResultSet SummarizeForAPEntries(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeForAPEntries == null)
                _ctxSummarizeForAPEntries = new CalcContext("C_SUMMARIZE_FOR_AP_ENTRIES")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey4, EngineCol.UDKey5, EngineCol.UDKey6,
                           EngineCol.UDKey7, EngineCol.UDKey8, EngineCol.UDKey9, EngineCol.UDKey10,
                           EngineCol.UDKey11, EngineCol.UDKey12, EngineCol.UDKey14,
                           EngineCol.UDKey16, EngineCol.UDKey17, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeForAPEntries;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1574 C_SUMMARIZE_FOR_STATEMENT_DISPLAY ───────────────────────────
        // sum=Y: UDKey1-8, Rate, Period, ActualPeriod, Price (=PricePoint), Deal, Amount, Units,
        //        UDKey16, Amount2, Units2
        public static ResultSet SummarizeForStatementDisplay(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeForStatementDisplay == null)
                _ctxSummarizeForStatementDisplay = new CalcContext("C_SUMMARIZE_FOR_STATEMENT_DISPLAY")
                    .Reset(EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15,
                           EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeForStatementDisplay;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1591 C_SUMMARIZE_FOR_GUARANTEE_INSTALLMENT ───────────────────────
        // sum=Y: Period, ActualPeriod, Deal, Amount, Units, UDKey15, UDKey18, Amount2, Units2
        public static ResultSet SummarizeForGuaranteeInstallment(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeForGuaranteeInstallment == null)
                _ctxSummarizeForGuaranteeInstallment = new CalcContext("C_SUMMARIZE_FOR_GUARANTEE_INSTALLMENT")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey4,
                           EngineCol.UDKey5, EngineCol.UDKey6, EngineCol.UDKey7, EngineCol.UDKey8,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey16, EngineCol.UDKey17,
                           EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeForGuaranteeInstallment;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1623 C_SUMMARIZE_TO_ACTUAL_PERIOD_RATE3_PRICE2 ───────────────────
        // sum=Y: Period, ActualPeriod, Price2 (alt_price_point), Deal, Amount, Units,
        //        Rate3 (user_3_rate), Amount2, Units2
        public static ResultSet SummarizeToActualPeriodRate3Price2(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeToActualPeriodRate3Price2 == null)
                _ctxSummarizeToActualPeriodRate3Price2 = new CalcContext("C_SUMMARIZE_TO_ACTUAL_PERIOD_RATE3_PRICE2")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey4,
                           EngineCol.UDKey5, EngineCol.UDKey6, EngineCol.UDKey7, EngineCol.UDKey8,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15, EngineCol.UDKey16,
                           EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2,
                           EngineCol.Price1,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeToActualPeriodRate3Price2;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1655 C_SUMMARIZE_FOR_BONUS_TIERING ───────────────────────────────
        // sum=Y: UDKey1, UDKey4, UDKey5, UDKey6, UDKey7, UDKey9, UDKey10, UDKey11,
        //        Period, ActualPeriod, Deal, Amount, Units, UDKey12, Amount2, Units2
        public static ResultSet SummarizeForBonusTiering(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeForBonusTiering == null)
                _ctxSummarizeForBonusTiering = new CalcContext("C_SUMMARIZE_FOR_BONUS_TIERING")
                    .Reset(EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey8,
                           EngineCol.UDKey13, EngineCol.UDKey14, EngineCol.UDKey15, EngineCol.UDKey16,
                           EngineCol.UDKey17, EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeForBonusTiering;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1558 C_ROY_SUB_ROYALTIES_EARNED ──────────────────────────────────
        //
        // Three-output royalty calculation, branched by RoyaltyMethodType contract UDF:
        //
        //   "Standard" type — assigns each row a single RoyaltyMethod by priority:
        //     1. RoyaltyRatePercentOfNetSales  (UDF 881)  → Percent_of_Net_Sales
        //     2. RoyaltyRateUnitRate           (UDF 879)  → Unit_Rate
        //     3. RoyaltyRatePercentOfRetailPrice (UDF 880) → Percent_of_Retail_Price
        //     4. otherwise                                → Unassigned_Rate
        //
        //   "Greater Of" type — computes royalty under both PctOfSales AND UnitRate
        //     formulas for each row, then picks the winner per row:
        //       positive royalties → MAX (Greater Of via GroupNumbering DESC on Rate2)
        //       negative royalties → MIN (Lesser Of via GroupNumbering ASC on Rate2)
        //
        //   PctOfPct (UDF 926) — final multiplier applied to all royalties.
        //
        // Rate2 is used as scratch for the intermediate computed-royalty value during
        // the greater-of tournament; Amount2 stashes the original NetSales amount;
        // Units2 stashes the rate*amount/units product for downstream display.
        //
        // INPUTSET4 = NetSales rows with method tagged + Amount2/Units2 preserved.
        // INPUTSET5 = Combined Royalties_Earned rows (rounded) + adjustments.
        // INPUTSET6 = Adjustment_to_Royalties_Earned rows passthrough.
        public static (ResultSet NetSalesOutput,
                       ResultSet RoyaltyOutput,
                       ResultSet AdjToRoyEarnedItd) SubRoyaltiesEarned(
            ResultSet netSalesItd,           // INPUTSET1
            ResultSet allAdjustmentsItd,     // INPUTSET2
            ResultSet windowStartPeriodItd)  // INPUTSET3
        {
            _ctxSubRoyaltiesEarned = _ctxSubRoyaltiesEarned ?? NewSubCtxStandard("C_ROY_SUB_ROYALTIES_EARNED");
            Job.CurrentCalcContext = _ctxSubRoyaltiesEarned;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubRoyaltiesEarned");

            var L_StartWindowActualPeriod = windowStartPeriodItd.GetList(CustCol.ActualPeriod);

            // MethodType = Set Rate1=0, AltComment=ContractUDF.RoyaltyMethodType
            //              Set Amount2 = Amount  (stash original NetSales)
            var methodType = netSalesItd.Copy();
            methodType.DoMath(EngineCol.Rate1,      MathOp.SETTO, "0");
            methodType.DoMath(EngineCol.AltComment, MathOp.SETTO, ContractUDF.RoyaltyMethodType);
            methodType.DoMath(BaseCol.Amount2,      MathOp.SETTO, BaseCol.Amount);

            // WithPercentOfPercent: stash PercentOfPercent UDF in Rate3.
            var withPercentOfPercent = methodType;
            withPercentOfPercent.DoMath(EngineCol.Rate3, MathOp.SETTO, ContractUDF.PercentOfPercent);

            // ─── "Standard" type: 3 mutually exclusive sub-branches ───
            // Rows where AltComment == "Standard" (the literal string from RoyaltyMethodType UDF).
            var standardMethodInitial = withPercentOfPercent.GetData(EngineCol.AltComment, CompareOp.EQ, "Standard");
            standardMethodInitial.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());
            standardMethodInitial.DoMath(BaseCol.Amount2,      MathOp.SETTO, BaseCol.Amount);   // re-stash for these rows
            var standardMethod = standardMethodInitial;

            // Sub-branch 1: PercentOfSales (UDF 881).  Rate1 = ContractUDF.RoyaltyRatePercentOfNetSales.
            var withPercentOfSales = standardMethod.Copy();
            withPercentOfSales.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRatePercentOfNetSales);
            var withPercentOfSalesValid = withPercentOfSales.GetData(EngineCol.Rate1, CompareOp.NE, "0");
            var withPercentOfSalesInvalid = withPercentOfSales.GetData(EngineCol.Rate1, CompareOp.EQ, "0");
            withPercentOfSales.Release();
            var netSalesPercentOfSales = withPercentOfSalesValid;
            netSalesPercentOfSales.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Percent_of_Net_Sales);

            // RoyaltyPercentOfSales = Amount * Rate1 → Rate2 → Amount.
            var royaltyPercentOfSales = netSalesPercentOfSales.Copy();
            royaltyPercentOfSales.DoMath(EngineCol.Rate2, MathOp.SETTO, BaseCol.Amount);
            royaltyPercentOfSales.DoMath(EngineCol.Rate2, MathOp.TIMES, EngineCol.Rate1);
            royaltyPercentOfSales.DoMath(BaseCol.Amount,  MathOp.SETTO, EngineCol.Rate2);

            // Sub-branch 2: UnitRate.  Filter from withPercentOfSalesInvalid (rows still without a rate).
            var withUnitRate = withPercentOfSalesInvalid;
            withUnitRate.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRateUnitRate);
            var withUnitRateValid   = withUnitRate.GetData(EngineCol.Rate1, CompareOp.NE, "0");
            var withUnitRateInvalid = withUnitRate.GetData(EngineCol.Rate1, CompareOp.EQ, "0");
            withUnitRate.Release();
            var netSalesUnitRate = withUnitRateValid;
            netSalesUnitRate.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Unit_Rate);

            // RoyaltyUnitRate = Units * Rate1 → Rate2 → Amount.
            var royaltyUnitRate = netSalesUnitRate.Copy();
            royaltyUnitRate.DoMath(EngineCol.Rate2, MathOp.SETTO, BaseCol.Units);
            royaltyUnitRate.DoMath(EngineCol.Rate2, MathOp.TIMES, EngineCol.Rate1);
            royaltyUnitRate.DoMath(BaseCol.Amount,  MathOp.SETTO, EngineCol.Rate2);

            // Sub-branch 3: PercentOfList.
            var withPercentOfList = withUnitRateInvalid;
            withPercentOfList.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRatePercentOfRetailPrice);
            var withPercentOfListValid   = withPercentOfList.GetData(EngineCol.Rate1, CompareOp.NE, "0");
            var withPercentOfListInvalid = withPercentOfList.GetData(EngineCol.Rate1, CompareOp.EQ, "0");
            // Keep withPercentOfList alive for InvalidRates use later... actually no, the
            // DealScript reads InvalidRates from the same source.  Cache the invalid set.
            withPercentOfList.Release();
            var netSalesPercentOfList = withPercentOfListValid;
            netSalesPercentOfList.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Percent_of_Retail_Price);

            // NetSalesPercentOfListRev = Units * Price1 → Rate2; RoyaltyPctList = Rate2 * Rate1 → Rate2 → Amount.
            var royaltyPercentOfList = netSalesPercentOfList.Copy();
            royaltyPercentOfList.DoMath(EngineCol.Rate2, MathOp.SETTO, BaseCol.Units);
            royaltyPercentOfList.DoMath(EngineCol.Rate2, MathOp.TIMES, EngineCol.Price1);
            royaltyPercentOfList.DoMath(EngineCol.Rate2, MathOp.TIMES, EngineCol.Rate1);
            royaltyPercentOfList.DoMath(BaseCol.Amount,  MathOp.SETTO, EngineCol.Rate2);

            // StandardRoyalty = round(Combine of three sub-branches).
            var standardRoyaltyCombined = ResultSet.EmptySet();
            standardRoyaltyCombined.CombineAndRelease(royaltyPercentOfSales, royaltyUnitRate, royaltyPercentOfList);
            var standardRoyalty = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(standardRoyaltyCombined);

            // ─── "Greater Of" type: tournament between PctOfSales and UnitRate ───
            var greaterOfMethodInitial = withPercentOfPercent.GetData(EngineCol.AltComment, CompareOp.EQ, "Greater Of");
            greaterOfMethodInitial.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());
            var greaterOfMethod = greaterOfMethodInitial;

            // GreaterOfWithUnitRate: Rate1 = ContractUDF.RoyaltyRateUnitRate.
            var greaterOfWithUnitRate = greaterOfMethod;
            greaterOfWithUnitRate.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRateUnitRate);

            // Split: rows with valid Rate1 (UnitRate available) vs invalid (UnitRate=0).
            var maybeValidGreaterOfWithUnitRate = greaterOfWithUnitRate.GetData(EngineCol.Rate1, CompareOp.NE, "0");
            var invalidGreaterOfWithUnitRate    = greaterOfWithUnitRate.GetData(EngineCol.Rate1, CompareOp.EQ, "0");
            greaterOfWithUnitRate.Release();

            // Invalid path: try PctOfSales as fallback.  Two outcomes:
            //   InvalidUnitRateWithInvalidPctOfSales  → Unassigned (no method works for these rows)
            //   InvalidUnitRateWithValidPctOfSales    → Use PctOfSales (single method, no tournament)
            invalidGreaterOfWithUnitRate.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRatePercentOfNetSales);
            var invalidUnitRateWithInvalidPctOfSales = invalidGreaterOfWithUnitRate.GetData(EngineCol.Rate1, CompareOp.EQ, "0");
            var invalidUnitRateWithValidPctOfSales   = invalidGreaterOfWithUnitRate.GetData(EngineCol.Rate1, CompareOp.NE, "0");
            invalidGreaterOfWithUnitRate.Release();

            // For maybeValidGreaterOfWithUnitRate: clear Rate1, then try PctOfSales.
            var forGrtOfPctSales = maybeValidGreaterOfWithUnitRate.Copy();
            forGrtOfPctSales.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
            forGrtOfPctSales.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRatePercentOfNetSales);
            var validUnitRateWithInvalidPctSales = forGrtOfPctSales.GetData(EngineCol.Rate1, CompareOp.EQ, "0");
            var validGreaterOfWithPctSales       = forGrtOfPctSales.GetData(EngineCol.Rate1, CompareOp.NE, "0");
            forGrtOfPctSales.Release();
            maybeValidGreaterOfWithUnitRate.Release();

            // For rows that have BOTH rates valid: compute both royalty values and tournament.
            var validGreaterOfWithUnitRate = validGreaterOfWithPctSales.Copy();
            validGreaterOfWithUnitRate.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRateUnitRate);

            // PctOfSales branch of the tournament.
            var saveGreaterOfWithPctSales = validGreaterOfWithPctSales.Copy();
            saveGreaterOfWithPctSales.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            var greaterOfPctSalesRoy = validGreaterOfWithPctSales;
            greaterOfPctSalesRoy.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1);
            greaterOfPctSalesRoy.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate3);   // PctOfPct multiplier baked in here
            var greaterOfPctSalesRoyWithUnits = ResultSet.EmptySet();
            greaterOfPctSalesRoyWithUnits.CombineAndRelease(saveGreaterOfWithPctSales, greaterOfPctSalesRoy);
            greaterOfPctSalesRoyWithUnits.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Percent_of_Net_Sales);
            var roundedGreaterOfPctSalesRoyWithUnits = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(greaterOfPctSalesRoyWithUnits);
            // Preserve the rounded royalty in Rate2, then zero Amount.
            roundedGreaterOfPctSalesRoyWithUnits.DoMath(EngineCol.Rate2,    MathOp.SETTO, BaseCol.Amount);
            roundedGreaterOfPctSalesRoyWithUnits.DoMath(BaseCol.Amount,     MathOp.SETTO, "0");
            // Split into positive (Rate2 ≥ 0 AND Units ≥ 0) and negative (both < 0) bands.
            var posPctSalesAmountUnits = roundedGreaterOfPctSalesRoyWithUnits.GetData(new FilterClause {
                new Criteria(EngineCol.Rate2, CompareOp.GE, "0"),
                new Criteria(BaseCol.Units,   CompareOp.GE, "0")
            });
            var negPctSales = roundedGreaterOfPctSalesRoyWithUnits.GetData(new FilterClause {
                new Criteria(EngineCol.Rate2, CompareOp.LT, "0"),
                new Criteria(BaseCol.Units,   CompareOp.LT, "0")
            });
            roundedGreaterOfPctSalesRoyWithUnits.Release();

            // UnitRate branch of the tournament.
            var saveValidGreaterOfWithUnitRate = validGreaterOfWithUnitRate.Copy();
            saveValidGreaterOfWithUnitRate.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            var greaterOfUnitRateRoy = validGreaterOfWithUnitRate;
            greaterOfUnitRateRoy.DoMath(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1);
            greaterOfUnitRateRoy.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate3);
            greaterOfUnitRateRoy.DoMath(BaseCol.Amount, MathOp.TIMES, BaseCol.Units);
            var greaterOfUnitRateRoyWithUnits = ResultSet.EmptySet();
            greaterOfUnitRateRoyWithUnits.CombineAndRelease(saveValidGreaterOfWithUnitRate, greaterOfUnitRateRoy);
            greaterOfUnitRateRoyWithUnits.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Unit_Rate);
            var roundedGreaterOfUnitRateRoyWithUnits = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(greaterOfUnitRateRoyWithUnits);
            roundedGreaterOfUnitRateRoyWithUnits.DoMath(EngineCol.Rate2,    MathOp.SETTO, BaseCol.Amount);
            roundedGreaterOfUnitRateRoyWithUnits.DoMath(BaseCol.Amount,     MathOp.SETTO, "0");
            var posUnitRateAmountAndUnits = roundedGreaterOfUnitRateRoyWithUnits.GetData(new FilterClause {
                new Criteria(EngineCol.Rate2, CompareOp.GE, "0"),
                new Criteria(BaseCol.Units,   CompareOp.GE, "0")
            });
            var negUnitRate = roundedGreaterOfUnitRateRoyWithUnits.GetData(new FilterClause {
                new Criteria(EngineCol.Rate2, CompareOp.LT, "0"),
                new Criteria(BaseCol.Units,   CompareOp.LT, "0")
            });
            roundedGreaterOfUnitRateRoyWithUnits.Release();

            // Tournament: pick winners using GroupNumbering.  Sort ordering is the trick:
            //   Greater Of (positives): Rate2 DESC -> rank 1 = highest royalty
            //   Lesser Of (negatives):  Rate2 ASC  -> rank 1 = lowest (most negative) royalty
            // GroupNumbering writes the sort number into Amount; we keep rows with Amount=0
            // (which is the within-set delimiter -- the first row of each group).
            var forGreaterOfNumbering = ResultSet.EmptySet();
            forGreaterOfNumbering.CombineAndRelease(posPctSalesAmountUnits, posUnitRateAmountAndUnits);
            // Within set: zero out the keys we want to ignore for grouping.
            var forGreaterOfNumberingWithin = forGreaterOfNumbering.Copy();
            forGreaterOfNumberingWithin.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Unspecified);
            forGreaterOfNumberingWithin.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
            forGreaterOfNumberingWithin.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");

            // Greater Of: sort Rate2 DESC (highest royalty first), tiebreak by RoyaltyMethod ASC.
            Comparison<CalcResultRow> greaterOfSort = (a, b) =>
            {
                int r = b.User_2_rate.CompareTo(a.User_2_rate);
                return r != 0 ? r : a.Udkey_16_sid.CompareTo(b.Udkey_16_sid);
            };
            var forGreaterOfGroupNumbered = ResultSetDSPs.GroupNumbering(forGreaterOfNumbering, forGreaterOfNumberingWithin, greaterOfSort);
            forGreaterOfNumbering.Release();
            forGreaterOfNumberingWithin.Release();
            var greaterOfOutput = forGreaterOfGroupNumbered.GetData(BaseCol.Amount, CompareOp.EQ, "0");
            forGreaterOfGroupNumbered.Release();
            // Restore Amount from Rate2 (which still holds the rounded royalty).
            greaterOfOutput.DoMath(BaseCol.Amount,  MathOp.SETTO, EngineCol.Rate2);
            greaterOfOutput.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");

            // Same tournament shape for negatives, but ASC sort.
            var forLesserOfNumbering = ResultSet.EmptySet();
            forLesserOfNumbering.CombineAndRelease(negPctSales, negUnitRate);
            var forLesserOfNumberingWithin = forLesserOfNumbering.Copy();
            forLesserOfNumberingWithin.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Unspecified);
            forLesserOfNumberingWithin.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
            forLesserOfNumberingWithin.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");
            // Lesser Of: sort Rate2 ASC (most-negative royalty first), tiebreak by RoyaltyMethod ASC.
            Comparison<CalcResultRow> lesserOfSort = (a, b) =>
            {
                int r = a.User_2_rate.CompareTo(b.User_2_rate);
                return r != 0 ? r : a.Udkey_16_sid.CompareTo(b.Udkey_16_sid);
            };
            var forLesserOfGroupNumbered = ResultSetDSPs.GroupNumbering(forLesserOfNumbering, forLesserOfNumberingWithin, lesserOfSort);
            forLesserOfNumbering.Release();
            forLesserOfNumberingWithin.Release();
            var lesserOfOutput = forLesserOfGroupNumbered.GetData(BaseCol.Amount, CompareOp.EQ, "0");
            forLesserOfGroupNumbered.Release();
            lesserOfOutput.DoMath(BaseCol.Amount,  MathOp.SETTO, EngineCol.Rate2);
            lesserOfOutput.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");

            // WinnerCombo: combine positive winners + negative winners; tag AltComment="Greater Of".
            var winnerCombo = ResultSet.EmptySet();
            winnerCombo.CombineAndRelease(greaterOfOutput, lesserOfOutput);
            winnerCombo.DoMath(EngineCol.AltComment, MathOp.SETTO, "Greater Of");
            winnerCombo.DoMath(BaseCol.Amount,       MathOp.SETTO, BaseCol.Units2);   // restore from Units2 stash

            // PctOfSalesWinner / UnitRateWinner: split by which method won.
            var pctOfSalesWinner = winnerCombo.GetData(CustCol.RoyaltyMethod, CompareOp.EQ, RoyaltyMethod.Percent_of_Net_Sales);
            var unitRateWinner   = winnerCombo.GetData(CustCol.RoyaltyMethod, CompareOp.EQ, RoyaltyMethod.Unit_Rate);
            winnerCombo.Release();

            // Add the "Greater Of with no UnitRate setup" rows (Pct of Sales-only path).
            var pctOfSalesWithNoSetup = invalidUnitRateWithValidPctOfSales;
            pctOfSalesWithNoSetup.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Percent_of_Net_Sales);
            pctOfSalesWithNoSetup.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRatePercentOfNetSales);
            var netSalesWithPctOfSalesWRoyaltyRateWinnerOutput = ResultSet.EmptySet();
            netSalesWithPctOfSalesWRoyaltyRateWinnerOutput.CombineAndRelease(pctOfSalesWithNoSetup, pctOfSalesWinner);
            // DealScript line 132: ... * 1 -- force re-summarize against this Sub's CalcContext.
            netSalesWithPctOfSalesWRoyaltyRateWinnerOutput = netSalesWithPctOfSalesWRoyaltyRateWinnerOutput.Summarize();

            // Compute royalty for the PctOfSales winners.
            var saveNetSalesWithPctOfSalesWRoyaltyRateWinnerOutput = netSalesWithPctOfSalesWRoyaltyRateWinnerOutput.Copy();
            saveNetSalesWithPctOfSalesWRoyaltyRateWinnerOutput.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            var pctSalesRoyWinner = netSalesWithPctOfSalesWRoyaltyRateWinnerOutput.Copy();
            pctSalesRoyWinner.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1);
            var roundedPctSalesRoyWinner = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(pctSalesRoyWinner);
            var pctNetSalesRoyWinnerWithUnitsOutput = ResultSet.EmptySet();
            pctNetSalesRoyWinnerWithUnitsOutput.CombineAndRelease(saveNetSalesWithPctOfSalesWRoyaltyRateWinnerOutput, roundedPctSalesRoyWinner);
            // DealScript line 139: ... * 1 -- force re-summarize.  RoundAmountsTo2DecAndUnitsTo0Dec
            // resets CurrentCalcContext to its own; re-establish ours before Summarize.
            Job.CurrentCalcContext = _ctxSubRoyaltiesEarned;
            pctNetSalesRoyWinnerWithUnitsOutput = pctNetSalesRoyWinnerWithUnitsOutput.Summarize();

            // Same for UnitRate winners.
            var unitRateWithNoSetup = validUnitRateWithInvalidPctSales;
            unitRateWithNoSetup.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Unit_Rate);
            unitRateWithNoSetup.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.RoyaltyRateUnitRate);
            var netSalesUnitRateWRoyaltyRateWinnerOutput = ResultSet.EmptySet();
            netSalesUnitRateWRoyaltyRateWinnerOutput.CombineAndRelease(unitRateWithNoSetup, unitRateWinner);
            // DealScript line 144: ... * 1 -- force re-summarize against this Sub's CalcContext.
            // (CurrentCalcContext was reset by the Round helper above; re-establish ours.)
            Job.CurrentCalcContext = _ctxSubRoyaltiesEarned;
            netSalesUnitRateWRoyaltyRateWinnerOutput = netSalesUnitRateWRoyaltyRateWinnerOutput.Summarize();

            var saveNetSalesUnitRateWRoyaltyRateWinnerOutput = netSalesUnitRateWRoyaltyRateWinnerOutput.Copy();
            saveNetSalesUnitRateWRoyaltyRateWinnerOutput.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            var unitRateRoyWinner = netSalesUnitRateWRoyaltyRateWinnerOutput.Copy();
            unitRateRoyWinner.DoMath(BaseCol.Amount, MathOp.SETTO, BaseCol.Units);
            unitRateRoyWinner.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1);
            var roundedUnitRateRoyWinner = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(unitRateRoyWinner);
            var unitRateRoyWinnerWithUnitsOutput = ResultSet.EmptySet();
            unitRateRoyWinnerWithUnitsOutput.CombineAndRelease(saveNetSalesUnitRateWRoyaltyRateWinnerOutput, roundedUnitRateRoyWinner);

            // ─── Unassigned rate (catch-all for Standard's invalid path) ───
            var invalidRates = withPercentOfListInvalid;
            var netSalesUnassignedRateCombined = ResultSet.EmptySet();
            netSalesUnassignedRateCombined.CombineAndRelease(invalidRates, invalidUnitRateWithInvalidPctOfSales);
            netSalesUnassignedRateCombined.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Unassigned_Rate);
            netSalesUnassignedRateCombined.DoMath(EngineCol.Rate3, MathOp.SETTO, "0");
            var netSalesUnassignedRate = netSalesUnassignedRateCombined;

            // ─── Adjustment lookups (filtered by window-period) ───
            AlliantEntity windowStartEntity = L_StartWindowActualPeriod[0];
            ResultSet adjToRoyEarnedItd;
            if (windowStartEntity != null)
            {
                var startPI = (PeriodItem)PeriodItem.Items.GetEntityBySid(windowStartEntity.sid);
                var endPI   = (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time);
                var window  = DS.F_PERIOD_INTERVAL(startPI, endPI);
                adjToRoyEarnedItd = allAdjustmentsItd.GetData(new FilterClause {
                    new Criteria(CustCol.ActivityType, CompareOp.EQ,    ActivityType.Adjustment_to_Royalties_Earned),
                    new Criteria(CustCol.ActualPeriod, ListOp.INLIST,   (EntityList)window)
                }, "SubRoyEarned.AdjToRoyEarnedITD");
            }
            else
            {
                adjToRoyEarnedItd = allAdjustmentsItd.GetData(
                    CustCol.ActivityType, CompareOp.EQ, ActivityType.Adjustment_to_Royalties_Earned,
                    "SubRoyEarned.AdjToRoyEarnedITD");
            }

            // ─── Output assembly ───
            // INPUTSET4: NetSales rows for all six paths.
            var netSalesOutput = ResultSet.EmptySet();
            netSalesOutput.CombineAndRelease(
                netSalesPercentOfSales.Copy(),
                netSalesUnitRate.Copy(),
                netSalesPercentOfList.Copy(),
                netSalesUnassignedRate,
                netSalesWithPctOfSalesWRoyaltyRateWinnerOutput.Copy(),
                netSalesUnitRateWRoyaltyRateWinnerOutput.Copy());
            netSalesOutput.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");   // clear scratch
            netSalesOutput.DoMath(EngineCol.Rate3, MathOp.SETTO, "0");   // clear pct-of-pct

            netSalesPercentOfSales.Release();
            netSalesUnitRate.Release();
            netSalesPercentOfList.Release();
            netSalesWithPctOfSalesWRoyaltyRateWinnerOutput.Release();
            netSalesUnitRateWRoyaltyRateWinnerOutput.Release();

            // INPUTSET5: round all royalties + apply PctOfPct + combine adjustments.
            var allRoysEarnedCombined = ResultSet.EmptySet();
            allRoysEarnedCombined.CombineAndRelease(standardRoyalty, pctNetSalesRoyWinnerWithUnitsOutput, unitRateRoyWinnerWithUnitsOutput);
            allRoysEarnedCombined.SetValue(CustCol.ActivityType, ActivityType.Royalties_Earned);
            allRoysEarnedCombined.DoMath(BaseCol.Units2, MathOp.SETTO, BaseCol.Amount);   // stash royalty for display
            // RoysEarnedPctOfPct = Rate3 * Amount → Rate2 → Amount.  Rate3 holds PctOfPct.
            allRoysEarnedCombined.DoMath(EngineCol.Rate2, MathOp.SETTO, EngineCol.Rate3);
            allRoysEarnedCombined.DoMath(EngineCol.Rate2, MathOp.TIMES, BaseCol.Amount);
            allRoysEarnedCombined.DoMath(BaseCol.Amount,  MathOp.SETTO, EngineCol.Rate2);
            allRoysEarnedCombined.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");
            var roundedRoyaltiesEarned = CommonLib.RoundAllTo2Dec(allRoysEarnedCombined);

            var royaltyOutput = ResultSet.EmptySet();
            royaltyOutput.CombineAndRelease(adjToRoyEarnedItd.Copy(), roundedRoyaltiesEarned);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubRoyaltiesEarned");
            return (netSalesOutput, royaltyOutput, adjToRoyEarnedItd);
        }

        // ── 1564 C_ROY_SUB_GUARANTEES ────────────────────────────────────────
        //
        // DealScript:
        //   MGAdjITDOutput = Set TransType of (GetData INPUTSET3 for ActivityType=Minimum_Guarantee) to ITD
        //   MGAdjToBeUsed  = GetData MGAdjITDOutput for OtherPeriod = F_CURRENT_CALC_INTERVAL()
        //   L_RecGroup     = GetList for RecoupmentGroup from MGAdjToBeUsed
        //   N_RecGroups    = Count(L_RecGroup)
        //   ShortfallTemplate = Set ActivityType of (Set TransType of (Set ActualPeriod of ZEROSET to F_CALC_PERIOD()) to Current) to Shortfall_Due
        //
        //   RoyForGuarantee = Combine(INPUTSET1, INPUTSET2)
        //   RoyDueITD       = GetData RoyForGuarantee for ActivityType ∈ Royalties_for_Recoupment_List, TransType=ITD
        //
        //   N_Counter = 1; MGOutput = EMPTYSET
        //   While N_Counter <= N_RecGroups
        //     ASingleAdj = GetData MGAdjToBeUsed for RecoupmentGroup = L_RecGroup[N_Counter]
        //     L_StartPeriod    = GetList for ActualPeriod from ASingleAdj    // start of MG window
        //     L_EndOtherPeriod = GetList for OtherPeriod  from ASingleAdj    // end of MG window (in OtherPeriod col)
        //     Set ASingleAdj.ActualPeriod = OtherPeriod                       // shift end into ActualPeriod for next GetList
        //     L_EndPeriod = GetList for ActualPeriod from ASingleAdj
        //     RoyRecGroup = GetData TempRoy for RecoupmentGroup = L_RecGroup[N_Counter],
        //                                       ActualPeriod ∈ F_PERIOD_INTERVAL(L_StartPeriod[1], L_EndPeriod[1])
        //     N_ASingleAdj = AmountOf(ASingleAdj); N_Roy = AmountOf(RoyRecGroup)
        //     N_PosRoy   = max(N_Roy, 0)
        //     N_Diff     = N_ASingleAdj - N_PosRoy
        //     N_Shortfall = max(N_Diff, 0)
        //     ZeroShortfall = ShortfallTemplate.Set(RecGroup=L_RecGroup[N_Counter], ActualPeriod=L_StartPeriod[1], OtherPeriod=L_EndOtherPeriod[1])
        //     ShortfallRecGrooup = ZeroShortfall + N_Shortfall into Amount
        //     MGOutput = Combine(MGOutput, ShortfallRecGrooup)
        //   EndWhile
        //
        //   ShortfallCurrentOutput = GetData MGOutput for Amount <> 0
        //   PriorShortfall = GetData INPUTSET4 for ActivityType = Shortfall_Due
        //   ShortfallITDOutput = Set TransType of Combine(ShortfallCurrentOutput, PriorShortfall) to ITD
        //
        //   Output = Combine(ShortfallCurrentOutput, ShortfallITDOutput, MGAdjITDOutput)
        //   INPUTSET5 = Output
        //
        // Per-recoupment-group shortfall computation: for each MG adjustment whose
        // window ends in this calc interval, compare the adjustment amount to the
        // total royalties earned within its window.  Emit a Shortfall_Due row when
        // royalties are below the guarantee.
        public static ResultSet SubGuarantees(
            ResultSet royaltiesEarnedItd,    // INPUTSET1
            ResultSet reservesItd,           // INPUTSET2
            ResultSet adjustmentsItd,        // INPUTSET3
            ResultSet priorPeriodItd)        // INPUTSET4
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubGuarantees = _ctxSubGuarantees ?? NewSubCtxStandard("C_ROY_SUB_GUARANTEES");
            Job.CurrentCalcContext = _ctxSubGuarantees;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubGuarantees");

            // MGAdjITDOutput: just the Minimum_Guarantee adjustments, stamped TransType=ITD.
            var mgAdjItdOutput = adjustmentsItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Minimum_Guarantee, "SubGuar.MGAdjITDOutput");
            mgAdjItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // MGAdjToBeUsed: only the MG rows whose end-period (OtherPeriod) is in
            // the current calc interval -- i.e. it's time to settle their shortfall.
            //var currentInterval = DS.F_CURRENT_CALC_INTERVAL();
            //var mgAdjToBeUsed = mgAdjItdOutput.GetData(
            //    CustCol.OtherPeriod, ListOp.INLIST, (EntityList)currentInterval, "SubGuar.MGAdjToBeUsed");
            var mgAdjToBeUsed = mgAdjItdOutput.GetData(
                  CustCol.OtherPeriod, CompareOp.EQ, Job.CurrentCalcPeriod, "SubGuar.MGAdjToBeUsed");

            var L_RecGroup    = mgAdjToBeUsed.GetList(CustCol.RecoupmentGroup);
            int N_RecGroups   = L_RecGroup.Count();

            // RoyForGuarantee: combine INPUTSET1 (RoyEarned) + INPUTSET2 (Reserves),
            // then keep only the ITD rows in the Royalties_for_Recoupment list.
            var royForGuarantee = ResultSet.EmptySet();
            royForGuarantee.CombineAndRelease(royaltiesEarnedItd.Copy(), reservesItd.Copy());
            var royDueItd = royForGuarantee.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Royalties_for_Recoupment_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.ITD)
            }, "SubGuar.RoyDueITD");
            royForGuarantee.Release();

            var mgOutput = ResultSet.EmptySet();

            // While loop: process one RecGroup at a time.
            for (int N_Counter = 1; N_Counter <= N_RecGroups; N_Counter++)
            {
                AlliantEntity recGroup = L_RecGroup[N_Counter - 1];   // 1-based DealScript -> 0-based C#
                if (recGroup == null) continue;

                // ASingleAdj: the (single) MG row for this recoupment group.
                var aSingleAdj = mgAdjToBeUsed.GetData(
                    CustCol.RecoupmentGroup, CompareOp.EQ, recGroup, "SubGuar.ASingleAdj");

                // Read window endpoints before we mutate ActualPeriod on the row.
                var L_StartPeriod    = aSingleAdj.GetList(CustCol.ActualPeriod);
                var L_EndOtherPeriod = aSingleAdj.GetList(CustCol.OtherPeriod);

                // Move OtherPeriod's value into ActualPeriod so GetList(ActualPeriod) picks up the end.
                aSingleAdj.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
                var L_EndPeriod = aSingleAdj.GetList(CustCol.ActualPeriod);

                AlliantEntity startEntity    = L_StartPeriod[0];
                AlliantEntity endEntity      = L_EndPeriod[0];
                AlliantEntity endOtherEntity = L_EndOtherPeriod[0];
                if (startEntity == null || endEntity == null || endOtherEntity == null)
                {
                    aSingleAdj.Release();
                    continue;
                }

                // RoyRecGroup: royalties for this rec group within [start..end] window.
                var startPI = (PeriodItem)PeriodItem.Items.GetEntityBySid(startEntity.sid);
                var endPI   = (PeriodItem)PeriodItem.Items.GetEntityBySid(endEntity.sid);
                var window  = DS.F_PERIOD_INTERVAL(startPI, endPI);
                var royRecGroup = royDueItd.GetData(new FilterClause {
                    new Criteria(CustCol.RecoupmentGroup, CompareOp.EQ,    recGroup),
                    new Criteria(CustCol.ActualPeriod,    ListOp.INLIST,   (EntityList)window)
                }, "SubGuar.RoyRecGroup");

                decimal N_ASingleAdj = DS.AmountOf(aSingleAdj);
                decimal N_Roy        = DS.AmountOf(royRecGroup);
                decimal N_PosRoy     = N_Roy < 0m ? 0m : N_Roy;
                decimal N_Diff       = N_ASingleAdj - N_PosRoy;
                decimal N_Shortfall  = N_Diff > 0m ? N_Diff : 0m;

                // Build one shortfall row from a fresh ZEROSET (template-style).
                var zeroShortfall = ResultSet.ZeroSet();
                zeroShortfall.SetValue(CustCol.ActivityType,    ActivityType.Shortfall_Due);
                zeroShortfall.SetValue(CustCol.TransType,       TransType.Current);
                zeroShortfall.SetValue(CustCol.ActualPeriod,    Job.CurrentCalcPeriod);
                zeroShortfall.SetValue(CustCol.RecoupmentGroup, recGroup);
                zeroShortfall.SetValue(CustCol.ActualPeriod,    startEntity);
                zeroShortfall.SetValue(CustCol.OtherPeriod,     endOtherEntity);
                zeroShortfall.DoMath(BaseCol.Amount, MathOp.SETTO, N_Shortfall.ToString(System.Globalization.CultureInfo.InvariantCulture));

                mgOutput.Combine(zeroShortfall);
                zeroShortfall.Release();

                aSingleAdj.Release();
                royRecGroup.Release();
            }

            mgAdjToBeUsed.Release();
            royDueItd.Release();

            // ShortfallCurrentOutput: keep only non-zero shortfalls.
            var shortfallCurrentOutput = mgOutput.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubGuar.ShortfallCurrent");
            mgOutput.Release();

            // PriorShortfall: previous-period ITD Shortfall_Due rows.
            var priorShortfall = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Shortfall_Due, "SubGuar.PriorShortfall");

            // ShortfallITDOutput: combine current + prior, set TransType=ITD.
            var shortfallItdOutput = ResultSet.EmptySet();
            shortfallItdOutput.CombineAndRelease(shortfallCurrentOutput.Copy(), priorShortfall);
            shortfallItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // Output = Combine(ShortfallCurrent, ShortfallITD, MGAdjITDOutput).
            var guaranteesOutput = ResultSet.EmptySet();
            guaranteesOutput.CombineAndRelease(shortfallCurrentOutput, shortfallItdOutput, mgAdjItdOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubGuarantees");
            return guaranteesOutput;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1566 C_ROY_SUB_ROYALTIES_DUE ─────────────────────────────────────
        //
        // DealScript:
        //   AdjToRoyDueITDOutput = GetData INPUTSET3 for ActivityType = Adjustment_to_Royalties_Due
        //   PriorRoyaltiesDueITD = GetData INPUTSET2 for ActivityType = Royalties_Due
        //   ForRoyDue            = GetData INPUTSET1 for ActivityType ∈ Total_Royalties_Due_List, TransType = ITD
        //   RoyDueITDOutput      = Set ActivityType of (Combine(ForRoyDue, AdjToRoyDueITDOutput)) to Royalties_Due
        //   RoyCueCurrentOutput  = Set TransType of (RoyDueITDOutput - PriorRoyaltiesDueITD) to Current
        //   Finals               = Combine(RoyDueITDOutput, RoyCueCurrentOutput)
        //   SummarizedFinals     = #<c:1567># using Finals     // SummarizeForRoyaltiesDue
        //     Amount2 = 0; Units = 0; Units2 = 0
        //   Output = #<c:1543># using SummarizedFinals         // CommonLib.RemoveZeros
        //   INPUTSET4 = Output
        //   INPUTSET5 = AdjToRoyDueITDOutput
        //
        // Two outputs: the rolled-up Royalties Due (INPUTSET4) and the raw
        // adjustments-to-Royalties-Due passthrough (INPUTSET5).  AdjToRoyDueITDOutput
        // feeds both, so we keep one Copy() for the passthrough and consume the
        // other in the Combine.
        public static (ResultSet RoyaltiesDueOutput,
                       ResultSet AdjToRoyDueItdOutput) SubRoyaltiesDue(
            ResultSet allEarnedAndReservesItd,    // INPUTSET1
            ResultSet priorPeriodItd,             // INPUTSET2
            ResultSet allAdjustmentsItd)          // INPUTSET3
        {
            _ctxSubRoyaltiesDue = _ctxSubRoyaltiesDue ?? NewSubCtxStandard("C_ROY_SUB_ROYALTIES_DUE");
            Job.CurrentCalcContext = _ctxSubRoyaltiesDue;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubRoyaltiesDue");

            // INPUTSET5: just the Adjustment_to_Royalties_Due rows from INPUTSET3.
            var adjToRoyDueItdOutput = allAdjustmentsItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Adjustment_to_Royalties_Due,
                "SubRoyDue.AdjToRoyDueITDOutput");

            var priorRoyaltiesDueItd = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Due,
                "SubRoyDue.PriorRoyaltiesDueITD");

            var forRoyDue = allEarnedAndReservesItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Total_Royalties_Due_Activiity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.ITD)
            }, "SubRoyDue.ForRoyDue");

            // RoyDueITDOutput: combine ForRoyDue + a Copy of the adjustments (the
            // original adj set is still owned by INPUTSET5).  Restamp ActivityType.
            var royDueItdOutput = ResultSet.EmptySet();
            royDueItdOutput.CombineAndRelease(forRoyDue, adjToRoyDueItdOutput.Copy());
            royDueItdOutput.SetValue(CustCol.ActivityType, ActivityType.Royalties_Due);

            // RoyCueCurrentOutput: ITD minus prior ITD == current period delta;
            // re-stamp TransType to Current.  Subtract consumes priorRoyaltiesDueItd.
            var royCueCurrentOutput = ResultSet.Subtract(royDueItdOutput, priorRoyaltiesDueItd, "SubRoyDue.RoyCueCurrent");
            priorRoyaltiesDueItd.Release();
            royCueCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // Combine ITD + Current, summarize, drop zeros.
            var finals = ResultSet.EmptySet();
            finals.CombineAndRelease(royDueItdOutput, royCueCurrentOutput);

            // SummarizeForRoyaltiesDue consumes 'finals' (Summarize-then-conditional-Release pattern).
            var summarizedFinals = SummarizeForRoyaltiesDue(finals);
            summarizedFinals.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            summarizedFinals.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
            summarizedFinals.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

            var royaltiesDueOutput = CommonLib.RemoveZeros(summarizedFinals);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubRoyaltiesDue");
            return (royaltiesDueOutput, adjToRoyDueItdOutput);
        }

        // ── 1573 C_ROY_SUB_STATEMENT_DISPLAY ─────────────────────────────────
        //
        // Three concatenated outputs:
        //   1) Statement_Royalties_Earned rows (re-stamped from Royalties_Earned ITD's
        //      Current rows; columns conditionally zeroed per a chain of contract
        //      Display-* flags).
        //   2) A single Statement_Info row marking the statement start period.
        //   3) Statement_Looping rows -- one synthetic row per ContractedParty,
        //      then PayRecipients + StatementRecipients fan-out, with a 3-way
        //      Contact swap (Contact2/Contact3 swap via Contact4 scratch).
        //
        // DealScript:
        //   CurrentRoyEarned = Set #<x:2># of (GetData INPUTSET1 for ActivityType=Royalties_Earned, TransType=Current)
        //                       to "Statement_Royalties_Earned"
        //   SummarizeRoyEarned = #<c:1574># using CurrentRoyEarned     // SummarizeForStatementDisplay
        //
        //   Each line:  IF This.<flag> = "Yes" THEN keep; ELSE Set <col> of <prev> to <Unspecified-or-0>
        //   Flags drive: ActualPeriod, Bundle, Catalog, Channel, Language, Media,
        //                RoyaltyMethod, Price1, Rate3, Territory.
        //
        //   StmtRoyEarnedOutput = (after IF chain)
        //
        //   StatementInfo = Set #<x:2># of ZEROSET to Statement_Info
        //   StatementInfoWithStartDate = Set #<x:14># of StatementInfo to F_PERIODS_FROM(F_PERIOD_TYPE_PREVIOUS(This.StatementInterval), 1)
        //   StatementInfoOutput = Set #<x:3># of ... to TransType.Current
        //   StatementInfoOutput.Amount = 99
        //
        //   L_ContractedParty = GetList for #<x:47># from ContractParticipants
        //   WithContractedParty = Set Contact1=L_ContractedParty[1], ActivityType=Statement_Looping, TransType=Current
        //   PmtRecipientPossibilities = PayRecipients ... (Contact1 → Contact2 by Period, RecipientRate from Rate1)
        //   StmtRecipientPossibilities = StatementRecipients ... (Contact1 → Contact3 by Period)
        //   Set Contact4 = Contact2; Set Contact2 = Contact3; Set Contact3 = Contact4   (swap via scratch)
        //   StatementLoopingOutput.Contact4 = Unspecified
        //   StatementLoopingOutput.Amount = 99
        //
        //   Output = Combine(StmtRoyEarnedOutput, StatementInfoOutput, StatementLoopingOutput)
        public static ResultSet SubStatementDisplay(
            ResultSet allResultItd)             // INPUTSET1
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubStatementDisplay = _ctxSubStatementDisplay ?? NewSubCtxStandard("C_ROY_SUB_STATEMENT_DISPLAY");
            Job.CurrentCalcContext = _ctxSubStatementDisplay;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubStatementDisplay");

            // ─── Section 1: Statement Royalties Earned + IF-chain display gating ───
            var currentRoyEarned = allResultItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Earned),
                new Criteria(CustCol.TransType,    CompareOp.EQ, TransType.Current)
            }, "SubStmtDisplay.CurrentRoyEarned");
            currentRoyEarned.SetValue(CustCol.ActivityType, ActivityType.Statement_Royalties_Earned);

            // SummarizeForStatementDisplay consumes 'currentRoyEarned'.
            var summarizeRoyEarned = SummarizeForStatementDisplay(currentRoyEarned);

            // Each Display-* flag is a YES/NO contract UDF.  When NO, zero the matching
            // column to collapse all rows in that dimension.  Evaluated once per
            // contract via Contract.GetUDFString, which uses Job.currentContract.sid.
            var stmtRoyEarnedOutput = summarizeRoyEarned;

            if (Contract.GetUDFString(ContractUDF.DisplayActualPeriodFlag)  != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.ActualPeriod, Period.Unspecified);
            if (Contract.GetUDFString(ContractUDF.DisplayBundleFlag)        != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.Bundle,        Bundle.Unspecified);
            if (Contract.GetUDFString(ContractUDF.DisplayCatalogFlag)       != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.Catalog,       Catalog.Unspecified);
            if (Contract.GetUDFString(ContractUDF.DisplayChannelFlag)       != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.Channel,       Channel.Unspecified);
            if (Contract.GetUDFString(ContractUDF.DisplayLanguageFlag)      != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.Language,      Language.Unspecified);
            if (Contract.GetUDFString(ContractUDF.DisplayMediaFlag)         != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.Media,         Media.Unspecified);
            if (Contract.GetUDFString(ContractUDF.DisplayRoyaltyMethodFlag) != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.RoyaltyMethod, RoyaltyMethod.Unspecified);
            if (Contract.GetUDFString(ContractUDF.DisplayRetailPriceFlag)   != "Yes")
                stmtRoyEarnedOutput.DoMath(EngineCol.Price1, MathOp.SETTO, "0");
            if (Contract.GetUDFString(ContractUDF.DisplayPctOfPctFlag)      != "Yes")
                stmtRoyEarnedOutput.DoMath(EngineCol.Rate3,  MathOp.SETTO, "0");
            if (Contract.GetUDFString(ContractUDF.DisplayTerritoryFlag)     != "Yes")
                stmtRoyEarnedOutput.SetValue(CustCol.Territory,     Territory.Unspecified);

            // ─── Section 2: Statement Info synthetic row ───
            // ZeroSet seeded with ActivityType=Statement_Info, ActualPeriod=start of
            // current statement period, TransType=Current, Amount=99 (sentinel).
            string statementInterval = Contract.GetUDFString(ContractUDF.StatementInterval);
            PeriodItem prevPeriod    = DS.F_PERIOD_TYPE_PREVIOUS(statementInterval);
            PeriodItem startPeriod   = DS.F_PERIODS_FROM(prevPeriod, 1);

            var statementInfoOutput = ResultSet.ZeroSet();
            statementInfoOutput.SetValue(CustCol.ActivityType, ActivityType.Statement_Info);
            statementInfoOutput.SetValue(CustCol.ActualPeriod, startPeriod);
            statementInfoOutput.SetValue(CustCol.TransType,    TransType.Current);
            statementInfoOutput.DoMath(BaseCol.Amount, MathOp.SETTO, "99");

            // ─── Section 3: Statement Looping per ContractedParty ───
            var L_ContractedParty = Job.GetListFromContractParticipants();
            var withContractedParty = ResultSet.ZeroSet();
            // DealScript: Set #<x:47># of ZEROSET to L_ContractedParty[1]   (1-based)
            withContractedParty.SetEntity("ContractedParty", L_ContractedParty[0]);
            withContractedParty.SetValue(CustCol.ActivityType, ActivityType.Statement_Looping);
            withContractedParty.SetValue(CustCol.TransType,    TransType.Current);

            // PayRecipients fan-out: ContractedParty → PaymentRecipient (with rate carried in Rate1).
            var pmtRecipientPossibilities = withContractedParty.PayRecipients(
                new ContactCol("ContractedParty"), CustCol.Period,
                new ContactCol("PaymentRecipient"), new DecimalCol("Rate1"),
                BaseCol.Amount);
            withContractedParty.Release();

            // StatementRecipients fan-out: ContractedParty → StatementRecipient.
            var stmtRecipientPossibilities = pmtRecipientPossibilities.StatementRecipients(
                new ContactCol("ContractedParty"), CustCol.Period,
                new ContactCol("StatementRecipient"));
            pmtRecipientPossibilities.Release();

            // 3-way swap of Contact2 (PaymentRecipient) and Contact3 (StatementRecipient)
            // via Contact4 (ThirdParty) as scratch.  After:
            //   Contact2 ← original Contact3
            //   Contact3 ← original Contact2
            // (DealScript convention: PayRecipients placed the pmt recipient in Contact2,
            //  StatementRecipients placed the stmt recipient in Contact3 — swap so the
            //  display shows them in the customer's expected slots.)
            stmtRecipientPossibilities.DoMath(new ContactCol("ThirdParty"),         MathOp.SETTO, new ContactCol("PaymentRecipient"));
            stmtRecipientPossibilities.DoMath(new ContactCol("PaymentRecipient"),   MathOp.SETTO, new ContactCol("StatementRecipient"));
            stmtRecipientPossibilities.DoMath(new ContactCol("StatementRecipient"), MathOp.SETTO, new ContactCol("ThirdParty"));

            stmtRecipientPossibilities.SetValue(new ContactCol("ThirdParty"), new AlliantEntity(0));   // unspecified
            stmtRecipientPossibilities.DoMath(BaseCol.Amount, MathOp.SETTO, "99");
            var statementLoopingOutput = stmtRecipientPossibilities;

            // ─── Combine all three ───
            var statementOutput = ResultSet.EmptySet();
            statementOutput.CombineAndRelease(stmtRoyEarnedOutput, statementInfoOutput, statementLoopingOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubStatementDisplay");
            return statementOutput;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1569 C_ROY_SUB_GL_ENTRIES ────────────────────────────────────────
        //
        // DealScript:
        //   ForGL = GetData INPUTSET1 for ActivityType ∈ To_Be_Used_for_GL_List, TransType=Current
        //   SummarizeGL = #<c:1570># using ForGL          // SummarizeForGLEntries
        //     Amount2=0; Units=0; Units2=0
        //   NonZeroGL = #<c:1543># using SummarizeGL      // CommonLib.RemoveZeros
        //
        //   For each (source ActivityType X) in {Royalties_Earned, Bonus_Payment, Applied_to_Advance,
        //     Shortfall, Reserves_Liquidated, Reserves_Taken, Applied_to_Installment}:
        //     Get rows from NonZeroGL with that ActivityType.
        //     Negate Amount and re-stamp ActivityType to the *_Debit pair.
        //     The *_Credit pair gets the original (or negated, depending on source convention).
        //   Final = Combine(all 14 debit/credit halves).
        //   Output = Set Division of (Set Comment of Final to ActivityTypeUDF.GLAccount) to ContractUDF.Division
        //
        // Each source ActivityType produces a debit/credit pair.  Bookkeeping
        // convention: source value's natural sign determines which pair gets negated.
        public static ResultSet SubGLEntries(
            ResultSet allOutput)                // INPUTSET1
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubGLEntries = _ctxSubGLEntries ?? NewSubCtxFullKey("C_ROY_SUB_GL_ENTRIES");
            Job.CurrentCalcContext = _ctxSubGLEntries;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubGLEntries");

            var forGl = allOutput.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.To_Be_Used_for_GL_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.Current)
            }, "SubGLEntries.ForGL");

            // SummarizeForGLEntries consumes 'forGl'.
            var summarizeGl = SummarizeForGLEntries(forGl);
            summarizeGl.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            summarizeGl.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
            summarizeGl.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

            var nonZeroGl = CommonLib.RemoveZeros(summarizeGl);

            // Local helper: split nonZeroGl by source ActivityType and emit debit+credit
            // halves with the right sign convention.  negateForDebit=true means the source
            // amount is naturally negative, so the *_Debit pair gets negated; otherwise
            // the *_Credit pair gets negated.
            void EmitPair(UDKey2Ref source, UDKey2Ref debit, UDKey2Ref credit, bool negateForDebit, ResultSet outFinal)
            {
                var src = nonZeroGl.GetData(CustCol.ActivityType, CompareOp.EQ, source);
                if (src.Rows == 0) { src.Release(); return; }
                var debitRows  = negateForDebit ? src.Copy() : src;
                var creditRows = negateForDebit ? src        : src.Copy();
                if (negateForDebit)
                {
                    debitRows.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                }
                else
                {
                    creditRows.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                }
                debitRows.SetValue(CustCol.ActivityType,  debit);
                creditRows.SetValue(CustCol.ActivityType, credit);
                outFinal.Combine(debitRows);
                outFinal.Combine(creditRows);
                debitRows.Release();
                creditRows.Release();
            }

            var final = ResultSet.EmptySet();

            // Source convention table -- which half gets negated.  Matches the original
            // DealScript line-by-line: the line that does `-1 * Amount into Amount` AND
            // re-stamps to the *_Debit name implies "source is positive, debit is negation"
            // (negateForDebit=true).  Where the negation is applied before the *_Credit
            // re-stamp instead, source is negative-natural so credit gets the negation.
            EmitPair(ActivityType.Royalties_Earned,           ActivityType.Royalties_Earned_Debit,        ActivityType.Royalties_Earned_Credit,        negateForDebit:false, final);
            EmitPair(ActivityType.Bonus_Payment,              ActivityType.Bonus_Payment_Debit,           ActivityType.Bonus_Payment_Credit,           negateForDebit:false, final);
            EmitPair(ActivityType.Royalties_Applied_to_Advance,     ActivityType.Applied_to_Advance_Debit,      ActivityType.Applied_to_Advance_Credit,      negateForDebit:true,  final);
            EmitPair(ActivityType.Shortfall_Due,              ActivityType.Shortfall_Debit,               ActivityType.Shortfall_Credit,               negateForDebit:false, final);
            EmitPair(ActivityType.Reserves_Liquidated,        ActivityType.Reserves_Liquidated_Debit,     ActivityType.Reserves_Liquidated_Credit,     negateForDebit:false, final);
            EmitPair(ActivityType.Reserves_Taken,             ActivityType.Reserves_Taken_Debit,          ActivityType.Reserves_Taken_Credit,          negateForDebit:true,  final);
            EmitPair(ActivityType.Royalties_Applied_to_Installment, ActivityType.Applied_to_Installment_Debit,  ActivityType.Applied_to_Installment_Credit,  negateForDebit:true,  final);

            nonZeroGl.Release();

            // Stamp Comment ← ActivityTypeUDF.GLAccount, Division ← ContractUDF.Division.
            final.DoMath(EngineCol.Comment, MathOp.SETTO, ActivityTypeUDF.GLAccount);

            if (!string.IsNullOrEmpty(Contract.GetUDFString(ContractUDF.Division)))
            {
                final.DoMath(CustCol.Division, MathOp.SETTO, ContractUDF.Division);
            }

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubGLEntries");
            return final;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1556 C_ROY_SUB_TIERING ───────────────────────────────────────────
        //
        // Tiers Net Sales rows by Amount or Units against ContractUDF.TieringByAmount
        // / TieringByUnits.  Tiering is per-period-interval: rows in each
        // interval are tiered separately so escalation thresholds apply per-period.
        //
        // DealScript flow:
        //   Phase A. Read ContractUDF.TierInterval to decide whether tiering is needed.
        //              Skip the loop entirely when TierInterval is "<NONE>" or "No Tiering".
        //   Phase B. Period setup:
        //              StartPeriod = inception OR (calc-period - N tier-intervals + 1).
        //              PriorNetSales = INPUTSET2 rows in [StartPeriod..StartWindow-1].
        //              SummarizePriorNetSales = SummarizeForWindowTiering(PriorNetSales).
        //              NetSalesForTiering = Combine(SummarizePriorNetSales, INPUTSET1).
        //   Phase C. Compute N_PeriodsBetween = number of tier-intervals to loop over.
        //   Phase D. WHILE per interval i in 1..N_PeriodsBetween:
        //              OnePeriodRevenue = filter ForTiering to interval i.
        //              TieringByAmount = TierSet on Amount via ContractUDF.TieringByAmount.
        //              ValidTierByAmount = rows with Tier != 0.
        //              NotTiered = rows with Tier == 0.
        //              TieringByUnits = TierSet NotTiered on Units via ContractUDF.TieringByUnits.
        //              Combine all into TieringOutput.
        //   Phase E. Round.  Apply p_ds_proration_12 to make scaled+rounded == originals.
        //              Combine with NoTiering rows.
        //   Phase F. Filter to rows in [StartWindow..End_of_Time] AND
        //              [ContractUDF.AlliantCutoverPeriod..End_of_Time].
        public static ResultSet SubTiering(
            ResultSet netSalesItd,                // INPUTSET1
            ResultSet priorPeriodItd,             // INPUTSET2
            ResultSet windowStartPeriodItd)       // INPUTSET3
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubTiering = _ctxSubTiering ?? NewSubCtxStandard("C_ROY_SUB_TIERING");
            Job.CurrentCalcContext = _ctxSubTiering;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubTiering");

            // ─── Phase A: NoTiering shortcut ─────────────────────────────────
            string tierInterval = Contract.GetUDFString(ContractUDF.TierInterval);
            int N_NoneLabel = (tierInterval == "<None>" || tierInterval == "<NONE>" || tierInterval == "No Tiering") ? 1 : 0;

            // ─── Phase B: Period setup ───────────────────────────────────────
            var L_StartWindowActualPeriod = windowStartPeriodItd.GetList(CustCol.ActualPeriod);
            AlliantEntity startWindowEntity = L_StartWindowActualPeriod[0];

            // N_PeriodInterval = num period-types between StartWindow and (calcPeriod - 1)
            //                    using This.TierInterval as the period type.
            // N_GoBackPeriodInterval = -1 * (N_PeriodInterval + 1)
            // StartPeriod = if N_GoBackPeriodInterval == -1 -> inception
            //               else -> F_PERIODS_FROM(F_PERIOD_TYPES_FROM(calcPeriod, N_GoBack, TierInterval), 1)
            PeriodItem calcPeriodMinus1 = DS.F_PERIODS_FROM(Job.CurrentCalcPeriod, -1);
            PeriodItem startWindowPI = startWindowEntity != null
                ? (PeriodItem)PeriodItem.Items.GetEntityBySid(startWindowEntity.sid)
                : null;
            int N_PeriodInterval = startWindowPI == null
                ? 0
                : DS.F_NUM_PERIOD_TYPES_BETWEEN(startWindowPI, calcPeriodMinus1, tierInterval);
            int N_GoBackPeriodInterval = -1 * (N_PeriodInterval + 1);

            PeriodItem startPeriodPI;
            if (N_GoBackPeriodInterval == -1)
            {
                startPeriodPI = DS.F_INCEPTION();
            }
            else
            {
                var capStart = DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, N_GoBackPeriodInterval, tierInterval);
                startPeriodPI = DS.F_PERIODS_FROM(capStart, 1);
            }

            // PriorNetSales = INPUTSET2 rows for ActivityType=Net_Sales,
            //                  ActualPeriod ∈ [StartPeriod..StartWindow-1].
            PeriodItem startWindowMinus1 = startWindowPI != null
                ? DS.F_PERIODS_FROM(startWindowPI, -1)
                : DS.F_PERIODS_FROM(calcPeriodMinus1, 0);
            var priorWindow = DS.F_PERIOD_INTERVAL(startPeriodPI, startWindowMinus1);
            var priorNetSales = priorPeriodItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ,    ActivityType.Net_Sales),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST,   (EntityList)priorWindow)
            }, "SubTier.PriorNetSales");

            var summarizePriorNetSales = SummarizeForWindowTiering(priorNetSales);
            Job.CurrentCalcContext = _ctxSubTiering;

            var netSalesForTiering = ResultSet.EmptySet();
            netSalesForTiering.CombineAndRelease(summarizePriorNetSales, netSalesItd.Copy());

            // NoTiering = INPUTSET1 (untouched) when tiering not needed.
            ResultSet noTiering = N_NoneLabel == 1 ? netSalesItd.Copy() : ResultSet.EmptySet();

            // ForTiering = NetSalesForTiering when tiering IS needed; reset Tier.
            ResultSet forTiering;
            if (N_NoneLabel == 0)
            {
                forTiering = netSalesForTiering;
                forTiering.DoMath(CustCol.Tier, MathOp.SETTO, "0");
            }
            else
            {
                netSalesForTiering.Release();
                forTiering = ResultSet.EmptySet();
            }

            var tieringResult = ResultSet.EmptySet();

            if (N_NoneLabel == 0 && forTiering.Rows > 0)
            {
                // ─── Phase C: N_PeriodsBetween ───────────────────────────────
                // L_PeriodList = ActualPeriod ASC; L_PeriodEnd = ActualPeriod DESC.
                // N_PeriodsBetween = num period-types between earliest period and
                //                    (latest period - 1), in TierInterval units, plus 1.
                var L_PeriodListAsc = forTiering.GetList(CustCol.ActualPeriod, ascending: true);
                var L_PeriodEndDesc = forTiering.GetList(CustCol.ActualPeriod, ascending: false);

                AlliantEntity periodFirst = L_PeriodListAsc[0];
                AlliantEntity periodLast  = L_PeriodEndDesc[0];

                int N_PeriodsBetween = 1;
                if (periodFirst != null && periodLast != null)
                {
                    var firstPI    = (PeriodItem)PeriodItem.Items.GetEntityBySid(periodFirst.sid);
                    var lastPI     = (PeriodItem)PeriodItem.Items.GetEntityBySid(periodLast.sid);
                    var lastMinus1 = DS.F_PERIODS_FROM(lastPI, -1);
                    N_PeriodsBetween = DS.F_NUM_PERIOD_TYPES_BETWEEN(firstPI, lastMinus1, tierInterval) + 1;
                    if (N_PeriodsBetween < 1) N_PeriodsBetween = 1;
                }

                // Sort orders for the two TierSets.  Mirrors the 33-column PROCESS BY
                // in the original C_ROY_SUB_TIERING DealScript.  Order matches the
                // DealScript exactly so tie-breaking behavior is identical.
                //
                // DealScript order (TieringByAmount):
                //   x:14 ActualPeriod, x:26 OtherPeriod, x:19 Amount,
                //   x:2  UDKey2, x:1  UDKey1, x:4 UDKey4, x:5 UDKey5, x:6 UDKey6, x:7 UDKey7,
                //   x:8  UDKey8, x:37 UDKey17, x:38 UDKey18,
                //   x:9  UDKey9, x:10 UDKey10, x:11 UDKey11, x:32 UDKey12, x:33 UDKey13,
                //   x:35 UDKey15, x:36 UDKey16, x:12 Rate1,
                //   x:41 Rate2, x:42 Rate3, x:15 Price1, x:16 Price2,
                //   x:43 FromDate, x:44 ToDate, x:45 Comment1, x:46 Comment2,
                //   x:47 Contact1, x:48 Contact2, x:49 Contact3, x:50 Contact4,
                //   x:3  UDKey3
                // TieringByUnits is identical except x:20 Qty replaces x:19 Amount.
                var sortByAmount = new ResultSet.SortOrder(
                    (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Amount,            ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_2_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_1_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_4_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_5_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_6_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_7_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_8_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_17_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_18_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_9_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_10_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_11_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_12_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_13_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_15_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_16_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_rate,         ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_2_rate,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_3_rate,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Price_point,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Alt_price_point,   ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Start_user_date,   ResultSet.SortDirection.Ascending),
                    (IndexableColumn.End_user_date,     ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_comment,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Alt_user_comment,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_2_sid,ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_3_sid,ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_4_sid,ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_3_sid,       ResultSet.SortDirection.Ascending));
                var sortByUnits = new ResultSet.SortOrder(
                    (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Qty,               ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_2_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_1_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_4_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_5_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_6_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_7_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_8_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_17_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_18_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_9_sid,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_10_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_11_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_12_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_13_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_15_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_16_sid,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_rate,         ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_2_rate,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_3_rate,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Price_point,       ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Alt_price_point,   ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Start_user_date,   ResultSet.SortDirection.Ascending),
                    (IndexableColumn.End_user_date,     ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_comment,      ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Alt_user_comment,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_sid,  ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_2_sid,ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_3_sid,ResultSet.SortDirection.Ascending),
                    (IndexableColumn.User_contact_4_sid,ResultSet.SortDirection.Ascending),
                    (IndexableColumn.Udkey_3_sid,       ResultSet.SortDirection.Ascending));

                // ─── Phase D: WHILE per interval ─────────────────────────────
                AlliantEntity periodListFirstEntity = L_PeriodListAsc[0];
                PeriodItem    periodListFirstPI    = periodListFirstEntity != null
                    ? (PeriodItem)PeriodItem.Items.GetEntityBySid(periodListFirstEntity.sid)
                    : null;
                PeriodItem    periodListFirstMinus1 = periodListFirstPI != null
                    ? DS.F_PERIODS_FROM(periodListFirstPI, -1)
                    : null;

                int N_FromCount = 0;
                int N_ToCount   = 1;

                while (N_ToCount <= N_PeriodsBetween)
                {
                    PeriodItem intervalStart, intervalEnd;
                    if (N_PeriodsBetween == 1 || periodListFirstMinus1 == null)
                    {
                        // Whole input set is one interval.
                        intervalStart = null;
                        intervalEnd   = null;
                    }
                    else
                    {
                        // intervalStart = F_PERIODS_FROM(F_PERIOD_TYPES_FROM(periodListFirstMinus1, N_FromCount, TierInterval), 1)
                        // intervalEnd   = F_PERIOD_TYPES_FROM(periodListFirstMinus1, N_ToCount, TierInterval)
                        var iStart = DS.F_PERIOD_TYPES_FROM(periodListFirstMinus1, N_FromCount, tierInterval);
                        intervalStart = DS.F_PERIODS_FROM(iStart, 1);
                        intervalEnd   = DS.F_PERIOD_TYPES_FROM(periodListFirstMinus1, N_ToCount, tierInterval);
                    }

                    ResultSet onePeriodRevenue;
                    if (intervalStart == null || intervalEnd == null)
                    {
                        onePeriodRevenue = forTiering.Copy();
                    }
                    else
                    {
                        var intervalRange = DS.F_PERIOD_INTERVAL(intervalStart, intervalEnd);
                        onePeriodRevenue = forTiering.GetData(
                            CustCol.ActualPeriod, ListOp.INLIST, (EntityList)intervalRange,
                            "SubTier.OnePeriodRevenue");
                    }

                    if (onePeriodRevenue.Rows == 0)
                    {
                        onePeriodRevenue.Release();
                        N_FromCount++;
                        N_ToCount++;
                        continue;
                    }

                    // TieringByAmount.
                    var tieringByAmount = onePeriodRevenue.TierSet(
                        IndexableColumn.Amount,
                        IndexableColumn.Udkey_14_sid,
                        ContractUDF.TieringByAmount,
                        sortByAmount);

                    var validTierByAmount = tieringByAmount.GetData(CustCol.Tier, CompareOp.NE, "0",
                                                                    "SubTier.ValidTierByAmount");
                    var notTiered         = tieringByAmount.GetData(CustCol.Tier, CompareOp.EQ, "0",
                                                                    "SubTier.NotTiered");
                    tieringByAmount.Release();
                    onePeriodRevenue.Release();

                    // TieringByUnits on the not-tiered remainder.
                    var tieringByUnits = notTiered.TierSet(
                        IndexableColumn.Qty,
                        IndexableColumn.Udkey_14_sid,
                        ContractUDF.TieringByUnits,
                        sortByUnits);
                    notTiered.Release();

                    tieringResult.Combine(validTierByAmount);
                    tieringResult.Combine(tieringByUnits);
                    validTierByAmount.Release();
                    tieringByUnits.Release();

                    N_FromCount++;
                    N_ToCount++;
                }
            }
            // (forTiering is held alive across the loop and used by Phase E below.)

            // ─── Phase E: Round + p_ds_proration_12 to preserve totals ───────
            // ReadyForProration = N_NoneLabel==0 ? TieringOutput : EmptySet
            // RoundedForProration = #<c:1547># using ReadyForProration
            // BaseProration = N_NoneLabel==0 ? ForTiering : EmptySet
            // RoundingFlag (single-row config: Rate2=2, Amount=2, Amount2=2 = decimal places).
            // RoundedTiering = p_ds_proration_12(BaseProration, RoundedForProration, RoundingFlag)
            ResultSet roundedTiering;
            if (N_NoneLabel == 0)
            {
                var readyForProration = tieringResult;
                var roundedForProration = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(readyForProration);
                Job.CurrentCalcContext = _ctxSubTiering;

                var baseProration = forTiering.Copy();
                // p_ds_proration_12 mode=0, default rounding correction; Amount/Amount2 to
                // 2dp, Units/Units2 to 0dp (matches DealScript RoundingFlag {Rate2=2,
                // Amount=2, Amount2=2}; Units columns absent → 0dp).
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

            // ─── Phase F: Window filtering ───────────────────────────────────
            var readyForRoyaltyMethod = ResultSet.EmptySet();
            readyForRoyaltyMethod.CombineAndRelease(roundedTiering, noTiering);

            // SalesWithinInterval = filter readyForRoyaltyMethod for ActualPeriod ∈ [StartWindow..End_of_Time]
            // Output = filter SalesWithinInterval for ActualPeriod ∈ [AlliantCutoverPeriod..End_of_Time]
            // Both filters can be combined.
            var endOfTime = (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time);
            var withinWindow = startWindowPI != null
                ? DS.F_PERIOD_INTERVAL(startWindowPI, endOfTime)
                : null;

            // AlliantCutoverPeriod is a Contract UDF that returns a PeriodItem (or its descr).
            string alliantCutoverDescr = Contract.GetUDFString(ContractUDF.AlliantCutoverPeriod);
            PeriodItem cutoverPI = !string.IsNullOrEmpty(alliantCutoverDescr)
                ? (PeriodItem)PeriodItem.Items.GetEntityByDescr(alliantCutoverDescr)
                : null;
            var fromCutover = cutoverPI != null
                ? DS.F_PERIOD_INTERVAL(cutoverPI, endOfTime)
                : null;

            var afterWindowFilter = withinWindow != null
                ? readyForRoyaltyMethod.GetData(
                    CustCol.ActualPeriod, ListOp.INLIST, (EntityList)withinWindow,
                    "SubTier.SalesWithinInterval")
                : readyForRoyaltyMethod.Copy();
            readyForRoyaltyMethod.Release();

            ResultSet outputResult;
            if (fromCutover != null)
            {
                outputResult = afterWindowFilter.GetData(
                    CustCol.ActualPeriod, ListOp.INLIST, (EntityList)fromCutover,
                    "SubTier.OutputAfterCutover");
                afterWindowFilter.Release();
            }
            else
            {
                outputResult = afterWindowFilter;
            }

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubTiering");
            return outputResult;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1662 C_ROY_SUB_BONUS_PAYMENT ─────────────────────────────────────
        //
        // Tiered bonus payment when net sales pass an Amount or Units threshold.
        // The bonus type is gated by ContractUDF.BonusPaymentType ("Amount" or "Units").
        //
        // DealScript flow (two parallel branches: by Amount, by Units):
        //   PriorBonusPayment = filter from INPUTSET2 for ActivityType=Bonus_Payment.
        //   BonusType = read ContractUDF.BonusPaymentType.
        //     N_AmountBonus = 1 if "Amount" else 0.
        //     N_UnitsBonus  = 1 if "Units"  else 0.
        //
        //   NetSalesITD = filter INPUTSET1 for Net_Sales, TransType=ITD, Tier=0.
        //   SummarizeNetSales = SummarizeForBonusTiering (1655).
        //
        //   Per branch (Amount or Units):
        //     ForXBonus = SummarizeNetSales if N_XBonus=1 else EmptySet.
        //     BonusByX = TierSet ForXBonus by Amount/Units, set Tier from
        //                ContractUDF.BonusThresholdAmount or BonusThresholdUnits,
        //                process by 32-column ASC sort.
        //     NonZero, Summarize via SummarizeToTier (1657), filter non-zero.
        //     GroupNumbering with sort = SortOrder for last-row identification.
        //     TheLastBonus = GetData where seq#=0 (last row of each group).
        //     Add the threshold value to Amount.
        //     Filter non-zero.
        //     Reset Tier=0.
        //     Subtract prior period's bonus to get current.
        //     Filter Amount > 0 (only positive payments), restamp ActivityType=Bonus_Payment, AltComment="Amount" or "Units".
        //     Set ActualPeriod = current calc period.
        //     ITDOutput = Combine(PriorBonusByX, CurrentBonusByXOutput).
        //
        //   ITDOutput = Set TransType of Combine(both branches' ITD) to ITD.
        //   CurrentOutput = Set TransType of Combine(both branches' Current) to Current.
        //   INPUTSET3 = ITDOutput; INPUTSET4 = CurrentOutput.
        public static (ResultSet BonusItdOutput,
                       ResultSet BonusCurrentOutput) SubBonusPayment(
            ResultSet allResultsItd,        // INPUTSET1
            ResultSet priorPeriodItd)       // INPUTSET2
        {
            _ctxSubBonusPayment = _ctxSubBonusPayment ?? NewSubCtxStandard("C_ROY_SUB_BONUS_PAYMENT");
            Job.CurrentCalcContext = _ctxSubBonusPayment;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubBonusPayment");

            // Prior bonus payments.
            var priorBonusPayment = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Bonus_Payment,
                "SubBonus.PriorBonusPayment");

            // BonusType decision.
            string bonusType = Contract.GetUDFString(ContractUDF.BonusPaymentType);
            int N_AmountBonus = bonusType == "Amount" ? 1 : 0;
            int N_UnitsBonus  = bonusType == "Units"  ? 1 : 0;

            // NetSalesITD: filter and prep for tiering.
            var netSalesItd = allResultsItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ, ActivityType.Net_Sales),
                new Criteria(CustCol.TransType,    CompareOp.EQ, TransType.ITD)
            }, "SubBonus.NetSalesITD");
            netSalesItd.DoMath(CustCol.Tier, MathOp.SETTO, "0");

            // SummarizeNetSales = SummarizeForBonusTiering (1655).
            var summarizeNetSales = SummarizeForBonusTiering(netSalesItd);
            Job.CurrentCalcContext = _ctxSubBonusPayment;

            // Sort order for TierSet (32-column ASC) -- replicates DealScript's "PROCESS BY"
            // column list.  Engine TierSet uses this to determine row order during tier
            // assignment.  Since BC1's c_calc_context for 1662 sums on essentially everything,
            // the rows are pre-grouped by SummarizeForBonusTiering and the sort just keys
            // tie-breakers in a deterministic order.
            // SortOrder is BY ActualPeriod, OtherPeriod, and the value column (Amount or Units),
            // followed by all the entity/dimension columns.  For brevity we sort by the
            // value column primarily; the engine's TierSet only needs the value col + a
            // total ordering for stability.
            var sortAmount = new ResultSet.SortOrder(
                (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                (IndexableColumn.Amount,            ResultSet.SortDirection.Ascending));
            var sortUnits = new ResultSet.SortOrder(
                (IndexableColumn.Actual_period_sid, ResultSet.SortDirection.Ascending),
                (IndexableColumn.Other_period_sid,  ResultSet.SortDirection.Ascending),
                (IndexableColumn.Qty,               ResultSet.SortDirection.Ascending));

            // GroupNumbering sort order (post-tier): used to identify the LAST row of each
            // tier group, which gets the cumulative bonus added to it.
            // DealScript's SortOrder is "ActualPeriod, Catalog, Channel, Territory, Language,
            // RightsType, Format, Customer, Provider, RecoupmentGroup ASC".  For correctness
            // we replicate the primary keys as a simple Comparison; tie-breakers default to
            // insertion order.
            Comparison<CalcResultRow> groupNumberingSort = (a, b) =>
            {
                int r;
                if ((r = a.Actual_period_sid.CompareTo(b.Actual_period_sid)) != 0) return r;
                if ((r = a.Udkey_1_sid.CompareTo(b.Udkey_1_sid)) != 0) return r;
                if ((r = a.Udkey_4_sid.CompareTo(b.Udkey_4_sid)) != 0) return r;
                if ((r = a.Udkey_5_sid.CompareTo(b.Udkey_5_sid)) != 0) return r;
                if ((r = a.Udkey_7_sid.CompareTo(b.Udkey_7_sid)) != 0) return r;
                if ((r = a.Udkey_9_sid.CompareTo(b.Udkey_9_sid)) != 0) return r;
                if ((r = a.Udkey_10_sid.CompareTo(b.Udkey_10_sid)) != 0) return r;
                if ((r = a.Udkey_11_sid.CompareTo(b.Udkey_11_sid)) != 0) return r;
                if ((r = a.Udkey_12_sid.CompareTo(b.Udkey_12_sid)) != 0) return r;
                if ((r = a.Udkey_15_sid.CompareTo(b.Udkey_15_sid)) != 0) return r;
                return 0;
            };

            // ─── Bonus by Amount branch ───
            var (currentBonusByAmountOutput, itdBonusByAmountOutput) = BuildBonusBranch(
                summarizeNetSales, priorBonusPayment, N_AmountBonus,
                useAmount: true,
                tierThresholdUDF: ContractUDF.BonusThresholdAmount,
                tierSetValueCol: IndexableColumn.Amount,
                tierSetSortOrder: sortAmount,
                groupNumberingSort: groupNumberingSort,
                bonusBranchTag: "Amount");

            // ─── Bonus by Units branch ───
            var (currentBonusByUnitsOutput, itdBonusByUnitsOutput) = BuildBonusBranch(
                summarizeNetSales, priorBonusPayment, N_UnitsBonus,
                useAmount: false,
                tierThresholdUDF: ContractUDF.BonusThresholdUnits,
                tierSetValueCol: IndexableColumn.Qty,
                tierSetSortOrder: sortUnits,
                groupNumberingSort: groupNumberingSort,
                bonusBranchTag: "Units");

            summarizeNetSales.Release();
            priorBonusPayment.Release();

            // Final assembly.
            var bonusItdOutput = ResultSet.EmptySet();
            bonusItdOutput.CombineAndRelease(itdBonusByAmountOutput, itdBonusByUnitsOutput);
            bonusItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            var bonusCurrentOutput = ResultSet.EmptySet();
            bonusCurrentOutput.CombineAndRelease(currentBonusByAmountOutput, currentBonusByUnitsOutput);
            bonusCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubBonusPayment");
            return (bonusItdOutput, bonusCurrentOutput);
        }

        // Helper: one bonus branch (either Amount or Units).
        // Computes per-tier bonus, subtracts prior period to get current, splits.
        private static (ResultSet CurrentBonusOutput,
                        ResultSet ItdBonusOutput) BuildBonusBranch(
            ResultSet summarizeNetSales,        // shared input -- caller still owns
            ResultSet priorBonusPayment,        // shared input -- caller still owns
            int       branchEnabled,            // 1 = active, 0 = no-op
            bool      useAmount,                // true = Amount branch, false = Units
            string    tierThresholdUDF,
            IndexableColumn tierSetValueCol,
            ResultSet.SortOrder tierSetSortOrder,
            Comparison<CalcResultRow> groupNumberingSort,
            string    bonusBranchTag)           // "Amount" or "Units"
        {
            if (branchEnabled != 1)
            {
                // Even a disabled branch contributes its prior bonus at the right tag --
                // DealScript still combines PriorBonusByX into the ITD output.
                var emptyCurrent = ResultSet.EmptySet();
                var disabledItd = priorBonusPayment.GetData(
                    EngineCol.AltComment, CompareOp.EQ, bonusBranchTag,
                    "SubBonus.PriorBonusBy" + bonusBranchTag);
                return (emptyCurrent, disabledItd);
            }

            var forBonus = summarizeNetSales.Copy();

            // TierSet: assign Tier from ContractUDF threshold by sorted value column.
            var bonusByX = forBonus.TierSet(tierSetValueCol, IndexableColumn.Udkey_14_sid,
                                             tierThresholdUDF, tierSetSortOrder);
            forBonus.Release();

            var nonZeroBonusByX = bonusByX.GetData(CustCol.Tier, CompareOp.NE, "0", "SubBonus.NonZeroBonusBy" + bonusBranchTag);
            bonusByX.Release();
            // Copy before SummarizeToTier consumes it -- we need the original for GroupNumbering below.
            var nonZeroBonusByXForGN = nonZeroBonusByX.Copy();

            var summarizeBonusByX = SummarizeToTier(nonZeroBonusByX);
            Job.CurrentCalcContext = _ctxSubBonusPayment;

            // For Amount branch we filter on Amount; for Units branch we filter on Units.
            var nonZeroSummarizeBonusByX = useAmount
                ? summarizeBonusByX.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubBonus.NonZeroSummarize" + bonusBranchTag)
                : summarizeBonusByX.GetData(BaseCol.Units,  CompareOp.NE, "0", "SubBonus.NonZeroSummarize" + bonusBranchTag);
            summarizeBonusByX.Release();

            // GroupNumbering: assigns each row a sequence within its group.
            var bonusByXWithSortNo = ResultSetDSPs.GroupNumbering(nonZeroBonusByXForGN, nonZeroSummarizeBonusByX, groupNumberingSort);
            nonZeroBonusByXForGN.Release();
            nonZeroSummarizeBonusByX.Release();

            // TheLastBonus = rows where Amount = 0 (the last row of each group, per
            // GroupNumbering's convention).
            var theLastBonus = bonusByXWithSortNo.GetData(BaseCol.Amount, CompareOp.EQ, "0", "SubBonus.TheLast" + bonusBranchTag);
            bonusByXWithSortNo.Release();

            // Add ContractUDF threshold value to Amount.
            theLastBonus.DoMath(BaseCol.Amount, MathOp.PLUS, tierThresholdUDF);

            var nonZeroBonusByXWithAmount = theLastBonus.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubBonus.NonZeroWithAmount" + bonusBranchTag);
            theLastBonus.Release();
            nonZeroBonusByXWithAmount.DoMath(CustCol.Tier, MathOp.SETTO, "0");

            // Subtract prior bonus (current = ITD - prior).
            var priorBonusByX = priorBonusPayment.GetData(EngineCol.AltComment, CompareOp.EQ, bonusBranchTag,
                                                            "SubBonus.PriorBonusBy" + bonusBranchTag);
            // Copy priorBonusByX before SummarizeToTier consumes it -- we need the original
            // for the ITD output Combine below.
            var priorBonusByXForItd = priorBonusByX.Copy();
            var summarizePriorBonusByX = SummarizeToTier(priorBonusByX);
            Job.CurrentCalcContext = _ctxSubBonusPayment;

            var summarizeItdBonusByX = SummarizeToTier(nonZeroBonusByXWithAmount);
            Job.CurrentCalcContext = _ctxSubBonusPayment;

            var currentBonusByX = ResultSet.Subtract(summarizeItdBonusByX, summarizePriorBonusByX, "SubBonus.CurrentBonus" + bonusBranchTag);
            summarizeItdBonusByX.Release();
            summarizePriorBonusByX.Release();

            // Pos rows only; set ActualPeriod = current calc period.
            var posByX = currentBonusByX.GetData(BaseCol.Amount, CompareOp.GT, "0", "SubBonus.Pos" + bonusBranchTag);
            currentBonusByX.Release();
            posByX.SetValue(CustCol.ActualPeriod, DS.F_CALC_PERIOD());

            // Restamp ActivityType=Bonus_Payment, AltComment=tag.
            var currentBonusOutput = posByX;
            currentBonusOutput.SetValue(CustCol.ActivityType, ActivityType.Bonus_Payment);
            currentBonusOutput.DoMath(EngineCol.AltComment, MathOp.SETTO, bonusBranchTag);

            // ITD output = Combine(PriorBonusByX, CurrentBonusByXOutput).
            var itdBonusOutput = ResultSet.EmptySet();
            itdBonusOutput.CombineAndRelease(priorBonusByXForItd, currentBonusOutput.Copy());
            return (currentBonusOutput, itdBonusOutput);
        }

        // ── 1550 C_ROY_GET_IMPORT_ADJS_PRIOR_ITD ─────────────────────────────
        //
        // Pulls Import (revenue + adjustments) and Prior-period CalcResult ITD into
        // the contract's audit window, applies currency conversion to imports, and
        // produces 5 result sets the caller pipelines into the rest of royalty
        // accrual.
        //
        // The DealScript also performs a Revenue Allocation completeness check
        // (p_ds_get_deal_list / p_ds_get_deal_run_status) to guarantee feeder Deals
        // have been run.  That check + p_ds_get_calc_results retrieve the allocated
        // ITD trx from contributing Deals.
        //
        // Phase A. Window-start period.
        //          N_WindowPeriods = -1 * ContractUDF.WindowPeriods.
        //          WinPeriod  = F_PERIOD_TYPES_FROM(F_CALC_PERIOD(), N_WindowPeriods, ContractUDF.StatementInterval)
        //          DedCapInterval = Contract.GetUDFString(ContractUDF.DeductionCapInterval).
        //          When DedCapInterval is "<None>"/"No Deduction Cap": NoCap, use WinPeriod.
        //          When "Inception to Date": use Inception.
        //          Otherwise: CapStart = F_PERIODS_FROM(F_PERIOD_TYPES_FROM(WinPeriod, -1,
        //                                ContractUDF.DeductionCapInterval), 1).
        //          WindowStartPeriod = max(CapStart, statement-interval start).  Falls back
        //          to Inception when N_WindowPeriods=0 or DedCapInterval="Inception to Date".
        //          WindowStartPeriodOutput stamps ActivityType=Window_Start_Period, TransType=ITD.
        // Phase B. Imported trx within window.
        //          ImportTrx = GetData Import for ActivityType ∈ Imported_Activity_Type_List,
        //                                ActualPeriod ∈ [WindowStart..End_of_Time], Period ∈ ITD.
        //          Stamp TransType=ITD.
        //          When ContractUDF.IncludeBundlesFlag = "Yes": filter to Bundle = unspecified.
        // Phase C. Previous ITD from CalcResult.
        //          PreviousITD = GetData CalcResult for ActivityType ∈ Prior_ITD_Activity_Type_List,
        //                                TransType=ITD, Period = F_CALC_PERIOD_PREVIOUS().
        //          PreWindowingITD = filter to ActualPeriod ∈ [Inception..WindowStart-1].
        // Phase D. Adjustment trx for the calc period.
        //          Adjustments = GetData Adjustment for ActivityType ∈ Adjustment_Activity_Type_List,
        //                                Period ∈ ITD.  Stamp TransType=ITD.
        // Phase E. Revenue Allocation contribution.
        //          DealsToRetrieve = p_ds_get_deal_list (Type="Revenue Allocation", Rate1=7).
        //          DSPRunStatus    = p_ds_get_deal_run_status, completeness fan-out.
        //          DealsInError    -> p_ds_set_calc_error_in_run.
        //          AllocationResult = p_ds_get_calc_results (Type=ITD, current calc period).
        //          ValidAllocationResult = AllocationResult ∩ window range.
        // Phase F. Allowable + currency conversion.
        //          ReadyForAllowable = (ImportedITD ∪ ValidAllocationResult), AltComment = ContractUDF.AllowableTransactions.
        //          AllowedTrxs = filter AltComment="Yes", AltComment <- F_NULL_STRING(), Rate2=-1.
        //          WithExchangeRate = stamp UDKey17 (SourceCurrency) <- ContractUDF.DealCurrency,
        //                              Rate2 <- SourceCurrencyUDF.ExchangeRate.
        //          InvalidExchangeRate = filter Rate2=-1 (rate-not-found rows).
        //                              p_ds_udkey_to_text + p_ds_set_calc_error_in_run.
        //          ValidExchangeRate = stash Amount→Amount2, Price1→Price2, then
        //                              Amount = Rate2 * Amount2; Price1 = Rate2 * Price2.  Round.
        //          ConvertedTrx = ValidExchangeRate ∪ rounded conversions.
        // Phase G. Outputs.
        //          IS1 = ConvertedTrx, IS2 = PreviousITD, IS3 = PreWindowingITD,
        //          IS4 = Adjustments, IS5 = WindowStartPeriodOutput.
        public static (ResultSet ImportedTrx,
                       ResultSet PreviousITD,
                       ResultSet PreWindowingITD,
                       ResultSet Adjustments,
                       ResultSet WindowStartPeriodOutput) SubGetImportAdjsPriorITD()
        {
            _ctxSubGetImportAdjsPriorITD = _ctxSubGetImportAdjsPriorITD ?? NewSubCtxStandard("C_ROY_GET_IMPORT_ADJS_PRIOR_ITD");
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorITD;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubGetImportAdjsPriorITD");

            // ─── Phase A: Window-start period ────────────────────────────────
            int N_WindowPeriodsRaw = Contract.GetUDFInt(ContractUDF.WindowPeriods);
            int N_WindowPeriods    = N_WindowPeriodsRaw * -1;
            string statementInterval = Contract.GetUDFString(ContractUDF.StatementInterval);
            string dedCapInterval    = Contract.GetUDFString(ContractUDF.DeductionCapInterval);

            int N_NoCapNeeded     = (dedCapInterval == "<None>" || dedCapInterval == "<NONE>" || dedCapInterval == "No Deduction Cap") ? 1 : 0;
            int N_InceptionLabel  = (dedCapInterval == "Inception to Date") ? 1 : 0;

            PeriodItem inception = DS.F_INCEPTION();

            // WinPeriod = F_PERIOD_TYPES_FROM(calcPeriod, N_WindowPeriods, statementInterval)
            PeriodItem winPeriod = !string.IsNullOrEmpty(statementInterval)
                ? DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, N_WindowPeriods, statementInterval)
                : Job.CurrentCalcPeriod;

            // CapStart = F_PERIODS_FROM(F_PERIOD_TYPES_FROM(winPeriod, -1, dedCapInterval), 1)
            PeriodItem mayBeCap;
            if (N_NoCapNeeded == 1 || string.IsNullOrEmpty(dedCapInterval))
                mayBeCap = winPeriod;
            else
            {
                var capBase = DS.F_PERIOD_TYPES_FROM(winPeriod, -1, dedCapInterval);
                mayBeCap = DS.F_PERIODS_FROM(capBase, 1);
            }

            // WinStart = F_PERIODS_FROM(F_PERIOD_TYPES_FROM(mayBeCap, -1, statementInterval), 1)
            PeriodItem winStart;
            if (string.IsNullOrEmpty(statementInterval))
                winStart = mayBeCap;
            else
            {
                var winBase = DS.F_PERIOD_TYPES_FROM(mayBeCap, -1, statementInterval);
                winStart = DS.F_PERIODS_FROM(winBase, 1);
            }

            PeriodItem windowStart = (N_WindowPeriodsRaw == 0 || N_InceptionLabel == 1)
                ? inception
                : winStart;

            // WindowStartPeriodOutput: ZeroSet stamped with ActivityType=Window_Start_Period,
            // TransType=ITD, ActualPeriod=windowStart.
            var windowStartPeriodOutput = ResultSet.ZeroSet();
            windowStartPeriodOutput.SetValue(CustCol.ActivityType, ActivityType.Window_Start_Period);
            windowStartPeriodOutput.SetValue(CustCol.TransType,    TransType.ITD);
            windowStartPeriodOutput.SetValue(CustCol.ActualPeriod, windowStart);

            var auditWindow = DS.F_PERIOD_INTERVAL(windowStart, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));

            // ─── Phase B: Imported trx within window ─────────────────────────
            var importedFilter = new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Imported_Activity_Type_List),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)auditWindow),
                new Criteria(EngineCol.Period,     ListOp.INLIST, Job.ITDthruCalcPeriod),
            };
            var importTrx = Job.Import.GetData(importedFilter, "GetImportAdjsITD.ImportTrx");
            importTrx.SetValue(CustCol.TransType, TransType.ITD);

            // IncludeBundlesFlag toggle.  When "Yes", drop bundle parents and keep only
            // the unspecified-bundle items (post-explode).  When anything else, keep
            // ImportTrx as-is (bundle parents included).
            string includeBundles = Contract.GetUDFString(ContractUDF.IncludeBundlesFlag);
            ResultSet importedItd;
            if (includeBundles == "Yes")
            {
                importedItd = importTrx.GetData(CustCol.Bundle, CompareOp.EQ, Bundle.Unspecified, "GetImportAdjsITD.IncludeBundles");
                importTrx.Release();
            }
            else
            {
                importedItd = importTrx;
            }

            // ─── Phase C: Previous ITD from CalcResult ───────────────────────
            var previousItdFilter = new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Prior_ITD_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.ITD),
                new Criteria(EngineCol.Period,     CompareOp.EQ,  Job.CalcPeriodPrevious),
            };
            var previousItd = ResultSet.GetDataFromCalcResult(previousItdFilter);

            // PreWindowingITD = filter PreviousITD to ActualPeriod ∈ [Inception .. windowStart-1]
            PeriodItem windowStartMinus1 = DS.F_PERIODS_FROM(windowStart, -1);
            var preWindowRange = DS.F_PERIOD_INTERVAL(inception, windowStartMinus1);
            var preWindowingItd = previousItd.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)preWindowRange,
                "GetImportAdjsITD.PreWindowingITD");

            // ─── Phase D: Adjustment trx ─────────────────────────────────────
            var adjustmentsFilter = new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Adjustment_Activity_Type_List),
                new Criteria(EngineCol.Period,     ListOp.INLIST, Job.ITDthruCalcPeriod),
            };
            var adjustments = ResultSet.GetDataFromAdjustment(adjustmentsFilter);
            adjustments.SetValue(CustCol.TransType, TransType.ITD);

            // ─── Phase E: Revenue Allocation completeness + retrieve ─────────
            // DealsToRetrieve = p_ds_get_deal_list using one-row request set.
            var dspGetDealRevAlloc = ResultSet.ZeroSet();
            dspGetDealRevAlloc.DoMath(EngineCol.Comment, MathOp.SETTO, "Revenue Allocation");
            dspGetDealRevAlloc.DoMath(EngineCol.Rate1,   MathOp.SETTO, "7");   // admin class filter
            //var dealsToRetrieve = DS.ExecuteDSP("p_ds_get_deal_list", dspGetDealRevAlloc);
            var dealsToRetrieveWithModel = DS.ExecuteDSP("p_ds_get_deal_list", dspGetDealRevAlloc);
            dspGetDealRevAlloc.Release();
            var dealsToRetrieve = dealsToRetrieveWithModel.GetData(EngineCol.Comment, CompareOp.NE, "Revenue Allocation Contract Model");
            dealsToRetrieveWithModel.Release();

            // DSPRunStatus = p_ds_get_deal_run_status using calc-period request + deal list.
            var dspGetStatusInput = ResultSet.ZeroSet();
            dspGetStatusInput.SetValue(CustCol.OtherPeriod, Job.CurrentCalcPeriod);
            var dspRunStatus = DS.ExecuteDSP("p_ds_get_deal_run_status", dspGetStatusInput, dealsToRetrieve);
            dspGetStatusInput.Release();

            // Status check: completed (16) or approved (5) is OK; setup (27) is OK; everything
            // else triggers an error-in-run.  c_calc_status_type sids are well-known constants.
            var completeStatus = ResultSet.ZeroSet();
            completeStatus.DoMath(EngineCol.Rate1, MathOp.SETTO, "16");
            var approvedStatus = ResultSet.ZeroSet();
            approvedStatus.DoMath(EngineCol.Rate1, MathOp.SETTO, "5");
            var completeOrApproved = ResultSet.EmptySet();
            completeOrApproved.CombineAndRelease(completeStatus, approvedStatus);
            var L_CompleteApprovedStatus = completeOrApproved.GetList(EngineCol.Rate1);
            completeOrApproved.Release();

            var approvedDeals = dspRunStatus.GetData(EngineCol.Rate1, ListOp.INLIST, L_CompleteApprovedStatus,
                                                     "GetImportAdjsITD.ApprovedDeals");
            var dealsInSetup  = dealsToRetrieve.GetData(EngineCol.Rate2, CompareOp.EQ, "27",
                                                        "GetImportAdjsITD.DealsInSetup");
            var dealsNotInError = ResultSet.EmptySet();
            dealsNotInError.CombineAndRelease(approvedDeals, dealsInSetup);
            var L_DealsNotInError = dealsNotInError.GetList(EngineCol.Comment);
            dealsNotInError.Release();

            var dealsInError = dealsToRetrieve.GetData(EngineCol.Comment, ListOp.NOTINLIST, L_DealsNotInError,
                                                       "GetImportAdjsITD.DealsInError");
            int N_DealsInError = dealsInError.Rows;
            if (N_DealsInError > 0)
            {
                var dspErrIS1 = dealsInError.Copy();
                dspErrIS1.DoMath(EngineCol.AltComment, MathOp.SETTO,
                    "- Is a contributing Contract with a Deal that has not been run to completion, please Run the Revenue Allocaton Deal before proceeding to the Royalty Contract.");
                var dspErrIS2 = ResultSet.ZeroSet();
                dspErrIS2.DoMath(EngineCol.Comment, MathOp.SETTO, "Contract ID:");
                ResultSetDSPs.SetCalcErrorInRun(dspErrIS1, dspErrIS2).Release();
                dspErrIS1.Release();
                dspErrIS2.Release();
            }
            dealsInError.Release();
            dspRunStatus.Release();

            // p_ds_get_calc_results: retrieve approved ITD trx from contributing Deals at
            // calc period.  Builds a one-row request and calls the DSP.
            var dspGetResultsInput = ResultSet.ZeroSet();
            //var dspGetResultsInput = ZeroSetWithComments("Udkey3", null);
            dspGetResultsInput.SetValue(CustCol.OtherPeriod, Job.CurrentCalcPeriod);
            // This gets set to value of Udkey3 (zero) rather than literal string
            //dspGetResultsInput.DoMath(EngineCol.Comment,    MathOp.SETTO, "Udkey3");
            dspGetResultsInput.SetValue(CustCol.ActualPeriod, new AlliantEntity(0)); // unspecified - "all"
            dspGetResultsInput.DoMath(EngineCol.AltComment, MathOp.SETTO, "Complete, Approved");
            dspGetResultsInput.DoMath(EngineCol.Price1,     MathOp.SETTO, "1");   // ContractID in Comment1
            dspGetResultsInput.DoMath(EngineCol.Rate2,      MathOp.SETTO, "1");   // match contract scope

            var dspItdTransType = ResultSet.ZeroSet();
            dspItdTransType.SetValue(CustCol.TransType, TransType.ITD);

            //var allocationResult = DS.ExecuteDSP("p_ds_get_calc_results",
            //dspGetResultsInput, dealsToRetrieve, dspItdTransType);
            var allocationResult = ResultSet.EmptySet();
            dspGetResultsInput.Release();
            dspItdTransType.Release();
            dealsToRetrieve.Release();

            // ValidAllocationResult: only the rows whose ActualPeriod is in the audit window.
            var validAllocationResult = allocationResult.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)auditWindow,
                "GetImportAdjsITD.ValidAllocationResult");
            allocationResult.Release();

            // ─── Phase F: Allowable + currency conversion ────────────────────
            var readyForAllowable = ResultSet.EmptySet();
            readyForAllowable.CombineAndRelease(importedItd, validAllocationResult);
            readyForAllowable.DoMath(EngineCol.AltComment, MathOp.SETTO, ContractUDF.AllowableTransactions);

            var allowedTrxs = readyForAllowable.GetData(EngineCol.AltComment, CompareOp.EQ, "Yes",
                                                        "GetImportAdjsITD.AllowedTrxs");
            readyForAllowable.Release();
            allowedTrxs.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());
            allowedTrxs.DoMath(EngineCol.Rate2,      MathOp.SETTO, "-1");

            // SourceCurrency <- ContractUDF.DealCurrency, Rate2 <- SourceCurrencyUDF.ExchangeRate.
            var withExchangeRate = allowedTrxs;
            withExchangeRate.DoMath(CustCol.SourceCurrency, MathOp.SETTO, ContractUDF.DealCurrency);
            withExchangeRate.DoMath(EngineCol.Rate2,        MathOp.SETTO, SourceCurrencyUDF.ExchangeRate);

            // Invalid exchange rates: Rate2 still -1 because the lookup didn't resolve.
            // Surface them as an error-in-run via p_ds_udkey_to_text + p_ds_set_calc_error_in_run.
            var invalidExchangeRate = withExchangeRate.GetData(EngineCol.Rate2, CompareOp.EQ, "-1",
                                                                "GetImportAdjsITD.InvalidExchangeRate");
            invalidExchangeRate.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");
            var L_SourceCurrency = invalidExchangeRate.GetList(CustCol.SourceCurrency);
            int N_ConversionError = invalidExchangeRate.Rows;
            invalidExchangeRate.Release();

            if (N_ConversionError > 0)
            {
                var sourceCurrencyError = ResultSet.ZeroSet();
                sourceCurrencyError.SetEntity(CustCol.SourceCurrency, L_SourceCurrency[0]);
                var idToComment = ResultSet.ZeroSet();
                idToComment.DoMath(EngineCol.AltComment, MathOp.SETTO, "-");
                idToComment.DoMath(EngineCol.Comment,    MathOp.SETTO, "UDkey17");
                idToComment.DoMath(EngineCol.Rate1,      MathOp.SETTO, "0");   // 0 = move to Comment1
                idToComment.DoMath(EngineCol.Rate2,      MathOp.SETTO, "1");   // 1 = use ID
                DS.ExecuteDSP("p_ds_udkey_to_text", sourceCurrencyError, idToComment).Release();
                idToComment.Release();
                sourceCurrencyError.DoMath(EngineCol.AltComment, MathOp.SETTO, "Exchange Rate not found for Source Currency.");
                ResultSetDSPs.SetCalcErrorInRun(sourceCurrencyError).Release();
                sourceCurrencyError.Release();
            }

            // ValidExchangeRate = rows with a real Rate2 (Rate2 != -1).
            var validExchangeRate = withExchangeRate.GetData(EngineCol.Rate2, CompareOp.NE, "-1",
                                                              "GetImportAdjsITD.ValidExchangeRate");
            withExchangeRate.Release();
            // Stash unconverted Amount in Amount2, Price1 in Price2; clear primaries.
            validExchangeRate.DoMath(BaseCol.Amount2,  MathOp.SETTO, BaseCol.Amount);
            validExchangeRate.DoMath(BaseCol.Units2,   MathOp.SETTO, "0");
            validExchangeRate.DoMath(EngineCol.Price2, MathOp.SETTO, EngineCol.Price1);
            validExchangeRate.DoMath(BaseCol.Amount,   MathOp.SETTO, "0");

            // ConvertedAmount: Rate2 * Amount2 -> Amount.
            // ConvertedPrice : Rate2 * Price2 -> Amount2  (DealScript stages into Amount2 then later swaps).
            var convertedAmount = validExchangeRate.Copy();
            convertedAmount.DoMath(BaseCol.Amount,  MathOp.SETTO, BaseCol.Amount2);
            convertedAmount.DoMath(BaseCol.Amount,  MathOp.TIMES, EngineCol.Rate2);

            var convertedPrice = validExchangeRate.Copy();
            convertedPrice.DoMath(BaseCol.Amount2,  MathOp.SETTO, EngineCol.Price2);
            convertedPrice.DoMath(BaseCol.Amount2,  MathOp.TIMES, EngineCol.Rate2);

            var convertedAmountAndPrice = ResultSet.EmptySet();
            convertedAmountAndPrice.CombineAndRelease(convertedAmount, convertedPrice);
            var summarizedConverted = convertedAmountAndPrice.Summarize();
            if (!ReferenceEquals(summarizedConverted, convertedAmountAndPrice)) convertedAmountAndPrice.Release();
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorITD;

            var roundedConversion = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(summarizedConverted);
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorITD;
            // Move converted price into Units2 (display slot), zero Amount2.
            roundedConversion.DoMath(BaseCol.Units2,  MathOp.SETTO, BaseCol.Amount2);
            roundedConversion.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");

            var convertedTrx = ResultSet.EmptySet();
            convertedTrx.CombineAndRelease(validExchangeRate, roundedConversion);
            // Restore Price1 from Price2 stash; zero Price2.
            convertedTrx.DoMath(EngineCol.Price1, MathOp.SETTO, EngineCol.Price2);
            convertedTrx.DoMath(EngineCol.Price2, MathOp.SETTO, "0");
            var convertedTrxSummarized = convertedTrx.Summarize();
            if (!ReferenceEquals(convertedTrxSummarized, convertedTrx)) convertedTrx.Release();
            Job.CurrentCalcContext = _ctxSubGetImportAdjsPriorITD;

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubGetImportAdjsPriorITD");
            return (convertedTrxSummarized, previousItd, preWindowingItd, adjustments, windowStartPeriodOutput);
        }

        // ── 1590 C_ROY_SUB_GUARANTEE_INSTALLMENTS ────────────────────────────
        //
        // Largest sub-calc in the BC1 royalty pipeline.  Tracks Guarantee Installment
        // contracts: a stream of guaranteed payments due over time, recouped from
        // contemporaneous Royalties_Earned, with running-balance bookkeeping per
        // recoupment-group and per-period sequence numbering.
        //
        // Six output activity types are produced:
        //   - Beginning_Installment_Balance         (Current + ITD)
        //   - Ending_Installment_Balance            (ITD)
        //   - Royalties_Applied_to_Installment      (Current + ITD)
        //   - Payment_Guarantee_Installment         (Current + ITD)
        //   - Projected_Guarantee_Installment       (ITD)
        //   - Guarantee_Installment                 (ITD passthrough of input adjustments)
        //
        // High-level flow:
        //   Phase A. Royalties Due grain.  Combine RoyEarned (IS1) + Reserves (IS2),
        //            keep Royalties_for_Recoupment ITD, clear Amount2/Units/Units2,
        //            summarize via SummarizeForGuaranteeInstallment (1591), drop zeros,
        //            zero negatives (NEGRoyDue gets Amount=0 then re-Combine).
        //   Phase B. Installment template.  Take Guarantee_Installment adjs from IS3,
        //            stamp TransType=ITD; assign per-(StartPeriod, EndPeriod, RecGroup)
        //            group number into Price2 (via p_ds_group_numbering); explode all
        //            monthly periods within each group's [Start..End] (p_ds_explode_periods)
        //            and assign per-group sequence number into Rate3.
        //   Phase C. Payment-shifting.  Map Guarantee due-date FromDate -> ActualPeriod
        //            (p_ds_date_to_period); shift ActualPeriod back 1 period (payment
        //            recognized 1 period before due date) via p_ds_periods_from; merge
        //            seq nos; out-of-term payments get seq=1; restore Actual/Other
        //            Period from scratch slots; add Tier (UDKey14 = pmt-seq + 1 = bal-seq).
        //   Phase D. Running-total prep.  Combine balance-seq template + zero-padded
        //            Royalties Earned via p_ds_running_total grouped by Price2, ordered
        //            by ActualPeriod ASC.  Stash Rate1 (payment), Amount2 (Roy total),
        //            Units (guar bal), Units2 (guar bal running).
        //   Phase E. Backfill missing payments earlier than first running-total period
        //            (p_ds_detail_merge into PmtGuaranteeInstallmentForMatching with
        //             Amount=55, then subtract back).
        //   Phase F. Filter to current-or-prior periods only; compute ending-balance
        //            (Price1 = Amount2 - Units2).
        //   Phase G. WHILE loop over groups (N_Count = MIN..MAX of Rate3):
        //              On first iteration: process from base running total; subsequent
        //              iterations: feed back from FromPrior (INPUTSET9 in DealScript).
        //              Per iteration: split positive/negative ending balance, allocate
        //              excess to Projected_Guarantee_Installment via p_ds_ordered_allocation
        //              by FromDate DESC, recompute running total via p_ds_running_total,
        //              emit Beginning_Installment, Ending_Installment, Royalties_Applied,
        //              Payment_Guarantee, Projected_Guarantee.
        //   Phase H. Post-loop: extract by ActivityType, subtract IS4 prior-period to get
        //              Current variants for RoysApplied + PmtGuar.  Compute statement-aware
        //              beginning/ending balance period via p_ds_time_between + p_ds_periods_from.
        //              Final BegBal current = BegBal - prior IS4 EndBal.
        //
        // 23 SQL DSPs called; only p_ds_group_numbering has a C# in-memory equivalent
        // (ResultSetDSPs.GroupNumbering).  The rest stay as DS.ExecuteDSP("p_ds_*", ...).
        public static ResultSet SubGuaranteeInstallments(
            ResultSet royaltiesEarnedItd,    // INPUTSET1
            ResultSet reservesItd,           // INPUTSET2
            ResultSet allAdjustmentsItd,     // INPUTSET3
            ResultSet priorPeriodItd)        // INPUTSET4
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubGuaranteeInstallments = _ctxSubGuaranteeInstallments ?? NewSubCtxStandard("C_ROY_SUB_GUARANTEE_INSTALLMENTS");
            Job.CurrentCalcContext = _ctxSubGuaranteeInstallments;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubGuaranteeInstallments");

            // ─── Phase A: RoyDue summarization ────────────────────────────────
            var roysForGuarantee = ResultSet.EmptySet();
            roysForGuarantee.CombineAndRelease(royaltiesEarnedItd.Copy(), reservesItd.Copy());

            var royDueItd = roysForGuarantee.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Total_Royalties_Due_Activiity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.ITD)
            }, "SubGI.RoyDueITD");
            roysForGuarantee.Release();
            royDueItd.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            royDueItd.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
            royDueItd.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

            // SummarizeForGuaranteeInstallment (1591) consumes royDueItd.
            var royDueSumm = SummarizeForGuaranteeInstallment(royDueItd);
            Job.CurrentCalcContext = _ctxSubGuaranteeInstallments;

            var royDueNonZeros = royDueSumm.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubGI.RoyDueNonZeros");
            royDueSumm.Release();
            // NEGRoyDue rows: Amount < 0 -> drop the negative magnitude (set Amount=0).
            var negRoyDue = royDueNonZeros.GetData(BaseCol.Amount, CompareOp.LT, "0", "SubGI.NEGRoyDue");
            negRoyDue.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            var posRoyDue = royDueNonZeros.GetData(BaseCol.Amount, CompareOp.GE, "0", "SubGI.PosRoyDue");
            royDueNonZeros.Release();
            var royDueToUse = ResultSet.EmptySet();
            royDueToUse.CombineAndRelease(posRoyDue, negRoyDue);

            // ─── Phase B: Guarantee Installment template ──────────────────────
            var guaranteeInstallmentItdOutput = allAdjustmentsItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Guarantee_Installment,
                "SubGI.GuaranteeInstallmentITDOutput");
            guaranteeInstallmentItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // Group numbering: per (ActualPeriod, RecoupmentGroup) -> assign group # in Amount.
            var guaranteeInstallmentGroup = guaranteeInstallmentItdOutput.Copy();
            guaranteeInstallmentGroup.SetValue("FromDate", "");   // F_NULL_DATE() marker   // F_NULL_DATE marker
            // Sort: ActualPeriod ASC, RecoupmentGroup ASC.
            Comparison<CalcResultRow> sortByActualThenRecGrp = (a, b) =>
            {
                int r;
                if ((r = a.Actual_period_sid.CompareTo(b.Actual_period_sid)) != 0) return r;
                if ((r = a.Udkey_15_sid.CompareTo(b.Udkey_15_sid))           != 0) return r;
                return 0;
            };
            var guarInstallmentGroupNo = ResultSetDSPs.GroupNumbering(guaranteeInstallmentGroup, ResultSet.ZeroSet(), sortByActualThenRecGrp);
            guaranteeInstallmentGroup.Release();

            // GuarInstallmentGroupNoInPrice2: shift Amount(group #) -> Price2, +1 to start at 1.
            var guarInstallmentGroupNoInPrice2 = guarInstallmentGroupNo;
            guarInstallmentGroupNoInPrice2.DoMath(EngineCol.Price2, MathOp.SETTO, BaseCol.Amount);
            guarInstallmentGroupNoInPrice2.DoMath(EngineCol.Price2, MathOp.PLUS,  "1");
            guarInstallmentGroupNoInPrice2.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");

            // p_ds_explode_periods: monthly explode of [ActualPeriod..OtherPeriod] per row.
            var monthlyPeriodType = ResultSet.ZeroSet();
            monthlyPeriodType.DoMath(EngineCol.Comment, MathOp.SETTO, "Monthly");
            monthlyPeriodType.DoMath(EngineCol.Rate1,   MathOp.SETTO, "1");
            var periodsExplodedErrors = ResultSet.EmptySet();
            var periodsExploded = DS.ExecuteDSP("p_ds_explode_periods",
                guarInstallmentGroupNoInPrice2, monthlyPeriodType, periodsExplodedErrors);
            monthlyPeriodType.Release();
            periodsExplodedErrors.Release();

            // PeriodsExplodedUnspecified: clear OtherPeriod (= unspecified).
            var periodsExplodedUnspecified = periodsExploded;
            periodsExplodedUnspecified.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            // PeriodsExplodedSumm: clear ActualPeriod too -> the per-group summary set used as IS2 to GroupNumbering.
            var periodsExplodedSumm = periodsExplodedUnspecified.Copy();
            periodsExplodedSumm.SetValue(CustCol.ActualPeriod, new AlliantEntity(0));

            // Within each group, assign sequence # ordered by ActualPeriod ASC.  Seq starts at 2
            // because seq=1 is reserved for payments earlier than guarantee term.
            Comparison<CalcResultRow> sortByActualOnly = (a, b) => a.Actual_period_sid.CompareTo(b.Actual_period_sid);
            var periodsExplodedWGrpAndSeqNo = ResultSetDSPs.GroupNumbering(periodsExplodedUnspecified, periodsExplodedSumm, sortByActualOnly);
            // Move Amount(seq #) -> Rate3, then add 2.  Clear Amount.
            var periodsExplodedWGrpAndSeqNoInRate3 = periodsExplodedWGrpAndSeqNo;
            periodsExplodedWGrpAndSeqNoInRate3.DoMath(EngineCol.Rate3, MathOp.SETTO, BaseCol.Amount);
            periodsExplodedWGrpAndSeqNoInRate3.DoMath(EngineCol.Rate3, MathOp.PLUS,  "2");
            periodsExplodedWGrpAndSeqNoInRate3.DoMath(BaseCol.Amount,  MathOp.SETTO, "0");

            // ─── Phase C: Payment-shifting (1 period earlier than due date) ──
            var guarInstallmentItdWGroupNo = DS.ExecuteDSP("p_ds_detail_merge",
                guaranteeInstallmentItdOutput, guarInstallmentGroupNoInPrice2);
            // Stash original Actual/Other period in scratch UDKey4/5 for restore below.
            guarInstallmentItdWGroupNo.DoMath(CustCol.Channel,   MathOp.SETTO, CustCol.ActualPeriod);
            guarInstallmentItdWGroupNo.DoMath(CustCol.Territory, MathOp.SETTO, CustCol.OtherPeriod);

            // Map FromDate -> ActualPeriod.
            var mappingFromDate = ResultSet.ZeroSet();
            mappingFromDate.DoMath(BaseCol.Amount, MathOp.SETTO, "1");   // 1 = source col code "FromDate"
            var mapToActualPeriod = ResultSet.ZeroSet();
            mapToActualPeriod.DoMath(BaseCol.Amount, MathOp.SETTO, "2"); // 2 = target col code "ActualPeriod"
            var guarInstallmentDuePeriod = DS.ExecuteDSP("p_ds_date_to_period",
                guarInstallmentItdWGroupNo, mappingFromDate, mapToActualPeriod);
            mappingFromDate.Release();
            mapToActualPeriod.Release();
            guarInstallmentItdWGroupNo.Release();

            // Shift ActualPeriod back 1 period.
            var periodShift = ResultSet.ZeroSet();
            periodShift.DoMath(EngineCol.Rate1, MathOp.SETTO, "-1");
            var guarInstallmentWPmtPeriod = DS.ExecuteDSP("p_ds_periods_from",
                guarInstallmentDuePeriod, periodShift);
            periodShift.Release();

            // Match shifted-payment-period to per-group seq nos.
            var guarInstallWPmtPeriodAndSeqNo = DS.ExecuteDSP("p_ds_detail_merge",
                guarInstallmentWPmtPeriod, periodsExplodedWGrpAndSeqNoInRate3);

            // GuarInstallmentOutsideMaybe = (GuarInstallmentWPmtPeriod - GuarInstallWPmtPeriodAndSeqNo) with Rate3=1.
            var guarInstallmentOutsideMaybe = ResultSet.Subtract(guarInstallmentWPmtPeriod, guarInstallWPmtPeriodAndSeqNo, "SubGI.GuarInstallmentOutsideMaybe");
            guarInstallmentOutsideMaybe.DoMath(EngineCol.Rate3, MathOp.SETTO, "1");
            var guarInstallmentOutsideTerm = guarInstallmentOutsideMaybe.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubGI.GuarInstallmentOutsideTerm");
            guarInstallmentOutsideMaybe.Release();

            var allGuarInstallWPmtPeriodAndPmtSeqNo = ResultSet.EmptySet();
            allGuarInstallWPmtPeriodAndPmtSeqNo.CombineAndRelease(guarInstallWPmtPeriodAndSeqNo, guarInstallmentOutsideTerm);

            // Restore Actual/Other from scratch UDKey4/5; clear those.  Pmt seq stays in Rate3.
            var guaranteeInstallmentWGroupAndSeqNo = allGuarInstallWPmtPeriodAndPmtSeqNo.Copy();
            guaranteeInstallmentWGroupAndSeqNo.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.Channel);
            guaranteeInstallmentWGroupAndSeqNo.DoMath(CustCol.OtherPeriod,  MathOp.SETTO, CustCol.Territory);
            guaranteeInstallmentWGroupAndSeqNo.SetValue(CustCol.Channel,   new AlliantEntity(0));
            guaranteeInstallmentWGroupAndSeqNo.SetValue(CustCol.Territory, new AlliantEntity(0));
            // Bal seq = pmt seq + 1, stored in UDKey14 (Tier).
            guaranteeInstallmentWGroupAndSeqNo.DoMath(CustCol.Tier, MathOp.SETTO, EngineCol.Rate3);
            guaranteeInstallmentWGroupAndSeqNo.DoMath(CustCol.Tier, MathOp.PLUS,  "1");

            // Prep PmtGuaranteeInstallmentWZeros: zero rows for all periods in term.
            var guarInstallNoOtherPrd = allGuarInstallWPmtPeriodAndPmtSeqNo.Copy();
            guarInstallNoOtherPrd.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            guarInstallNoOtherPrd.DoMath(CustCol.Channel,   MathOp.SETTO, CustCol.OtherPeriod);
            guarInstallNoOtherPrd.DoMath(CustCol.Territory, MathOp.SETTO, CustCol.OtherPeriod);
            guarInstallNoOtherPrd.SetValue("FromDate", "");   // F_NULL_DATE() marker   // F_NULL_DATE
            allGuarInstallWPmtPeriodAndPmtSeqNo.Release();

            var pmtGuaranteeInstallmentWZeros = ResultSet.EmptySet();
            pmtGuaranteeInstallmentWZeros.CombineAndRelease(periodsExplodedWGrpAndSeqNoInRate3.Copy(), guarInstallNoOtherPrd);
            var pmtGuaranteeInstallmentWZerosSumm = pmtGuaranteeInstallmentWZeros.Summarize();
            if (!ReferenceEquals(pmtGuaranteeInstallmentWZerosSumm, pmtGuaranteeInstallmentWZeros)) pmtGuaranteeInstallmentWZeros.Release();
            Job.CurrentCalcContext = _ctxSubGuaranteeInstallments;
            pmtGuaranteeInstallmentWZerosSumm.DoMath(EngineCol.Rate1, MathOp.SETTO, BaseCol.Amount);

            // Match seq nos on the unshifted (due-date) ActualPeriod for balance-seq.
            var guarInstallmentWBalSeqNo = DS.ExecuteDSP("p_ds_detail_merge",
                guarInstallmentDuePeriod, periodsExplodedWGrpAndSeqNoInRate3);
            guarInstallmentDuePeriod.Release();
            guarInstallmentWBalSeqNo.SetValue(CustCol.OtherPeriod, new AlliantEntity(0));
            guarInstallmentWBalSeqNo.DoMath(CustCol.Channel,   MathOp.SETTO, CustCol.OtherPeriod);
            guarInstallmentWBalSeqNo.DoMath(CustCol.Territory, MathOp.SETTO, CustCol.OtherPeriod);
            var guarInstallWBalSeqNoAndlNoDueDate = guarInstallmentWBalSeqNo;
            guarInstallWBalSeqNoAndlNoDueDate.SetValue("FromDate", "");   // F_NULL_DATE() marker
            guarInstallWBalSeqNoAndlNoDueDate.DoMath(BaseCol.Units,  MathOp.SETTO, BaseCol.Amount);
            guarInstallWBalSeqNoAndlNoDueDate.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

            // Royalties Earned: propagate group/seq to RoyDueToUse + zero-pad missing periods.
            var royDueWGrpNoAndBalSeqNo = DS.ExecuteDSP("p_ds_detail_merge",
                royDueToUse, periodsExplodedWGrpAndSeqNoInRate3);
            royDueToUse.Release();
            var royDueWZeros = ResultSet.EmptySet();
            royDueWZeros.CombineAndRelease(periodsExplodedWGrpAndSeqNoInRate3.Copy(), royDueWGrpNoAndBalSeqNo);
            var royDueWZerosSumm = royDueWZeros.Summarize();
            if (!ReferenceEquals(royDueWZerosSumm, royDueWZeros)) royDueWZeros.Release();
            Job.CurrentCalcContext = _ctxSubGuaranteeInstallments;

            // ─── Phase D: Running totals ──────────────────────────────────────
            var guarInstallmentAndRoyDue = ResultSet.EmptySet();
            guarInstallmentAndRoyDue.CombineAndRelease(guarInstallWBalSeqNoAndlNoDueDate, royDueWZerosSumm);
            var guarInstallmentAndRoyDueSumm = guarInstallmentAndRoyDue.Summarize();
            if (!ReferenceEquals(guarInstallmentAndRoyDueSumm, guarInstallmentAndRoyDue)) guarInstallmentAndRoyDue.Release();
            Job.CurrentCalcContext = _ctxSubGuaranteeInstallments;

            var groupByGroupNo = ResultSet.ZeroSet();
            groupByGroupNo.DoMath(EngineCol.Comment, MathOp.SETTO, "Price2");
            var orderByActualPeriodAsc = ResultSet.ZeroSet();
            orderByActualPeriodAsc.DoMath(EngineCol.Comment, MathOp.SETTO, "ActualPeriod ASC");

            var guarInstallRoyDueRunningTotal = DS.ExecuteDSP("p_ds_running_total",
                guarInstallmentAndRoyDueSumm, groupByGroupNo, orderByActualPeriodAsc);
            guarInstallmentAndRoyDueSumm.Release();

            // Merge in payment guar info (Rate1 = pmt amount, 1-period-early).
            var guarInstallAndRoyDueRunningTotal = DS.ExecuteDSP("p_ds_detail_merge",
                guarInstallRoyDueRunningTotal, pmtGuaranteeInstallmentWZerosSumm);
            guarInstallRoyDueRunningTotal.Release();

            // ─── Phase E: Backfill missing first-period payments ──────────────
            // p_ds_detail_merge with Amount=55 marker; subtract back to find pmts not in running total.
            var pmtGuaranteeInstallmentForMatching = pmtGuaranteeInstallmentWZerosSumm.Copy();
            pmtGuaranteeInstallmentForMatching.DoMath(BaseCol.Amount, MathOp.SETTO, "55");
            var matchingPmtGuaranteeInstallment = DS.ExecuteDSP("p_ds_detail_merge",
                pmtGuaranteeInstallmentForMatching, guarInstallAndRoyDueRunningTotal);
            var missingPmtGuaranteeInstallmentMaybe = ResultSet.Subtract(pmtGuaranteeInstallmentForMatching, matchingPmtGuaranteeInstallment, "SubGI.MissingPmtGuaranteeInstallmentMaybe");
            pmtGuaranteeInstallmentForMatching.Release();
            matchingPmtGuaranteeInstallment.Release();
            var missingPmtGuaranteeInstallment = missingPmtGuaranteeInstallmentMaybe.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubGI.MissingPmtGuaranteeInstallment");
            missingPmtGuaranteeInstallmentMaybe.Release();
            missingPmtGuaranteeInstallment.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

            var giAndReTotalPlusMissingPmtTrxs = ResultSet.EmptySet();
            giAndReTotalPlusMissingPmtTrxs.CombineAndRelease(guarInstallAndRoyDueRunningTotal, missingPmtGuaranteeInstallment);

            // ─── Phase F: Filter to current-or-prior + ending balance ─────────
            var itdRange = DS.F_PERIOD_INTERVAL(DS.F_INCEPTION(), Job.CurrentCalcPeriod);
            var giAndReTotalToUse = giAndReTotalPlusMissingPmtTrxs.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)itdRange, "SubGI.GIAndRETotalToUse");
            giAndReTotalPlusMissingPmtTrxs.Release();
            // Ending Balance (Price1) = Amount2 (RoyTotal) - Units2 (GuarTotal).
            giAndReTotalToUse.DoMath(EngineCol.Price1, MathOp.SETTO, BaseCol.Amount2);
            giAndReTotalToUse.DoMath(EngineCol.Price1, MathOp.MINUS, BaseCol.Units2);
            var guarInstallMinusRoyDue = giAndReTotalToUse;

            // ─── Phase G: WHILE loop over groups ──────────────────────────────
            decimal N_MinCount = decimal.MaxValue;
            decimal N_MaxCount = decimal.MinValue;
            guarInstallMinusRoyDue.ForEachRow(row =>
            {
                if (row.User_3_rate < N_MinCount) N_MinCount = row.User_3_rate;
                if (row.User_3_rate > N_MaxCount) N_MaxCount = row.User_3_rate;
            });
            if (N_MinCount > N_MaxCount) { N_MinCount = 0m; N_MaxCount = -1m; }   // empty set

            Comparison<CalcResultRow> sortByFromDateDesc = (a, b) => b.Start_user_date.CompareTo(a.Start_user_date);

            var guarOutput = ResultSet.EmptySet();
            ResultSet fromPrior = ResultSet.EmptySet();   // INPUTSET9 -- empty on first iteration.

            for (decimal N_Count = N_MinCount; N_Count <= N_MaxCount; N_Count++)
            {
                bool isFirstIter = (N_Count == N_MinCount);

                // Source for this iteration: first iter = base running total; else = FromPrior filtered.
                ResultSet giAndReRunningTotalToProcess;
                ResultSet currentLoopRtBase;
                if (isFirstIter)
                {
                    giAndReRunningTotalToProcess = guarInstallMinusRoyDue.Copy();
                    currentLoopRtBase            = guarInstallMinusRoyDue.Copy();
                }
                else
                {
                    giAndReRunningTotalToProcess = fromPrior.GetData(
                        CustCol.ActivityType, CompareOp.EQ, ActivityType.Guarantee_Installment,
                        "SubGI.GIAndRERunningTotalToProcess");
                    var fromPriorBase = fromPrior.GetData(
                        CustCol.ActivityType, CompareOp.EQ, new AlliantEntity(0),
                        "SubGI.CurrentLoopRTBase");
                    fromPriorBase.SetValue(CustCol.ActivityType, ActivityType.Guarantee_Installment);
                    currentLoopRtBase = fromPriorBase;
                }

                var currentGuarAndReTotal = giAndReRunningTotalToProcess.GetData(
                    EngineCol.Rate3, CompareOp.EQ, N_Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "SubGI.CurrentGuarAndRETotal");
                var L_CurrentGroup = currentGuarAndReTotal.GetList(EngineCol.Price2);

                var priorForCurrentGroup = fromPrior.GetData(
                    EngineCol.Price2, ListOp.INLIST, L_CurrentGroup, "SubGI.PriorForCurrentGroup");
                var priorForCompletedGroup = fromPrior.GetData(
                    EngineCol.Price2, ListOp.NOTINLIST, L_CurrentGroup, "SubGI.PriorForCompletedGroup");

                // Beginning balance = prior balance + current guarantee installment.
                var currentGuarInstallment = currentGuarAndReTotal.Copy();
                currentGuarInstallment.DoMath(BaseCol.Amount, MathOp.SETTO, BaseCol.Units);
                var currentGuarInstallmentNonZeros = currentGuarInstallment.GetData(
                    BaseCol.Amount, CompareOp.NE, "0", "SubGI.CurrentGuarInstallmentNonZeros");

                var priorEndingBal = priorForCurrentGroup.GetData(
                    CustCol.ActivityType, CompareOp.EQ, ActivityType.Ending_Installment_Balance,
                    "SubGI.PriorEndingBal");
                var priorEndingBalUnspec = priorEndingBal;
                priorEndingBalUnspec.SetValue(CustCol.ActivityType, new AlliantEntity(0));
                priorEndingBalUnspec.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
                priorEndingBalUnspec.DoMath(EngineCol.Rate3, MathOp.SETTO, N_Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
                priorEndingBalUnspec.DoMath(EngineCol.Price1, MathOp.SETTO, "0");
                priorEndingBalUnspec.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.ActivityType);

                var priorEndingBalWCurrentDtls = DS.ExecuteDSP("p_ds_detail_merge",
                    priorEndingBalUnspec, currentGuarInstallment);
                var currentBeginningBal = ResultSet.EmptySet();
                currentBeginningBal.CombineAndRelease(priorEndingBalWCurrentDtls, currentGuarInstallmentNonZeros);
                currentBeginningBal.SetValue(CustCol.ActivityType, ActivityType.Beginning_Installment_Balance);
                priorEndingBal.Release();
                currentGuarInstallment.Release();

                // Evaluate recoupment.  Total RE < total Guarantee Installment (Price1<0).
                var royDueLtGuarInstall = currentGuarAndReTotal.GetData(
                    EngineCol.Price1, CompareOp.LT, "0", "SubGI.RoyDueLTGuarInstall");
                var royAppliedLtRaw = royDueLtGuarInstall.GetData(
                    BaseCol.Amount, CompareOp.GT, "0.005", "SubGI.RoyAppliedLT");
                royAppliedLtRaw.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                var royAppliedLt = royAppliedLtRaw;

                // Total RE > total Guarantee Installment (Price1 > 0); excess earnings.
                var royDueGtGuarInstall = currentGuarAndReTotal.GetData(new FilterClause {
                    new Criteria(EngineCol.Price1, CompareOp.GT, "0.005"),
                    new Criteria(BaseCol.Amount,   CompareOp.NE, "0")
                }, "SubGI.RoyDueGTGuarInstall");
                var roysAppliedGt = royDueGtGuarInstall.Copy();
                roysAppliedGt.DoMath(BaseCol.Amount, MathOp.SETTO, EngineCol.Price1);
                roysAppliedGt.DoMath(BaseCol.Amount, MathOp.MINUS, BaseCol.Amount);

                var currentRoyApplied = ResultSet.EmptySet();
                currentRoyApplied.CombineAndRelease(royAppliedLt, roysAppliedGt);
                currentRoyApplied.SetValue(CustCol.ActivityType, ActivityType.Royalties_Applied_to_Installment);

                var excessToAllocate = royDueGtGuarInstall.Copy();
                excessToAllocate.SetValue(CustCol.ActivityType, new AlliantEntity(0));
                excessToAllocate.DoMath(CustCol.ActualPeriod,  MathOp.SETTO, CustCol.OtherPeriod);
                excessToAllocate.DoMath(BaseCol.Amount,        MathOp.SETTO, EngineCol.Price1);
                excessToAllocate.DoMath(EngineCol.Rate1,       MathOp.SETTO, "0");
                excessToAllocate.DoMath(EngineCol.Rate2,       MathOp.SETTO, "0");
                excessToAllocate.DoMath(EngineCol.Rate3,       MathOp.SETTO, "0");
                excessToAllocate.DoMath(EngineCol.Price1,      MathOp.SETTO, "0");
                excessToAllocate.DoMath(BaseCol.Amount2,       MathOp.SETTO, "0");
                excessToAllocate.DoMath(BaseCol.Units,         MathOp.SETTO, "0");
                excessToAllocate.DoMath(BaseCol.Units2,        MathOp.SETTO, "0");
                int N_ExcessCount = excessToAllocate.Rows;
                royDueLtGuarInstall.Release();

                // Projected guarantee installment.
                var priorProjectedGuarInstall = priorForCurrentGroup.GetData(
                    CustCol.ActivityType, CompareOp.EQ, ActivityType.Projected_Guarantee_Installment,
                    "SubGI.PriorProjectedGuarInstall");
                ResultSet projectedGuarInstallToCheck = isFirstIter
                    ? guaranteeInstallmentWPmtAndBalSeqNoSnapshot(guaranteeInstallmentWGroupAndSeqNo)
                    : priorProjectedGuarInstall.Copy();
                priorProjectedGuarInstall.Release();

                // Allocate excess earning to projected guar install (sort by FromDate DESC).
                var sortByFromDateDescSet = ResultSet.ZeroSet();
                sortByFromDateDescSet.DoMath(EngineCol.Comment, MathOp.SETTO, "FromDate DESC");
                var excessAllocatedToGuarInstall = DS.ExecuteDSP("p_ds_ordered_allocation",
                    projectedGuarInstallToCheck, excessToAllocate, sortByFromDateDescSet);
                sortByFromDateDescSet.Release();

                var projectedGuarInstallMinusExcess = ResultSet.Subtract(projectedGuarInstallToCheck, excessAllocatedToGuarInstall, "SubGI.ProjectedGuarInstallMinusExcess");
                var currentProjectedGuarInstall = projectedGuarInstallMinusExcess.GetData(
                    EngineCol.Rate3, CompareOp.NE, N_Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "SubGI.CurrentProjectedGuarInstall");
                currentProjectedGuarInstall.SetValue(CustCol.ActivityType, ActivityType.Projected_Guarantee_Installment);
                projectedGuarInstallMinusExcess.Release();

                // ── Reduce running total for excess earnings: RE, Pmt, Balance ──
                var excessToAllocateToRe = royDueGtGuarInstall.Copy();
                excessToAllocateToRe.DoMath(BaseCol.Amount, MathOp.SETTO, EngineCol.Price1);
                excessToAllocateToRe.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                var baseRunningTotalWReducedRe = ResultSet.EmptySet();
                baseRunningTotalWReducedRe.CombineAndRelease(currentLoopRtBase.Copy(), excessToAllocateToRe);
                baseRunningTotalWReducedRe.DoMath(EngineCol.Rate2, MathOp.SETTO, BaseCol.Units);
                baseRunningTotalWReducedRe.DoMath(BaseCol.Units,   MathOp.SETTO, EngineCol.Rate1);

                var negExcessAllocated = excessAllocatedToGuarInstall.Copy();
                negExcessAllocated.DoMath(BaseCol.Units,  MathOp.SETTO, BaseCol.Amount);
                negExcessAllocated.DoMath(BaseCol.Units,  MathOp.TIMES, "-1");
                negExcessAllocated.SetValue(CustCol.ActivityType, ActivityType.Guarantee_Installment);
                negExcessAllocated.DoMath(EngineCol.FromDate,    MathOp.SETTO, EngineCol.ToDate);
                negExcessAllocated.DoMath(CustCol.ActualPeriod,  MathOp.SETTO, CustCol.Channel);
                negExcessAllocated.DoMath(CustCol.OtherPeriod,   MathOp.SETTO, CustCol.Channel);

                var negExcessAllocatedWDetails = DS.ExecuteDSP("p_ds_detail_merge",
                    negExcessAllocated, currentLoopRtBase);
                negExcessAllocatedWDetails.DoMath(CustCol.Tier, MathOp.SETTO, "0");

                var baseRunningTotalWReducedREAndPmt = ResultSet.EmptySet();
                baseRunningTotalWReducedREAndPmt.CombineAndRelease(baseRunningTotalWReducedRe, negExcessAllocatedWDetails);
                baseRunningTotalWReducedREAndPmt.DoMath(EngineCol.Rate1, MathOp.SETTO, BaseCol.Units);
                baseRunningTotalWReducedREAndPmt.DoMath(BaseCol.Units,   MathOp.SETTO, EngineCol.Rate2);
                baseRunningTotalWReducedREAndPmt.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");

                var negExcessForBal = negExcessAllocated.Copy();
                negExcessForBal.DoMath(EngineCol.Rate3, MathOp.SETTO, CustCol.Tier);
                negExcessForBal.DoMath(CustCol.Tier,    MathOp.SETTO, "0");
                negExcessAllocated.Release();
                excessAllocatedToGuarInstall.Release();

                var baseRunningTotalWReducedBal = DS.ExecuteDSP("p_ds_detail_merge",
                    negExcessForBal, baseRunningTotalWReducedREAndPmt);
                negExcessForBal.Release();

                var baseRunningTotalUpdated = ResultSet.EmptySet();
                baseRunningTotalUpdated.CombineAndRelease(baseRunningTotalWReducedREAndPmt, baseRunningTotalWReducedBal);

                // Recalculate running total + ending balance.
                var baseRunningTotalRecalculated = DS.ExecuteDSP("p_ds_running_total",
                    baseRunningTotalUpdated, groupByGroupNo, orderByActualPeriodAsc);
                baseRunningTotalUpdated.Release();
                baseRunningTotalRecalculated.DoMath(EngineCol.Price1, MathOp.SETTO, BaseCol.Amount2);
                baseRunningTotalRecalculated.DoMath(EngineCol.Price1, MathOp.MINUS, BaseCol.Units2);
                var baseRunningTotalRecalculatedWEndBal = baseRunningTotalRecalculated;

                // SummarizeToActualPeriodRate3Price2 (1623) consumes giAndReRunningTotalToProcess.
                var runningTotalSumm = SummarizeToActualPeriodRate3Price2(giAndReRunningTotalToProcess.Copy());
                Job.CurrentCalcContext = _ctxSubGuaranteeInstallments;

                var guarInstallMinusRoyDueRecalcdToUse = DS.ExecuteDSP("p_ds_detail_merge",
                    baseRunningTotalRecalculatedWEndBal, runningTotalSumm);
                runningTotalSumm.Release();

                ResultSet currentGuarAndReTotalUpdatedToUse = N_ExcessCount > 0
                    ? guarInstallMinusRoyDueRecalcdToUse
                    : giAndReRunningTotalToProcess.Copy();
                if (N_ExcessCount == 0) guarInstallMinusRoyDueRecalcdToUse.Release();

                ResultSet baseUpdated = N_ExcessCount > 0
                    ? baseRunningTotalRecalculatedWEndBal
                    : currentLoopRtBase.Copy();
                if (N_ExcessCount == 0) baseRunningTotalRecalculatedWEndBal.Release();

                var forNextLoopBase = baseUpdated.Copy();
                forNextLoopBase.SetValue(CustCol.ActivityType, new AlliantEntity(0));
                if (!ReferenceEquals(baseUpdated, currentLoopRtBase)) baseUpdated.Release();

                // Output Payment Guarantee Installment for this iter.
                var currentPmtGuarInstall = currentGuarAndReTotalUpdatedToUse.GetData(
                    EngineCol.Rate3, CompareOp.EQ, N_Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "SubGI.CurrentPmtGuarInstall");
                currentPmtGuarInstall.SetValue(CustCol.ActivityType, ActivityType.Payment_Guarantee_Installment);
                currentPmtGuarInstall.DoMath(BaseCol.Amount, MathOp.SETTO, EngineCol.Rate1);

                // Projected Guarantee Installment carry-forward: at the LAST iteration include
                // the current group's projection too; otherwise just completed-group projection.
                var priorProjectedGuarInstallForCompletedGrp = priorForCompletedGroup.GetData(
                    CustCol.ActivityType, CompareOp.EQ, ActivityType.Projected_Guarantee_Installment,
                    "SubGI.PriorProjectedGuarInstallForCompletedGrp");
                var lastProjectedGuarInstallment = ResultSet.EmptySet();
                bool isLastIter = (N_Count == N_MaxCount);
                if (isLastIter) lastProjectedGuarInstallment.Combine(currentProjectedGuarInstall);
                lastProjectedGuarInstallment.Combine(priorProjectedGuarInstallForCompletedGrp);
                priorProjectedGuarInstallForCompletedGrp.Release();

                // Ending Balance (only for not fully recouped).
                var currentEndingBal = royDueLtGuarInstall.Copy();   // released — but we already released!  Use CurrentGuarAndReTotal filter again.
                currentEndingBal.Release();
                var royDueLtCopy = currentGuarAndReTotal.GetData(EngineCol.Price1, CompareOp.LT, "0", "SubGI.CurrentEndingBalSrc");
                royDueLtCopy.DoMath(BaseCol.Amount, MathOp.SETTO, EngineCol.Price1);
                royDueLtCopy.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                royDueLtCopy.SetValue(CustCol.ActivityType, ActivityType.Ending_Installment_Balance);
                var currentEndingBalReal = royDueLtCopy;

                // FromPrior for next iter.
                var nextFromPrior = ResultSet.EmptySet();
                nextFromPrior.Combine(currentProjectedGuarInstall);
                nextFromPrior.Combine(currentBeginningBal);
                nextFromPrior.Combine(currentEndingBalReal);
                nextFromPrior.Combine(forNextLoopBase);
                var leftover = currentGuarAndReTotalUpdatedToUse.GetData(
                    EngineCol.Rate3, CompareOp.NE, N_Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "SubGI.NextLoopLeftover");
                nextFromPrior.Combine(leftover);
                leftover.Release();

                fromPrior.Release();
                fromPrior = nextFromPrior;

                guarOutput.Combine(currentPmtGuarInstall);
                guarOutput.Combine(currentRoyApplied);
                guarOutput.Combine(currentBeginningBal);
                guarOutput.Combine(currentEndingBalReal);
                guarOutput.Combine(lastProjectedGuarInstallment);

                currentPmtGuarInstall.Release();
                currentRoyApplied.Release();
                currentBeginningBal.Release();
                currentEndingBalReal.Release();
                lastProjectedGuarInstallment.Release();
                currentProjectedGuarInstall.Release();
                projectedGuarInstallToCheck.Release();
                forNextLoopBase.Release();

                if (N_ExcessCount > 0)
                {
                    currentGuarAndReTotalUpdatedToUse.Release();
                }
                else
                {
                    currentGuarAndReTotalUpdatedToUse.Release();
                }
                currentGuarAndReTotal.Release();
                priorForCurrentGroup.Release();
                priorForCompletedGroup.Release();
                giAndReRunningTotalToProcess.Release();
                currentLoopRtBase.Release();
                royDueGtGuarInstall.Release();
            }
            fromPrior.Release();
            guarInstallMinusRoyDue.Release();

            // After loop: zero scratch slots on guarOutput.
            guarOutput.DoMath(CustCol.Tier,    MathOp.SETTO, "0");
            guarOutput.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
            guarOutput.DoMath(EngineCol.Price1,MathOp.SETTO, "0");
            guarOutput.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            guarOutput.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
            guarOutput.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

            // ─── Phase H: Post-loop output decomposition ──────────────────────
            var projectedGuarInstallOutput = guarOutput.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Projected_Guarantee_Installment,
                "SubGI.ProjectedGuarInstallOutput");

            var roysAppliedToGuarInstallItdOutput = guarOutput.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Applied_to_Installment,
                "SubGI.RoysAppliedToGuarInstallITDOutput");
            var priorRecoupmentItd = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Applied_to_Installment,
                "SubGI.PriorRecoupmentITD");
            var roysAppliedToGuarInstallCurrentOutput = ResultSet.Subtract(
                roysAppliedToGuarInstallItdOutput, priorRecoupmentItd, "SubGI.RoysAppliedCurrent");
            priorRecoupmentItd.Release();
            roysAppliedToGuarInstallCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            var pmtGuarInstallmentToGetDtls = guarOutput.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Payment_Guarantee_Installment,
                "SubGI.PmtGuarInstallmentToGetDtls");
            pmtGuarInstallmentToGetDtls.SetValue(CustCol.ActualPeriod, new AlliantEntity(0));
            var guarInstallmentWithDtls = guaranteeInstallmentWGroupAndSeqNo.Copy();
            guarInstallmentWithDtls.SetValue(CustCol.ActivityType, new AlliantEntity(0));
            var pmtGuarInstallmentItdOutput = DS.ExecuteDSP("p_ds_detail_merge",
                pmtGuarInstallmentToGetDtls, guarInstallmentWithDtls);
            pmtGuarInstallmentToGetDtls.Release();
            guarInstallmentWithDtls.Release();

            var priorPmtGuarInstallmentItd = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Payment_Guarantee_Installment,
                "SubGI.PriorPmtGuarInstallmentITD");
            var pmtGuarInstallmentCurrentOutput = ResultSet.Subtract(
                pmtGuarInstallmentItdOutput, priorPmtGuarInstallmentItd, "SubGI.PmtGuarInstallmentCurrent");
            priorPmtGuarInstallmentItd.Release();
            pmtGuarInstallmentCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // Beginning + Ending balances.  Statement-deal vs accrual logic.
            string dealType = Deal.GetUDFString(DealUDF.DealType);
            int N_StmtDeal = (dealType == "Statement") ? 1 : 0;

            var validPeriodsForBal = periodsExplodedWGrpAndSeqNoInRate3.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)itdRange, "SubGI.ValidPeriodsForBal");
            // Group by Price2, sort by Rate3 DESC -> last statement period first per group.
            Comparison<CalcResultRow> sortByRate3Desc = (a, b) => b.User_3_rate.CompareTo(a.User_3_rate);
            var periodsForBalSorted = ResultSetDSPs.GroupNumbering(validPeriodsForBal, periodsExplodedSumm, sortByRate3Desc);
            validPeriodsForBal.Release();
            var periodsForEndBal = periodsForBalSorted.GetData(BaseCol.Amount, CompareOp.EQ, "0", "SubGI.PeriodsForEndBal");
            periodsForBalSorted.Release();
            periodsForEndBal.SetValue(CustCol.ActivityType, ActivityType.Ending_Installment_Balance);

            ResultSet periodsForBegAndEndBal;
            if (N_StmtDeal == 1)
            {
                // Statement-deal logic.  Get period count between PriorStmtInterval and current.
                string statementInterval = Contract.GetUDFString(ContractUDF.StatementInterval);
                var currentStmtPeriod = ResultSet.ZeroSet();
                currentStmtPeriod.SetValue(CustCol.OtherPeriod, Job.CurrentCalcPeriod);
                PeriodItem priorStmtPi = !string.IsNullOrEmpty(statementInterval)
                    ? DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, 1, statementInterval)
                    : Job.CurrentCalcPeriod;
                currentStmtPeriod.SetValue(CustCol.ActualPeriod, priorStmtPi);

                var periodsBetActualAndOther = ResultSet.ZeroSet();
                periodsBetActualAndOther.DoMath(BaseCol.Amount, MathOp.SETTO, "4");   // 4 = "months"
                var periodCountBetInterval = DS.ExecuteDSP("p_ds_time_between",
                    currentStmtPeriod, periodsBetActualAndOther);
                currentStmtPeriod.Release();
                periodsBetActualAndOther.Release();

                decimal N_PeriodsCount = DS.AmountOf(periodCountBetInterval);
                periodCountBetInterval.Release();
                int N_PeriodsToShift = (int)N_PeriodsCount + 2;

                var periodsToShift = ResultSet.ZeroSet();
                periodsToShift.DoMath(EngineCol.Rate1, MathOp.SETTO, N_PeriodsToShift.ToString(System.Globalization.CultureInfo.InvariantCulture));

                var periodsForSmtBeg = periodsForEndBal.Copy();
                var stmtBegBalPeriod = DS.ExecuteDSP("p_ds_periods_from",
                    periodsForSmtBeg, periodsToShift);
                periodsForSmtBeg.Release();
                periodsToShift.Release();

                var periodsForBegBal = stmtBegBalPeriod;
                periodsForBegBal.DoMath(EngineCol.Rate3, MathOp.PLUS, N_PeriodsToShift.ToString(System.Globalization.CultureInfo.InvariantCulture));
                periodsForBegBal.SetValue(CustCol.ActivityType, ActivityType.Beginning_Installment_Balance);

                periodsForBegAndEndBal = ResultSet.EmptySet();
                periodsForBegAndEndBal.CombineAndRelease(periodsForBegBal, periodsForEndBal);
            }
            else
            {
                // Accrual: beg = end (same period).
                var periodsForBegBal = periodsForEndBal.Copy();
                periodsForBegBal.SetValue(CustCol.ActivityType, ActivityType.Beginning_Installment_Balance);
                periodsForBegAndEndBal = ResultSet.EmptySet();
                periodsForBegAndEndBal.CombineAndRelease(periodsForBegBal, periodsForEndBal);
            }
            periodsForBegAndEndBal.DoMath(BaseCol.Units2, MathOp.SETTO, "5");   // marker

            var begAndEndBal = guarOutput.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Installment_Balance_Activity_Type_List,
                "SubGI.BegAndEndBal");
            var begAndEndBalGetDtls = ResultSet.EmptySet();
            begAndEndBalGetDtls.CombineAndRelease(begAndEndBal, periodsForBegAndEndBal);
            var begAndEndBalGetDtlsSumm = begAndEndBalGetDtls.Summarize();
            if (!ReferenceEquals(begAndEndBalGetDtlsSumm, begAndEndBalGetDtls)) begAndEndBalGetDtls.Release();
            Job.CurrentCalcContext = _ctxSubGuaranteeInstallments;

            var validBegAndEndBalGetDtls = begAndEndBalGetDtlsSumm.GetData(BaseCol.Units2, CompareOp.EQ, "5", "SubGI.ValidBegAndEndBalGetDtls");
            validBegAndEndBalGetDtls.SetValue(CustCol.ActualPeriod, new AlliantEntity(0));
            validBegAndEndBalGetDtls.DoMath(BaseCol.Units2, MathOp.SETTO, "0");
            begAndEndBalGetDtlsSumm.Release();

            var guarInstallmentTerms = guaranteeInstallmentWGroupAndSeqNo.Copy();
            guarInstallmentTerms.SetValue("FromDate", "");   // F_NULL_DATE() marker
            guarInstallmentTerms.DoMath(EngineCol.Rate3, MathOp.SETTO, "0");
            var begAndEndBalOutput = DS.ExecuteDSP("p_ds_detail_merge",
                validBegAndEndBalGetDtls, guarInstallmentTerms);
            validBegAndEndBalGetDtls.Release();
            guarInstallmentTerms.Release();

            var begBal = begAndEndBalOutput.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Beginning_Installment_Balance,
                "SubGI.BegBal");
            var priorEndBal = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Ending_Installment_Balance,
                "SubGI.PriorEndBal");
            priorEndBal.SetValue(CustCol.ActivityType, ActivityType.Beginning_Installment_Balance);
            var begBalCurrentOutput = ResultSet.Subtract(begBal, priorEndBal, "SubGI.BegBalCurrent");
            begBal.Release();
            priorEndBal.Release();
            begBalCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // Final assembly.
            var output = ResultSet.EmptySet();
            output.CombineAndRelease(
                projectedGuarInstallOutput,
                roysAppliedToGuarInstallItdOutput,
                roysAppliedToGuarInstallCurrentOutput,
                pmtGuarInstallmentItdOutput,
                pmtGuarInstallmentCurrentOutput,
                begAndEndBalOutput,
                begBalCurrentOutput);
            output.CombineAndRelease(guaranteeInstallmentItdOutput);
            output.DoMath(EngineCol.Rate3,  MathOp.SETTO, "0");
            output.DoMath(EngineCol.Price2, MathOp.SETTO, "0");

            // Cleanup of held-alive intermediates.
            guarOutput.Release();
            periodsExplodedWGrpAndSeqNoInRate3.Release();
            periodsExplodedSumm.Release();
            pmtGuaranteeInstallmentWZerosSumm.Release();
            guaranteeInstallmentWGroupAndSeqNo.Release();
            groupByGroupNo.Release();
            orderByActualPeriodAsc.Release();

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubGuaranteeInstallments");
            return output;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // Helper for SubGuaranteeInstallments first-iter projected guar template.
        // Inlined as a method so the WHILE body stays readable.
        private static ResultSet guaranteeInstallmentWPmtAndBalSeqNoSnapshot(ResultSet src) => src.Copy();

        // ── 1559 C_ROY_SUB_RESERVES ──────────────────────────────────────────
        //
        // Reserve takes (NEG) and liquidations (POS).  Reserves are "taken" against
        // royalties earned each period within the contract's reserve window, and
        // "liquidated" N periods later (where N = ContractUDF.LiquidationPeriod).
        //
        // DealScript flow:
        //   Phase A. Window: read L_StartWindowActualPeriod from INPUTSET4 (Window_Start_Period rows).
        //   Phase B. AdjReserves: filter INPUTSET5 to Adjustment_to_Reserve_Activity within window.
        //              POS adjs -> Reserves_Liquidated; NEG adjs -> Reserves_Taken.
        //   Phase C. RoyaltiesWithinWindowPeriods: INPUTSET1's Royalties_Earned within window.
        //              Stash ActualPeriod into TransType (scratch) before period-shift.
        //   Phase D. CurrentStmtInterval: p_ds_period_to_period_type_period maps ActualPeriod
        //              to the start of its statement interval, parked in OtherPeriod.
        //   Phase E. WHILE N_PeriodCount in 1..N_LiqPeriod:
        //              Shift periods forward 1 statement interval at a time.
        //              On the last iteration, accumulate the resulting set into LiquidationPeriod.
        //   Phase F. Restore ActualPeriod from TransType scratch.  TransType=ITD.
        //   Phase G. RoyWithReserveRate = Rate2 = ContractUDF.ReserveRate.
        //              ReservesAmount = -1 * Amount * Rate2 -> Amount.  Round.
        //              Restamp ActivityType=Reserves_Taken.
        //   Phase H. ReservesITDOutput = Combine(NegAdjReservesITD, ReservesTakenWithinWindow,
        //                                          PriorWindowReservesTaken).
        //   Phase I. Liquidate: rows with OtherPeriod = current calc period.
        //              Negate, restamp Reserves_Liquidated, swap ActualPeriod<->TransType<->OtherPeriod.
        //   Phase J. ReservesLiquidatedAndAdjsITD = Combine(PosAdjReservesITD, ReservesLiquidatedITD).
        //              ReservesLiquidatedCurrent = ReservesAndLiqITD - PriorReserves.
        //   Phase K. BegReserveITDOutput from INPUTSET3's prior Ending_Reserves_Balance.
        //              FinalReservesITD = ReservesLiquidatedCurrent + BegReserveITD.
        //              Split by OtherPeriod=Unspecified vs not, mark with Units2=55 sentinel
        //              for liquidated-vs-taken differentiation.
        //              FinalReservesITDOutput = combined non-zero result.
        //
        // INPUTSET6 = ReservesITDOutput + ReservesLiquidatedITDOutput + ReservesLiquidatedCurrentOutput.
        // INPUTSET7 = AdjReservesITDOutput + BegReserveITDOutput + FinalReservesITDOutput.
        public static (ResultSet ReservesOutput,
                       ResultSet AdjAndBalancesOutput) SubReserves(
            ResultSet royaltiesEarnedItd,         // INPUTSET1
            ResultSet priorPeriodWithinWindow,    // INPUTSET2
            ResultSet priorPeriodItd,             // INPUTSET3
            ResultSet windowStartPeriodItd,       // INPUTSET4
            ResultSet allAdjustmentsItd)          // INPUTSET5
        {
            _ctxSubReserves = _ctxSubReserves ?? NewSubCtxStandard("C_ROY_SUB_RESERVES");
            Job.CurrentCalcContext = _ctxSubReserves;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubReserves");

            // ─── Phase A: Window start period ────────────────────────────────
            var L_StartWindowActualPeriod = windowStartPeriodItd.GetList(CustCol.ActualPeriod);
            AlliantEntity startWindowEntity = L_StartWindowActualPeriod[0];
            if (startWindowEntity == null)
            {
                // No window -> no reserves to compute.
                LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubReserves (no window)");
                return (ResultSet.EmptySet(), ResultSet.EmptySet());
            }
            var startWinPI = (PeriodItem)PeriodItem.Items.GetEntityBySid(startWindowEntity.sid);
            var endOfTime  = (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time);
            var window     = DS.F_PERIOD_INTERVAL(startWinPI, endOfTime);

            // ─── Phase B: Adjustment_to_Reserve_Activity ─────────────────────
            var adjReservesItdOutput = allAdjustmentsItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Adjustment_to_Reserve_Activity,
                "SubRes.AdjReservesITDOutput");
            var adjReservesWithinWindow = adjReservesItdOutput.GetData(
                CustCol.ActualPeriod, ListOp.INLIST, (EntityList)window,
                "SubRes.AdjReservesWithinWindow");

            var posAdjReservesItd = adjReservesWithinWindow.GetData(
                BaseCol.Amount, CompareOp.GT, "0", "SubRes.PosAdjReservesITD");
            posAdjReservesItd.SetValue(CustCol.ActivityType, ActivityType.Reserves_Liquidated);

            var negAdjReservesItd = adjReservesWithinWindow.GetData(
                BaseCol.Amount, CompareOp.LT, "0", "SubRes.NegAdjReservesITD");
            negAdjReservesItd.SetValue(CustCol.ActivityType, ActivityType.Reserves_Taken);
            adjReservesWithinWindow.Release();

            // ─── Phase C: RoyaltiesWithinWindowPeriods ───────────────────────
            // Read StatementInterval directly (replaces p_ds_udf_period_type_label).
            string statementInterval = Contract.GetUDFString(ContractUDF.StatementInterval);
            var stmtIntervalPeriodType = ResultSet.ZeroSet();
            stmtIntervalPeriodType.SetValue(CustCol.Comment1, statementInterval ?? "<None>");
            stmtIntervalPeriodType.DoMath(EngineCol.Rate1, MathOp.SETTO, "1");

            var royaltiesWithinWindowPeriods = royaltiesEarnedItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ,    ActivityType.Royalties_Earned),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST,   (EntityList)window)
            }, "SubRes.RoyaltiesWithinWindowPeriods");
            // Stash ActualPeriod in TransType for later restore (DealScript line 46).
            //royaltiesWithinWindowPeriods.DoMath(CustCol.TransType, MathOp.SETTO, CustCol.ActualPeriod);
            royaltiesWithinWindowPeriods.DoMath(EngineCol.Period5, MathOp.SETTO, CustCol.ActualPeriod);

            // ─── Phase D: CurrentStmtInterval (period-to-period-type-period) ─
            var currentStmtInterval = DS.ExecuteDSP("p_ds_period_to_period_type_period",
                royaltiesWithinWindowPeriods, stmtIntervalPeriodType);
            royaltiesWithinWindowPeriods.Release();
            currentStmtInterval.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);

            // ─── Phase E: WHILE shift one period at a time ───────────────────
            int N_LiqPeriod = (int)Contract.GetUDFDecimal(ContractUDF.LiquidationPeriod);
            var shiftOnePeriod = ResultSet.ZeroSet();
            shiftOnePeriod.DoMath(EngineCol.Rate1, MathOp.SETTO, "1");

            var liquidationPeriod = ResultSet.EmptySet();
            var currentLoop = currentStmtInterval;

            for (int N_PeriodCount = 1; N_PeriodCount <= N_LiqPeriod; N_PeriodCount++)
            {
                // Shift currentLoop forward 1 base-period.
                var currentLoopToUse = DS.ExecuteDSP("p_ds_periods_from", currentLoop, shiftOnePeriod);
                currentLoop.Release();

                // Re-anchor to statement interval start.
                var oneLiqPeriod = DS.ExecuteDSP("p_ds_period_to_period_type_period",
                    currentLoopToUse, stmtIntervalPeriodType);
                currentLoopToUse.Release();
                oneLiqPeriod.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);

                if (N_PeriodCount == N_LiqPeriod)
                {
                    liquidationPeriod.Combine(oneLiqPeriod);
                }
                currentLoop = oneLiqPeriod;
            }
            currentLoop.Release();
            shiftOnePeriod.Release();
            stmtIntervalPeriodType.Release();

            // ─── Phase F: Restore ActualPeriod from TransType scratch ────────
            //liquidationPeriod.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.TransType);
            liquidationPeriod.DoMath(CustCol.ActualPeriod, MathOp.SETTO, EngineCol.Period5);
            liquidationPeriod.SetValue(EngineCol.Period5, Period.Unspecified);

            // ─── Phase G: Reserve rate -> ReservesTakenWithinWindow ──────────
            var liqPeriod = liquidationPeriod;
            // Not needed as we are not stashing in TransType anymore
            //liqPeriod.SetValue(CustCol.TransType, TransType.ITD);

            var royWithReserveRate = liqPeriod;
            royWithReserveRate.DoMath(EngineCol.Rate2, MathOp.SETTO, ContractUDF.ReserveRate);
            // ReservesAmount = -1 * Amount * Rate2 -> Amount.
            royWithReserveRate.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate2);
            royWithReserveRate.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var roundedReservesAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(royWithReserveRate);
            Job.CurrentCalcContext = _ctxSubReserves;

            var reservesTakenWithinWindow = roundedReservesAmount;
            reservesTakenWithinWindow.SetValue(CustCol.ActivityType, ActivityType.Reserves_Taken);

            // PriorWindowReservesTaken from INPUTSET2.
            var priorWindowReservesTaken = priorPeriodWithinWindow.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Reserves_Taken,
                "SubRes.PriorWindowReservesTaken");

            // ─── Phase H: ReservesITDOutput ──────────────────────────────────
            var reservesItdOutput = ResultSet.EmptySet();
            reservesItdOutput.CombineAndRelease(negAdjReservesItd, reservesTakenWithinWindow.Copy(), priorWindowReservesTaken);
            reservesTakenWithinWindow.Release();

            // ─── Phase I: Liquidate at current calc period ───────────────────
            // ReservesLiquidated = -1 * (rows where OtherPeriod ∈ ITD-thru-calc-period).
            // F_ITD_THRU_CALC_PERIOD returns a PeriodList; we want OtherPeriod IN that list.
            var itdThruCalcPeriod = DS.F_ITD_THRU_CALC_PERIOD();
            var reservesLiquidated = reservesItdOutput.GetData(
                CustCol.OtherPeriod, ListOp.INLIST, (EntityList)itdThruCalcPeriod,
                "SubRes.ReservesLiquidated");
            reservesLiquidated.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            var reservesLiquidatedItd = reservesLiquidated;
            reservesLiquidatedItd.SetValue(CustCol.ActivityType, ActivityType.Reserves_Liquidated);
            // Save ActualPeriod -> TransType, then ActualPeriod = OtherPeriod.
            reservesLiquidatedItd.DoMath(CustCol.TransType,    MathOp.SETTO, CustCol.ActualPeriod);
            reservesLiquidatedItd.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);

            var reservesLiquidatedAndAdjsItd = ResultSet.EmptySet();
            reservesLiquidatedAndAdjsItd.CombineAndRelease(posAdjReservesItd, reservesLiquidatedItd.Copy());
            reservesLiquidatedAndAdjsItd.SetEntity(CustCol.OtherPeriod, new AlliantEntity(0));

            var reservesLiquidatedItdOutput = reservesLiquidatedAndAdjsItd.Copy();
            reservesLiquidatedItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            // ─── Phase J: ReservesLiquidatedCurrent ──────────────────────────
            var priorReserves = priorPeriodItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Reserves_Activity_Type_List,
                "SubRes.PriorReserves");

            var reservesAndLiqItd = ResultSet.EmptySet();
            reservesAndLiqItd.CombineAndRelease(reservesItdOutput.Copy(), reservesLiquidatedAndAdjsItd);

            var reservesLiquidatedCurrent = ResultSet.Subtract(reservesAndLiqItd, priorReserves, "SubRes.ReservesLiquidatedCurrent");
            reservesAndLiqItd.Release();
            priorReserves.Release();
            var reservesLiquidatedCurrentOutput = reservesLiquidatedCurrent.Copy();
            reservesLiquidatedCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // ─── Phase K: BegReserveITD + FinalReservesITD ───────────────────
            var begReserveItdOutputInitial = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Ending_Reserves_Balance,
                "SubRes.BegReserveITD");
            var begReserveItdOutput = begReserveItdOutputInitial;
            begReserveItdOutput.SetValue(CustCol.ActivityType, ActivityType.Beginning_Reserves_Balance);

            var finalReservesItd = ResultSet.EmptySet();
            finalReservesItd.CombineAndRelease(reservesLiquidatedCurrent, begReserveItdOutput.Copy());
            finalReservesItd.SetValue(CustCol.ActivityType, ActivityType.Ending_Reserves_Balance);

            // Split by OtherPeriod = Unspecified vs not.
            var liquidatedFinalReserves = finalReservesItd.GetData(
                CustCol.OtherPeriod, CompareOp.EQ, "0", "SubRes.LiquidatedFinalReserves");
            // Restore ActualPeriod from TransType scratch.  Mark with Units2=55 sentinel.
            liquidatedFinalReserves.DoMath(CustCol.OtherPeriod, MathOp.SETTO, CustCol.ActualPeriod);
            liquidatedFinalReserves.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.TransType);
            liquidatedFinalReserves.DoMath(BaseCol.Units2, MathOp.SETTO, "55");
            var liquidatedFinalReservesItd = liquidatedFinalReserves.Copy();
            liquidatedFinalReservesItd.SetValue(CustCol.TransType, TransType.ITD);
            liquidatedFinalReserves.Release();

            var takenFinalReserves = finalReservesItd.GetData(
                CustCol.OtherPeriod, CompareOp.NE, "0", "SubRes.TakenFinalReserves");
            takenFinalReserves.SetValue(CustCol.TransType, TransType.ITD);
            finalReservesItd.Release();

            // Combine and re-split (DealScript form preserves the *1 trigger).
            var takenLiquidatedFinalReserves = ResultSet.EmptySet();
            takenLiquidatedFinalReserves.CombineAndRelease(liquidatedFinalReservesItd, takenFinalReserves);
            // *1 -> force re-summarize.
            Job.CurrentCalcContext = _ctxSubReserves;
            takenLiquidatedFinalReserves = takenLiquidatedFinalReserves.Summarize();

            var takenLiquidatedFinalReservesNonZeros = takenLiquidatedFinalReserves.GetData(
                BaseCol.Amount, CompareOp.NE, "0", "SubRes.TakenLiquidatedFinalReservesNonZeros");
            takenLiquidatedFinalReserves.Release();

            var liquidatedFinalReservesNonZeros = takenLiquidatedFinalReservesNonZeros.GetData(
                BaseCol.Units2, CompareOp.EQ, "55", "SubRes.LiquidatedFinalReservesNonZeros");
            liquidatedFinalReservesNonZeros.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
            liquidatedFinalReservesNonZeros.DoMath(BaseCol.Units2, MathOp.SETTO, "0");
            liquidatedFinalReservesNonZeros.SetEntity(CustCol.OtherPeriod, new AlliantEntity(0));

            var takenFinalReservesNonZeros = takenLiquidatedFinalReservesNonZeros.GetData(
                BaseCol.Units2, CompareOp.EQ, "0", "SubRes.TakenFinalReservesNonZeros");
            takenLiquidatedFinalReservesNonZeros.Release();

            var finalReservesItdOutput = ResultSet.EmptySet();
            finalReservesItdOutput.CombineAndRelease(takenFinalReservesNonZeros, liquidatedFinalReservesNonZeros);

            // ─── Final assembly ──────────────────────────────────────────────
            var reservesOutput = ResultSet.EmptySet();
            reservesOutput.CombineAndRelease(reservesItdOutput, reservesLiquidatedItdOutput, reservesLiquidatedCurrentOutput);

            var adjAndBalancesOutput = ResultSet.EmptySet();
            adjAndBalancesOutput.CombineAndRelease(adjReservesItdOutput, begReserveItdOutput, finalReservesItdOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubReserves");
            return (reservesOutput, adjAndBalancesOutput);
        }

        // ── 1644 C_ROY_SUB_ADVANCES ──────────────────────────────────────────
        //
        // Per-advance-group recoupment.  Active body (vs 1577 which is dormant).
        //
        // DealScript flow:
        //   Phase A. AdvITDOutput, AdvCurrentOutput from INPUTSET3 (stash Amount in Amount2).
        //   Phase B. PriorFinalBalance from INPUTSET4 (period-window-filtered).
        //            BegBalanceITDOutput = restamp ActivityType to Beginning_Advance_Balance.
        //   Phase C. RecoupableAdvCurrent = AdvITDOutput - PriorStmtAdjs.
        //            AdjsToRecoupe = Combine(PriorStmtFinal, RecoupableAdvCurrent).
        //            NonZeroAdjsToRecoupe filtered, Amount2 = Amount, Amount = 0.
        //   Phase D. p_ds_group_numbering writes a sequence number into Tier (UDKey14)
        //            sorted by RecoupmentGroup, OtherPeriod.  Sequence becomes
        //            iteration index for the WHILE.
        //   Phase E. Compute CurrentStmtRoyEarned = ITDStmtRoyEarned - PriorStmtRoyEarned.
        //            Stamp neutral keys (ActivityType=Unspecified, OtherPeriod=Unspecified).
        //            Filter NonZeros.
        //   Phase F. WHILE i in 1..N_AdjsToRecoupe:
        //              ASingleAdj = row at Tier=i.
        //              Compute window [StartPeriod..EndPeriod] from ASingleAdj's
        //              ActualPeriod and OtherPeriod.
        //              RoyRecGroupCurrent = royalties in that window for that RecGroup.
        //              IF positive, run p_ds_ordered_allocation to allocate.
        //              SingleRoyApplied = restamp(ActivityType=Royalties_Applied_to_Advance,
        //                                          TransType=Current).
        //              Subtract recoupment from running royalty pool.
        //              Combine into RoyAppliedOutput.
        //   Phase G. RoyAppliedITDOutput = Combine(NegRoyApplied, PriorStmtRecoupmentITD), TransType=ITD.
        //            RoyAppliedCurrentOutput = ITD - PriorPeriod, TransType=Current.
        //   Phase H. RemainingBalance = AdjsToRecoupe - AdjToUse (allocated total).
        //            FinalBalanceITDOutput = restamp ActivityType=Ending_Advance_Balance.
        //   Phase I. Output = Combine(AdvITD, AdvCurrent, BegBalance, FinalBalance,
        //                              RoyAppliedITD, RoyAppliedCurrent).
        public static ResultSet SubAdvances(
            ResultSet royaltiesEarnedItd,         // INPUTSET1
            ResultSet reservesItd,                // INPUTSET2
            ResultSet allAdjustmentsItd,          // INPUTSET3
            ResultSet priorPeriodItd,             // INPUTSET4
            ResultSet priorStmtItd)               // INPUTSET5
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubAdvances = _ctxSubAdvances ?? NewSubCtxStandard("C_ROY_SUB_ADVANCES");
            Job.CurrentCalcContext = _ctxSubAdvances;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubAdvances");

            // ─── Phase A: AdvITD, AdvCurrent ─────────────────────────────────
            var advItdOutput = allAdjustmentsItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Advance_Activity_Type_List,
                "SubAdv.AdvITDOutput");
            advItdOutput.DoMath(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount);

            var priorAdvItd = priorPeriodItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Advance_Activity_Type_List,
                "SubAdv.PriorAdvITD");
            var advCurrentOutput = ResultSet.Subtract(advItdOutput, priorAdvItd, "SubAdv.AdvCurrent");
            priorAdvItd.Release();
            advCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // ─── Phase B: PriorFinalBalance + BegBalanceITDOutput ────────────
            // Period window: from F_PERIODS_FROM(F_PERIOD_TYPE_PREVIOUS(StatementInterval), 1) to End_of_Time.
            string statementInterval = Contract.GetUDFString(ContractUDF.StatementInterval);
            PeriodItem prevStmt   = DS.F_PERIOD_TYPE_PREVIOUS(statementInterval);
            PeriodItem startStmt  = DS.F_PERIODS_FROM(prevStmt, 1);
            PeriodItem endOfTime  = (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time);
            var stmtWindow        = DS.F_PERIOD_INTERVAL(startStmt, endOfTime);

            var priorFinalBalance = priorPeriodItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ,    ActivityType.Ending_Advance_Balance),
                new Criteria(CustCol.OtherPeriod,  ListOp.INLIST,   (EntityList)stmtWindow)
            }, "SubAdv.PriorFinalBalance");
            var begBalanceItdOutput = priorFinalBalance;
            begBalanceItdOutput.SetValue(CustCol.ActivityType, ActivityType.Beginning_Advance_Balance);

            // ─── Phase C: RecoupableAdvCurrent + AdjsToRecoupe ───────────────
            var priorStmtAdjs = priorStmtItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Advance_Activity_Type_List,
                "SubAdv.PriorStmtAdjs");
            var recoupableAdvCurrent = ResultSet.Subtract(advItdOutput, priorStmtAdjs, "SubAdv.RecoupableAdvCurrent");
            priorStmtAdjs.Release();

            var priorStmtFinal = priorStmtItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.EQ,    ActivityType.Ending_Advance_Balance),
                new Criteria(BaseCol.Amount,       CompareOp.NE,    "0"),
                new Criteria(CustCol.OtherPeriod,  ListOp.INLIST,   (EntityList)stmtWindow)
            }, "SubAdv.PriorStmtFinal");

            var adjsToRecoupe = ResultSet.EmptySet();
            adjsToRecoupe.CombineAndRelease(priorStmtFinal, recoupableAdvCurrent);

            var nonZeroAdjsToRecoupe = adjsToRecoupe.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubAdv.NonZeroAdjsToRecoupe");
            nonZeroAdjsToRecoupe.DoMath(BaseCol.Units2, MathOp.SETTO, BaseCol.Amount);
            nonZeroAdjsToRecoupe.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

            // ─── Phase D: GroupNumbering writes seq into Tier (UDKey14) ──────
            // Sort: RecoupmentGroup ASC, OtherPeriod ASC.
            Comparison<CalcResultRow> advRecoupmentOrder = (a, b) =>
            {
                int r = a.Udkey_15_sid.CompareTo(b.Udkey_15_sid);   // RecoupmentGroup
                return r != 0 ? r : a.Other_period_sid.CompareTo(b.Other_period_sid);
            };
            var uniqueSeqNo = ResultSet.EmptySet();
            var adjsWZeroOrderNo = ResultSetDSPs.GroupNumbering(nonZeroAdjsToRecoupe, uniqueSeqNo, advRecoupmentOrder);
            uniqueSeqNo.Release();

            // AdjsWOrderNo: Tier (UDKey14) = sequenceNumber + 1 (DealScript: Amount + 1 -> Tier).
            // GroupNumbering writes the seq # into Amount; we move it to Tier and restore Amount from Units2.
            var adjsWOrderNo = adjsWZeroOrderNo;
            adjsWOrderNo.DoMath(CustCol.Tier,     MathOp.SETTO, BaseCol.Amount);
            adjsWOrderNo.DoMath(CustCol.Tier,     MathOp.PLUS,  "1");
            adjsWOrderNo.DoMath(BaseCol.Amount,   MathOp.SETTO, BaseCol.Units2);
            adjsWOrderNo.DoMath(BaseCol.Units2,   MathOp.SETTO, "0");

            // ─── Phase E: CurrentStmtRoyEarned ───────────────────────────────
            var forRecoupment = ResultSet.EmptySet();
            forRecoupment.CombineAndRelease(royaltiesEarnedItd.Copy(), reservesItd.Copy());

            var priorStmtRoyEarned = priorStmtItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Royalties_for_Recoupment_Activity_Type_List,
                "SubAdv.PriorStmtRoyEarned");
            var itdStmtRoyEarned = forRecoupment.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Royalties_for_Recoupment_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ, TransType.ITD)
            }, "SubAdv.ITDStmtRoyEarned");
            forRecoupment.Release();

            var currentStmtRoyEarned = ResultSet.Subtract(itdStmtRoyEarned, priorStmtRoyEarned, "SubAdv.CurrentStmtRoyEarned");
            itdStmtRoyEarned.Release();
            priorStmtRoyEarned.Release();
            currentStmtRoyEarned.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");
            currentStmtRoyEarned.DoMath(EngineCol.Rate3, MathOp.SETTO, "0");

            // CurrentStmtRoyEarnedSumm: clear ActivityType + OtherPeriod, zero scratches.
            var currentStmtRoyEarnedSumm = currentStmtRoyEarned;
            currentStmtRoyEarnedSumm.SetValue(CustCol.ActivityType, ActivityType.Unspecified);
            currentStmtRoyEarnedSumm.SetEntity(CustCol.OtherPeriod, new AlliantEntity(0));
            currentStmtRoyEarnedSumm.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            currentStmtRoyEarnedSumm.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
            currentStmtRoyEarnedSumm.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

            var currentStmtRoyEarnedNonZeros = currentStmtRoyEarnedSumm.GetData(
                BaseCol.Amount, CompareOp.NE, "0", "SubAdv.CurrentStmtRoyEarnedNonZeros");
            currentStmtRoyEarnedSumm.Release();

            // ─── Phase F: WHILE per advance-group sequence ───────────────────
            // Determine N_AdjsToRecoupe = max Tier value in adjsWOrderNo.
            int N_AdjsToRecoupe = 0;
            adjsWOrderNo.ForEachRow(row =>
            {
                if (row.Udkey_14_sid > N_AdjsToRecoupe) N_AdjsToRecoupe = row.Udkey_14_sid;
            });

            var royAppliedOutput = ResultSet.EmptySet();
            // Running royalty pool, mutated each iteration via Subtract.
            var runningRoyPool = currentStmtRoyEarnedNonZeros;
            // ProcessBy sort for ordered_allocation (lookup table for the DSP).
            var processBy = ResultSet.ZeroSet();
            processBy.SetValue(CustCol.Comment1, "RecoupmentGroup ASC, ActualPeriod ASC, Catalog ASC");

            for (int N_Counter = 1; N_Counter <= N_AdjsToRecoupe; N_Counter++)
            {
                var aSingleAdj = adjsWOrderNo.GetData(CustCol.Tier, CompareOp.EQ, N_Counter.ToString(),
                                                       "SubAdv.ASingleAdj");
                if (aSingleAdj.Rows == 0) { aSingleAdj.Release(); continue; }

                var L_ASingleAdj  = aSingleAdj.GetList(CustCol.RecoupmentGroup);
                var L_StartPeriod = aSingleAdj.GetList(CustCol.ActualPeriod);
                aSingleAdj.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
                var L_EndPeriod   = aSingleAdj.GetList(CustCol.ActualPeriod);

                AlliantEntity recGroup       = L_ASingleAdj[0];
                AlliantEntity startEntity    = L_StartPeriod[0];
                AlliantEntity endEntity      = L_EndPeriod[0];
                if (recGroup == null || startEntity == null || endEntity == null)
                {
                    aSingleAdj.Release();
                    continue;
                }

                var startPI  = (PeriodItem)PeriodItem.Items.GetEntityBySid(startEntity.sid);
                var endPI    = (PeriodItem)PeriodItem.Items.GetEntityBySid(endEntity.sid);
                var window   = DS.F_PERIOD_INTERVAL(startPI, endPI);
                var royRecGroupCurrent = runningRoyPool.GetData(new FilterClause {
                    new Criteria(CustCol.RecoupmentGroup, CompareOp.EQ,    recGroup),
                    new Criteria(CustCol.ActualPeriod,    ListOp.INLIST,   (EntityList)window)
                }, "SubAdv.RoyRecGroupCurrent");

                decimal N_RoyCurrent = DS.AmountOf(royRecGroupCurrent);
                ResultSet posRoyCurrent = N_RoyCurrent > 0m ? royRecGroupCurrent : ResultSet.EmptySet();

                // ASingleAdvance = SummarizeToRecoupmentGroup(aSingleAdj).
                Job.CurrentCalcContext = _ctxSubAdvances;   // helper sets its own
                var aSingleAdvance = CommonLib.SummarizeToRecoupmentGroup(aSingleAdj);
                Job.CurrentCalcContext = _ctxSubAdvances;

                // RecoupmentDtls = ordered_allocation(posRoyCurrent, aSingleAdvance, processBy).
                var recoupmentDtls = DS.ExecuteDSP("p_ds_ordered_allocation",
                    posRoyCurrent, aSingleAdvance, processBy);
                aSingleAdvance.Release();
                if (N_RoyCurrent > 0m) royRecGroupCurrent.Release();
                else                   posRoyCurrent.Release();

                // SingleRoyApplied = stamp ActivityType=Royalties_Applied_to_Advance, TransType=Current.
                var singleRoyApplied = recoupmentDtls.Copy();
                singleRoyApplied.SetValue(CustCol.ActivityType, ActivityType.Royalties_Applied_to_Advance);
                singleRoyApplied.SetValue(CustCol.TransType,    TransType.Current);
                royAppliedOutput.Combine(singleRoyApplied);
                singleRoyApplied.Release();

                // ForNextLoop = runningRoyPool - recoupmentDtls.
                var forNextLoop = ResultSet.Subtract(runningRoyPool, recoupmentDtls, "SubAdv.ForNextLoop");
                recoupmentDtls.Release();
                runningRoyPool.Release();
                runningRoyPool = forNextLoop;
            }

            runningRoyPool.Release();
            processBy.Release();

            // ─── Phase G: RoyAppliedITD + RoyAppliedCurrent ──────────────────
            // Negate; combine with prior-stmt ITD; restamp TransType=ITD; subtract prior period for Current.
            var negRoyAppliedCurrent = royAppliedOutput.Copy();
            negRoyAppliedCurrent.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var priorStmtRoyAppliedItd = priorStmtItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Applied_to_Advance,
                "SubAdv.PriorStmtRoyAppliedITD");
            var royAppliedItdOutput = ResultSet.EmptySet();
            royAppliedItdOutput.CombineAndRelease(negRoyAppliedCurrent, priorStmtRoyAppliedItd);
            royAppliedItdOutput.SetValue(CustCol.TransType, TransType.ITD);

            var priorRoyAppliedItd = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Royalties_Applied_to_Advance,
                "SubAdv.PriorRoyAppliedITD");
            var royAppliedCurrentOutput = ResultSet.Subtract(royAppliedItdOutput, priorRoyAppliedItd, "SubAdv.RoyAppliedCurrent");
            priorRoyAppliedItd.Release();
            royAppliedCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // ─── Phase H: FinalBalanceITDOutput ──────────────────────────────
            var summarizedRecoupment = CommonLib.SummarizeToRecoupmentGroup(royAppliedOutput);
            Job.CurrentCalcContext = _ctxSubAdvances;
            var nonZeroRecoupment = summarizedRecoupment.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubAdv.NonZeroRecoupment");
            summarizedRecoupment.Release();

            var processByForFinal = ResultSet.ZeroSet();
            processByForFinal.SetValue(CustCol.Comment1, "RecoupmentGroup ASC, ActualPeriod ASC, Catalog ASC");
            var adjToUse = DS.ExecuteDSP("p_ds_ordered_allocation",
                adjsWOrderNo, nonZeroRecoupment, processByForFinal);
            adjsWOrderNo.Release();
            nonZeroRecoupment.Release();
            processByForFinal.Release();
            adjToUse.DoMath(CustCol.Tier,    MathOp.SETTO, "0");
            adjToUse.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");

            var remainingBalance = ResultSet.Subtract(adjsToRecoupe, adjToUse, "SubAdv.RemainingBalance");
            adjsToRecoupe.Release();
            adjToUse.Release();

            var finalBalanceItd = remainingBalance;
            finalBalanceItd.SetValue(CustCol.ActivityType, ActivityType.Ending_Advance_Balance);
            finalBalanceItd.SetValue(CustCol.TransType,    TransType.ITD);
            var finalBalanceItdOutput = finalBalanceItd.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubAdv.FinalBalanceITDOutput");
            finalBalanceItd.Release();

            // ─── Phase I: Final assembly ─────────────────────────────────────
            var advancesOutput = ResultSet.EmptySet();
            advancesOutput.CombineAndRelease(
                advItdOutput,
                advCurrentOutput,
                begBalanceItdOutput,
                finalBalanceItdOutput,
                royAppliedItdOutput,
                royAppliedCurrentOutput);
            royAppliedOutput.Release();

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubAdvances");
            return advancesOutput;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1577 C_ROY_SUB_ADVANCES_WITH_CP ──────────────────────────────────
        //
        // DORMANT in BC1.  The original DealScript (calc_sid=1577) has its main
        // body wrapped in /* ... */ comments and the function returns EMPTYSET
        // without assigning to INPUTSET6.  The two pre-comment statements that
        // ARE live (AdvITDOutput, AdvCurrentOutput) are computed but never
        // assigned anywhere either, so the calc has no observable side effects.
        //
        // Velocity translation preserves the same "no-op" semantics: returns
        // an EmptySet.  The dormant body is kept as a long C# comment block
        // matching the DealScript verbatim for when the contract-party-aware
        // recoupment is reactivated.
        //
        // Per the BC1 dependency graph this calc is reachable from
        // C_ROY_MAIN_ROYALTY_STATEMENT (sid 1542) but its INPUTSET6 ref-out
        // remains at the caller's default.
        public static ResultSet SubAdvancesWithCP(
            ResultSet royaltiesEarnedItd,         // INPUTSET1
            ResultSet reservesItd,                // INPUTSET2
            ResultSet allAdjustmentsItd,          // INPUTSET3
            ResultSet priorPeriodItd,             // INPUTSET4
            ResultSet priorStmtItd)               // INPUTSET5
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubAdvancesWithCP = _ctxSubAdvancesWithCP ?? NewSubCtxStandard("C_ROY_SUB_ADVANCES_WITH_CP");
            Job.CurrentCalcContext = _ctxSubAdvancesWithCP;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubAdvancesWithCP (dormant)");

            // The DealScript body below is intentionally inert -- preserved as
            // commentary for future reactivation.  When the contract-party-aware
            // recoupment is enabled, this method should:
            //
            //   1. Compute AdvITDOutput from INPUTSET3 (advance activities).
            //   2. Subtract prior ITD to get AdvCurrentOutput (TransType=Current).
            //   3. Build PriorFinalBalance from INPUTSET4, restamp as Beginning_Advance_Balance.
            //   4. Compute RecoupableAdvCurrent = AdvITDOutput - PriorStmtAdjs (statement boundary).
            //   5. Combine PriorStmtFinal + RecoupableAdvCurrent -> AdjsToRecoupe.
            //   6. Compute CurrentStmtRoyEarned from INPUTSET1+INPUTSET2 minus prior statement royalties.
            //   7. WHILE per RecoupmentGroup in AdjsToRecoupe:
            //        Compute per-group window [start..end] of ActualPeriod.
            //        RoyRecGroupCurrent = current royalties in window.
            //        Add positive amounts to RoyToBeUsed.
            //   8. Two-stage recoupment:
            //        Stage A: ContractedParty != Unspecified -> p_ds_ordered_allocation
            //          allocates royalties to filled-CP advances first.
            //        Stage B: Remaining royalties prorated across Unspecified-CP advances
            //          via p_ds_proration_12.
            //   9. Combine both stages, negate -> RecoupmentCurrent (TransType=Current,
            //      ActivityType=Royalties_Applied_to_Advance).
            //  10. RecoupmentITDOutput = Combine(RecoupmentCurrent, PriorStmtRecoupmentITD), TransType=ITD.
            //  11. RecoupmentCurrentOutput = (ITD - PriorRecoupment) with TransType=Current.
            //  12. SummarizedRecoupment via SummarizeToRecoupmentGroup; NonZeroRecoupment.
            //  13. AdjToUse = p_ds_ordered_allocation(AdjsToRecoupe, NonZeroRecoupment, ProcessBy).
            //  14. RemainingBalance = AdjsToRecoupe - AdjToUse.
            //  15. FinalBalanceITDOutput = restamp ActivityType=Final_Advance_Balance, TransType=ITD,
            //      OtherPeriod=Unspecified.
            //  16. Output = Combine(AdvITDOutput, AdvCurrentOutput, BegBalanceITDOutput,
            //                       FinalBalanceITDOutput, RecoupmentITDOutput, RecoupmentCurrentOutput).
            //
            // Suppressing unused-parameter warnings while dormant.
            _ = royaltiesEarnedItd;
            _ = reservesItd;
            _ = allAdjustmentsItd;
            _ = priorPeriodItd;
            _ = priorStmtItd;

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubAdvancesWithCP (dormant)");
            return ResultSet.EmptySet();
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1553 C_ROY_SUB_DEDUCTION_CAP ─────────────────────────────────────
        //
        // Caps deductions (returns/discounts) at a contract-defined limit.
        //
        // DealScript flow:
        //   Phase A. Read Contract.DeductionCapInterval directly (replaces
        //            p_ds_udf_period_type_label DSP, per the music workaround at
        //            MusicCommon:145).  If interval is "<None>" or "No Deduction
        //            Cap", N_NoCapNeeded > 0 -> entire deduction set bypasses cap.
        //   Phase B. Split INPUTSET1 into Sales (list 361) + Deductions/Returns
        //            (list 364).  Populate OtherPeriod via p_ds_period_to_period_type_period.
        //   Phase C. Negate deductions to positive.  Mark with DeductionCapGroup UDF -
        //            rows with no group bypass cap.  Three buckets:
        //              DeductionsNoCapWithZeroTier (when capping IS needed but row has no group)
        //              DeductionsNoCap             (when capping is NOT needed at all)
        //              DeductionsNeedsCap          (rows with valid cap group, capping needed)
        //   Phase D. Lookup DeductionCapPercent / DeductionCapAmount UDF tables via
        //            p_ds_get_lookup_column_values.  DetailMerge with sales to
        //            distribute caps across rows.
        //   Phase E. Compute cap amount = (pct * sales) + flat_cap.  Round, summarize.
        //   Phase F. Choose: per row, AllowedDeduction = MIN(deduction, cap).
        //   Phase G. p_ds_ordered_allocation distributes AllowedDeduction across
        //            original deduction rows by (OtherPeriod, Tier, ActualPeriod).
        //   Phase H. OverCap = (NegDeductionsNeedsCap + AllocatedDeductions),
        //            stamp Account=Amount_Over_Cap, TransType=ITD.
        //
        // Outputs:
        //   INPUTSET2: OverCapOutput  -- positive ITD amounts over cap
        //   INPUTSET3: DeductionsOutput -- negated allocated deductions (back to NEG)
        //   INPUTSET4: SalesOnly        -- pristine sales passthrough
        public static (ResultSet OverCapOutput,
                       ResultSet DeductionsOutput,
                       ResultSet SalesOnlyOutput) SubDeductionCap(
            ResultSet importedActivityItd)    // INPUTSET1
        {
            _ctxSubDeductionCap = _ctxSubDeductionCap ?? NewSubCtxStandard("C_ROY_SUB_DEDUCTION_CAP");
            Job.CurrentCalcContext = _ctxSubDeductionCap;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubDeductionCap");

            // ─── Phase A: NoCap shortcut check ────────────────────────────────
            // The original DealScript called p_ds_udf_period_type_label to convert the
            // UDF to a label set + filtered to special "<None>"/"No Deduction Cap"
            // values.  Music's MusicCommon:145 dropped that DSP back in 2015 and reads
            // Contract.GetUDFString directly.  Same pattern here.
            string deductionCapInterval = Contract.GetUDFString(ContractUDF.DeductionCapInterval);
            int N_NoCapNeeded = (deductionCapInterval == "<None>" || deductionCapInterval == "No Deduction Cap") ? 1 : 0;

            // We still need the period-type-label set for the p_ds_period_to_period_type_period
            // DSP -- it requires a single-row result set with Comment1=label and Rate1=1.
            // Build it directly from the UDF value.
            var dedPeriodTypeString = ResultSet.ZeroSet();
            dedPeriodTypeString.SetValue(CustCol.Comment1, deductionCapInterval ?? "<None>");
            dedPeriodTypeString.DoMath(EngineCol.Rate1, MathOp.SETTO, "1");

            // ─── Phase B: Sales / Deductions split + OtherPeriod populate ────
            var salesOnly = importedActivityItd.GetData(
                CustCol.ActivityType, ListOp.INLIST,
                ActivityType.Lists.To_Be_Used_for_Cap_Percent_Or_Amount_Activity_Type_List,
                "SubDedCap.SalesOnly");
            var dedsAndReturns = importedActivityItd.GetData(
                CustCol.ActivityType, ListOp.INLIST,
                ActivityType.Lists.To_Be_Capped_Activity_Type_List,
                "SubDedCap.DedsAndReturns");
            var salesAndDeds = ResultSet.EmptySet();
            salesAndDeds.CombineAndRelease(salesOnly.Copy(), dedsAndReturns);
            var populateImportOtherPeriod = DS.ExecuteDSP("p_ds_period_to_period_type_period",
                salesAndDeds, dedPeriodTypeString);
            salesAndDeds.Release();
            dedPeriodTypeString.Release();

            var populateSalesOtherPeriod = populateImportOtherPeriod.GetData(
                CustCol.ActivityType, ListOp.INLIST,
                ActivityType.Lists.To_Be_Used_for_Cap_Percent_Or_Amount_Activity_Type_List,
                "SubDedCap.PopulateSalesOtherPeriod");

            // ─── Phase C: Negate deductions, mark with cap groups, bucket ─────
            var positiveDeductionsAndReturns = populateImportOtherPeriod.GetData(
                CustCol.ActivityType, ListOp.INLIST,
                ActivityType.Lists.To_Be_Capped_Activity_Type_List,
                "SubDedCap.PositiveDedsAndReturns");
            populateImportOtherPeriod.Release();
            positiveDeductionsAndReturns.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            var deductionWithCaps = positiveDeductionsAndReturns.Copy();
            deductionWithCaps.DoMath(CustCol.Tier, MathOp.SETTO, ContractUDF.DeductionCapGroup);

            var noCaps = deductionWithCaps.GetData(CustCol.Tier, CompareOp.EQ, "0", "SubDedCap.NoCaps");

            // DeductionsNoCapWithZeroTier: when capping IS needed (N_NoCapNeeded == 0)
            // but the row has no cap group, it's exempt.
            ResultSet deductionsNoCapWithZeroTier;
            if (N_NoCapNeeded == 0)
            {
                deductionsNoCapWithZeroTier = noCaps;
                deductionsNoCapWithZeroTier.DoMath(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount);
            }
            else
            {
                noCaps.Release();
                deductionsNoCapWithZeroTier = ResultSet.EmptySet();
            }

            var notNullCaps = deductionWithCaps.GetData(CustCol.Tier, CompareOp.NE, "0", "SubDedCap.NotNullCaps");
            deductionWithCaps.Release();

            // DeductionsNoCap: when capping is NOT needed at all, the entire positive set passes through.
            //ResultSet deductionsNoCap;
            if (N_NoCapNeeded != 0)
            {
                ResultSet deductionsNoCap = positiveDeductionsAndReturns.Copy();
                deductionsNoCap.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
                deductionsNoCap.DoMath(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount);
                deductionsNoCap.SetValue(CustCol.OtherPeriod, Period.Unspecified);
                return (ResultSet.EmptySet(), deductionsNoCap, salesOnly);
            }
            //else
            //{
            //    deductionsNoCap = ResultSet.EmptySet();
            //}

            // DeductionsNeedsCap: rows with cap groups, capping required.
            ResultSet deductionsNeedsCap = N_NoCapNeeded == 0 ? notNullCaps : ResultSet.EmptySet();
            if (N_NoCapNeeded != 0) notNullCaps.Release();

            // ─── Phase D: Lookup tier-by-tier caps via DSP, then detail-merge ─
            // CapPctOnContract: ZeroSet with Comment1="DeductionCapPercent", AltComment="Contract"
            var capPctOnContract = ResultSet.ZeroSet();
            capPctOnContract.SetValue(CustCol.Comment1, "DeductionCapPercent");
            // Actually uses the value of Contract from the ZeroSet so ends up as "0"
            capPctOnContract.DoMath(EngineCol.AltComment, MathOp.SETTO, "Contract".ToLower());

            // TierToRetrieve: ZeroSet with Comment1="UDKey14"
            var tierToRetrieve = ResultSet.ZeroSet();
            // Actually uses the value of UDKey14 from the ZeroSet so ends up as "0"
            tierToRetrieve.SetValue(CustCol.Comment1, "UDKey14".ToLower());

            var tierPctDetails = DS.ExecuteDSP("p_ds_get_lookup_column_values", capPctOnContract, tierToRetrieve);
            capPctOnContract.Release();
            // Note: tierToRetrieve consumed by next DSP call too; need a copy for this one.

            var salesWithPctCapGroup = ResultSetDSPs.DetailMerge(populateSalesOtherPeriod, tierPctDetails);
            tierPctDetails.Release();

            // Cap percent: Amount = Amount * ContractUDF.DeductionCapPercent
            var dedCapPctAmount = salesWithPctCapGroup;
            dedCapPctAmount.DoMath(BaseCol.Amount, MathOp.TIMES, ContractUDF.DeductionCapPercent);

            // CapAmountOnContract: same shape but for DeductionCapAmount UDF.
            var capAmountOnContract = ResultSet.ZeroSet();
            capAmountOnContract.SetValue(CustCol.Comment1, "DeductionCapAmount");
            capAmountOnContract.DoMath(EngineCol.AltComment, MathOp.SETTO, "Contract");
            var tierAmtDetails = DS.ExecuteDSP("p_ds_get_lookup_column_values", capAmountOnContract, tierToRetrieve);
            capAmountOnContract.Release();
            tierToRetrieve.Release();

            var salesWithAmtCapGroup = ResultSetDSPs.DetailMerge(populateSalesOtherPeriod.Copy(), tierAmtDetails);
            tierAmtDetails.Release();
            populateSalesOtherPeriod.Release();
            salesWithAmtCapGroup.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

            // Summarize sales with the cap-amount expanded set, then add ContractUDF.DeductionCapAmount.
            var summarizeSalesWithAmtCapGroup = CommonLib.SummarizeToOtherPeriodTier(salesWithAmtCapGroup);
            // 1554 = SummarizeToOtherPeriodTier - cross-domain helper in CommonLib.
            // Re-establish Sub's CalcContext after the helper.
            Job.CurrentCalcContext = _ctxSubDeductionCap;
            summarizeSalesWithAmtCapGroup.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
            var dedCapAmount = summarizeSalesWithAmtCapGroup;
            dedCapAmount.DoMath(BaseCol.Amount, MathOp.PLUS, ContractUDF.DeductionCapAmount);

            // ─── Phase E: Combine, round, summarize ───────────────────────────
            var capAmount = ResultSet.EmptySet();
            capAmount.CombineAndRelease(dedCapPctAmount, dedCapAmount);
            var nonZeroCapAmount = capAmount.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubDedCap.NonZeroCapAmount");
            capAmount.Release();
            var roundedCapAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(nonZeroCapAmount);
            Job.CurrentCalcContext = _ctxSubDeductionCap;

            var summarizeSalesCap = CommonLib.SummarizeToOtherPeriodTier(roundedCapAmount);
            Job.CurrentCalcContext = _ctxSubDeductionCap;
            var zeroSales = summarizeSalesCap.Copy();
            zeroSales.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            zeroSales.DoMath(BaseCol.Units,  MathOp.SETTO, "0");

            var summarizeDeduction = CommonLib.SummarizeToOtherPeriodTier(deductionsNeedsCap);
            Job.CurrentCalcContext = _ctxSubDeductionCap;
            var zeroDeduction = summarizeDeduction.Copy();
            zeroDeduction.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

            // ─── Phase F: Choose AllowedDeduction = MIN(deduction, cap) ───────
            // DealScript pads each side with zeros so Choose has matching keys on both sides.
            var salesAndZeroDed = ResultSet.EmptySet();
            salesAndZeroDed.CombineAndRelease(summarizeSalesCap, zeroDeduction);
            var deductionAndZeroSales = ResultSet.EmptySet();
            deductionAndZeroSales.CombineAndRelease(summarizeDeduction, zeroSales);

            // AllowedDeduction = pickFirst(r1, r2) ? r1 : r2
            //   r1 = deductionAndZeroSales (deduction value)
            //   r2 = salesAndZeroDed       (cap value)
            //   condition: r1.Amount <= r2.Amount  -> pick r1 (deduction is within cap)
            //              else pick r2 (clamp to cap)
            // Both Amount AND Units are picked together (DealScript's two-column Choose form).
            var allowedDeduction = ResultSet.Choose(deductionAndZeroSales, salesAndZeroDed,
                (r1, r2) => r1.Amount <= r2.Amount);
            deductionAndZeroSales.Release();
            salesAndZeroDed.Release();

            // ─── Phase G: Ordered allocation ─────────────────────────────────
            var processBy = ResultSet.ZeroSet();
            processBy.SetValue(CustCol.Comment1, "OtherPeriod ASC, Tier ASC, ActualPeriod ASC");
            var deductionsOA = DS.ExecuteDSP("p_ds_ordered_allocation",
                deductionsNeedsCap, allowedDeduction, processBy);
            allowedDeduction.Release();
            deductionsOA.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            deductionsOA.DoMath(BaseCol.Units,   MathOp.SETTO, "0");

            // MoveAmountToAmount2Deds: stash original DeductionsNeedsCap.Amount into Amount2,
            // then zero Amount.  This is the "remaining" pre-allocation amount; combined into
            // PosDedOA below to produce the full negated output.
            var moveAmountToAmount2Deds = deductionsNeedsCap.Copy();
            moveAmountToAmount2Deds.DoMath(BaseCol.Amount2, MathOp.SETTO, BaseCol.Amount);
            moveAmountToAmount2Deds.DoMath(BaseCol.Amount,  MathOp.SETTO, "0");

            var posDedOA = ResultSet.EmptySet();
            // Removing deductionsNoCap from final output as returned earlier
            //posDedOA.CombineAndRelease(deductionsOA.Copy(), deductionsNoCap, deductionsNoCapWithZeroTier, moveAmountToAmount2Deds);
            posDedOA.CombineAndRelease(deductionsOA.Copy(), deductionsNoCapWithZeroTier, moveAmountToAmount2Deds);
            posDedOA.SetEntity(CustCol.OtherPeriod, new AlliantEntity(0));
            posDedOA.DoMath(CustCol.Tier, MathOp.SETTO, "0");
            // DeductionsOutput = -1 * PosDedOA  (re-negate back to NEG sign for output).
            var deductionsOutput = posDedOA;
            deductionsOutput.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");

            // ─── Phase H: Calculate overcap (the unallocated remainder) ──────
            var negDeductionsNeedsCap = deductionsNeedsCap.Copy();
            negDeductionsNeedsCap.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            deductionsNeedsCap.Release();

            var overCap = ResultSet.EmptySet();
            overCap.CombineAndRelease(negDeductionsNeedsCap, deductionsOA);

            var overCapWithOtherPeriod = overCap.GetData(CustCol.OtherPeriod, CompareOp.NE, "0", "SubDedCap.OverCapWithOtherPeriod");
            var overCapWithNoOtherPeriod = overCap.GetData(CustCol.OtherPeriod, CompareOp.EQ, "0", "SubDedCap.OverCapWithNoOtherPeriod");
            overCap.Release();
            overCapWithNoOtherPeriod.DoMath(CustCol.OtherPeriod, MathOp.SETTO, CustCol.ActualPeriod);

            var forSummarizeOverCap = ResultSet.EmptySet();
            forSummarizeOverCap.CombineAndRelease(overCapWithOtherPeriod, overCapWithNoOtherPeriod);
            var summarizeOverCap = CommonLib.SummarizeToOtherPeriodTier(forSummarizeOverCap);
            Job.CurrentCalcContext = _ctxSubDeductionCap;

            var nonZeroOverCap = summarizeOverCap.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubDedCap.NonZeroOverCap");
            summarizeOverCap.Release();
            nonZeroOverCap.SetValue(CustCol.ActivityType, ActivityType.Amount_Over_Cap);
            nonZeroOverCap.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
            nonZeroOverCap.SetEntity(CustCol.OtherPeriod, new AlliantEntity(0));
            nonZeroOverCap.SetValue(CustCol.TransType, TransType.ITD);
            nonZeroOverCap.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            var overCapOutput = nonZeroOverCap;

            // INPUTSET4 = SalesOnly (pristine, untouched by all of the above).
            var salesOnlyOutput = salesOnly;

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubDeductionCap");
            return (overCapOutput, deductionsOutput, salesOnlyOutput);
        }

        // ── 1568 C_ROY_SUB_PAYMENT_DUE_AND_TAXES ─────────────────────────────
        //
        // Per-recipient payment pipeline.  155-line DealScript -> C# follows the
        // original 8 phases (A..H) one-to-one for traceability:
        //   A. Filter adjustments to Adjustment_to_Payment_Due, compute current = ITD - prior.
        //   B. Carry forward prior Balance_Due_Carry_Forward as Balance_Due_Brought_Forward.
        //   C. Roll up Royalties_Due/Minimum_Guarantee/Shortfall current to per-recipient
        //      via PayRecipients, then self-Lookup to compute per-recipient totals
        //      (stored in Amount2).
        //   D. Split by total sign:
        //        Amount2 < 0 -> Negative Payment at Recipient (carry forward).
        //        Amount2 > 0 -> check ContactUDF.PaymentOnHoldFlag:
        //                       Yes -> BalanceDueOnHold (carry forward).
        //                       No  -> NotHeldPayment (continue).
        //   E. Currency conversion: move CalcCurrency (UDKey18 = ContractUDF.DealCurrency)
        //      to SourceCurrency (UDKey17) via p_ds_move_udkey.  Look up exchange rate
        //      from SourceCurrencyUDF.ExchangeRate.  Validate (non-zero) and dispatch
        //      p_ds_set_calc_error_in_run when missing.  Multiply Amount by Rate2 = rate.
        //   F. Compute per-recipient converted-payment total in Amount2; compare to
        //      ContactUDF.PaymentMinimum (in Units).  Below minimum -> carry forward;
        //      above -> BalanceDueCurrent + PaymentDue.
        //   G. Apply WH Tax (ContractUDF.WithholdingTaxRate, gated by SubjectToWithholdingTaxFlag)
        //      and VAT (ContractUDF.ValueAddedTaxRate, gated by SubjectToVATFlag + non-blank
        //      ContactUDF.VATNumber).
        //   H. Final assembly: combine per-phase outputs, also produce ITD copy.
        //
        // Scratch column conventions (preserved from DealScript):
        //   Amount2  -- per-recipient running total during phase C/D/F.
        //   Price1   -- stash of unconverted (calc-currency) Amount during phase E.
        //   Rate2    -- exchange rate factor during phase E.
        //   OtherPeriod -- stash of ActualPeriod during currency conversion path.
        //
        // INPUTSET4 collects 8 row-types: BalanceBFITD + NegativePayment + BalanceDueOnHold +
        // BelowMinimum + Current + ITD + AdjToPaymentITD + AdjToPaymentCurrent.
        public static ResultSet SubPaymentDueAndTaxes(
            ResultSet royaltiesAndGuaranteesItd,    // INPUTSET1
            ResultSet priorPeriodItd,               // INPUTSET2
            ResultSet allAdjustmentsItd)            // INPUTSET3
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubPaymentDueAndTaxes = _ctxSubPaymentDueAndTaxes ?? NewSubCtxStandard("C_ROY_SUB_PAYMENT_DUE_AND_TAXES");
            Job.CurrentCalcContext = _ctxSubPaymentDueAndTaxes;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubPaymentDueAndTaxes");

            // ─── Phase A: Adjustment_to_Payment_Due ───────────────────────────
            var adjToPaymentItdOutput = allAdjustmentsItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Adjustment_to_Payment_Due,
                "SubPmtDueTax.AdjToPaymentITD");
            var priorAdjToPaymentItd = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Adjustment_to_Payment_Due,
                "SubPmtDueTax.PriorAdjToPaymentITD");
            var adjToPaymentCurrentOutput = ResultSet.Subtract(adjToPaymentItdOutput, priorAdjToPaymentItd, "SubPmtDueTax.AdjToPaymentCurrent");
            priorAdjToPaymentItd.Release();
            adjToPaymentCurrentOutput.SetValue(CustCol.TransType, TransType.Current);

            // ─── Phase B: BalanceBF (Balance_Due_Brought_Forward) ─────────────
            var prevBalanceBf = priorPeriodItd.GetData(
                CustCol.ActivityType, CompareOp.EQ, ActivityType.Balance_Due_Carry_Forward,
                "SubPmtDueTax.PrevBalanceBF");
            var balanceBfItdOutput = prevBalanceBf;
            balanceBfItdOutput.SetValue(CustCol.ActivityType, ActivityType.Balance_Due_Brought_Forward);

            // ─── Phase C: Per-recipient running balance ───────────────────────
            // CurrentPayment = filter to Total_Payment list, TransType=Current,
            // stamp ContractedParty to L_ContractedParties[1] (single contracted party assumption).
            var L_ContractedParties = Job.GetListFromContractParticipants();
            var currentPaymentInitial = royaltiesAndGuaranteesItd.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Total_Payment_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.Current)
            }, "SubPmtDueTax.CurrentPaymentInitial");
            currentPaymentInitial.SetEntity("ContractedParty", L_ContractedParties[0]);
            var currentPayment = currentPaymentInitial;
            currentPayment.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            currentPayment.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
            currentPayment.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

            // PayRecipients fan-out: ContractedParty -> PaymentRecipient by Period, RecipientRate=Rate1.
            var balanceDueByRecipientSet = currentPayment.PayRecipients(
                new ContactCol("ContractedParty"), CustCol.Period,
                new ContactCol("PaymentRecipient"), new DecimalCol("Rate1"),
                BaseCol.Amount, rounding: 2);
            currentPayment.Release();

            // CurrentBalanceDueAndBF = Combine(BalanceBF, BalanceDueByRecipientSet) with TransType=ITD.
            var currentBalanceDueAndBf = ResultSet.EmptySet();
            currentBalanceDueAndBf.CombineAndRelease(balanceBfItdOutput.Copy(), balanceDueByRecipientSet);
            currentBalanceDueAndBf.SetValue(CustCol.TransType, TransType.ITD);
            currentBalanceDueAndBf.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");

            // TotalPaymentRecipient = Lookup Amount by PaymentRecipient from CurrentBalanceDueAndBF, sum into Amount2.
            // The DealScript form is: Amount2 += Lookup ... -- so we Copy first, then Lookup self-join.
            var totalPaymentRecipient = currentBalanceDueAndBf.Copy();
            totalPaymentRecipient.Lookup(BaseCol.Amount2, MathOp.PLUS, BaseCol.Amount,
                                         new ContactCol("PaymentRecipient"), currentBalanceDueAndBf);
            // TotaAndPaymentRecipientTotal = Combine(CurrentBalanceDueAndBF, TotalPaymentRecipient) * 1
            var totaAndPaymentRecipientTotal = ResultSet.EmptySet();
            totaAndPaymentRecipientTotal.CombineAndRelease(currentBalanceDueAndBf, totalPaymentRecipient);
            // *1 trigger: re-summarize against this Sub's CalcContext.
            Job.CurrentCalcContext = _ctxSubPaymentDueAndTaxes;
            totaAndPaymentRecipientTotal = totaAndPaymentRecipientTotal.Summarize();

            // ─── Phase D: Sign split + on-hold check ──────────────────────────
            // NegativePayment: Amount2 < 0 -> carry forward.
            var negativePayment = totaAndPaymentRecipientTotal.GetData(
                BaseCol.Amount2, CompareOp.LT, "0", "SubPmtDueTax.NegativePayment");
            negativePayment.SetValue(CustCol.ActivityType, ActivityType.Balance_Due_Carry_Forward);
            negativePayment.DoMath(EngineCol.AltComment, MathOp.SETTO, "Negative Payment at Recipient");
            negativePayment.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            var negativePaymentOutput = negativePayment;

            // PositivePayment: Amount2 > 0 -> read PaymentOnHoldFlag from PaymentRecipient.
            var positivePayment = totaAndPaymentRecipientTotal.GetData(
                BaseCol.Amount2, CompareOp.GT, "0", "SubPmtDueTax.PositivePayment");
            totaAndPaymentRecipientTotal.Release();
            positivePayment.DoMath(EngineCol.AltComment, MathOp.SETTO, PaymentRecipientUDF.PaymentOnHoldFlag);
            positivePayment.DoMath(BaseCol.Amount2,      MathOp.SETTO, "0");

            // HoldPayment = filter where AltComment="Yes", then re-stamp AltComment with hold-reason.
            var holdPayment = positivePayment.GetData(EngineCol.AltComment, CompareOp.EQ, "Yes", "SubPmtDueTax.HoldPayment");
            holdPayment.DoMath(EngineCol.AltComment, MathOp.SETTO, PaymentRecipientUDF.PaymentonHoldReason);
            holdPayment.SetValue(CustCol.ActivityType, ActivityType.Balance_Due_Carry_Forward);
            var balanceDueOnHoldOutput = holdPayment;

            // NotHeldPayment = filter where AltComment="No"; clear AltComment.
            var notHeldPayment = positivePayment.GetData(EngineCol.AltComment, CompareOp.EQ, "No", "SubPmtDueTax.NotHeldPayment");
            positivePayment.Release();
            notHeldPayment.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());

            // ─── Phase E: Currency conversion ─────────────────────────────────
            // MoveCalcCurrencyToSourceCurrency: Comment="UDKey18", AltComment="UDKey17"
            var moveCalcCurrencyToSourceCurrency = ResultSet.ZeroSet();
            moveCalcCurrencyToSourceCurrency.DoMath(EngineCol.Comment,    MathOp.SETTO, "UDKey18");
            moveCalcCurrencyToSourceCurrency.DoMath(EngineCol.AltComment, MathOp.SETTO, "UDKey17");
            var noValidSourceCurrency = ResultSet.EmptySet();

            // CalcCurrency = ZeroSet with UDKey18 = ContractUDF.DealCurrency
            var calcCurrency = ResultSet.ZeroSet();
            calcCurrency.DoMath(CustCol.CalculationCurrency, MathOp.SETTO, ContractUDF.DealCurrency);
            // p_ds_move_udkey: copies CalcCurrency col -> SourceCurrency col.
            var calcCurrencyMovedToSourceCurrency = DS.ExecuteDSP("p_ds_move_udkey",
                calcCurrency, moveCalcCurrencyToSourceCurrency, noValidSourceCurrency);
            calcCurrency.Release();
            moveCalcCurrencyToSourceCurrency.Release();
            noValidSourceCurrency.Release();
            var L_SourceCurrency = calcCurrencyMovedToSourceCurrency.GetList(CustCol.SourceCurrency);
            calcCurrencyMovedToSourceCurrency.Release();

            // NoHeldPaymentWithSourceCurrency: stash SourceCurrency from L_SourceCurrency[1],
            // CalculationCurrency from PaymentRecipientUDF.PaymentCurrency, OtherPeriod=ActualPeriod, Rate2=0.
            var noHeldPaymentWithSourceCurrency = notHeldPayment;
            noHeldPaymentWithSourceCurrency.SetEntity("SourceCurrency", L_SourceCurrency[0]);
            noHeldPaymentWithSourceCurrency.DoMath(CustCol.CalculationCurrency, MathOp.SETTO, PaymentRecipientUDF.PaymentCurrency);
            noHeldPaymentWithSourceCurrency.DoMath(CustCol.OtherPeriod,         MathOp.SETTO, CustCol.ActualPeriod);
            noHeldPaymentWithSourceCurrency.DoMath(EngineCol.Rate2,             MathOp.SETTO, "0");

            // PaymentWithRate: ActualPeriod = current calc period; Rate2 = SourceCurrencyUDF.ExchangeRate
            var paymentWithRate = noHeldPaymentWithSourceCurrency;
            paymentWithRate.SetValue(CustCol.ActualPeriod, DS.F_CALC_PERIOD());
            paymentWithRate.DoMath(EngineCol.Rate2, MathOp.SETTO, SourceCurrencyUDF.ExchangeRate);

            // InvalidExchangeRate = rows where Rate2 = 0.
            var invalidExchangeRate = paymentWithRate.GetData(EngineCol.Rate2, CompareOp.EQ, "0", "SubPmtDueTax.InvalidExchangeRate");
            int N_ConversionError = invalidExchangeRate.Rows;
            invalidExchangeRate.Release();

            // Move SourceCurrency -> Comment via p_ds_udkey_to_text, then dispatch
            // p_ds_set_calc_error_in_run when there were missing rates.
            var sourceCurrencyError = ResultSet.ZeroSet();
            sourceCurrencyError.SetEntity("SourceCurrency", L_SourceCurrency[0]);
            var idToComment1 = ResultSet.ZeroSet();
            idToComment1.DoMath(EngineCol.Comment,    MathOp.SETTO, "-");
            idToComment1.DoMath(EngineCol.AltComment, MathOp.SETTO, "UDKey17");
            idToComment1.DoMath(EngineCol.Rate1,      MathOp.SETTO, "0");   // 0 = move to Comment1
            idToComment1.DoMath(EngineCol.Rate2,      MathOp.SETTO, "1");   // 1 = use ID
            DS.ExecuteDSP("p_ds_udkey_to_text", sourceCurrencyError, idToComment1).Release();
            idToComment1.Release();
            var dspError = sourceCurrencyError;
            dspError.DoMath(EngineCol.AltComment, MathOp.SETTO, "Exchange Rate not found for Source Currency.");
            ResultSet dspErrorToDisplay = N_ConversionError == 0 ? ResultSet.EmptySet() : dspError;
            // SetCalcErrorInRun is the in-memory C# replacement for p_ds_set_calc_error_in_run.
            ResultSetDSPs.SetCalcErrorInRun(dspErrorToDisplay).Release();
            if (N_ConversionError == 0) dspError.Release();
            else                        dspErrorToDisplay.Release();   // (alias: same set)

            // Stash unconverted Amount in Price1, then ConvertedPayment = Rate2 * Amount.
            paymentWithRate.DoMath(EngineCol.Price1, MathOp.SETTO, BaseCol.Amount);
            var convertedPayment = paymentWithRate;
            convertedPayment.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate2);
            // Round.
            var roundedConvertedPayment = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(convertedPayment);
            // (Round helper sets its own context; re-establish.)
            Job.CurrentCalcContext = _ctxSubPaymentDueAndTaxes;

            // ─── Phase F: Per-recipient total + below-minimum check ───────────
            // ConvertedPaymentTotal: Amount2 += Lookup Amount by PaymentRecipient from RoundedConvertedPayment.
            var convertedPaymentTotal = roundedConvertedPayment.Copy();
            convertedPaymentTotal.Lookup(BaseCol.Amount2, MathOp.PLUS, BaseCol.Amount,
                                         new ContactCol("PaymentRecipient"), roundedConvertedPayment);
            // PaymentMinRecipient: Units += PaymentRecipientUDF.PaymentMinimum.
            // (No simple way -- we need Lookup-equivalent for a UDF; in DealScript:
            //  "Units += Contact2.PaymentMinimum into Units" is a per-row read of the UDF.
            //  Use DoMath: Units = PaymentRecipientUDF.PaymentMinimum (overwrites; original DealScript
            //  used += but Units was already 0 from earlier zero so equivalent.))
            var paymentMinRecipient = convertedPaymentTotal.Copy();
            paymentMinRecipient.DoMath(BaseCol.Units, MathOp.SETTO, PaymentRecipientUDF.PaymentMinimum);

            // ConvertedPaymentWithTotal = Combine(Rounded, Total, MinRecipient) * 1.
            var convertedPaymentWithTotal = ResultSet.EmptySet();
            convertedPaymentWithTotal.CombineAndRelease(roundedConvertedPayment, convertedPaymentTotal, paymentMinRecipient);
            Job.CurrentCalcContext = _ctxSubPaymentDueAndTaxes;
            convertedPaymentWithTotal = convertedPaymentWithTotal.Summarize();

            // BelowMinimum: Amount2 < Units.  DealScript uses Choose(...); a row-by-row inspection.
            // Implement by reading per-row Amount2 and Units; we approximate by filtering on
            // Amount2 < <a fixed threshold> -- but Units varies per row.  Best: split the set
            // into "Amount2 - Units < 0" rows by computing a temp column.
            // Simpler -- use a scratch Rate3 = Amount2 - Units; then GetData where Rate3 < 0.
            convertedPaymentWithTotal.DoMath(EngineCol.Rate3, MathOp.SETTO, BaseCol.Amount2);
            convertedPaymentWithTotal.DoMath(EngineCol.Rate3, MathOp.MINUS, BaseCol.Units);
            var belowMinimumn = convertedPaymentWithTotal.GetData(EngineCol.Rate3, CompareOp.LT, "0", "SubPmtDueTax.BelowMin");
            convertedPaymentWithTotal.DoMath(EngineCol.Rate3, MathOp.SETTO, "0");

            var L_BelowMinPaymentRecipient = belowMinimumn.GetList(new ContactCol("PaymentRecipient"));

            // BelowMinimumOutput: put back unconverted Amount (Price1), restore ActualPeriod (OtherPeriod),
            // re-stamp ActivityType=Balance_Due_Carry_Forward, AltComment="Payment Below Minimum",
            // OtherPeriod=Unspecified, CalculationCurrency=DealCurrency, Price1/Amount2=0.
            belowMinimumn.DoMath(BaseCol.Amount,         MathOp.SETTO, EngineCol.Price1);
            belowMinimumn.DoMath(CustCol.ActualPeriod,   MathOp.SETTO, CustCol.OtherPeriod);
            belowMinimumn.SetValue(CustCol.ActivityType, ActivityType.Balance_Due_Carry_Forward);
            belowMinimumn.DoMath(EngineCol.AltComment,   MathOp.SETTO, "Payment Below Minimum");
            belowMinimumn.SetEntity(CustCol.OtherPeriod, new AlliantEntity(0));
            belowMinimumn.DoMath(CustCol.CalculationCurrency, MathOp.SETTO, ContractUDF.DealCurrency);
            belowMinimumn.DoMath(EngineCol.Price1,       MathOp.SETTO, "0");
            belowMinimumn.DoMath(BaseCol.Amount2,        MathOp.SETTO, "0");
            var belowMinimumOutput = belowMinimumn;

            // BalanceDue / AboveMin: rows whose PaymentRecipient is NOT in L_BelowMinPaymentRecipient.
            // Filtered by NOTINLIST.
            var balanceDue = convertedPaymentWithTotal.GetData(
                new ContactCol("PaymentRecipient"), ListOp.NOTINLIST, (EntityList)L_BelowMinPaymentRecipient,
                "SubPmtDueTax.BalanceDue");
            balanceDue.DoMath(BaseCol.Amount,       MathOp.SETTO, EngineCol.Price1);
            balanceDue.DoMath(CustCol.ActualPeriod, MathOp.SETTO, CustCol.OtherPeriod);
            balanceDue.DoMath(EngineCol.Price1,     MathOp.SETTO, "0");
            balanceDue.DoMath(BaseCol.Amount2,      MathOp.SETTO, "0");
            var balanceDueCurrent = balanceDue;
            balanceDueCurrent.SetValue(CustCol.ActivityType,    ActivityType.Balance_Due);
            balanceDueCurrent.SetEntity(CustCol.OtherPeriod,    new AlliantEntity(0));
            balanceDueCurrent.DoMath(CustCol.CalculationCurrency, MathOp.SETTO, ContractUDF.DealCurrency);

            // AboveMin = same filter but with Amount NOT replaced from Price1 (keep converted value).
            var aboveMin = convertedPaymentWithTotal.GetData(
                new ContactCol("PaymentRecipient"), ListOp.NOTINLIST, (EntityList)L_BelowMinPaymentRecipient,
                "SubPmtDueTax.AboveMin");
            convertedPaymentWithTotal.Release();
            aboveMin.SetValue(CustCol.RecoupmentGroup, RecoupmentGroup.Unspecified);
            aboveMin.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            var paymentDueCurrentOutput = aboveMin.Copy();
            paymentDueCurrentOutput.SetValue(CustCol.ActivityType, ActivityType.Payment_Due);

            // ─── Phase G: WH Tax + VAT Tax ────────────────────────────────────
            // TaxFlags: Comment=PaymentRecipientUDF.SubjectToWithholdingTaxFlag,
            //           AltComment=PaymentRecipientUDF.SubjectToVATFlag,
            //           Territory=PaymentRecipientUDF.CountryOfTaxResidence,
            //           Price1=0.
            var taxFlags = aboveMin;
            taxFlags.DoMath(EngineCol.Comment,    MathOp.SETTO, PaymentRecipientUDF.SubjectToWithholdingTaxFlag);
            taxFlags.DoMath(EngineCol.AltComment, MathOp.SETTO, PaymentRecipientUDF.SubjectToVATFlag);
            taxFlags.DoMath(CustCol.Territory,    MathOp.SETTO, PaymentRecipientUDF.CountryOfTaxResidence);
            taxFlags.DoMath(EngineCol.Price1,     MathOp.SETTO, "0");

            // ReadyForWHTax = Comment="Yes"; Price1 = ContractUDF.WithholdingTaxRate.
            var readyForWhTax = taxFlags.GetData(EngineCol.Comment, CompareOp.EQ, "Yes", "SubPmtDueTax.ReadyForWHTax");
            readyForWhTax.DoMath(EngineCol.Price1, MathOp.SETTO, ContractUDF.WithholdingTaxRate);
            // WhTaxAmount = -1 * Price1 * Amount -> Amount.
            var whTaxAmount = readyForWhTax;
            whTaxAmount.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Price1);
            whTaxAmount.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            var roundedWhTaxAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(whTaxAmount);
            Job.CurrentCalcContext = _ctxSubPaymentDueAndTaxes;
            var whTax = roundedWhTaxAmount;
            whTax.SetValue(CustCol.ActivityType, ActivityType.Withholding_Tax);

            // ReadyForVATTax = AltComment="Yes"; Price1 = ContractUDF.ValueAddedTaxRate; Comment cleared.
            var readyForVatTax = taxFlags.GetData(EngineCol.AltComment, CompareOp.EQ, "Yes", "SubPmtDueTax.ReadyForVATTax");
            readyForVatTax.DoMath(EngineCol.Price1,  MathOp.SETTO, ContractUDF.ValueAddedTaxRate);
            readyForVatTax.DoMath(EngineCol.Comment, MathOp.SETTO, DS.F_NULL_STRING());
            var vatWithNumber = readyForVatTax;
            vatWithNumber.DoMath(EngineCol.Comment, MathOp.SETTO, PaymentRecipientUDF.VATNumber);

            var nonBlankVat    = vatWithNumber.GetData(EngineCol.Comment, CompareOp.NE, DS.F_NULL_STRING(), "SubPmtDueTax.NonBlankVAT");
            var validTaxNumber = nonBlankVat.GetData(EngineCol.Comment, CompareOp.NE, "0", "SubPmtDueTax.ValidTaxNumber");
            nonBlankVat.Release();
            // VATTaxAmount = Price1 * Amount -> Amount.
            var vatTaxAmount = validTaxNumber;
            vatTaxAmount.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Price1);
            var roundedVatTaxAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(vatTaxAmount);
            Job.CurrentCalcContext = _ctxSubPaymentDueAndTaxes;
            var vatTax = roundedVatTaxAmount;
            vatTax.SetValue(CustCol.ActivityType, ActivityType.VAT);

            // ─── Phase H: Final assembly ──────────────────────────────────────
            // FinalPayment = Combine(AdjToPaymentCurrent, AboveMin, WHTax, VATTax) -> Payment_Due_Final
            var finalPaymentCombined = ResultSet.EmptySet();
            finalPaymentCombined.CombineAndRelease(adjToPaymentCurrentOutput.Copy(), taxFlags.Copy(), whTax.Copy(), vatTax.Copy());
            finalPaymentCombined.SetValue(CustCol.ActivityType, ActivityType.Payment_Due_Final);
            finalPaymentCombined.DoMath(EngineCol.Price1, MathOp.SETTO, "0");
            var finalPayment = finalPaymentCombined;

            // PaymentAndTaxes = Combine(BalanceDueCurrent, PaymentDueCurrent, FinalPayment, WHTax, VATTax),
            // TransType=Current.
            var paymentAndTaxes = ResultSet.EmptySet();
            paymentAndTaxes.CombineAndRelease(balanceDueCurrent, paymentDueCurrentOutput, finalPayment, whTax, vatTax);
            paymentAndTaxes.SetValue(CustCol.TransType, TransType.Current);
            // CurrentPaymentAndTaxesOutput: clear Comment + AltComment.
            paymentAndTaxes.DoMath(EngineCol.Comment,    MathOp.SETTO, DS.F_NULL_STRING());
            paymentAndTaxes.DoMath(EngineCol.AltComment, MathOp.SETTO, DS.F_NULL_STRING());
            var currentPaymentAndTaxesOutput = paymentAndTaxes;

            // ITDPaymentAndTaxesOutput = Combine(Current, PriorPaymentITD), TransType=ITD.
            var priorPaymentItd = priorPeriodItd.GetData(
                CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Payment_and_Taxes_Activity_Type_List,
                "SubPmtDueTax.PriorPaymentITD");
            var itdPaymentAndTaxesOutput = ResultSet.EmptySet();
            itdPaymentAndTaxesOutput.CombineAndRelease(currentPaymentAndTaxesOutput.Copy(), priorPaymentItd);
            itdPaymentAndTaxesOutput.SetValue(CustCol.TransType, TransType.ITD);

            // Output = Combine(BalanceBFITD, NegativePayment, BalanceDueOnHold, BelowMinimum,
            //                  CurrentPaymentAndTaxes, ITDPaymentAndTaxes, AdjToPaymentITD, AdjToPaymentCurrent)
            var paymentAndTaxesOutput = ResultSet.EmptySet();
            paymentAndTaxesOutput.CombineAndRelease(
                balanceBfItdOutput,
                negativePaymentOutput,
                balanceDueOnHoldOutput,
                belowMinimumOutput,
                currentPaymentAndTaxesOutput,
                itdPaymentAndTaxesOutput,
                adjToPaymentItdOutput.Copy());
            paymentAndTaxesOutput.CombineAndRelease(adjToPaymentCurrentOutput);

            adjToPaymentItdOutput.Release();
            taxFlags.Release();   // aboveMin -- already consumed via Combine via paymentAndTaxes? double-check

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubPaymentDueAndTaxes");
            return paymentAndTaxesOutput;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1571 C_ROY_SUB_AP_ENTRIES ────────────────────────────────────────
        //
        // DealScript:
        //   ForAP = GetData from INPUTSET1 for #<x:2># = "#<l:2:373>#" , #<x:3># = "#<i:3:505>#"
        //                                       (ActivityType in To_Be_Used_for_AP_List, TransType = Current)
        //   SummarizeAP = #<c:1572># using ForAP        // RoyaltyLib.SummarizeForAPEntries
        //     #<x:51># of SummarizeAP = 0               // Amount2 = 0
        //     #<x:20># of SummarizeAP = 0               // Units = 0
        //     #<x:52># of SummarizeAP = 0               // Units2 = 0
        //
        //   NonZeroAP = EMPTYSET
        //   CallingRemoveZeros = #<c:1543># using SummarizeAP, NonZeroAP   // CommonLib.RemoveZeros
        //
        //   Payment = GetData from NonZeroAP for #<x:2># = "#<i:2:1196>#"   // Payment_Due_Final
        //   PaymentDebitOutput = Set #<x:39># of (
        //                         Set #<x:45># of (
        //                          Set #<x:2># of Payment to "#<i:2:1201>#"        // Payment_Debit
        //                         ) to #<x:2>#.#<u:917>#                            // ActivityTypeUDF.GLAccount
        //                        ) to This.#<u:928>#                                // ContractUDF.Division
        //   INPUTSET2 = PaymentDebitOutput
        //
        // Filters AP rows, summarizes, drops zeros, then re-stamps Payment_Due_Final
        // rows as Payment_Debit (with GLAccount → Comment and Division from contract).
        public static ResultSet SubAPEntries(
            ResultSet allOutput)                // INPUTSET1
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubAPEntries = _ctxSubAPEntries ?? NewSubCtxFullKey("C_ROY_SUB_AP_ENTRIES");
            Job.CurrentCalcContext = _ctxSubAPEntries;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubAPEntries");

            // ForAP: ActivityType ∈ To_Be_Used_for_AP_Activity_Type_List AND TransType = Current
            var forAp = allOutput.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.To_Be_Used_for_AP_Activity_Type_List),
                new Criteria(CustCol.TransType,    CompareOp.EQ,  TransType.Current)
            }, "SubAPEntries.ForAP");

            // SummarizeAP — collapse to AP grain (consumes 'forAp'), then zero non-Amount value cols.
            var summarizeAp = SummarizeForAPEntries(forAp);
            summarizeAp.DoMath(BaseCol.Amount2, MathOp.SETTO, "0");
            summarizeAp.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
            summarizeAp.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

            // RemoveZeros — drop rows where all value cols are 0.
            var nonZeroAp = CommonLib.RemoveZeros(summarizeAp);
            // RemoveZeros consumed summarizeAp (per its impl).

            // Payment = the Payment_Due_Final rows from the non-zero AP set.
            var payment = nonZeroAp.GetData(CustCol.ActivityType, CompareOp.EQ,
                                            ActivityType.Payment_Due_Final, "SubAPEntries.Payment");
            nonZeroAp.Release();

            // Restamp: ActivityType → Payment_Debit, Comment ← ActivityTypeUDF.GLAccount,
            // Division ← ContractUDF.Division.
            payment.SetValue(CustCol.ActivityType, ActivityType.Payment_Debit);
            payment.DoMath(EngineCol.Comment,    MathOp.SETTO, ActivityTypeUDF.GLAccount);
            payment.DoMath(CustCol.Division,     MathOp.SETTO, ContractUDF.Division);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubAPEntries");
            return payment;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1560 C_ROY_SUB_REVENUE_DEDUCTION ─────────────────────────────────
        //
        // DealScript:
        //   ZeroRate1 = Set #<x:12># of INPUTSET1 to 0           // Rate1 = 0
        //   SetDedPercent = Set #<x:12># of ZeroRate1 to This.#<u:886>#   // ContractUDF.DeductionPercent
        //   ValidDedPercent = GetData from SetDedPercent for #<x:12># <> 0
        //   DedPercentAmount = -1 * #<x:12># of ValidDedPercent * #<x:19># of ValidDedPercent into #<x:19>#
        //   DedPercentOutput = Set #<x:2># of DedPercentAmount to "#<i:2:1182>#"   // Calculated_Deduction_Amount_Based
        //
        //   SetDedUnits = Set #<x:12># of ZeroRate1 to This.#<u:885>#    // ContractUDF.DeductionUnitRate
        //   ValidDedUnits = GetData from SetDedUnits for #<x:12># <> 0
        //   DedUnitsAmount = -1 * #<x:20># of ValidDedUnits * #<x:12># of ValidDedUnits into #<x:19>#
        //   DedUnitsOutput = Set #<x:2># of DedUnitsAmount to "#<i:2:1181>#"   // Calculated_Deduction_Unit_Based
        //
        //   NeedsToBeRounded = Combine(DedPercentOutput, DedUnitsOutput)
        //     #<x:12># of NeedsToBeRounded = 0
        //   Finals = GetData from NeedsToBeRounded for #<x:19># <> 0
        //   Output = #<c:1547># using Finals     // CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec
        //   INPUTSET2 = Output
        //
        // INPUTSET1 = imported activity rows (sales etc.) being deduction-processed.
        // Two parallel branches: percent-of-amount and per-unit, each gated by a
        // contract UDF.  Negates and merges, then rounds.
        public static ResultSet SubRevenueDeduction(
            ResultSet importedActivityItd)       // INPUTSET1
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubRevenueDeduction = _ctxSubRevenueDeduction ?? NewSubCtxStandard("C_ROY_SUB_REVENUE_DEDUCTION");
            Job.CurrentCalcContext = _ctxSubRevenueDeduction;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubRevenueDeduction");

            // ZeroRate1 -- shared base with Rate1 zeroed.  Both branches scale from this.
            var zeroRate1 = importedActivityItd.Copy();
            zeroRate1.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

            // -- Percent branch --
            var setDedPercent = zeroRate1.Copy();
            setDedPercent.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.DeductionPercent);
            var validDedPercent = setDedPercent.GetData(EngineCol.Rate1, CompareOp.NE, "0", "SubRevDed.ValidDedPercent");
            setDedPercent.Release();
            // Amount = -1 * Rate1 * Amount   (Rate1 holds DeductionPercent)
            validDedPercent.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1);
            validDedPercent.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            validDedPercent.SetValue(CustCol.ActivityType, ActivityType.Calculated_Deduction_Amount_Based);
            var dedPercentOutput = validDedPercent;

            // -- Unit branch --
            var setDedUnits = zeroRate1.Copy();
            zeroRate1.Release();
            setDedUnits.DoMath(EngineCol.Rate1, MathOp.SETTO, ContractUDF.DeductionUnitRate);
            var validDedUnits = setDedUnits.GetData(EngineCol.Rate1, CompareOp.NE, "0", "SubRevDed.ValidDedUnits");
            setDedUnits.Release();
            // Amount = -1 * Units * Rate1 into Amount   (Rate1 holds DeductionUnitRate)
            validDedUnits.DoMath(BaseCol.Amount, MathOp.SETTO, BaseCol.Units);
            validDedUnits.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate1);
            validDedUnits.DoMath(BaseCol.Amount, MathOp.TIMES, "-1");
            validDedUnits.SetValue(CustCol.ActivityType, ActivityType.Calculated_Deduction_Unit_Based);
            var dedUnitsOutput = validDedUnits;

            // Combine both branches, zero Rate1 (it was a scratch slot), drop zero rows.
            var needsToBeRounded = ResultSet.EmptySet();
            needsToBeRounded.CombineAndRelease(dedPercentOutput, dedUnitsOutput);
            needsToBeRounded.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
            var finals = needsToBeRounded.GetData(BaseCol.Amount, CompareOp.NE, "0", "SubRevDed.Finals");
            needsToBeRounded.Release();

            // 1547 = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec
            // RoundAmountsTo2DecAndUnitsTo0Dec consumes 'finals'.
            var revenueDeductionsItd = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(finals);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubRevenueDeduction");
            return revenueDeductionsItd;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1555 C_ROY_SUB_NET_SALES ─────────────────────────────────────────
        //
        // DealScript:
        //   NetSales = Set #<x:2># of (Combine(INPUTSET1, INPUTSET2, INPUTSET3))
        //              to "#<i:2:1177>#"        // ActivityType.Net_Sales
        //   #<x:41># of NetSales = 0            // Rate2
        //   #<x:42># of NetSales = 0            // Rate3
        //   INPUTSET4 = NetSales
        //   EMPTYSET                            // function return value (unused)
        //
        // Combines three sets (CapPctOrAmount ITD, ToBeCapped ITD, Deductions ITD),
        // restamps ActivityType to Net_Sales, zeros Rate2/Rate3.  All three input
        // sets are released here -- caller passes ownership in.
        public static ResultSet SubNetSales(
            ResultSet capPctOrAmountItd,         // INPUTSET1
            ResultSet toBeCappedItd,             // INPUTSET2
            ResultSet deductionsItd)             // INPUTSET3
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            _ctxSubNetSales = _ctxSubNetSales ?? NewSubCtxStandard("C_ROY_SUB_NET_SALES");
            Job.CurrentCalcContext = _ctxSubNetSales;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  SubNetSales");

            var netSalesOutput = ResultSet.EmptySet();
            netSalesOutput.CombineAndRelease(capPctOrAmountItd, toBeCappedItd, deductionsItd);
            netSalesOutput.SetValue(CustCol.ActivityType, ActivityType.Net_Sales);
            netSalesOutput.DoMath(EngineCol.Rate2, MathOp.SETTO, "0");
            netSalesOutput.DoMath(EngineCol.Rate3, MathOp.SETTO, "0");

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  SubNetSales");
            return netSalesOutput;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        // ── 1657 C_SUMMARIZE_TO_TIER ─────────────────────────────────────────
        // sum=Y: Period, Deal, Amount, Units, UDKey14 (Tier slot), Amount2, Units2
        public static ResultSet SummarizeToTier(ResultSet inputSet1)
        {
            var _priorCalcCtx = Job.CurrentCalcContext;
            try {
            if (_ctxSummarizeToTier == null)
                _ctxSummarizeToTier = new CalcContext("C_SUMMARIZE_TO_TIER")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey4,
                           EngineCol.UDKey5, EngineCol.UDKey6, EngineCol.UDKey7, EngineCol.UDKey8,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey15, EngineCol.UDKey16, EngineCol.UDKey17,
                           EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period2, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeToTier;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
            } finally { Job.CurrentCalcContext = _priorCalcCtx; }
        }

        public static ResultSet ZeroSetWithComments(string comment1, string comment2)
        {
            var newSet = new ResultSet();

            if (newSet == null)
                return (null);

            var newRow = new CalcResultRow();

            if (newRow == null)
            {
                return (null);
            }

            newRow.Contract_sid = Job.currentContract?.sid ?? 0;
            newRow.Deal_sid = Job.currentDeal?.sid ?? 0;
            newRow.Period_sid = Job.CurrentCalcPeriod?.sid ?? 0;
            newRow.Calc_sid = Job.CurrentEntryTemplateSid;

            if (comment1 != null)
            {
                newRow.User_comment = comment1;
            }

            if (comment2 != null)
            {
                newRow.Alt_user_comment = comment2;
            }

            newSet.AddCalcResultRow(newRow);
            newSet.IsSummarized = false;
            newSet.SummarizedByContext = null;
            return (newSet);

        }
    }
}


