using System;
using System.Collections.Generic;

namespace VelocityProto
{
    // BaseConfig library — DealScript Lookup / LookupUDF replacements.
    // Standalone static class; methods are extension methods on ResultSet so
    // call sites keep their fluent shape (`mySet.Lookup(...)`).  TAs use the
    // engine's ResultSet surface from the outside rather than extending it
    // via partial class.
    public static class ResultSetLookup
    {
        // ════════════════════════════════════════════════════════════════════
        //  Lookup  —  in-memory C# translation of DealScript's keyed-lookup join
        //
        //    Output = #<x:target># of LeftSet (op) Lookup #<x:value>#
        //                                  by #<x:key># from RightSet
        //                                  into #<x:target>#
        //
        //  For each row in LeftSet, finds the row(s) in RightSet whose
        //  keyCol matches the left row's keyCol.  Sums valueCol across those
        //  matched right rows (DealScript "summary lookup" semantics) and
        //  combines that sum with the left row's target column via MathOp.
        //  Rows with no matching right entry are left unchanged (identity).
        //
        //  Distinction from p_ds_detail_merge: detail-merge auto-detects all
        //  shared TC keys and applies multi-column factors; Lookup uses one
        //  named key column and applies one operation to one named column.
        // ════════════════════════════════════════════════════════════════════
        public static void Lookup(
            this ResultSet self,
            IDecimalCol target,
            MathOp      op,
            IDecimalCol valueCol,
            IListableCol keyCol,
            ResultSet   rightSet)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2,
                "Start: Lookup, target={0}, op={1}, value={2}, key={3}, right.rows={4}",
                target.InternalName, op, valueCol.InternalName, keyCol.InternalName, rightSet?.Rows ?? 0);

            if (rightSet == null || rightSet.Rows == 0)
            {
                LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: Lookup (right empty, no-op)");
                return;
            }

            string targetInternal = Terminology.TranslateContextElement(target.InternalName);
            string valueInternal  = Terminology.TranslateContextElement(valueCol.InternalName);
            string keyInternal    = Terminology.TranslateContextElement(keyCol.InternalName);

            // Build sid → summed-value map from the right set.
            var rightMap = new Dictionary<int, decimal>();
            rightSet.ForEachRow(r =>
            {
                int sid = ResultSet.GetSidFromRow(r, keyInternal);
                decimal v = ReadDecimalCol(r, valueInternal);
                if (rightMap.TryGetValue(sid, out decimal existing))
                    rightMap[sid] = existing + v;
                else
                    rightMap[sid] = v;
            });

