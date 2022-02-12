using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKEncourageDraftingDecision : BannerKingsDecision
    {
        public BKEncourageDraftingDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Encourage able-bodied men to be drafted and be available to serve as soldiers").ToString();

        public override string GetIdentifier() => "decision_drafting_encourage";

        public override string GetName() => new TextObject("{=!}Encourage drafting").ToString();
    }
}
