using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKExportSlavesDecision : BannerKingsDecision
    {
        public BKExportSlavesDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {
        }

        public override string GetHint()
        {
            return new TextObject("{=8tvTd5Bu}Slave caravans will be formed when the share of slave population is large enough").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_slaves_export";
        }

        public override string GetName()
        {
            return new TextObject("{=2Y5XjALc}Allow slaves to be exported").ToString();
        }
    }
}