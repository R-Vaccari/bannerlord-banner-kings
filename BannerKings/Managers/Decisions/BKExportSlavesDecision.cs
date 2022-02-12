using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKExportSlavesDecision : BannerKingsDecision
    {
        public BKExportSlavesDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Slave caravans will be formed when the share of slave population is large enough").ToString();

        public override string GetIdentifier() => "decision_slaves_export";

        public override string GetName() => new TextObject("{=!}Allow slaves to be exported").ToString();
    }
}
