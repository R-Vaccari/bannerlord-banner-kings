using BannerKings.Managers.Court;
using BannerKings.Managers.Education;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
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

                CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(kingdom.RulingClan);
                if (council.GetHeroPositions(armyLeader).Any(x => x.Privileges.Contains(CouncilPrivileges.ARMY_PRIVILEGE)))
                {
                    return true;
                }

                FeudalTitle kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                FeudalTitle heroTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(armyLeader);
                if (kingdomTitle != null)
                {
                    if (kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyPrivate) && heroTitle != null)
                    {
                        if (kingdom.ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
                        {
                            if (heroTitle.TitleType <= TitleType.Dukedom)
                            {
                                return true;
                            }
                        }
                        else if (heroTitle.TitleType < TitleType.Lordship)
                        {
                            return true;
                        }
                    }
                    else if (kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyHorde))
                        return armyLeader.IsClanLeader();
                } else return armyLeader.IsClanLeader();
            }

            return false;
        }

        public override List<MobileParty> GetMobilePartiesToCallToArmy(MobileParty leaderParty)
        {
            List<MobileParty> results = base.GetMobilePartiesToCallToArmy(leaderParty);
            var kingdom = leaderParty.LeaderHero?.Clan.Kingdom;
            if (kingdom != null)
            {
                FeudalTitle kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (kingdomTitle != null && kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion))
                {
                    foreach (MobileParty p in results)
                    {
                        if (p != leaderParty && p.LeaderHero != null && CanCreateArmy(p.LeaderHero))
                        {
                            results.Remove(p);
                        }
                    }
                }
            }

            return results;
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

            if (army.MapFaction != null)
            {
                FeudalTitle kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(army.MapFaction);
                if (kingdomTitle != null)
                {
                    if (kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyHorde))
                    {
                        result.Add(-0.5f, DefaultDemesneLaws.Instance.ArmyHorde.Name);
                    }
                    else if (kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion))
                    {
                        result.Add(0.5f, DefaultDemesneLaws.Instance.ArmyLegion.Name);
                    }
                }
            }

            return result;
        }

        public override float DailyBeingAtArmyInfluenceAward(MobileParty armyMemberParty)
        {
            var result = base.DailyBeingAtArmyInfluenceAward(armyMemberParty);
            if (armyMemberParty.MapFaction.IsKingdomFaction)
            {
                Kingdom kingdom = (armyMemberParty.MapFaction as Kingdom);
                if (kingdom.ActivePolicies.Contains(BKPolicies.Instance.LimitedArmyPrivilege))
                {
                    result *= 1.5f;
                }

                FeudalTitle kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (kingdomTitle != null)
                {
                    if (kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion))
                    {
                        result *= 0.7f;
                    }
                }
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