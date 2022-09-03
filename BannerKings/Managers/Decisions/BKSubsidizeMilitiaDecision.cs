using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKSubsidizeMilitiaDecision : BannerKingsDecision
    {
        public BKSubsidizeMilitiaDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {
        }

        public override string GetHint()
        { return new TextObject("{=4WBR3jvK}Improve militia quality by subsidizing their equipment and training").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_militia_subsidize";
        }

        public override string GetName()
        {
            return new TextObject("{=OyJ4NDWX}Subsidize the militia").ToString();
        }
    }
}