using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Extensions
{
    public static class MobilePartyExtensions
    {
        public static bool IsAvailableForArmies(this MobileParty mobileParty)
        {
            Hero leaderHero = mobileParty.LeaderHero;
            if (mobileParty.IsLordParty &&
                mobileParty.Army == null &&
                leaderHero != null &&
                !mobileParty.IsMainParty &&
                leaderHero != leaderHero.MapFaction.Leader &&
                !mobileParty.Ai.DoNotMakeNewDecisions)
            {
                Settlement currentSettlement = mobileParty.CurrentSettlement;
                if (((currentSettlement != null) ? currentSettlement.SiegeEvent : null) == null &&
                    !mobileParty.IsDisbanding && mobileParty.Food > -(mobileParty.FoodChange * 5f) &&
                    mobileParty.PartySizeRatio > 0.6f && leaderHero.CanLeadParty() &&
                    mobileParty.MapEvent == null &&
                    mobileParty.BesiegedSettlement == null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
