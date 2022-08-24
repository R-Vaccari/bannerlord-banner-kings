using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKTaxSlavesDecision : BannerKingsDecision
    {
        public BKTaxSlavesDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {
        }

        public override string GetHint()
        {
            return new TextObject("{=JgxOrZij9}Privately owned slaves' work will be taxed by the state, according to the tax policy, generating extra revenue. Slave owners will be dissatisfied.").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_slaves_tax";
        }

        public override string GetName()
        {
            return new TextObject("{=1qfXA46S6}Tax private slaves").ToString();
        }
    }
}