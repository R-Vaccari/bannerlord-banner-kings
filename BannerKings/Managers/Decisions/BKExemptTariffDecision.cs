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
            return new TextObject("{=EyMxKdRrx}Exempt merchants from tariffs, reducing prices and attracting caravans").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_tariff_exempt";
        }

        public override string GetName()
        {
            return new TextObject("{=YvRT6fONa}Tariffs exemption").ToString();
        }
    }
}