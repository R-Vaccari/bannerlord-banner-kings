using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKArmyManagementModel : DefaultArmyManagementCalculationModel
    {
        public bool CanCreateArmy(Hero armyLeader)
        {
            var kingdom = armyLeader.Clan.Kingdom;
            if (kingdom != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(armyLeader);
                if (title != null)
                {
                    if (kingdom.ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
                    {
                        var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(kingdom.RulingClan);
                        if (title.type <= TitleType.Dukedom || armyLeader == council.Marshall)
                        {
                            return true;
                        }
                    }
                    else if (title.type < TitleType.Lordship)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public override float DailyBeingAtArmyInfluenceAward(MobileParty armyMemberParty)
        {
            var result = base.DailyBeingAtArmyInfluenceAward(armyMemberParty);
            if (armyMemberParty.MapFaction.IsKingdomFaction &&
                (armyMemberParty.MapFaction as Kingdom).ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
            {
                result *= 1.5f;
            }

            if (armyMemberParty.LeaderHero != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(armyMemberParty.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.MercenaryFamousSellswords))
                {
                    result *= 1.3f;
                }

                if(education.HasPerk(BKPerks.Instance.KheshigHonorGuard))
                {
                    result *= 1.3f;
                }
            }

            return result;
        }

        public override int CalculatePartyInfluenceCost(MobileParty armyLeaderParty, MobileParty party)
        {
            if (party.LeaderHero == null || armyLeaderParty.LeaderHero == null)
            {
                return base.AverageCallToArmyCost;
            }


            float result = base.CalculatePartyInfluenceCost(armyLeaderParty, party);
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                var leader = armyLeaderParty.LeaderHero;
                var summonedLeader = party.LeaderHero;
                if (leader != null && summonedLeader != null)
                {
                    var leaderTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);
                    var summonedLeaderTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(summonedLeader);

                    if (leaderTitle != null && summonedLeaderTitle != null)
                    {
                        var rank = leaderTitle.type;
                        var summonedRank = summonedLeaderTitle.type;

                        var factor = 1f + 0.25f * (rank - summonedRank);
                        result *= factor;
                    }
                }
            }

            return (int) result;
        }
    }
}