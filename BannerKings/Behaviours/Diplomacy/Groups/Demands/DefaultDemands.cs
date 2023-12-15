using System.Collections.Generic;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class DefaultDemands : DefaultTypeInitializer<DefaultDemands, Demand>
    {
        public Demand CouncilPosition { get; } = new CouncilPositionDemand();
        public Demand PolicyChange { get; } = new PolicyChangeDemand();
        public Demand LawChange { get; } = new DemesneLawChangeDemand();
        public Demand AssumeFaith { get; } = new AssumeFaithDemand();
        public Demand DemandTitle { get; } = new TitleDemand();
        /*public Demand PolicyChange { get; } = new Demand("policy_change");
        = new Demand("law_change");
        public Demand CeaseWar { get; } = new Demand("cease_war");
        public Demand DeclareWar { get; } = new Demand("declare_war");*/
        public override IEnumerable<Demand> All
        {
            get
            {
                yield return CouncilPosition;
                yield return PolicyChange;
                yield return LawChange;
                yield return AssumeFaith;
                foreach (Demand demand in ModAdditions)
                {
                    yield return demand;
                }
            }
        }

        public override void Initialize()
        {
        }
    }
}
