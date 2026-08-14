// BC1 BaseConfig -- Participations Statement entry point.
// Source: bc1_master c_calc 1685 = C_MAGR_MAIN_PARTICIPATION_STATEMENT.
// CTG entry-class: ParticipationsStatement.  Template group: "Participations - Statement".
//
// The MAGR pipeline body lives in MagrLib.RunParticipationsPipeline so the Accrual
// entry point can reuse it (DealScript c:1686 Accrual = `Output = #<c:1685>#`).
using System;
using System.Reflection;
using Velocity.Handles;

namespace Velocity
{
    public class ParticipationsStatement
    {
        public static ResultSet Run() => new ParticipationsStatement().Main();

        public static string CtgName =>
            (typeof(ParticipationsStatement)
                .GetMethod(nameof(Main))
                ?.GetCustomAttribute<CalcTemplateGroupAttribute>()
                ?.Name)
            ?? throw new InvalidOperationException("CalcTemplateGroupAttribute missing from ParticipationsStatement.Main");

        [CalcTemplateGroup("Participations - Statement")]
        public ResultSet Main()
        {
            return MagrLib.RunParticipationsPipeline("C_MAGR_MAIN_PARTICIPATION_STATEMENT");
        }
    }
}
