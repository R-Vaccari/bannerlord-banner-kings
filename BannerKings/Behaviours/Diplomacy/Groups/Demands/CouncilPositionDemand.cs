using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class CouncilPositionDemand : Demand
    {
        private CouncilPosition position;
        private Hero benefactor;

        public CouncilPositionDemand(string stringId) : base(stringId)
        {
            Initialize(new TextObject("{=!}Council Position"),
                new TextObject());
        }

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return new DemandResponse(new TextObject(),
                    new TextObject(),
                    6,
                    250,
                    1000,
                    (Hero fulfiller) =>
                    {
                        return true;
                    },
                    (Hero benefactor) =>
                    {
                        return 1f;
                    },
                    (Hero fulfiller) =>
                    {
                    });
            }
        }

        public override Demand GetCopy(InterestGroup group)
        {
            CouncilPositionDemand demand = new CouncilPositionDemand(StringId);
            demand.Group = group;
            return demand;
        }

        public override (bool, TextObject) IsDemandCurrentlyAdequate()
        {
            throw new NotImplementedException();
        }

        public override void ShowPlayerDemandAnswers()
        {
            throw new NotImplementedException();
        }

        public override void ShowPlayerDemandOptions()
        {
            throw new NotImplementedException();
        }
    }
}
