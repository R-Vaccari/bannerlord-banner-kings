using BannerKings.Managers.AI;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Utils;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKLifestyleBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.HeroGainedSkill.AddNonSerializedListener(this, OnHeroGainedSkill);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty != null && mobileParty.IsCaravan && mobileParty.Owner != null && settlement.Notables != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.Owner);
                if (education.HasPerk(BKPerks.Instance.CaravaneerOutsideConnections) && MBRandom.RandomFloat < 0.1f)
                {
                    var random = settlement.Notables.GetRandomElement();
                    if (random != null)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(mobileParty.Owner, random, 2);
                    }
                }
            }
        }

        private void OnConversationEnded(IEnumerable<CharacterObject> characters)
        {
            CharacterObject character = characters.FirstOrDefault(x => x != Hero.MainHero.CharacterObject);
            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
            if (education.Lifestyle == DefaultLifestyles.Instance.Outlaw && character.IsHero)
            {
                if (character.Occupation != Occupation.GangLeader ||
                    character.HeroObject.GetTraitLevel(DefaultTraits.Honor) >= 0)
                {
                    var random = MBRandom.RandomFloat;
                    if (random < 0.05f)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, character.HeroObject, -3);
                    }
                }
            }
        }

        public void OnHeroGainedSkill(Hero hero, SkillObject skill, int change = 1, bool shouldNotify = true)
        {
            if (BannerKingsConfig.Instance.EducationManager == null)
            {
                return;
            }

            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
            if (education?.Lifestyle == null)
            {
                return;
            }

            var lf = education.Lifestyle;
            if (skill == lf.FirstSkill || skill == lf.SecondSkill)
            {
                education.AddProgress(education.CurrentLifestyleRate.ResultNumber / 10f);
            }
        }

        private void OnWeeklyTick()
        {
            foreach (var hero in Hero.AllAliveHeroes)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                if (CampaignTime.Now.GetDayOfYear == 1 && hero.Clan != null && hero.Clan.IsUnderMercenaryService)
                {
                    if (education.HasPerk(BKPerks.Instance.VaryagRecognizedMercenary))
                    {
                        hero.Clan.AddRenown(30f);
                    }
                }

                if (hero.Clan == Clan.PlayerClan || hero.IsChild)
                {
                    continue;
                }
                
                if (education.Lifestyle == null)
                {
                    education.SetCurrentLifestyle(ChooseLifestyle(hero));
                }
                else if (education.Lifestyle.CanInvestFocus(hero))
                {
                    education.Lifestyle.InvestFocus(education, hero);
                }

                if (CampaignTime.Now.GetDayOfSeason == 1 && education.HasPerk(BKPerks.Instance.RitterOathbound))
                {
                    if (MBRandom.RandomFloat < 0.25f)
                    {
                        GainSuzerainRelation(hero);
                    }
                }
            }
        }

        private Lifestyle ChooseLifestyle(Hero hero)
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
                                if (occupation == Occupation.GangLeader)
                                {
                                    rogueWeight += 2;
                                }

                                if (occupation == Occupation.Artisan)
                                {
                                    artisanWeight += 2;
                                }

                                if (occupation == Occupation.Merchant)
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

        private void GainSuzerainRelation(Hero hero)
        {
            var title = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(hero);
            if (title != null && title.deJure != null && title.deJure != hero && title.deJure.IsAlive)
            {
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, title.deJure, 5);
            }
        }
    }
}