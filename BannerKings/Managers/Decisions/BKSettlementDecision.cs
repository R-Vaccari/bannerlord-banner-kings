using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Managers.Decisions
{
    public abstract class BKSettlementDecision : BKDecision<Settlement>
    {
        protected BKSettlementDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }
    }
}