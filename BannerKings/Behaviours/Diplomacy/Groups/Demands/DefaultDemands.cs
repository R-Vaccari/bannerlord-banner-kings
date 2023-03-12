using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class DefaultDemands : DefaultTypeInitializer<DefaultDemands, Demand>
    {
        public Demand CouncilPosition { get; } = new CouncilPositionDemand();
        /*public Demand PolicyChange { get; } = new Demand("policy_change");
        public Demand LawChange { get; } = new Demand("law_change");
        public Demand CeaseWar { get; } = new Demand("cease_war");
        public Demand DeclareWar { get; } = new Demand("declare_war");*/
        public override IEnumerable<Demand> All
        {
            get
            {
                yield return CouncilPosition;
            }
        }

        public override void Initialize()
        {
        }
    }
}
