using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Education;
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
                if (kingdom.Leader == armyLeader)
                {
                    return true;
                }

                var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(kingdom.RulingClan);
                CouncilMember position = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Marshal);
                if (position != null && armyLeader == position.Member)
                {
                    return true;
                }

                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(armyLeader);
                if (title != null)
                {
                    if (kingdom.ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
                    {
                        if (title.TitleType <= TitleType.Dukedom)
                        {
                            return true;
                        }
                    }
                    else if (title.TitleType < TitleType.Lordship)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override ExplainedNumber CalculateDailyCohesionChange(Army army, bool includeDescriptions = false)
        {
            ExplainedNumber result =  base.CalculateDailyCohesionChange(army, includeDescriptions);
            if (result.ResultNumber < 0f && army.LeaderParty != null && army.LeaderParty.LeaderHero != null)
            {
                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(army.LeaderParty.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.CommanderInspirer))
                {
                    result.Add(result.ResultNumber * -0.12f, BKPerks.Instance.CommanderInspirer.Name);
                }
            }

            return result;
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
            if (BannerKingsConfig.Instance.TitleManager != null && !party.ActualClan.IsUnderMercenaryService)
            {
                var vassals = BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(armyLeaderParty.ActualClan);
                if (!vassals.Contains(party.LeaderHero))
                {
                    result *= 2f;
                }
            }

            return (int) result;
        }
    }
}