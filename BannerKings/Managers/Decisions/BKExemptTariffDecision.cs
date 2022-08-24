using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKExemptTariffDecision : BannerKingsDecision
    {
        public BKExemptTariffDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {
        }

        public override string GetHint()
        {
            return new TextObject("{=1uB3RyX0w}Exempt merchants from tariffs, reducing prices and attracting caravans").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_tariff_exempt";
        }

        public override string GetName()
        {
            return new TextObject("{=Lss6NV8vQ}Tariffs exemption").ToString();
        }
    }
}