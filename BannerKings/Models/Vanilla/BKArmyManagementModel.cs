using BannerKings.Behaviours;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class BKArmyManagementModel : DefaultArmyManagementCalculationModel
    {
        public override int CalculatePartyInfluenceCost(MobileParty armyLeaderParty, MobileParty party)
        {
            float result = base.CalculatePartyInfluenceCost(armyLeaderParty, party);
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                Hero leader = armyLeaderParty.LeaderHero;
                Hero summonedLeader = party.LeaderHero;
                if (leader != null && summonedLeader != null)
                {
                    if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(leader) && 
                        BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(summonedLeader))
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
            }
            return (int)result;
        }
    }
}
