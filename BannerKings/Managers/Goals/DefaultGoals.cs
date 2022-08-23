using System.Collections.Generic;
using BannerKings.Managers.Goals.Decisions;

namespace BannerKings.Managers.Goals
{
    internal class DefaultGoals : DefaultTypeInitializer<DefaultGoals, Goal>
    {
        public override IEnumerable<Goal> All
        {
            get
            {
                yield return GreaterBattania;
                yield return CalradicEmpireGoal;
                yield return RecruitCompanionDecision;
            }
        }

        public Goal GreaterBattania { get; private set; }
        public Goal CalradicEmpireGoal { get; private set; }
        public Goal RecruitCompanionDecision { get; private set; }

        public override void Initialize()
        {
            GreaterBattania = new GreaterBattaniaGoal();
            CalradicEmpireGoal = new CalradicEmpireGoal();
            RecruitCompanionDecision = new RecruitCompanionDecision();
        }
    }
}