using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityProto
{
    public partial class ResultSet
    {
        // ════════════════════════════════════════════════════════════════════
        //  SetCalcErrorInRun  —  in-memory C# translation of p_ds_set_calc_error_in_run
        //
        //  If IS1 is empty, returns without error.  Otherwise logs one warning per IS1
        //  row (user_comment = identifier, alt_user_comment = detail), optionally
        //  prefixed by IS2's first row user_comment, then throws CalcJobAbortException.
        //
        //  DealScript equivalent:
        //    _ = ExecuteProcedure p_ds_set_calc_error_in_run Using IS1 [, IS2]
        // ════════════════════════════════════════════════════════════════════

        public static ResultSet SetCalcErrorInRun(ResultSet is1, ResultSet is2 = null)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2,
                "Start: SetCalcErrorInRun, is1.rows={0}", is1?.Rows ?? 0);

            if (is1 == null || is1.Rows == 0) return EmptySet();

            string calcName = Job.CurrentCalcContext?.CalcName ?? "(unknown)";
            string prefix   = "";
            if (is2 != null && is2.Rows > 0)
                prefix = is2.GetRowByGlobalIndex(0).User_comment ?? "";

            string header = $"Calculation {calcName} requested error in run.";

            is1.ForEachRow(row =>
            {
                string part1 = row.User_comment     ?? "";
                string part2 = row.Alt_user_comment ?? "";
                string parts = string.Join(" ",
                    new[] { prefix, part1, part2 }.Where(s => s.Length > 0));
                LogMsg.Warn("{0} {1}", header, parts);
            });

            LogMsg.Error(header);   // throws CalcJobAbortException
            return EmptySet();   // unreachable
        }

        // ════════════════════════════════════════════════════════════════════
        //  DetailMerge  —  in-memory C# translation of p_ds_detail_merge
        //
        //  Merges two result sets that differ in detail level.
        //  The common summarization level is auto-detected: any TC column
        //  that has at least one non-default value in BOTH IS1 and IS2 is a
        //  join key.  IS1 rows are inner-joined to IS2 rows on those common
        //  columns.  Each output row takes TC column values from IS1 when IS1
        //  is non-zero/non-default, otherwise from IS2.  Data columns
        //  (amount, qty, alt_amount, alt_qty) always come from IS1.
        //  String comment columns use COALESCE(IS1, IS2).
        //
        //  DealScript equivalent:
        //    Output = ExecuteProcedure p_ds_detail_merge Using IS1, IS2
        // ════════════════════════════════════════════════════════════════════

        public static ResultSet DetailMerge(ResultSet is1, ResultSet is2)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2,
                "Start: DetailMerge, is1.rows={0}, is2.rows={1}", is1?.Rows ?? 0, is2?.Rows ?? 0);

            if (is1 == null || is1.Rows == 0) return EmptySet();
            if (is2 == null || is2.Rows == 0) return is1.Copy();

            // Detect which TC columns have at least one non-default value in each set.
            var activeIs1 = Dm_DetectActive(is1);
            var activeIs2 = Dm_DetectActive(is2);

            // Join columns: active in BOTH IS1 and IS2.
            var joinCols = new HashSet<int>(activeIs1);
            joinCols.IntersectWith(activeIs2);

            // Group IS2 rows by (deal_sid + join key).
            var is2Map = new Dictionary<string, List<CalcResultRow>>();
            is2.ForEachRow(r2 =>
            {
                string k = Dm_BuildKey(r2, joinCols);
                if (!is2Map.TryGetValue(k, out var list))
                    is2Map[k] = list = new List<CalcResultRow>();
                list.Add(r2);
            });

            // For each IS1 row, find matching IS2 rows and build merged output.
            var output = EmptySet();
            is1.ForEachRow(r1 =>
            {
                string k = Dm_BuildKey(r1, joinCols);
                if (!is2Map.TryGetValue(k, out var matches)) return;

                foreach (var r2 in matches)
                {
                    var outRow = r1.Clone();
                    // For TC columns that are default in IS1 but non-default in IS2: fill from IS2.
                    // This covers int/decimal/date columns.  Comments (indices 33-34) are handled
                    // identically: IsActive checks for null, so null IS1 comment → IS2 fills in.
                    for (int i = 0; i < Dm_Cols.Length; i++)
                    {
                        if (!Dm_Cols[i].IsActive(r1) && Dm_Cols[i].IsActive(r2))
                            Dm_Cols[i].CopyTo(r2, outRow);
                    }
                    // Data columns (amount, qty, alt_amount, alt_qty) always from IS1 (already in clone).
                    output.AddCalcResultRow(outRow);
                }
            });

            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1,
                "End: DetailMerge, result.rows={0}", output.Rows);
            return output;
        }

        // ════════════════════════════════════════════════════════════════════
        //  GroupNumbering  —  in-memory C# translation of p_ds_group_numbering
        //
        //  Assigns 0-based sequential numbers (written to Amount) to rows
        //  within groups.  IS2 defines the group level: TC columns that have
        //  at least one non-default value in IS2 form the group key.  An
        //  empty IS2 or a ZeroSet (all zeros) treats every deal_sid as its
        //  own single group.  The optional sort Comparison orders rows within
        //  each group; without it, insertion order is preserved.  All TC
        //  columns and all other data columns (qty, alt_amount, alt_qty) are
        //  passed through unchanged.
        //
        //  DealScript equivalent:
        //    Output = ExecuteProcedure p_ds_group_numbering Using IS1, IS2 [, IS3]
        // ════════════════════════════════════════════════════════════════════

        public static ResultSet GroupNumbering(
            ResultSet is1,
            ResultSet is2,
            Comparison<CalcResultRow> sort = null)
        {
            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 2,
                "Start: GroupNumbering, is1.rows={0}", is1?.Rows ?? 0);

            if (is1 == null || is1.Rows == 0) return EmptySet();

            // IS2 active columns define the group key.
            // Empty IS2 / ZeroSet → no active columns → one group per deal_sid.
            var groupCols = Dm_DetectActive(is2 ?? EmptySet());

            // Group IS1 rows by (deal_sid + group-key columns).
            var groups = new Dictionary<string, List<CalcResultRow>>();
            is1.ForEachRow(r1 =>
            {
                string k = Dm_BuildKey(r1, groupCols);
                if (!groups.TryGetValue(k, out var list))
                    groups[k] = list = new List<CalcResultRow>();
                list.Add(r1);
            });

            // Number rows within each group (0-based), writing to Amount.
            var output = EmptySet();
            foreach (var group in groups.Values)
            {
                IList<CalcResultRow> ordered = sort != null
                    ? (IList<CalcResultRow>)group
                        .OrderBy(r => r, Comparer<CalcResultRow>.Create(sort))
                        .ToList()
                    : group;

                for (int i = 0; i < ordered.Count; i++)
                {
                    var outRow = ordered[i].Clone();
                    outRow.Amount = i;
                    output.AddCalcResultRow(outRow);
                }
            }

            LogMsg.Debug(DebugCategory.DealScriptMethodCalls, 1,
                "End: GroupNumbering, result.rows={0}", output.Rows);
            return output;
        }

        // ── Shared TC column descriptors (p_ds_detail_merge / p_ds_group_numbering sort_order_nbr 2–36) ──
        //
        // Covers actual_period_sid through alt_user_comment.
        // period_sid and deal_sid are excluded (always taken from IS1 directly).
        // Int/decimal: active if != 0 and != C.InvalidInt/InvalidDecimal sentinel.
        // DateTime:    active if > 1900-01-01 (Dm_Epoch).
        // String:      active if != null.

        private struct DmColDesc
        {
            public Func<CalcResultRow, bool>            IsActive;
            public Func<CalcResultRow, string>          GetKeyPart;
            public Action<CalcResultRow, CalcResultRow> CopyTo;
        }

        private static readonly DmColDesc[] Dm_Cols    = BuildDmCols();
        private static readonly DateTime    Dm_Epoch   = new DateTime(1900, 1, 1);

        private static DmColDesc[] BuildDmCols()
        {
            DmColDesc IntCol(
                Func<CalcResultRow, int>   get,
                Action<CalcResultRow, int> set) => new DmColDesc
            {
                IsActive   = r => { int v = get(r); return v != 0 && v != C.InvalidInt; },
                GetKeyPart = r => get(r).ToString(),
                CopyTo     = (s, d) => set(d, get(s)),
            };

            DmColDesc DecCol(
                Func<CalcResultRow, decimal>   get,
                Action<CalcResultRow, decimal> set) => new DmColDesc
            {
                IsActive   = r => { decimal v = get(r); return v != 0m && v != C.InvalidDecimal; },
                GetKeyPart = r => get(r).ToString(),
                CopyTo     = (s, d) => set(d, get(s)),
            };

            DmColDesc DateCol(
                Func<CalcResultRow, DateTime>   get,
                Action<CalcResultRow, DateTime> set) => new DmColDesc
            {
                IsActive   = r => get(r) > Dm_Epoch,
                GetKeyPart = r => get(r).Ticks.ToString(),
                CopyTo     = (s, d) => set(d, get(s)),
            };

            DmColDesc StrCol(
                Func<CalcResultRow, string>   get,
                Action<CalcResultRow, string> set) => new DmColDesc
            {
                IsActive   = r => get(r) != null,
                GetKeyPart = r => get(r) ?? "",
                CopyTo     = (s, d) => set(d, get(s)),
            };

            return new DmColDesc[]
            {
                // [0]  actual_period_sid
                IntCol(r => r.Actual_period_sid,  (r, v) => r.Actual_period_sid  = v),
                // [1]  other_period_sid
                IntCol(r => r.Other_period_sid,   (r, v) => r.Other_period_sid   = v),
                // [2–21]  udkey_1_sid – udkey_20_sid
                IntCol(r => r.Udkey_1_sid,        (r, v) => r.Udkey_1_sid        = v),
                IntCol(r => r.Udkey_2_sid,        (r, v) => r.Udkey_2_sid        = v),
                IntCol(r => r.Udkey_3_sid,        (r, v) => r.Udkey_3_sid        = v),
                IntCol(r => r.Udkey_4_sid,        (r, v) => r.Udkey_4_sid        = v),
                IntCol(r => r.Udkey_5_sid,        (r, v) => r.Udkey_5_sid        = v),
                IntCol(r => r.Udkey_6_sid,        (r, v) => r.Udkey_6_sid        = v),
                IntCol(r => r.Udkey_7_sid,        (r, v) => r.Udkey_7_sid        = v),
                IntCol(r => r.Udkey_8_sid,        (r, v) => r.Udkey_8_sid        = v),
                IntCol(r => r.Udkey_9_sid,        (r, v) => r.Udkey_9_sid        = v),
                IntCol(r => r.Udkey_10_sid,       (r, v) => r.Udkey_10_sid       = v),
                IntCol(r => r.Udkey_11_sid,       (r, v) => r.Udkey_11_sid       = v),
                IntCol(r => r.Udkey_12_sid,       (r, v) => r.Udkey_12_sid       = v),
                IntCol(r => r.Udkey_13_sid,       (r, v) => r.Udkey_13_sid       = v),
                IntCol(r => r.Udkey_14_sid,       (r, v) => r.Udkey_14_sid       = v),
                IntCol(r => r.Udkey_15_sid,       (r, v) => r.Udkey_15_sid       = v),
                IntCol(r => r.Udkey_16_sid,       (r, v) => r.Udkey_16_sid       = v),
                IntCol(r => r.Udkey_17_sid,       (r, v) => r.Udkey_17_sid       = v),
                IntCol(r => r.Udkey_18_sid,       (r, v) => r.Udkey_18_sid       = v),
                IntCol(r => r.Udkey_19_sid,       (r, v) => r.Udkey_19_sid       = v),
                IntCol(r => r.Udkey_20_sid,       (r, v) => r.Udkey_20_sid       = v),
                // [22–25]  user_contact_sid – user_contact_4_sid
                IntCol(r => r.User_contact_sid,   (r, v) => r.User_contact_sid   = v),
                IntCol(r => r.User_contact_2_sid, (r, v) => r.User_contact_2_sid = v),
                IntCol(r => r.User_contact_3_sid, (r, v) => r.User_contact_3_sid = v),
                IntCol(r => r.User_contact_4_sid, (r, v) => r.User_contact_4_sid = v),
                // [26–28]  user rates
                DecCol(r => r.User_rate,          (r, v) => r.User_rate          = v),
                DecCol(r => r.User_2_rate,        (r, v) => r.User_2_rate        = v),
                DecCol(r => r.User_3_rate,        (r, v) => r.User_3_rate        = v),
                // [29–30]  price points
                DecCol(r => r.Price_point,        (r, v) => r.Price_point        = v),
                DecCol(r => r.Alt_price_point,    (r, v) => r.Alt_price_point    = v),
                // [31–32]  user dates
                DateCol(r => r.Start_user_date,   (r, v) => r.Start_user_date    = v),
                DateCol(r => r.End_user_date,     (r, v) => r.End_user_date      = v),
                // [33–34]  string comments — included for group-key detection / COALESCE fill-in
                StrCol(r => r.User_comment,       (r, v) => r.User_comment       = v),
                StrCol(r => r.Alt_user_comment,   (r, v) => r.Alt_user_comment   = v),
            };
        }

        // Returns the set of Dm_Cols indices for which at least one row in rs
        // has a non-default value.
        private static HashSet<int> Dm_DetectActive(ResultSet rs)
        {
            var active = new HashSet<int>();
            rs.ForEachRow(row =>
            {
                for (int i = 0; i < Dm_Cols.Length; i++)
                    if (!active.Contains(i) && Dm_Cols[i].IsActive(row))
                        active.Add(i);
            });
            return active;
        }

        // Builds a join-key string: deal_sid followed by one '\x01'-separated
        // segment per Dm_Cols entry.  Segments for columns not in cols are empty
        // (excluded from the key).
        private static string Dm_BuildKey(CalcResultRow row, HashSet<int> cols)
        {
            var sb = new StringBuilder();
            sb.Append(row.Deal_sid);
            for (int i = 0; i < Dm_Cols.Length; i++)
            {
                sb.Append('\x01');
                if (cols.Contains(i))
                    sb.Append(Dm_Cols[i].GetKeyPart(row));
            }
            return sb.ToString();
        }

    }
}
