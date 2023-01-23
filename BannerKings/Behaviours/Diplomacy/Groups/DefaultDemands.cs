using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class DefaultDemands : DefaultTypeInitializer<DefaultDemands, Demand>
    {
        public Demand CouncilPosition { get; } = new Demand("council_position");
        public override IEnumerable<Demand> All => throw new NotImplementedException();

        public override void Initialize()
        {
            CouncilPosition.Initialize(new TextObject("{=!}Council Position"),
                new TextObject("{=!}"),
                true,
                (InterestGroup group) =>
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
