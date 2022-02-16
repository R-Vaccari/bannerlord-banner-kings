using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKBanForeignersDecision : BannerKingsDecision
    {
        public BKBanForeignersDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Foreigners that refuse to assimilate will be gradually forced to leave the settlement").ToString();

        public override string GetIdentifier() => "decision_foreigner_ban";

        public override string GetName() => new TextObject("{=!}Ban foreigners").ToString();
    }
}
