// BC1 BaseConfig — Common library (cross-domain utilities used by both Royalty
// and MAGR pipelines).  Generated from bc1_master c_calc + c_calc_context.
//
// Contents:
//   1543  C_REMOVE_ZEROS                              — RemoveZeros
//   1547  C_ROUND_AMOUNTS_TO_2_DEC_AND_UNITS_TO_0_DEC — RoundAmountsTo2DecAndUnitsTo0Dec
//   1554  C_SUMMARIZE_TO_OTHERPERIOD_TIER             — SummarizeToOtherPeriodTier  (cross-domain)
//   1562  C_SUMMARIZE_TO_RECOUPMENT_GROUP             — SummarizeToRecoupmentGroup  (cross-domain)
//   1666  C_ROUND_ALL_TO_2_DEC                        — RoundAllTo2Dec
//
// CalcContext column lists are derived from c_calc_context.summarize_by_column_flag
// — the Reset() arg list is the COMPLEMENT of sum=Y columns (anything NOT in
// the sum_y set is reset to its zero/empty value before summarization).
using VelocityProto.Handles;

namespace VelocityProto
{
    public static class CommonLib
    {
        // CalcContext fields (lazy-allocated).
        // private static CalcContext _ctxRemoveZeros;
        private static CalcContext _ctxRoundAmountsTo2DecAndUnitsTo0Dec;
        private static CalcContext _ctxSummarizeToOtherPeriodTier;
        private static CalcContext _ctxSummarizeToRecoupmentGroup;
        private static CalcContext _ctxRoundAllTo2Dec;

        // ── C_REMOVE_ZEROS  (calc_sid=1543) ──────────────────────────────────
        //
        // DealScript:
        //   PassToNonZeros = INPUTSET1 * 1
        //   NonZeroAmount  = GetData From PassToNonZeros for #<x:19># <> 0
        //   NonZeroUnits   = GetData From PassToNonZeros for #<x:19># = 0 , #<x:20># <> 0
        //   NonZeroAmount2 = GetData From PassToNonZeros for #<x:19># = 0 , #<x:20># = 0 , #<x:51># <> 0
        //   NonZeroUnits2  = GetData From PassToNonZeros for #<x:19># = 0 , #<x:20># = 0 , #<x:51># = 0 , #<x:52># <> 0
        //   Output = Combine(NonZeroAmount, NonZeroUnits, NonZeroAmount2, NonZeroUnits2)
        //
        // Removes rows where Amount, Amount2, Units, AND Units2 are all zero.
        // Uses SplitData chain so each non-zero subset peels off in one pass.
        public static ResultSet RemoveZeros(ResultSet inputSet1)
        {
            var nonZeroAmount  = inputSet1.SplitData(BaseCol.Amount,  CompareOp.NE, "0");
            var nonZeroUnits   = inputSet1.SplitData(BaseCol.Units,   CompareOp.NE, "0");
            var nonZeroAmount2 = inputSet1.SplitData(BaseCol.Amount2, CompareOp.NE, "0");
            var nonZeroUnits2  = inputSet1.GetData  (BaseCol.Units2,  CompareOp.NE, "0");
            inputSet1.Release();

            var output = ResultSet.EmptySet();
            output.CombineAndRelease(nonZeroAmount, nonZeroUnits, nonZeroAmount2, nonZeroUnits2);
            nonZeroAmount.Release();
            nonZeroUnits.Release();
            nonZeroAmount2.Release();
            nonZeroUnits2.Release();
            return output;
        }

        // ── C_ROUND_AMOUNTS_TO_2_DEC_AND_UNITS_TO_0_DEC  (calc_sid=1547) ─────
        //
        // Reset columns (NOT in sum=Y set):
        //   Period5/Period4/Period3/Period2 (none — only Period (13) and ActualPeriod (14) preserved
        //   from period family).  Per c_calc_context inspection:
        //   sum=Y on:  UDKey1-8, UDKey12-18, Rate, Period, ActualPeriod, RetailPrice (Price_point),
        //              AltPricePoint, Deal, Amount, Units, OtherPeriod, Rate2, Rate3, FromDate, ToDate,
        //              Comment1, Comment2, Contact1-4, Amount2, Units2.
        //   Round on:  Amount/Amount2 → 2 dp ; Units/Units2 → 0 dp.
        //   Reset:     UDKey9-11, UDKey19-20, Contract.
        //
        // For now Reset is conservative — preserves what BC1's c_calc_context says
        // and resets everything else.  Caller pattern: set CurrentCalcContext, then
        // Summarize() the input.  The helper consumes the input -- on the new-set
        // return path it Releases the original; on the same-set return it does not.
        // (Mid-pipeline summarize -- NOT Output(), which is end-of-entry-point only.)
        public static ResultSet RoundAmountsTo2DecAndUnitsTo0Dec(ResultSet inputSet1)
        {
            if (_ctxRoundAmountsTo2DecAndUnitsTo0Dec == null)
                _ctxRoundAmountsTo2DecAndUnitsTo0Dec = new CalcContext("C_ROUND_AMOUNTS_TO_2_DEC_AND_UNITS_TO_0_DEC")
                    .Reset(EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11,
                           EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract);
            Job.CurrentCalcContext = _ctxRoundAmountsTo2DecAndUnitsTo0Dec;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
        }

