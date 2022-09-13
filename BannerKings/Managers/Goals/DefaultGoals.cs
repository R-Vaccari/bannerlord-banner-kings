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
                yield return AssumeCulture;
                yield return RecruitCompanionDecision;
                yield return RequestCouncil;
                yield return FoundKingdomGoal;
                yield return CalradicEmpireGoal;
                yield return GreaterBattania;
            }
        }

        internal Goal AssumeCulture { get; private set; }
        internal Goal GreaterBattania { get; private set; }
        internal Goal CalradicEmpireGoal { get; private set; }
        internal Goal FoundKingdomGoal { get; private set; }
        internal Goal RecruitCompanionDecision { get; private set; }
        internal Goal RequestCouncil { get; private set; }

        public override void Initialize()
        {
            AssumeCulture = new AssumeCultureDecision();
            GreaterBattania = new GreaterBattaniaGoal();
            CalradicEmpireGoal = new CalradicEmpireGoal();
            FoundKingdomGoal = new FoundKingdomGoal();
            RecruitCompanionDecision = new RecruitCompanionDecision();
            RequestCouncil = new RequestCouncilDecision();
        }
    }
}