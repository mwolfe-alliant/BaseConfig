// BC1 BaseConfig -- Participations Accrual entry point.
// Source: bc1_master c_calc 1686 = C_MAGR_MAIN_PARTICIPATION_ACCRUAL.
// CTG entry-class: ParticipationsAccrual.  Template group: "Participations - Accrual".
//
// DealScript: `Output = #<c:1685>#`  -- Accrual is the same pipeline as Statement,
// stamped with the Accrual calc-context name on log/perf rows.
using System;
using System.Reflection;
using VelocityProto.Handles;

namespace VelocityProto
{
    public class ParticipationsAccrual
    {
        public static ResultSet Run() => new ParticipationsAccrual().Main();

        public static string CtgName =>
            (typeof(ParticipationsAccrual)
                .GetMethod(nameof(Main))
                ?.GetCustomAttribute<CalcTemplateGroupAttribute>()
                ?.Name)
            ?? throw new InvalidOperationException("CalcTemplateGroupAttribute missing from ParticipationsAccrual.Main");

        [CalcTemplateGroup("Participations - Accrual")]
        public ResultSet Main()
        {
            return MagrLib.RunParticipationsPipeline("C_MAGR_MAIN_PARTICIPATION_ACCRUAL");
        }
    }
}
