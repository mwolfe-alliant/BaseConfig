using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityProto
{
    public partial class ResultSet
    {
        // ════════════════════════════════════════════════════════════════════
        //  Prorate12  —  in-memory C# translation of p_ds_proration_12
        //
        //  Allocates the less-detailed IS1 in proportion to the more-detailed
        //  IS2, using C# decimal (28-digit) intermediate fractions — equivalent
        //  to the SP's scale-12 fractions.
        //
        //  The "common level" (join key between IS1 and IS2) is auto-detected:
        //  any TC column that has at least one non-default value in BOTH sets
        //  is included in the key.  IS2-only active columns (active in IS2 but
        //  not in the common level) are the "expansion" detail — their values
        //  are copied from each IS2 row into the corresponding output row.
        //
        //  IS1 rows with no matching IS2 proration base are collected into
        //  unmatchedRows (overload) or silently dropped (simple overload).
        //
        //  Parameters replace the IS3 control-set from the original DealScript
        //  ExecuteProcedure call:
        //
        //    mode              0  zero-sum IS2 groups → output 0            (default)
        //                      1  zero-sum IS2 groups → use divisor 1
        //                         (preserves IS2 distribution shape; output
        //                          still sums to 0 at the less-detailed level)
        //
        //    roundingCorrMode  0  ON  – assign discrepancy to first row      (default)
        //                      1  OFF – no correction applied
        //                      2  ON  – assign to first row in SID sort order
        //                      3  ON  – spread in unit increments across rows
        //                               in SID sort order; remainder to row 1
        //
        //    amountScale       rounding decimal places for Amount            (default 2)
        //    qtyScale          rounding decimal places for Qty               (default 2)
        //    altAmountScale    rounding decimal places for Alt_amount        (default 2)
        //    altQtyScale       rounding decimal places for Alt_qty           (default 2)
        //
        //  DealScript equivalent:
        //    Output = ExecuteProcedure p_ds_proration_12 using IS1, IS2 [, IS3]
        // ════════════════════════════════════════════════════════════════════

        // Simple overload — unmatched IS1 rows are dropped (with Console.WriteLine).
        public static ResultSet Prorate12(
            ResultSet is1,
            ResultSet is2,
            int mode             = 0,
            int roundingCorrMode = 0,
            int amountScale      = 2,
            int qtyScale         = 2,
            int altAmountScale   = 2,
            int altQtyScale      = 2)
        {
            return Prorate12(is1, is2, mode, roundingCorrMode,
                amountScale, qtyScale, altAmountScale, altQtyScale,
                out _);
        }

        // Full overload — unmatched IS1 rows are placed into unmatchedRows.
        public static ResultSet Prorate12(
            ResultSet is1,
            ResultSet is2,
            int mode,
            int roundingCorrMode,
            int amountScale,
            int qtyScale,
            int altAmountScale,
            int altQtyScale,
            out ResultSet unmatchedRows)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2, "Start: Prorate12, is1.rows={0}, is2.rows={1}", is1?.Rows ?? 0, is2?.Rows ?? 0);
            if (roundingCorrMode < 0 || roundingCorrMode > 3)
            {
                LogMsg.Warn("Prorate12: roundingCorrMode {0} is invalid — only values 0-3 are allowed. Defaulting to 0.", roundingCorrMode);
                roundingCorrMode = 0;
            }

            // ── Step 1: detect active columns in each input set ───────────
            //
            // A column is "active" in a set if at least one row has a non-default
            // value (non-zero for ints/decimals, non-null for strings).
            // Mirrors the level-detection while loop in p_ds_proration_12.

            var activeIs1 = P12_DetectActiveColumns(is1);
            var activeIs2 = P12_DetectActiveColumns(is2);

            // Common level: active in both → forms the join key.
            var common = new HashSet<int>(activeIs1);
            common.IntersectWith(activeIs2);

            // IS2-only: active in IS2 but not in the common level.
            // These are the "expansion" columns copied to each output row.
            var is2Only = new HashSet<int>(activeIs2);
            is2Only.ExceptWith(common);

            // ── Step 2: group IS2 rows by (deal_sid + common key) ────────
            var is2Groups = new Dictionary<string, P12_Group>();
            is2.ForEachRow(is2Row =>
            {
                string key = P12_BuildKey(is2Row, common);
                if (!is2Groups.TryGetValue(key, out P12_Group grp))
                {
                    grp = new P12_Group();
                    is2Groups[key] = grp;
                }
                grp.Rows.Add(is2Row);
                grp.TotalAmount    += is2Row.Amount;
                grp.TotalQty       += is2Row.Qty;
                grp.TotalAltAmount += is2Row.Alt_amount;
                grp.TotalAltQty    += is2Row.Alt_qty;
            });

            // ── Step 3: prorate each IS1 row against its IS2 group ───────
            var output    = EmptySet();
            var unmatched = EmptySet();

            is1.ForEachRow(is1Row =>
            {
                string key = P12_BuildKey(is1Row, common);

                if (!is2Groups.TryGetValue(key, out P12_Group grp))
                {
                    // Per p_ds_proration_12: IS1 rows with no IS2 proration base
                    // are not included in the output.
                    LogMsg.Warn("Prorate12: IS1 row has no IS2 proration base (deal={0} period={1}) — row placed in unmatched set.", is1Row.Deal_sid, is1Row.Period_sid);
                    unmatched.AddCalcResultRow(is1Row.Clone());
                    return;
                }

                // Denominators for each data column.
                // Mode 0: zero group total → denominator 0 → output 0.
                // Mode 1: zero group total → denominator 1 → pass distribution through.
                decimal amtDen    = grp.TotalAmount    != 0m ? grp.TotalAmount    : (mode == 1 ? 1m : 0m);
                decimal qtyDen    = grp.TotalQty       != 0m ? grp.TotalQty       : (mode == 1 ? 1m : 0m);
                decimal altAmtDen = grp.TotalAltAmount != 0m ? grp.TotalAltAmount : (mode == 1 ? 1m : 0m);
                decimal altQtyDen = grp.TotalAltQty    != 0m ? grp.TotalAltQty    : (mode == 1 ? 1m : 0m);

                // Build output rows for this IS1 row — one per IS2 row in the group.
                var outRows = new List<CalcResultRow>(grp.Rows.Count);
                foreach (CalcResultRow is2Row in grp.Rows)
                {
                    var outRow = is1Row.Clone();
                    P12_CopyColumns(is2Row, outRow, is2Only);

                    // Intermediate fractions use full C# decimal precision (28 digits),
                    // equivalent to the SP's DECIMAL(23,12) fraction columns.
                    outRow.Amount     = amtDen    == 0m ? 0m
                        : Math.Round(is1Row.Amount     * (is2Row.Amount     / amtDen),    amountScale);
                    outRow.Qty        = qtyDen    == 0m ? 0m
                        : Math.Round(is1Row.Qty        * (is2Row.Qty        / qtyDen),    qtyScale);
                    outRow.Alt_amount = altAmtDen == 0m ? 0m
                        : Math.Round(is1Row.Alt_amount * (is2Row.Alt_amount / altAmtDen), altAmountScale);
                    outRow.Alt_qty    = altQtyDen == 0m ? 0m
                        : Math.Round(is1Row.Alt_qty    * (is2Row.Alt_qty    / altQtyDen), altQtyScale);

                    outRows.Add(outRow);
                }

                // ── Step 4: rounding correction ───────────────────────────
                // Ensures sum(output amounts) == IS1 target for each data column.
                // Skipped when roundingCorrMode == 1 (correction OFF) or when the
                // denominator was 0 (Mode 0 zero-sum group — IS1 amount intentionally lost).
                if (roundingCorrMode != 1 && outRows.Count > 0)
                {
                    if (amtDen    != 0m)
                        P12_ApplyRoundingCorrection(outRows, is1Row.Amount,
                            amountScale,    r => r.Amount,     (r, v) => r.Amount = v,     roundingCorrMode);
                    if (qtyDen    != 0m)
                        P12_ApplyRoundingCorrection(outRows, is1Row.Qty,
                            qtyScale,       r => r.Qty,        (r, v) => r.Qty = v,        roundingCorrMode);
                    if (altAmtDen != 0m)
                        P12_ApplyRoundingCorrection(outRows, is1Row.Alt_amount,
                            altAmountScale, r => r.Alt_amount, (r, v) => r.Alt_amount = v, roundingCorrMode);
                    if (altQtyDen != 0m)
                        P12_ApplyRoundingCorrection(outRows, is1Row.Alt_qty,
                            altQtyScale,    r => r.Alt_qty,    (r, v) => r.Alt_qty = v,    roundingCorrMode);
                }

                foreach (var outRow in outRows)
                    output.AddCalcResultRow(outRow);
            });

            unmatchedRows = unmatched;
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: Prorate12, result.rows={0}", output.Rows);
            return output;
        }

        // ── Column descriptor ─────────────────────────────────────────────
        //
        // Covers the columns inspected by p_ds_proration_12's level-detection
        // loop (c_context_element sort_order_nbr 1-34):
        //   period_sid, actual_period_sid, other_period_sid,
        //   udkey_1_sid – udkey_20_sid,
        //   user_contact_sid – user_contact_4_sid,
        //   user_rate, user_2_rate, user_3_rate,
        //   price_point, alt_price_point,
        //   user_comment, alt_user_comment.
        //
        // deal_sid is always included in the key directly (not via this list).
        // Int/decimal columns: active if != 0.
        // String columns:      active if != null.

        private struct P12_ColDesc
        {
            public Func<CalcResultRow, bool>               IsActive;
            public Func<CalcResultRow, string>             GetKeyPart;
            public Action<CalcResultRow, CalcResultRow>    CopyTo;
        }

        private static readonly P12_ColDesc[] P12_Cols = BuildP12Cols();

        private static P12_ColDesc[] BuildP12Cols()
        {
            P12_ColDesc IntCol(
                Func<CalcResultRow, int>    get,
                Action<CalcResultRow, int>  set) => new P12_ColDesc
            {
                IsActive   = r => get(r) != 0,
                GetKeyPart = r => get(r).ToString(),
                CopyTo     = (s, d) => set(d, get(s)),
            };

            P12_ColDesc DecCol(
                Func<CalcResultRow, decimal>    get,
                Action<CalcResultRow, decimal>  set) => new P12_ColDesc
            {
                IsActive   = r => get(r) != 0m,
                GetKeyPart = r => get(r).ToString(),
                CopyTo     = (s, d) => set(d, get(s)),
            };

            P12_ColDesc StrCol(
                Func<CalcResultRow, string>    get,
                Action<CalcResultRow, string>  set) => new P12_ColDesc
            {
                IsActive   = r => get(r) != null,
                GetKeyPart = r => get(r) ?? "",
                CopyTo     = (s, d) => set(d, get(s)),
            };

            return new P12_ColDesc[]
            {
                // [0]  period_sid
                IntCol(r => r.Period_sid,          (r, v) => r.Period_sid          = v),
                // [1]  actual_period_sid
                IntCol(r => r.Actual_period_sid,   (r, v) => r.Actual_period_sid   = v),
                // [2]  other_period_sid
                IntCol(r => r.Other_period_sid,    (r, v) => r.Other_period_sid    = v),
                // [3–22]  udkey_1_sid – udkey_20_sid
                IntCol(r => r.Udkey_1_sid,         (r, v) => r.Udkey_1_sid         = v),
                IntCol(r => r.Udkey_2_sid,         (r, v) => r.Udkey_2_sid         = v),
                IntCol(r => r.Udkey_3_sid,         (r, v) => r.Udkey_3_sid         = v),
                IntCol(r => r.Udkey_4_sid,         (r, v) => r.Udkey_4_sid         = v),
                IntCol(r => r.Udkey_5_sid,         (r, v) => r.Udkey_5_sid         = v),
                IntCol(r => r.Udkey_6_sid,         (r, v) => r.Udkey_6_sid         = v),
                IntCol(r => r.Udkey_7_sid,         (r, v) => r.Udkey_7_sid         = v),
                IntCol(r => r.Udkey_8_sid,         (r, v) => r.Udkey_8_sid         = v),
                IntCol(r => r.Udkey_9_sid,         (r, v) => r.Udkey_9_sid         = v),
                IntCol(r => r.Udkey_10_sid,        (r, v) => r.Udkey_10_sid        = v),
                IntCol(r => r.Udkey_11_sid,        (r, v) => r.Udkey_11_sid        = v),
                IntCol(r => r.Udkey_12_sid,        (r, v) => r.Udkey_12_sid        = v),
                IntCol(r => r.Udkey_13_sid,        (r, v) => r.Udkey_13_sid        = v),
                IntCol(r => r.Udkey_14_sid,        (r, v) => r.Udkey_14_sid        = v),
                IntCol(r => r.Udkey_15_sid,        (r, v) => r.Udkey_15_sid        = v),
                IntCol(r => r.Udkey_16_sid,        (r, v) => r.Udkey_16_sid        = v),
                IntCol(r => r.Udkey_17_sid,        (r, v) => r.Udkey_17_sid        = v),
                IntCol(r => r.Udkey_18_sid,        (r, v) => r.Udkey_18_sid        = v),
                IntCol(r => r.Udkey_19_sid,        (r, v) => r.Udkey_19_sid        = v),
                IntCol(r => r.Udkey_20_sid,        (r, v) => r.Udkey_20_sid        = v),
                // [23–26]  user_contact_sid – user_contact_4_sid
                IntCol(r => r.User_contact_sid,    (r, v) => r.User_contact_sid    = v),
                IntCol(r => r.User_contact_2_sid,  (r, v) => r.User_contact_2_sid  = v),
                IntCol(r => r.User_contact_3_sid,  (r, v) => r.User_contact_3_sid  = v),
                IntCol(r => r.User_contact_4_sid,  (r, v) => r.User_contact_4_sid  = v),
                // [27–29]  user rates
                DecCol(r => r.User_rate,           (r, v) => r.User_rate           = v),
                DecCol(r => r.User_2_rate,         (r, v) => r.User_2_rate         = v),
                DecCol(r => r.User_3_rate,         (r, v) => r.User_3_rate         = v),
                // [30–31]  price points
                DecCol(r => r.Price_point,         (r, v) => r.Price_point         = v),
                DecCol(r => r.Alt_price_point,     (r, v) => r.Alt_price_point     = v),
                // [32–33]  string comments
                StrCol(r => r.User_comment,        (r, v) => r.User_comment        = v),
                StrCol(r => r.Alt_user_comment,    (r, v) => r.Alt_user_comment    = v),
            };
        }

        // Returns the set of P12_Cols indices that have at least one
        // non-default value across all rows in the result set.
        private static HashSet<int> P12_DetectActiveColumns(ResultSet rs)
        {
            var active = new HashSet<int>();
            rs.ForEachRow(row =>
            {
                for (int i = 0; i < P12_Cols.Length; i++)
                {
                    if (!active.Contains(i) && P12_Cols[i].IsActive(row))
                        active.Add(i);
                }
            });
            return active;
        }

        // Builds a string join key: deal_sid followed by one segment per
        // P12_Cols entry.  A segment contains the column's value when the
        // column index is in commonIndices, or is empty otherwise.
        // '\x01' (ASCII SOH) is used as separator — safe for all data values.
        private static string P12_BuildKey(CalcResultRow row, HashSet<int> commonIndices)
        {
            var sb = new StringBuilder();
            sb.Append(row.Deal_sid);
            for (int i = 0; i < P12_Cols.Length; i++)
            {
                sb.Append('\x01');
                if (commonIndices.Contains(i))
                    sb.Append(P12_Cols[i].GetKeyPart(row));
                // else: empty segment — column not part of the common level
            }
            return sb.ToString();
        }

        // Copies the specified column values from src to dst.
        private static void P12_CopyColumns(CalcResultRow src, CalcResultRow dst,
            HashSet<int> colIndices)
        {
            foreach (int i in colIndices)
                P12_Cols[i].CopyTo(src, dst);
        }

        // Builds a deterministic sort key from all P12_Cols values on a row,
        // prefixed by deal_sid.  Used by rounding correction modes 2 and 3.
        private static string P12_BuildSortKey(CalcResultRow row)
        {
            var sb = new StringBuilder();
            sb.Append(row.Deal_sid);
            for (int i = 0; i < P12_Cols.Length; i++)
            {
                sb.Append('\x01');
                sb.Append(P12_Cols[i].GetKeyPart(row));
            }
            return sb.ToString();
        }

        // Returns 10^(-scale) as an exact decimal (avoids double intermediates).
        private static decimal P12_ScaleUnit(int scale)
        {
            decimal unit = 1m;
            for (int i = 0; i < scale; i++) unit /= 10m;
            return unit;
        }

        // Applies rounding correction to ensure sum(outRows[value]) == targetValue.
        //
        // corrMode 0: assign entire discrepancy to outRows[0] (first in list order).
        // corrMode 2: sort by SID key; assign entire discrepancy to first sorted row.
        // corrMode 3: sort by SID key; distribute in scale-unit increments starting
        //             from the first row and cycling through; remainder goes to row 1.
        //             (Algorithm per p_ds_proration_12 mode 3 description.)
        private static void P12_ApplyRoundingCorrection(
            List<CalcResultRow>             outRows,
            decimal                         targetValue,
            int                             scale,
            Func<CalcResultRow, decimal>    getVal,
            Action<CalcResultRow, decimal>  setVal,
            int                             corrMode)
        {
            decimal correction = targetValue - outRows.Sum(r => getVal(r));
            if (correction == 0m) return;

            // For modes 2 and 3 sort by the full SID key so the correction always
            // lands on the same row regardless of in-memory list order.
            List<CalcResultRow> rows = (corrMode == 0)
                ? outRows
                : outRows.OrderBy(P12_BuildSortKey).ToList();

            if (corrMode == 0 || corrMode == 2)
            {
                // Assign full discrepancy to the first row.
                setVal(rows[0], getVal(rows[0]) + correction);
                return;
            }

            // corrMode == 3: spread in unit increments, remainder to row 1.
            //
            // Algorithm (per SP documentation):
            //   While |remaining| >= unit: add ±unit to successive rows
            //   (cycling if needed); add any sub-unit remainder to row 1.
            //
            // Example: correction = 0.03456, scale = 2, unit = 0.01, 3 rows
            //   row 1 receives 0.01 + 0.00456 (unit + remainder) = 0.01456
            //   row 2 receives 0.01
            //   row 3 receives 0.01
            decimal unit      = P12_ScaleUnit(scale);
            decimal sign      = correction > 0m ? 1m : -1m;
            decimal remaining = correction;
            int     idx       = 0;

            while (Math.Abs(remaining) >= unit)
            {
                int rowIdx = idx % rows.Count;
                setVal(rows[rowIdx], getVal(rows[rowIdx]) + sign * unit);
                remaining -= sign * unit;
                idx++;
            }

            // Sub-unit remainder always goes to the first sorted row.
            if (remaining != 0m)
                setVal(rows[0], getVal(rows[0]) + remaining);
        }

        // Helper: holds the IS2 rows sharing a common-level key, plus their
        // pre-summed data column totals used as proration denominators.
        private class P12_Group
        {
            public List<CalcResultRow> Rows         = new List<CalcResultRow>();
            public decimal             TotalAmount;
            public decimal             TotalQty;
            public decimal             TotalAltAmount;
            public decimal             TotalAltQty;
        }
    }
}
