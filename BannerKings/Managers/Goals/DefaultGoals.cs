using System.Collections.Generic;
using BannerKings.Managers.Goals.Decisions;
using BannerKings.Settings;

namespace BannerKings.Managers.Goals
{
    public class DefaultGoals : DefaultTypeInitializer<DefaultGoals, Goal>
    {
        public override IEnumerable<Goal> All
        {
            get
            {
                //yield return MakeCamp;
                yield return CallBannersGoal;
                yield return AssumeCulture;
                yield return LevyDuty;
                if (BannerKingsSettings.Instance.Feasts)
                {
                    yield return OrganizeFeastDecision;
                }
                 
                yield return AcquireBookDecision;
                yield return RecruitCompanionDecision;
                yield return RequestCouncil;
                yield return RequestPeerageDecision;
                yield return DemesneLawChangeDecision;
                yield return FoundKingdomGoal;
                yield return CalradicEmpireGoal;
                yield return RelocateCourtGoal;
                yield return SentenceCriminal;              
                foreach (Goal item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public Goal LevyDuty { get; } = new LevyDuty();
        internal Goal AssumeCulture { get; private set; }
        internal Goal CalradicEmpireGoal { get; private set; }
        internal Goal FoundKingdomGoal { get; private set; }
        internal Goal RecruitCompanionDecision { get; private set; }
        internal Goal RequestCouncil { get; private set; }
        internal Goal AcquireBookDecision { get; private set; }
        internal Goal DemesneLawChangeDecision { get; private set; }
        internal Goal RequestPeerageDecision { get; private set; }
        internal Goal OrganizeFeastDecision { get; private set; }
        internal Goal CallBannersGoal { get; private set; }
        public Goal RelocateCourtGoal { get; } = new MoveCourtDecision();
        public Goal SentenceCriminal { get; } = new SentenceCriminalDecision();
        public Goal MakeCamp { get; } = new MakeCamp();

        public override void Initialize()
        {
            AssumeCulture = new AssumeCultureDecision();
            CalradicEmpireGoal = new CalradicEmpireGoal();
            FoundKingdomGoal = new FoundKingdomGoal();
            RecruitCompanionDecision = new RecruitCompanionDecision();
            RequestCouncil = new RequestCouncilDecision();
            AcquireBookDecision = new AcquireBookDecision();
            DemesneLawChangeDecision = new DemesneLawChangeDecision();
            OrganizeFeastDecision = new OrganizeFeastDecision();
            RequestPeerageDecision = new RequestPeerageDecision();
            CallBannersGoal = new CallBannersGoal();
        }
    }
}