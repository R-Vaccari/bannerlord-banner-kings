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
                yield return FoundKingdomGoal;
                yield return RecruitCompanionDecision;
            }
        }

        internal Goal GreaterBattania { get; private set; }
        internal Goal CalradicEmpireGoal { get; private set; }
        internal Goal FoundKingdomGoal { get; private set; }
        internal Goal RecruitCompanionDecision { get; private set; }

        public override void Initialize()
        {
            GreaterBattania = new GreaterBattaniaGoal();
            CalradicEmpireGoal = new CalradicEmpireGoal();
            FoundKingdomGoal = new FoundKingdomGoal();
            RecruitCompanionDecision = new RecruitCompanionDecision();
        }
    }
}