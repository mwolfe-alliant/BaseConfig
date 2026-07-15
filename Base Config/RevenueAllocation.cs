// BC1 BaseConfig — Royalty Revenue Allocation entry point.
//
// DealScript origin: c_calc 1545 C_ROY_MAIN_REVENUE_ALLOCATION (bc1_master).
//
// Purpose: explode imported "bundle" trx (1 row representing N components) into
// per-component rows using the bundle's BOM (bill of materials).  Two methods:
//
//   1. Percentage Based  (ContractUDF.ExplosionMethod = "Percentage Based")
//      Each BOM line carries a percent (0..1).  Output row Amount = bundle Amount
//      * percent.  Output Units = bundle Units (each component gets the full
//      bundle quantity — they're delivered together).
//
//   2. Units Based
//      Each BOM line carries a unit count.  Output row Amount = bundle Amount
//      pro-rated by the line's share of total BOM units.  Output Units = bundle
//      Units * BOM line units.
//
// Plus a window-period filter, a prior-period passthrough, and a "could not
// explode" error path that surfaces invalid BOM-method/import combinations as
// UnspecifiedAllocationITDOutput rows.
using System;
using System.Reflection;
using VelocityProto.Handles;

namespace VelocityProto
{
    public class RevenueAllocation
    {
        private static CalcContext _ctxMainRevenueAllocation;

        public static ResultSet Run() => new RevenueAllocation().Main();

        public static string CtgName =>
            (typeof(RevenueAllocation)
                .GetMethod(nameof(Main))
                ?.GetCustomAttribute<CalcTemplateGroupAttribute>()
                ?.Name)
            ?? throw new InvalidOperationException("CalcTemplateGroupAttribute missing from RevenueAllocation.Main");

