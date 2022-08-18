using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKRationDecision : BannerKingsDecision
    {
        public BKRationDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Food consumption reduced through enforced rationing. Decreases loyalty, with double effect if settlement is not besieged. Increases adm. costs.").ToString();

        public override string GetIdentifier() => "decision_ration";

        public override string GetName() => new TextObject("{=!}Enforce food rationing").ToString();
    }
}
