using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using static Populations.Managers.TitleManager;

namespace Populations.Models
{
    class FeudalArmyManagementModel : DefaultArmyManagementCalculationModel
    {
        public override int CalculatePartyInfluenceCost(MobileParty armyLeaderParty, MobileParty party)
        {
            float result = base.CalculatePartyInfluenceCost(armyLeaderParty, party);
            if (PopulationConfig.Instance.TitleManager != null)
            {
                Hero leader = armyLeaderParty.LeaderHero;
                Hero summonedLeader = party.LeaderHero;
                if (leader != null && summonedLeader != null)
                {
                    if (PopulationConfig.Instance.TitleManager.IsHeroTitleHolder(leader) && 
                        PopulationConfig.Instance.TitleManager.IsHeroTitleHolder(summonedLeader))
                    {
                        FeudalTitle leaderTitle = PopulationConfig.Instance.TitleManager.GetHighestTitle(leader);
                        TitleType rank = leaderTitle.type;

                        FeudalTitle summonedLeaderTitle = PopulationConfig.Instance.TitleManager.GetHighestTitle(summonedLeader);
                        TitleType summonedRank = summonedLeaderTitle.type;

                        float factor = 1f + (0.3f * (rank - summonedRank));
                        result *= factor;
                    }
                }
            }
            return (int)result;
        }
    }
}
