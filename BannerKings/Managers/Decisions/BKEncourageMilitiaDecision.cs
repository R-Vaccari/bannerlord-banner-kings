using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKEncourageMilitiaDecision : BannerKingsDecision
    {
        public BKEncourageMilitiaDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Encourage able-bodied men to join the active militia force").ToString();

        public override string GetIdentifier() => "decision_militia_encourage";

        public override string GetName() => new TextObject("{=!}Encourage militia").ToString();
    }
}
