using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models
{
    public class BKArmyManagementModel : DefaultArmyManagementCalculationModel
    {
        public override float DailyBeingAtArmyInfluenceAward(MobileParty armyMemberParty)
        {
            float result = base.DailyBeingAtArmyInfluenceAward(armyMemberParty);
            if (armyMemberParty.MapFaction.IsKingdomFaction &&
                (armyMemberParty.MapFaction as Kingdom).ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
                result *= 1.5f;

            return result;
        }

        public override int CalculatePartyInfluenceCost(MobileParty armyLeaderParty, MobileParty party)
        {
            float result = base.CalculatePartyInfluenceCost(armyLeaderParty, party);
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                Hero leader = armyLeaderParty.LeaderHero;
                Hero summonedLeader = party.LeaderHero;
                if (leader != null && summonedLeader != null)
                {
                    FeudalTitle leaderTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);
                    FeudalTitle summonedLeaderTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(summonedLeader);

                    if (leaderTitle != null && summonedLeaderTitle != null)
                    {
                        TitleType rank = leaderTitle.type;
                        TitleType summonedRank = summonedLeaderTitle.type;

                        float factor = 1f + (0.25f * (rank - summonedRank));
                        result *= factor;
                    }
                }
            }
            return (int)result;
        }
    }
}