        [CalcTemplateGroup("Royalty - Revenue Allocation")]
        public ResultSet Main()
        {
            _ctxMainRevenueAllocation = _ctxMainRevenueAllocation ?? new CalcContext("C_ROY_MAIN_REVENUE_ALLOCATION");
            Job.CurrentCalcContext = _ctxMainRevenueAllocation;
            LogMsg.Debug(DebugCategory.ContractModelProgress, 2, "Start:  RevenueAllocation");

            // ─── Phase A: Explosion method gating ────────────────────────────
            string explosionMethod = Contract.GetUDFString(ContractUDF.ExplosionMethod);
            int N_PctBased = (explosionMethod == "Percentage Based") ? 1 : 0;

            // ─── Phase B: Window-start period ────────────────────────────────
            int N_WindowPeriodsRaw = Contract.GetUDFInt(ContractUDF.WindowPeriods);
            int N_WindowPeriods    = N_WindowPeriodsRaw * -1;
            string statementInterval = Contract.GetUDFString(ContractUDF.StatementInterval);
            PeriodItem windowStart = (N_WindowPeriodsRaw == 0 || string.IsNullOrEmpty(statementInterval))
                ? DS.F_INCEPTION()
                : DS.F_PERIOD_TYPES_FROM(Job.CurrentCalcPeriod, N_WindowPeriods, statementInterval);

            var auditWindow = DS.F_PERIOD_INTERVAL(windowStart, (PeriodItem)PeriodItem.Items.GetEntityByDescr(Period.End_of_Time));

            // ─── Phase C: Prior-window ITD passthrough ───────────────────────
            // GetData CalcResult for ActivityType != Unspecified_Allocation,
            //                       ActualPeriod NOT IN auditWindow,
            //                       TransType = ITD,
            //                       Period   = F_CALC_PERIOD_PREVIOUS().
            var priorWindowItd = ResultSet.GetDataFromCalcResult(new FilterClause {
                new Criteria(CustCol.ActivityType, CompareOp.NE,    new AlliantEntity(0)),   // ≠ Unspecified_Allocation (sid 1170)
                new Criteria(CustCol.ActualPeriod, ListOp.NOTINLIST, (EntityList)auditWindow),
                new Criteria(CustCol.TransType,    CompareOp.EQ,    TransType.ITD),
                new Criteria(EngineCol.Period,     CompareOp.EQ,    Job.CalcPeriodPrevious),
            });

            // ─── Phase D: Imported window trx ────────────────────────────────
            var importWindowPeriod = Job.Import.GetData(new FilterClause {
                new Criteria(CustCol.ActivityType, ListOp.INLIST, ActivityType.Lists.Imported_Activity_Type_List),
                new Criteria(CustCol.ActualPeriod, ListOp.INLIST, (EntityList)auditWindow),
                new Criteria(CustCol.Bundle,       CompareOp.NE,  Bundle.Unspecified),
                new Criteria(EngineCol.Period,     ListOp.INLIST, Job.ITDthruCalcPeriod),
            }, "RevAlloc.ImportWindowPeriod");

            var importedTrx = importWindowPeriod;
            importedTrx.SetValue(CustCol.TransType, TransType.ITD);
            importedTrx.DoMath(EngineCol.Rate1,    MathOp.SETTO, "0");
            importedTrx.DoMath(BaseCol.Amount2,    MathOp.SETTO, "0");
            importedTrx.DoMath(BaseCol.Units2,     MathOp.SETTO, "0");

            // SummarizeImport via 1546 (RoyaltyLib.SummarizeToActualPeriodBundle); zero Amount/Units.
            var summarizeImport = RoyaltyLib.SummarizeToActualPeriodBundle(importedTrx.Copy());
            Job.CurrentCalcContext = _ctxMainRevenueAllocation;
            summarizeImport.DoMath(BaseCol.Amount, MathOp.SETTO, "0");
            summarizeImport.DoMath(BaseCol.Units,  MathOp.SETTO, "0");

            // ─── Phase E: Bundle explosion via p_ds_detail_explosion_from_bom ─
            var explodeByCatalogPercentAndUnits = ResultSet.ZeroSet();
            explodeByCatalogPercentAndUnits.DoMath(EngineCol.Comment,    MathOp.SETTO, "UDKey8");   // Bundle
            explodeByCatalogPercentAndUnits.DoMath(EngineCol.AltComment, MathOp.SETTO, "UDKey1");   // Catalog
            explodeByCatalogPercentAndUnits.DoMath(EngineCol.Rate1,      MathOp.SETTO, "3");        // mode 3
            explodeByCatalogPercentAndUnits.DoMath(EngineCol.Rate2,      MathOp.SETTO, "1");        // unchanged-units flag

            var explodedBom = DS.ExecuteDSP("p_ds_detail_explosion_from_bom",
                summarizeImport, explodeByCatalogPercentAndUnits);
            summarizeImport.Release();
            explodeByCatalogPercentAndUnits.Release();

            // ─── Phase F: Invalid-method detection ───────────────────────────
            var invalidMethod = explodedBom.GetData(BaseCol.Units2, CompareOp.NE, "1", "RevAlloc.InvalidMethod");
            invalidMethod.DoMath(EngineCol.Rate1,  MathOp.SETTO, "99");
            invalidMethod.DoMath(BaseCol.Units2,   MathOp.SETTO, "0");
            var invalidMerge = DS.ExecuteDSP("p_ds_detail_merge", importedTrx, invalidMethod);
            invalidMethod.Release();
            var invalidImportMethod = invalidMerge.GetData(EngineCol.Rate1, CompareOp.EQ, "99", "RevAlloc.InvalidImportMethod");
            invalidMerge.Release();
            invalidImportMethod.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

            // Move ActivityType ID into Comment2 via p_ds_udkey_to_text.
            var activityTypeIdToComment2 = ResultSet.ZeroSet();
            activityTypeIdToComment2.DoMath(EngineCol.Comment, MathOp.SETTO, "UDkey2");
            activityTypeIdToComment2.DoMath(EngineCol.Rate1,   MathOp.SETTO, "1");   // 1 = move to Comment2
            activityTypeIdToComment2.DoMath(EngineCol.Rate2,   MathOp.SETTO, "1");   // 1 = use ID
            DS.ExecuteDSP("p_ds_udkey_to_text", invalidImportMethod, activityTypeIdToComment2).Release();
            activityTypeIdToComment2.Release();

            var unspecifiedAllocationItdOutput = invalidImportMethod;
            unspecifiedAllocationItdOutput.SetValue(CustCol.ActivityType, new AlliantEntity(0));   // 1170 = Unspecified Allocation entity-instance

            // ─── Phase G: Percent-based explosion ────────────────────────────
            // (Active branch when N_PctBased == 1.)
            ResultSet pctRevenueOutput;
            if (N_PctBased == 1)
            {
                var percentMethod = explodedBom.GetData(BaseCol.Units2, CompareOp.EQ, "1", "RevAlloc.PercentMethod");

                // Save a copy that ignores Catalog: used as a marker set to find which
                // imports need proration (without splitting by component yet).
                var savePercentMethod = percentMethod.Copy();
                savePercentMethod.SetValue(CustCol.Catalog, new AlliantEntity(0));
                savePercentMethod.DoMath(EngineCol.Rate1, MathOp.SETTO, "99");

                // PercentMethod row: stash the percent in Rate3, zero Amount/Units/Units2.
                percentMethod.DoMath(EngineCol.Rate1, MathOp.SETTO, "99");
                percentMethod.DoMath(EngineCol.Rate3, MathOp.SETTO, BaseCol.Amount);
                percentMethod.DoMath(BaseCol.Units,   MathOp.SETTO, "0");
                percentMethod.DoMath(BaseCol.Amount,  MathOp.SETTO, "0");
                percentMethod.DoMath(BaseCol.Units2,  MathOp.SETTO, "0");

                var findImportForPercentProration = DS.ExecuteDSP("p_ds_detail_merge", importedTrx, savePercentMethod);
                var toBeUsedForPercentProration = findImportForPercentProration.GetData(EngineCol.Rate1, CompareOp.EQ, "99", "RevAlloc.ToBeUsedForPercentProration");
                findImportForPercentProration.Release();
                toBeUsedForPercentProration.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

                var validMergeWithPercent = DS.ExecuteDSP("p_ds_detail_merge", importedTrx, percentMethod);
                var validBundlesPercent = validMergeWithPercent.GetData(EngineCol.Rate1, CompareOp.EQ, "99", "RevAlloc.ValidBundlesPercent");
                validMergeWithPercent.Release();
                validBundlesPercent.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

                var saveValidBundlesPercent = validBundlesPercent.Copy();
                saveValidBundlesPercent.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

                // ExplodedPercentAmount = Amount * Rate3.
                var explodedPercentAmount = validBundlesPercent;
                explodedPercentAmount.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate3);

                var roundedExplodedPercentAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(explodedPercentAmount);
                Job.CurrentCalcContext = _ctxMainRevenueAllocation;

                var proratedPct = ResultSetProrate.Prorate12(
                    toBeUsedForPercentProration, roundedExplodedPercentAmount,
                    mode: 0, roundingCorrMode: 0,
                    amountScale: 2, qtyScale: 0, altAmountScale: 2, altQtyScale: 0);
                toBeUsedForPercentProration.Release();
                roundedExplodedPercentAmount.Release();

                pctRevenueOutput = ResultSet.EmptySet();
                pctRevenueOutput.CombineAndRelease(proratedPct, saveValidBundlesPercent);
                var pctRevenueOutputSumm = pctRevenueOutput.Summarize();
                if (!ReferenceEquals(pctRevenueOutputSumm, pctRevenueOutput)) pctRevenueOutput.Release();
                Job.CurrentCalcContext = _ctxMainRevenueAllocation;
                pctRevenueOutput = pctRevenueOutputSumm;

                savePercentMethod.Release();
                percentMethod.Release();
            }
            else
            {
                pctRevenueOutput = ResultSet.EmptySet();
            }

