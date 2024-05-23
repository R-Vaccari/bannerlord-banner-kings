using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

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
                    case true when title.Fief == null || title.deFacto != giver:
                    case false when title.Fief != null:
                        continue;
                }

                var value = BannerKingsConfig.Instance.StabilityModel.GetTitleScore(title);
                if (value > diff)
                {
                    continue;
                }

                if (title.TitleType == TitleType.Lordship &&
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

        public Hero ChooseVassalToGiftUnlandedTitle(Hero giver, FeudalTitle titleToGive, Dictionary<Clan, List<FeudalTitle>> vassals)
        {
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
                    var type = titleToGive.TitleType;
                    foreach (var title in pair.Value)
                    {
                        if (title.TitleType == type)
                        {
                            score -= 60;
                        }
                        else if (title.TitleType < type)
                        {
                            score -= 120;
                        }
                        else if (titleToGive.Vassals.Contains(title))
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

            if (titleToGive.TitleType != TitleType.Lordship)
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
                        var type = titleToGive.TitleType;
                        foreach (var title in pair.Value)
                        {
                            if (title.TitleType == type)
                            {
                                score -= 60;
                            }
                            else if (title.TitleType < type)
                            {
                                score -= 120;
                            }
                            else if (titleToGive.Vassals.Contains(title))
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
            if (hero == null)
            {
                return null;
            }

            Lifestyle result = null;
            ExceptionUtils.TryCatch(() =>
            {
                var candidates = new List<(Lifestyle, float)>();

                var rogueWeight = hero.GetTraitLevel(DefaultTraits.RogueSkills) - hero.GetTraitLevel(DefaultTraits.Mercy) -
                                  hero.GetTraitLevel(DefaultTraits.Honor) + hero.GetTraitLevel(DefaultTraits.Thug) +
                                  hero.GetTraitLevel(DefaultTraits.Smuggler);

                var politicianWeight =
                    hero.GetTraitLevel(DefaultTraits.Politician) + hero.GetTraitLevel(DefaultTraits.Commander);

                var merchantWeight = hero.GetTraitLevel(DefaultTraits.Blacksmith) + hero.GetTraitLevel(DefaultTraits.Manager);
                var artisanWeight = hero.GetTraitLevel(DefaultTraits.Blacksmith) * 3f;

                var siegeWeight = hero.GetTraitLevel(DefaultTraits.Siegecraft);

                var healerWeight = hero.GetTraitLevel(DefaultTraits.Surgery);

                var warriorWeight = hero.GetTraitLevel(DefaultTraits.ArcherFIghtingSkills) +
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
                            else
                            {
                                mercenaryWeight += 2;
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
                                if (hero.IsGangLeader)
                                {
                                    rogueWeight += 2;
                                }

                                if (hero.IsArtisan)
                                {
                                    artisanWeight += 2;
                                }

                                if (hero.IsMerchant)
                                {
                                    merchantWeight += 2;
                                }

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
                        if (hero.IsLord && hero.Clan.IsUnderMercenaryService)
                        {
                            tuple.Item2 += mercenaryWeight;
                        }

                        tuple.Item2 += rogueWeight;
                    }
                    else if (first == DefaultSkills.Crafting || second == DefaultSkills.Crafting)
                    {
                        tuple.Item2 += artisanWeight;
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

                result = MBRandom.ChooseWeighted(candidates);
            },
            GetType().Name,
            false);

            return result;
        }

        public bool AcceptNotableAid(Clan clan, PopulationData data)
        {
            var kingdom = clan.Kingdom;
            return data != null && data.Stability >= 0.5f && data.NotableSupport.ResultNumber >= 0.5f && kingdom != null &&
                   FactionManager.GetEnemyFactions(kingdom).Any() && clan.Influence > 50f * clan.Tier;
        }
    }
}