using BannerKings.UI.Items;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKSubsidizeMilitiaDecision : BannerKingsDecision
    {
        public BKSubsidizeMilitiaDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Improve militia quality by subsidizing their equipment and trainning").ToString();

        public override string GetIdentifier() => "decision_militia_subsidize";

        public override string GetName() => new TextObject("{=!}Subsidize the militia").ToString();
    }
}
