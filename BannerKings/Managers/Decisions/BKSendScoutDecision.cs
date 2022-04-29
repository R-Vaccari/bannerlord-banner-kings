﻿using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKSendScoutDecision : BannerKingsDecision
    {
        public BKSendScoutDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Send out a handful of horsemen to scout a large area around the settlement and report enemy movement activities").ToString();

        public override string GetIdentifier() => "decision_scout_send";

        public override string GetName() => new TextObject("{=!}Send out scouts").ToString();
    }
}