        // ── C_ROUND_ALL_TO_2_DEC  (calc_sid=1666) ────────────────────────────
        //
        // Same column set as 1547 (sum=Y on the same wide set), but rounds Amount,
        // Amount2, Units, Units2 ALL to 2 dp (vs 1547's 2/0/2/0).
        // The c_calc_context round flags would parameterize this, but Velocity's
        // CalcContext doesn't expose per-column rounding yet — the rounding happens
        // in the engine when results are written.  Same Reset() shape as 1547.
        public static ResultSet RoundAllTo2Dec(ResultSet inputSet1)
        {
            if (_ctxRoundAllTo2Dec == null)
                _ctxRoundAllTo2Dec = new CalcContext("C_ROUND_ALL_TO_2_DEC")
                    .Reset(EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11,
                           EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract);
            Job.CurrentCalcContext = _ctxRoundAllTo2Dec;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
        }

        // ── C_SUMMARIZE_TO_OTHERPERIOD_TIER  (calc_sid=1554) ─────────────────
        //
        // Cross-domain helper (called by ROY 1553 + MAGR 1691).
        // sum=Y on: Period (13), Deal (18), Amount (19), Units (20),
        //           OtherPeriod (26), UDKey14 (34), Amount2 (51), Units2 (52)
        // Reset:    everything else (UDKey1-8, UDKey9-11, UDKey15-20, UDKey12-13,
        //           Contract, ActualPeriod, Rate, Rate2, Rate3, Price points,
        //           Dates, Comments, Contacts).
        public static ResultSet SummarizeToOtherPeriodTier(ResultSet inputSet1)
        {
            if (_ctxSummarizeToOtherPeriodTier == null)
                _ctxSummarizeToOtherPeriodTier = new CalcContext("C_SUMMARIZE_TO_OTHERPERIOD_TIER")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey4,
                           EngineCol.UDKey5, EngineCol.UDKey6, EngineCol.UDKey7, EngineCol.UDKey8,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey15, EngineCol.UDKey16, EngineCol.UDKey17,
                           EngineCol.UDKey18, EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period2, // ActualPeriod
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeToOtherPeriodTier;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
        }

        // ── C_SUMMARIZE_TO_RECOUPMENT_GROUP  (calc_sid=1562) ─────────────────
        //
        // Cross-domain helper (called by ROY 1644 + MAGR 1701).
        // sum=Y on: Period (13), Deal (18), Amount (19), Units (20),
        //           UDKey15 (35), Amount2 (51), Units2 (52)
        public static ResultSet SummarizeToRecoupmentGroup(ResultSet inputSet1)
        {
            if (_ctxSummarizeToRecoupmentGroup == null)
                _ctxSummarizeToRecoupmentGroup = new CalcContext("C_SUMMARIZE_TO_RECOUPMENT_GROUP")
                    .Reset(EngineCol.UDKey1, EngineCol.UDKey2, EngineCol.UDKey3, EngineCol.UDKey4,
                           EngineCol.UDKey5, EngineCol.UDKey6, EngineCol.UDKey7, EngineCol.UDKey8,
                           EngineCol.UDKey9, EngineCol.UDKey10, EngineCol.UDKey11, EngineCol.UDKey12,
                           EngineCol.UDKey13, EngineCol.UDKey14,
                           EngineCol.UDKey16, EngineCol.UDKey17, EngineCol.UDKey18,
                           EngineCol.UDKey19, EngineCol.UDKey20,
                           EngineCol.Contract, EngineCol.Period2, EngineCol.Period3,
                           EngineCol.Rate1, EngineCol.Rate2, EngineCol.Rate3,
                           EngineCol.Price1, EngineCol.Price2,
                           EngineCol.FromDate, EngineCol.ToDate,
                           EngineCol.Comment, EngineCol.AltComment,
                           EngineCol.Contact1, EngineCol.Contact2, EngineCol.Contact3, EngineCol.Contact4);
            Job.CurrentCalcContext = _ctxSummarizeToRecoupmentGroup;
            var result = inputSet1.Summarize();
            if (!ReferenceEquals(result, inputSet1)) inputSet1.Release();
            return result;
        }
    }
}
