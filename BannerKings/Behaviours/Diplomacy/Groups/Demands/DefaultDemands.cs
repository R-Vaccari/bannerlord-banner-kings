using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class DefaultDemands : DefaultTypeInitializer<DefaultDemands, Demand>
    {
        public Demand CouncilPosition { get; } = new Demand("council_position");
        public Demand PolicyChange { get; } = new Demand("policy_change");
        public Demand LawChange { get; } = new Demand("law_change");
        public Demand CeaseWar { get; } = new Demand("cease_war");
        public Demand DeclareWar { get; } = new Demand("declare_war");
        public override IEnumerable<Demand> All => throw new NotImplementedException();

        public override void Initialize()
        {
            CouncilPosition.Initialize(new TextObject("{=!}Demand Council Position"),
                new TextObject("{=!}"),
                true,
                (group) =>
                {
                    return true;
                },
                new List<Demand.DemandResponse>()
                {
                    new Demand.DemandResponse(new TextObject(),
                    new TextObject(),
                    0,
                    (Hero fulfiller) =>
                    {
                        return true;
                    },
                    (InterestGroup group, Hero fulfiller) =>
                    {

                    })
                });

            LawChange.Initialize(new TextObject("{=!}Demand Law Change"),
                new TextObject("{=!}"),
                true,
                (group) =>
                {
                    return true;
                },
                new List<Demand.DemandResponse>()
                {
                    new Demand.DemandResponse(new TextObject(),
                    new TextObject(),
                    0,
                    (Hero fulfiller) =>
                    {
                        return true;
                    },
                    (InterestGroup group, Hero fulfiller) =>
                    {

                    })
                });

            PolicyChange.Initialize(new TextObject("{=!}Demand Policy Change"),
                new TextObject("{=!}"),
                true,
                (group) =>
                {
                    return true;
                },
                new List<Demand.DemandResponse>()
                {
                    new Demand.DemandResponse(new TextObject(),
                    new TextObject(),
                    0,
                    (Hero fulfiller) =>
                    {
                        return true;
                    },
                    (InterestGroup group, Hero fulfiller) =>
                    {

                    })
                });
        }
    }
}
