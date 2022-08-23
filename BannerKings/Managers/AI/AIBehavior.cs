using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;

namespace BannerKings.Managers.AI
{
    public class AIBehavior
    {
        public FeudalTitle ChooseTitleToGive(Hero giver, float diff, bool landed = true)
        {
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(giver);
            var candidates = new List<(FeudalTitle, float)>();

            foreach (var title in titles)
            {
                switch (landed)
                {
                    case true when title.fief == null || title.deFacto != giver:
                    case false when title.fief != null:
                        continue;
                }

                var value = BannerKingsConfig.Instance.StabilityModel.GetTitleScore(title);
                if (value > diff)
                {
                    continue;
                }

                if (title.type == TitleType.Lordship &&
                    BannerKingsConfig.Instance.StabilityModel.IsHeroOverVassalLimit(giver))
                {
                    continue;
                }

                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, giver);
                if (action.Possible)
                {
                    candidates.Add(new ValueTuple<FeudalTitle, float>(title, value));
                }
            }

            return MBRandom.ChooseWeighted(candidates);
        }

        public Hero ChooseVassalToGiftUnlandedTitle(Hero giver, FeudalTitle titleToGive)
        {
            var vassals = BannerKingsConfig.Instance.TitleManager.CalculateVassals(giver.Clan);
            var candidates = new List<(Hero, float)>();

            foreach (var pair in vassals)
            {
                var leader = pair.Key.Leader;
                if (BannerKingsConfig.Instance.StabilityModel.IsHeroOverUnlandedDemesneLimit(leader))
                {
                    continue;
                }

                float score = 100;
                score += giver.GetRelation(leader);
                if (pair.Value.Count > 0)
                {
                    var type = titleToGive.type;
                    foreach (var title in pair.Value)
                    {
                        if (title.type == type)
                        {
                            score -= 60;
                        }
                        else if (title.type < type)
                        {
                            score -= 120;
                        }
                        else if (titleToGive.vassals.Contains(title))
                        {
                            score += 40;
                        }
                    }
                }

                if (BannerKingsConfig.Instance.TitleModel.GetGrantCandidates(giver).Contains(leader))
                {
                    candidates.Add(new ValueTuple<Hero, float>(leader, score));
                }
            }

            return MBRandom.ChooseWeighted(candidates);
        }

        public Hero ChooseVassalToGiftLandedTitle(Hero giver, FeudalTitle titleToGive)
        {
            var candidates = new List<(Hero, float)>();

            if (titleToGive.type != TitleType.Lordship)
            {
                var vassals = BannerKingsConfig.Instance.TitleManager.CalculateVassals(giver.Clan);
                foreach (var pair in vassals)
                {
                    var leader = pair.Key.Leader;
                    if (BannerKingsConfig.Instance.StabilityModel.IsHeroOverUnlandedDemesneLimit(leader))
                    {
                        continue;
                    }

                    float score = 100;
                    score += giver.GetRelation(leader);
                    if (pair.Value.Count > 0)
                    {
                        var type = titleToGive.type;
                        foreach (var title in pair.Value)
                        {
                            if (title.type == type)
                            {
                                score -= 60;
                            }
                            else if (title.type < type)
                            {
                                score -= 120;
                            }
                            else if (titleToGive.vassals.Contains(title))
                            {
                                score += 40;
                            }
                        }
                    }

                    if (BannerKingsConfig.Instance.TitleModel.GetGrantCandidates(giver).Contains(leader))
                    {
                        candidates.Add(new ValueTuple<Hero, float>(leader, score));
                    }
                }
            }
            else
            {
                foreach (var companion in giver.Clan.Companions)
                {
                    float score = 100;
                    score += companion.GetSkillValue(DefaultSkills.Leadership);
                    score += companion.GetSkillValue(DefaultSkills.Tactics);

                    candidates.Add(new ValueTuple<Hero, float>(companion, score));
                }
            }


            return MBRandom.ChooseWeighted(candidates);
        }