            // Apply per left row.
            self.ForEachRow(left =>
            {
                int sid = ResultSet.GetSidFromRow(left, keyInternal);
                if (!rightMap.TryGetValue(sid, out decimal source)) return;  // no match → no change
                decimal current = ReadDecimalCol(left, targetInternal);
                WriteDecimalCol(left, targetInternal, ApplyMathOp(current, op, source));
            });

            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: Lookup");
        }

        // Multi-key overload of Lookup.  DealScript supports compound keys, e.g.
        //    Lookup #<x:20># by #<x:14># , #<x:1># , #<x:4># from RightSet
        // joins on (ActualPeriod, Product, ISRC).  Each left row finds matching
        // right rows whose ALL key columns equal the left row's, then sums the
        // value column across matched right rows.
        public static void Lookup(
            this ResultSet self,
            IDecimalCol  target,
            MathOp       op,
            IDecimalCol  valueCol,
            ResultSet    rightSet,
            params IListableCol[] keyCols)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2,
                "Start: Lookup (multi-key), target={0}, op={1}, value={2}, keyCount={3}, right.rows={4}",
                target.InternalName, op, valueCol.InternalName, keyCols?.Length ?? 0, rightSet?.Rows ?? 0);

            if (rightSet == null || rightSet.Rows == 0)
            {
                LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: Lookup (right empty, no-op)");
                return;
            }
            if (keyCols == null || keyCols.Length == 0)
            {
                LogMsg.Warn("Lookup: no key columns supplied; no-op");
                return;
            }

            string targetInternal = Terminology.TranslateContextElement(target.InternalName);
            string valueInternal  = Terminology.TranslateContextElement(valueCol.InternalName);
            string[] keyInternals = new string[keyCols.Length];
            for (int i = 0; i < keyCols.Length; i++)
                keyInternals[i] = Terminology.TranslateContextElement(keyCols[i].InternalName);

            string MakeKey(CalcResultRow row)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < keyInternals.Length; i++)
                {
                    if (i > 0) sb.Append('\x01');
                    sb.Append(ResultSet.GetSidFromRow(row, keyInternals[i]));
                }
                return sb.ToString();
            }

            var rightMap = new Dictionary<string, decimal>();
            rightSet.ForEachRow(r =>
            {
                string k = MakeKey(r);
                decimal v = ReadDecimalCol(r, valueInternal);
                if (rightMap.TryGetValue(k, out decimal existing))
                    rightMap[k] = existing + v;
                else
                    rightMap[k] = v;
            });

            self.ForEachRow(left =>
            {
                string k = MakeKey(left);
                if (!rightMap.TryGetValue(k, out decimal source)) return;
                decimal current = ReadDecimalCol(left, targetInternal);
                WriteDecimalCol(left, targetInternal, ApplyMathOp(current, op, source));
            });

            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: Lookup (multi-key)");
        }

        // ════════════════════════════════════════════════════════════════════
        //  LookupUDF  —  in-memory C# translation of DealScript's per-row
        //                UDF lookup join, e.g.
        //
        //    Output = #<x:target># of LeftSet (op) Lookup ContactUDF
        //                                  by PaymentRecipient into #<x:target>#
        //
        //  For each row in LeftSet, reads the entity SID from keyCol, calls
        //  DS.GetUDFValue(udfHandle, sid) to fetch the UDF's stored value,
        //  parses as decimal, and combines with target via op.  Rows whose
        //  key sid is 0 / unspecified, or whose UDF value is null/non-numeric,
        //  are left unchanged.
        // ════════════════════════════════════════════════════════════════════
        // Text variant of LookupUDF — for each row, read keyCol's entity sid and
        // write `DS.GetUDFValue(udfHandle, sid)` into the text column `target`.
        // Used to translate DealScript patterns like `Set <x:45> = <x:48>.<u:N>`
        // (e.g. PaymentRecipient.PayeeStatus, where the contact_sid comes from a
        // column other than the default ContractedParty).  No MathOp parameter:
        // text comparisons / merges aren't well-defined for string concatenation,
        // so the semantic is always SETTO (replace target with looked-up value).
        // Empty UDF results clear the target column.
        public static void LookupUDF(
            this ResultSet self,
            ITextCol     target,
            string       udfHandle,
            IListableCol keyCol)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2,
                "Start: LookupUDF (text), target={0}, udf={1}, key={2}",
                target.InternalName, udfHandle, keyCol.InternalName);

            if (string.IsNullOrEmpty(udfHandle))
            {
                LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: LookupUDF (null udfHandle, no-op)");
                return;
            }

            string targetInternal = Terminology.TranslateContextElement(target.InternalName);
            string keyInternal    = Terminology.TranslateContextElement(keyCol.InternalName);

            self.ForEachRow(left =>
            {
                int sid = ResultSet.GetSidFromRow(left, keyInternal);
                string raw = sid == 0 || sid == C.InvalidInt ? null : DS.GetUDFValue(udfHandle, sid);
                WriteTextCol(left, targetInternal, raw);
            });

            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: LookupUDF (text)");
        }

        // Writes a text column on a row by internal-CE name.  Mirrors the dispatch
        // in WriteDecimalCol for the engine's text columns.
        private static void WriteTextCol(CalcResultRow row, string internalName, string value)
        {
            switch (internalName)
            {
                case "Comment1":     row.User_comment      = value; break;
                case "Text1":        row.User_comment      = value; break;
                case "User_comment": row.User_comment      = value; break;
                case "Comment2":     row.Alt_user_comment  = value; break;
                case "Text2":        row.Alt_user_comment  = value; break;
                case "Alt_user_comment": row.Alt_user_comment = value; break;
                case "ScratchText1": row.Scratch_text1     = value; break;
                case "ScratchText2": row.Scratch_text2     = value; break;
                default:
                    LogMsg.Warn("LookupUDF: unknown text column '{0}', write skipped", internalName);
                    break;
            }
        }

        public static void LookupUDF(
            this ResultSet self,
            IDecimalCol  target,
            MathOp       op,
            string       udfHandle,
            IListableCol keyCol)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2,
                "Start: LookupUDF, target={0}, op={1}, udf={2}, key={3}",
                target.InternalName, op, udfHandle, keyCol.InternalName);

            if (string.IsNullOrEmpty(udfHandle))
            {
                LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: LookupUDF (null udfHandle, no-op)");
                return;
            }

            string targetInternal = Terminology.TranslateContextElement(target.InternalName);
            string keyInternal    = Terminology.TranslateContextElement(keyCol.InternalName);

            self.ForEachRow(left =>
            {
                int sid = ResultSet.GetSidFromRow(left, keyInternal);
                if (sid == 0 || sid == C.InvalidInt) return;
                string raw = DS.GetUDFValue(udfHandle, sid);
                if (string.IsNullOrEmpty(raw)) return;
                if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal source))
                    return;
                decimal current = ReadDecimalCol(left, targetInternal);
                WriteDecimalCol(left, targetInternal, ApplyMathOp(current, op, source));
            });

            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1, "End: LookupUDF");
        }

        // ── Internal helpers ──────────────────────────────────────────────

        // Reads a decimal column from a row by the engine's internal column name.
        // Internal names are the engine-side identifiers ("Amount", "Amount2",
        // "Rate1", "Rate2", "Rate3", "RetailPrice", "PPDPrice", "Units", "Units2").
        // Customer aliases pass through Terminology.TranslateContextElement first.
        private static decimal ReadDecimalCol(CalcResultRow row, string internalName)
        {
            switch (internalName)
            {
                case "Amount":      return row.Amount;
                case "Amount1":     return row.Amount;
                case "Amount2":     return row.Alt_amount;
                case "Alt_amount":  return row.Alt_amount;
                case "Amount3":     return row.Amount_3;
                case "Amount4":     return row.Amount_4;
                case "Units":       return row.Qty;
                case "Unit1":       return row.Qty;
                case "Qty":         return row.Qty;
                case "Units2":      return row.Alt_qty;
                case "Unit2":       return row.Alt_qty;
                case "Alt_qty":     return row.Alt_qty;
                case "Units3":      return row.Qty_3;
                case "Units4":      return row.Qty_4;
                case "Rate1":       return row.User_rate;
                case "Rate2":       return row.User_2_rate;
                case "Rate3":       return row.User_3_rate;
                case "Rate4":       return row.Rate_4;
                case "Rate5":       return row.Rate_5;
                case "RetailPrice": return row.Price_point;
                case "Price1":      return row.Price_point;
                case "PPDPrice":    return row.Alt_price_point;
                case "Price2":      return row.Alt_price_point;
                case "Price3":      return row.Price_point_3;
                case "Price4":      return row.Price_point_4;
                case "Price5":      return row.Price_point_5;
                case "Scratch1":    return row.Scratch1;
                case "Scratch2":    return row.Scratch2;
                default:
                    LogMsg.Warn("Lookup: unknown decimal column '{0}', returning 0", internalName);
                    return 0m;
            }
        }

        private static void WriteDecimalCol(CalcResultRow row, string internalName, decimal value)
        {
            switch (internalName)
            {
                case "Amount":      row.Amount         = value; break;
                case "Amount1":     row.Amount         = value; break;
                case "Amount2":     row.Alt_amount     = value; break;
                case "Alt_amount":  row.Alt_amount     = value; break;
                case "Amount3":     row.Amount_3       = value; break;
                case "Amount4":     row.Amount_4       = value; break;
                case "Units":       row.Qty            = value; break;
                case "Unit1":       row.Qty            = value; break;
                case "Qty":         row.Qty            = value; break;
                case "Units2":      row.Alt_qty        = value; break;
                case "Unit2":       row.Alt_qty        = value; break;
                case "Alt_qty":     row.Alt_qty        = value; break;
                case "Units3":      row.Qty_3          = value; break;
                case "Units4":      row.Qty_4          = value; break;
                case "Rate1":       row.User_rate      = value; break;
                case "Rate2":       row.User_2_rate    = value; break;
                case "Rate3":       row.User_3_rate    = value; break;
                case "Rate4":       row.Rate_4         = value; break;
                case "Rate5":       row.Rate_5         = value; break;
                case "RetailPrice": row.Price_point    = value; break;
                case "Price1":      row.Price_point    = value; break;
                case "PPDPrice":    row.Alt_price_point = value; break;
                case "Price2":      row.Alt_price_point = value; break;
                case "Price3":      row.Price_point_3  = value; break;
                case "Price4":      row.Price_point_4  = value; break;
                case "Price5":      row.Price_point_5  = value; break;
                case "Scratch1":    row.Scratch1       = value; break;
                case "Scratch2":    row.Scratch2       = value; break;
                default:
                    LogMsg.Warn("Lookup: unknown decimal column '{0}', write skipped", internalName);
                    break;
            }
        }

        // Applies the math operator (current op source) → new value.
        // For SETTO, the source replaces the current value (target = source).
        private static decimal ApplyMathOp(decimal current, MathOp op, decimal source)
        {
            switch (op)
            {
                case MathOp.SETTO:     return source;
                case MathOp.PLUS:      return current + source;
                case MathOp.MINUS:     return current - source;
                case MathOp.TIMES:     return current * source;
                case MathOp.DIVIDEDBY: return source == 0m ? current : current / source;
                case MathOp.ROUNDTO:
                    int digits = (int)source;
                    return Math.Round(current, digits, MidpointRounding.AwayFromZero);
                default:
                    LogMsg.Warn("Lookup: unsupported MathOp {0}, returning unchanged", op);
                    return current;
            }
        }
    }
}
