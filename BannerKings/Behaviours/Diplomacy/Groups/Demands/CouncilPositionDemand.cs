using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class CouncilPositionDemand : Demand
    {
        public CouncilPositionDemand(string stringId) : base(stringId)
        {
            Initialize(new TextObject("{=!}Council Position"),
                new TextObject());
        }

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return new DemandResponse();
            }
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
