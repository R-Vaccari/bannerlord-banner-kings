using BannerKings.Managers.Court;
using BannerKings.Managers.Education;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models
{
    public class BKArmyManagementModel : DefaultArmyManagementCalculationModel
    {

        public bool CanCreateArmy(Hero armyLeader)
        {
            if (armyLeader != armyLeader.Clan.Leader) return false;

            Kingdom kingdom = armyLeader.Clan.Kingdom;
            if (kingdom != null && BannerKingsConfig.Instance.TitleManager != null)
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(armyLeader);
                if (title == null || title.type == TitleType.Lordship) return false;

                if (kingdom.ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
                {
                    CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(kingdom.RulingClan);
                    if (armyLeader != council.Marshall || title.type > TitleType.Dukedom) return false;
                }
            }

            return true;
        }


        public override float DailyBeingAtArmyInfluenceAward(MobileParty armyMemberParty)
        {
            float result = base.DailyBeingAtArmyInfluenceAward(armyMemberParty);
            if (armyMemberParty.MapFaction.IsKingdomFaction &&
                (armyMemberParty.MapFaction as Kingdom).ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
                result *= 1.5f;

            if (armyMemberParty.LeaderHero != null)
            {
                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(armyMemberParty.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.MercenaryFamousSellswords))
                    result *= 1.3f;
            }

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