        public Lifestyle ChooseLifestyle(Hero hero)
        {
            var candidates = new List<(Lifestyle, float)>();

            var rogueWeight = hero.GetTraitLevel(DefaultTraits.RogueSkills) - hero.GetTraitLevel(DefaultTraits.Mercy) -
                              hero.GetTraitLevel(DefaultTraits.Honor) + hero.GetTraitLevel(DefaultTraits.Thug) +
                              hero.GetTraitLevel(DefaultTraits.Thief);

            var politicianWeight =
                hero.GetTraitLevel(DefaultTraits.Politician) + hero.GetTraitLevel(DefaultTraits.Commander);

            var merchantWeight = hero.GetTraitLevel(DefaultTraits.Blacksmith) + hero.GetTraitLevel(DefaultTraits.Manager);

            var siegeWeight = hero.GetTraitLevel(DefaultTraits.Siegecraft);

            var healerWeight = hero.GetTraitLevel(DefaultTraits.Surgery);

            var warriorWeight = hero.GetTraitLevel(DefaultTraits.ArcherFIghtingSkills) +
                                hero.GetTraitLevel(DefaultTraits.BossFightingSkills) +
                                hero.GetTraitLevel(DefaultTraits.CavalryFightingSkills) +
                                hero.GetTraitLevel(DefaultTraits.HuscarlFightingSkills) +
                                hero.GetTraitLevel(DefaultTraits.HopliteFightingSkills) +
                                hero.GetTraitLevel(DefaultTraits.HorseArcherFightingSkills) +
                                hero.GetTraitLevel(DefaultTraits.KnightFightingSkills) +
                                hero.GetTraitLevel(DefaultTraits.PeltastFightingSkills) +
                                hero.GetTraitLevel(DefaultTraits.Fighter);

            var mercenaryWeight = hero.GetTraitLevel(DefaultTraits.RogueSkills) - hero.GetTraitLevel(DefaultTraits.Honor);

            var occupation = hero.Occupation;
            switch (occupation)
            {
                case Occupation.Lord:
                {
                    politicianWeight += 2;
                    warriorWeight += 3;

                    if (!hero.Clan.IsMinorFaction)
                    {
                        mercenaryWeight = 0;
                    }

                    healerWeight -= 1;
                    break;
                }
                case Occupation.Wanderer:
                    warriorWeight += 4;
                    mercenaryWeight += 1;
                    break;
                default:
                {
                    if (hero.IsNotable)
                    {
                        if (occupation == Occupation.GangLeader)
                        {
                            rogueWeight += 2;
                        }

                        mercenaryWeight += 4;

                        politicianWeight = 0;
                        warriorWeight = 0;
                        mercenaryWeight = 0;
                    }

                    break;
                }
            }


            foreach (var lf in DefaultLifestyles.Instance.All)
            {
                if (!lf.CanLearn(hero))
                {
                    continue;
                }

                var first = lf.FirstSkill;
                var second = lf.SecondSkill;
                (Lifestyle, float) tuple = new(lf, 0f);

                if (first == DefaultSkills.Medicine || second == DefaultSkills.Medicine)
                {
                    tuple.Item2 += healerWeight;
                }
                else if (first == DefaultSkills.Engineering || second == DefaultSkills.Engineering)
                {
                    tuple.Item2 += siegeWeight;
                }
                else if (first == DefaultSkills.Trade || second == DefaultSkills.Trade)
                {
                    tuple.Item2 += merchantWeight;
                }
                else if (first == DefaultSkills.Leadership || second == DefaultSkills.Leadership ||
                         first == BKSkills.Instance.Lordship || second == BKSkills.Instance.Lordship)
                {
                    tuple.Item2 += politicianWeight;
                }
                else if (first == DefaultSkills.Roguery || second == DefaultSkills.Roguery)
                {
                    if (hero.Clan is {IsMinorFaction: true})
                    {
                        tuple.Item2 += mercenaryWeight;
                    }

                    tuple.Item2 += rogueWeight;
                }
                else
                {
                    tuple.Item2 += warriorWeight;
                }

                if (lf.Culture == hero.Culture && tuple.Item2 != 0f)
                {
                    tuple.Item2 += 1f;
                }

                candidates.Add(tuple);
            }

            return MBRandom.ChooseWeighted(candidates);
        }

        public bool AcceptNotableAid(Clan clan, PopulationData data)
        {
            var kingdom = clan.Kingdom;
            return data.Stability >= 0.5f && data.NotableSupport.ResultNumber >= 0.5f && kingdom != null &&
                   FactionManager.GetEnemyFactions(kingdom).Any() && clan.Influence > 50f * clan.Tier;
        }

        public void SettlementManagement(Settlement target)
        {
            if (BannerKingsConfig.Instance.PopulationManager == null ||
                !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target))
            {
                return;
            }

            if (target.OwnerClan == Clan.PlayerClan)
            {
                return;
            }

