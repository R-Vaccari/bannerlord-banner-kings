using BannerKings.Extensions;
using BannerKings.Managers.Court;
using BannerKings.Managers.Education;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Models.Vanilla.Abstract;
using BannerKings.Settings;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKArmyManagementModel : ArmyModel
    {
        public override bool CanCreateArmy(Hero armyLeader)
        {
            if (armyLeader.Clan == null) return false;

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
                    List<MobileParty> toRemove = new List<MobileParty>();
                    foreach (MobileParty p in results)
                    {
                        if (p != leaderParty && p.LeaderHero != null && CanCreateArmy(p.LeaderHero))
                        {
                            toRemove.Add(p);
                        }
                    }

                    foreach (MobileParty p in toRemove)
                    {
                        results.Remove(p);
                    }
                }

                foreach (var party in leaderParty.LeaderHero.Clan.WarPartyComponents)
                {
                    if (!results.Contains(party.MobileParty) && 
                        party.MobileParty != leaderParty && 
                        party.MobileParty.IsAvailableForArmies())
                    {
                        results.Add(party.MobileParty);
                    }
                }
            }

            return results;
        }

        public override ExplainedNumber CalculateDailyCohesionChange(Army army, bool includeDescriptions = false)
        {
            ExplainedNumber result =  base.CalculateDailyCohesionChange(army, includeDescriptions);
            result.LimitMax(-0.1f);
            
            if (result.ResultNumber < 0f && army.LeaderParty != null && army.LeaderParty.LeaderHero != null)
            {
                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(army.LeaderParty.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.CommanderInspirer))
                {
                    result.Add(result.ResultNumber * -0.12f, BKPerks.Instance.CommanderInspirer.Name);
                }
            }

            if (army.Kingdom != null)
            {
                FeudalTitle kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(army.Kingdom);
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

            result.Add(result.ResultNumber * -BannerKingsSettings.Instance.CohesionBoost, 
                new TaleWorlds.Localization.TextObject("{=hpWaDjNM}Army Cohesion Boost"));
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

            if (armyLeaderParty.ActualClan == party.ActualClan)
            {
                return 0;
            }

            float result = base.CalculatePartyInfluenceCost(armyLeaderParty, party);
            if (!party.ActualClan.IsUnderMercenaryService)
            {
                var vassals = BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(armyLeaderParty.ActualClan);
                if (!vassals.Contains(party.LeaderHero))
                {
                    result *= 1.5f;
                }
            }

            //result += BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(armyLeaderParty.LeaderHero.Clan).ResultNumber * 0.01f;

            var kingdom = armyLeaderParty.LeaderHero?.Clan.Kingdom;
            if (kingdom != null && CanCreateArmy(party.LeaderHero))
            {
                FeudalTitle kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (kingdomTitle != null)
                {
                    if (kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyPrivate))
                    {
                        result *= 2f;
                    }
                    else if (kingdomTitle.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion))
                    {
                        result *= 5f;
                    }
                }
            }

            return (int) result;
        }
    }
}