            // ─── Phase H: Units-based explosion ──────────────────────────────
            ResultSet unitsOutput;
            if (N_PctBased == 0)
            {
                var unitMethod = explodedBom.GetData(BaseCol.Units2, CompareOp.EQ, "1", "RevAlloc.UnitMethod");

                var saveUnitsMethod = unitMethod.Copy();
                saveUnitsMethod.SetValue(CustCol.Catalog, new AlliantEntity(0));
                saveUnitsMethod.DoMath(EngineCol.Rate1, MathOp.SETTO, "99");

                unitMethod.DoMath(BaseCol.Units2,    MathOp.SETTO, "0");
                unitMethod.DoMath(EngineCol.Rate2,   MathOp.SETTO, BaseCol.Units);
                unitMethod.DoMath(BaseCol.Units,     MathOp.SETTO, "0");
                unitMethod.DoMath(BaseCol.Amount,    MathOp.SETTO, "0");
                unitMethod.DoMath(EngineCol.Rate1,   MathOp.SETTO, "99");

                var findImportForUnitsProration = DS.ExecuteDSP("p_ds_detail_merge", importedTrx, saveUnitsMethod);
                var toBeUsedForUnitsProration = findImportForUnitsProration.GetData(EngineCol.Rate1, CompareOp.EQ, "99", "RevAlloc.ToBeUsedForUnitsProration");
                findImportForUnitsProration.Release();
                toBeUsedForUnitsProration.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");
                toBeUsedForUnitsProration.DoMath(BaseCol.Units,   MathOp.SETTO, "0");

                // Total units per (ActualPeriod, Bundle) for ratio denominator.
                var summarizeUnitMethod = RoyaltyLib.SummarizeToActualPeriodBundle(saveUnitsMethod.Copy());
                Job.CurrentCalcContext = _ctxMainRevenueAllocation;
                var nonZeroTotalUnits = summarizeUnitMethod.GetData(BaseCol.Units, CompareOp.NE, "0", "RevAlloc.NonZeroTotalUnits");
                summarizeUnitMethod.Release();

                // unitMethod.Rate3 = Rate2 / lookup(NonZeroTotalUnits.Units by ActualPeriod, Bundle).
                unitMethod.DoMath(EngineCol.Rate3, MathOp.SETTO, EngineCol.Rate2);
                unitMethod.Lookup(EngineCol.Rate3, MathOp.DIVIDEDBY, BaseCol.Units,
                                  nonZeroTotalUnits,
                                  CustCol.ActualPeriod, CustCol.Bundle);
                nonZeroTotalUnits.Release();

                var validMergeWithUnits = DS.ExecuteDSP("p_ds_detail_merge", importedTrx, unitMethod);
                var validBundlesUnits = validMergeWithUnits.GetData(EngineCol.Rate1, CompareOp.EQ, "99", "RevAlloc.ValidBundlesUnits");
                validMergeWithUnits.Release();
                validBundlesUnits.DoMath(EngineCol.Rate1, MathOp.SETTO, "0");

                var saveValidBundlesUnits = validBundlesUnits.Copy();
                saveValidBundlesUnits.DoMath(BaseCol.Amount, MathOp.SETTO, "0");

                var explodedUnitsAmount = validBundlesUnits;
                explodedUnitsAmount.DoMath(BaseCol.Amount, MathOp.TIMES, EngineCol.Rate3);

                var roundedExplodedUnitsAmount = CommonLib.RoundAmountsTo2DecAndUnitsTo0Dec(explodedUnitsAmount);
                Job.CurrentCalcContext = _ctxMainRevenueAllocation;

                var proratedAmountForUnitsMethod = ResultSetProrate.Prorate12(
                    toBeUsedForUnitsProration, roundedExplodedUnitsAmount,
                    mode: 0, roundingCorrMode: 0,
                    amountScale: 2, qtyScale: 0, altAmountScale: 2, altQtyScale: 0);
                toBeUsedForUnitsProration.Release();
                roundedExplodedUnitsAmount.Release();

                var unitsForUnits = saveValidBundlesUnits;
                unitsForUnits.DoMath(BaseCol.Units, MathOp.TIMES, EngineCol.Rate2);

                unitsOutput = ResultSet.EmptySet();
                unitsOutput.CombineAndRelease(proratedAmountForUnitsMethod, unitsForUnits);
                var unitsOutputSumm = unitsOutput.Summarize();
                if (!ReferenceEquals(unitsOutputSumm, unitsOutput)) unitsOutput.Release();
                Job.CurrentCalcContext = _ctxMainRevenueAllocation;
                unitsOutput = unitsOutputSumm;

                saveUnitsMethod.Release();
                unitMethod.Release();
            }
            else
            {
                unitsOutput = ResultSet.EmptySet();
            }

            explodedBom.Release();
            importedTrx.Release();

            // ─── Final assembly ──────────────────────────────────────────────
            var output = ResultSet.EmptySet();
            output.CombineAndRelease(priorWindowItd, unspecifiedAllocationItdOutput, pctRevenueOutput, unitsOutput);

            LogMsg.Debug(DebugCategory.ContractModelProgress, 1, "End:  RevenueAllocation");
            return output;
        }
    }
}