            var kingdom = target.OwnerClan.Kingdom;
            var currentDecisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(target);
            var changedDecisions = new List<BKSettlementDecision>();

            var town = target.Town;
            if (town is {Governor: { }})
            {
                if (town.FoodStocks < town.FoodStocksUpperLimit() * 0.2f && town.FoodChange < 0f)
                {
                    var rationDecision =
                        (BKRationDecision) currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                    rationDecision.Enabled = true;
                    changedDecisions.Add(rationDecision);
                }
                else
                {
                    var rationDecision =
                        (BKRationDecision) currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                    rationDecision.Enabled = false;
                    changedDecisions.Add(rationDecision);
                }

                var garrison = town.GarrisonParty;
                if (garrison != null)
                {
                    float wage = garrison.TotalWage;
                    var income = Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(town).ResultNumber;
                    if (wage >= income * 0.5f)
                    {
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                            new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Dischargement, target));
                    }
                    else if (wage <= income * 0.2f)
                    {
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                            new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Enlistment, target));
                    }
                    else
                    {
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                            new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Standard, target));
                    }
                }

                if (town.LoyaltyChange < 0)
                {
                    UpdateTaxPolicy(1, target);
                }
                else
                {
                    UpdateTaxPolicy(-1, target);
                }

                if (kingdom != null)
                {
                    var enemies = FactionManager.GetEnemyKingdoms(kingdom);
                    var atWar = enemies.Any();

                    if (target.Owner.GetTraitLevel(DefaultTraits.Calculating) > 0)
                    {
                        var subsidizeMilitiaDecision =
                            (BKSubsidizeMilitiaDecision) currentDecisions.FirstOrDefault(x =>
                                x.GetIdentifier() == "decision_militia_subsidize");
                        subsidizeMilitiaDecision.Enabled = atWar ? true : false;
                        changedDecisions.Add(subsidizeMilitiaDecision);
                    }
                }

                var criminal = (BKCriminalPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "criminal");
                var mercy = target.Owner.GetTraitLevel(DefaultTraits.Mercy);

                var targetCriminal = mercy switch
                {
                    > 0 => new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Forgiveness, target),
                    < 0 => new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Execution, target),
                    _ => new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Enslavement, target)
                };

                if (targetCriminal.Policy != criminal.Policy)
                {
                    BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, targetCriminal);
                }

                var taxSlavesDecision =
                    (BKTaxSlavesDecision) currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_tax");
                if (target.Owner.GetTraitLevel(DefaultTraits.Authoritarian) > 0)
                {
                    taxSlavesDecision.Enabled = true;
                }
                else if (target.Owner.GetTraitLevel(DefaultTraits.Egalitarian) > 0)
                {
                    taxSlavesDecision.Enabled = false;
                }

                changedDecisions.Add(taxSlavesDecision);

                var workforce = (BKWorkforcePolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "workforce");
                var workforcePolicies = new List<ValueTuple<WorkforcePolicy, float>> {(WorkforcePolicy.None, 1f)};
                var saturation = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).LandData
                    .WorkforceSaturation;
                if (saturation > 1f)
                {
                    workforcePolicies.Add((WorkforcePolicy.Land_Expansion, 2f));
                }

                if (town.Security < 20f)
                {
                    workforcePolicies.Add((WorkforcePolicy.Martial_Law, 2f));
                }

                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                    new BKWorkforcePolicy(MBRandom.ChooseWeighted(workforcePolicies), target));

                foreach (var dec in changedDecisions)
                {
                    BannerKingsConfig.Instance.PolicyManager.UpdateSettlementDecision(target, dec);
                }
            }
            else if (target.IsVillage && target.Village.Bound.Town.Governor != null)
            {
                var villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).VillageData;
                villageData.StartRandomProject();
                var hearths = target.Village.Hearth;
                switch (hearths)
                {
                    case < 300f:
                        UpdateTaxPolicy(-1, target);
                        break;
                    case > 1000f:
                        UpdateTaxPolicy(1, target);
                        break;
                }
            }
        }

        private static void UpdateTaxPolicy(int value, Settlement settlement)
        {
            var tax = (BKTaxPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            var taxType = tax.Policy;
            if ((value == 1 && taxType != TaxType.High) || (value == -1 && taxType != TaxType.Low))
            {
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement,
                    new BKTaxPolicy(taxType + value, settlement));
            }
        }
    }
}