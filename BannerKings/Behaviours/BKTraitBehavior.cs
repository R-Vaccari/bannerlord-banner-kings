using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.CampaignSystem.Issues.HeadmanNeedsToDeliverAHerdIssueBehavior;
using static TaleWorlds.CampaignSystem.Issues.LandlordNeedsAccessToVillageCommonsIssueBehavior;

namespace BannerKings.Behaviours
{
    public class BKTraitBehavior : BannerKingsBehavior
    {
        private bool traitsInitialized;
        public override void RegisterEvents()
        {
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener(this, OnQuestCompleted);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-traits-initialized", ref traitsInitialized);
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            if (!traitsInitialized)
            {
                foreach (var hero in Hero.AllAliveHeroes)
                {
                    InitPersonalityTraits(hero);
                    InitNonPersonalityTraits(hero);
                }
                traitsInitialized = true;
            }
        }

        private void OnHeroCreated(Hero hero, bool bornNaturally)
        {
            InitPersonalityTraits(hero);
            InitNonPersonalityTraits(hero);
            if (bornNaturally) InitCongenitalTraits(hero);
            AddAttributes(hero);
        }

        private void InitCongenitalTraits(Hero hero)
        {
            if (hero.Father != null)
            {
                foreach (TraitObject congenital in BKTraits.Instance.CongenitalTraits)
                {
                    int level = hero.Father.GetTraitLevel(congenital);
                    if (level != 0)
                    {
                        if (MBRandom.RandomFloat < 0.15f * MathF.Abs(level))
                        {
                            hero.SetTraitLevel(congenital, level);
                        }
                        else if (MBRandom.RandomFloat < 0.3f * MathF.Abs(level))
                        {
                            if (level == -2) level++;
                            else if (level == 2) level--;
                            hero.SetTraitLevel(congenital, level);
                        }
                    }
                }
            }

            if (hero.Mother != null)
            {
                foreach (TraitObject congenital in BKTraits.Instance.CongenitalTraits)
                {
                    int level = hero.Mother.GetTraitLevel(congenital);
                    if (level != 0)
                    {
                        if (MBRandom.RandomFloat < 0.15f * MathF.Abs(level))
                        {
                            hero.SetTraitLevel(congenital, level);
                        }
                        else if (MBRandom.RandomFloat < 0.3f * MathF.Abs(level))
                        {
                            if (level == -2) level++;
                            else if (level == 2) level--;
                            hero.SetTraitLevel(congenital, level);
                        }
                    }
                }
            }
        }

        private void AddAttributes(Hero hero)
        {
            int attractive = hero.GetTraitLevel(BKTraits.Instance.CongenitalAttractive);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Social, attractive, false);

            int intelect = hero.GetTraitLevel(BKTraits.Instance.CongenitalIntelligent);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Intelligence, intelect, false);

            int robust = hero.GetTraitLevel(BKTraits.Instance.CongenitalRobust);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Endurance, robust, false);
        }

        private void OnQuestCompleted(QuestBase quest, QuestBase.QuestCompleteDetails details)
        {
            if (details == QuestBase.QuestCompleteDetails.Timeout)
            {
                Utils.Helpers.AddTraitLevel(Hero.MainHero, BKTraits.Instance.Diligent, -1, 0.3f);
            }

            if (quest is LandlordNeedsAccessToVillageCommonsIssueQuest)
            {
                if (details == QuestBase.QuestCompleteDetails.Success)
                {
                    Utils.Helpers.AddTraitLevel(Hero.MainHero, BKTraits.Instance.Just, 1, 0.4f);
                }
                else if (details == QuestBase.QuestCompleteDetails.FailWithBetrayal)
                {
                    Utils.Helpers.AddTraitLevel(Hero.MainHero, BKTraits.Instance.Just, -1);
                }
            }
            else if (quest is HeadmanNeedsToDeliverAHerdIssueQuest)
            {
                if (details == QuestBase.QuestCompleteDetails.Success)
                {
                    Utils.Helpers.AddTraitLevel(Hero.MainHero, BKTraits.Instance.Diligent, 1, 0.2f);
                }
            }
        }

        private void InitNonPersonalityTraits(Hero hero)
        {
            int violentAptitude = 0;
            int eruditeAptitude = 0;
            int socialAptitude = 0;
            float authoritarian = 0;
            float egalitarian = 0;
            float oligarchic = 0;

            if (MBRandom.RandomFloat < MBRandom.RandomFloat)
            {
                violentAptitude += MBRandom.RandomInt(-1, 1);
            }

            if (MBRandom.RandomFloat < MBRandom.RandomFloat)
            {
                eruditeAptitude += MBRandom.RandomInt(-1, 1);
            }

            if (MBRandom.RandomFloat < MBRandom.RandomFloat)
            {
                socialAptitude += MBRandom.RandomInt(-1, 1);
            }

            int honor = hero.GetTraitLevel(DefaultTraits.Honor);
            if (MBRandom.RandomFloat < MathF.Abs(honor) * 0.3f)
            {
                socialAptitude += honor;
            }

            oligarchic += MBRandom.RandomInt(1, 3) * (float)honor;

            int calculating = hero.GetTraitLevel(DefaultTraits.Calculating);
            if (MBRandom.RandomFloat < MathF.Abs(calculating) * 0.3f)
            {
                eruditeAptitude += calculating;
            }

            oligarchic += MBRandom.RandomInt(1, 5) * (float)calculating;

            int mercy = hero.GetTraitLevel(DefaultTraits.Mercy);
            if (MBRandom.RandomFloat < MathF.Abs(mercy) * 0.3f)
            {
                violentAptitude += mercy;
            }

            egalitarian += MBRandom.RandomInt(1, 5) * (float)mercy;
            authoritarian -= MBRandom.RandomInt(1, 2) * (float)mercy;

            int generosity = hero.GetTraitLevel(DefaultTraits.Generosity);
            if (MBRandom.RandomFloat < MathF.Abs(generosity) * 0.3f)
            {
                socialAptitude += generosity;
            }

            egalitarian += MBRandom.RandomInt(1, 5) * (float)generosity;
            oligarchic -= MBRandom.RandomInt(1, 2) * (float)generosity;

            int valor = hero.GetTraitLevel(DefaultTraits.Valor);
            if (MBRandom.RandomFloat < MathF.Abs(valor) * 0.3f)
            {
                violentAptitude += valor;
            }

            authoritarian += MBRandom.RandomInt(1, 5) * (float)valor;

            hero.SetTraitLevel(BKTraits.Instance.AptitudeViolence, (int)MathF.Clamp(violentAptitude, -2, 2));
            hero.SetTraitLevel(BKTraits.Instance.AptitudeErudition, (int)MathF.Clamp(eruditeAptitude, -2, 2));
            hero.SetTraitLevel(BKTraits.Instance.AptitudeSocializing, (int)MathF.Clamp(socialAptitude, -2, 2));

            authoritarian += MBRandom.RandomInt(1, 3) * (float)violentAptitude;
            oligarchic += MBRandom.RandomInt(1, 3) * (float)eruditeAptitude;
            egalitarian += MBRandom.RandomInt(1, 3) * (float)socialAptitude;

            if (hero.IsLord)
            {
                oligarchic += 4;
                if (hero.MapFaction.Leader == hero)
                {
                    authoritarian += 2;
                }

                if (hero.IsFemale)
                {
                    egalitarian += 2;
                }
            }

            hero.SetTraitLevel(DefaultTraits.Oligarchic, (int)MathF.Clamp(oligarchic, DefaultTraits.Oligarchic.MinValue, DefaultTraits.Oligarchic.MaxValue));
            hero.SetTraitLevel(DefaultTraits.Authoritarian, (int)MathF.Clamp(authoritarian, DefaultTraits.Authoritarian.MinValue, DefaultTraits.Authoritarian.MaxValue));
            hero.SetTraitLevel(DefaultTraits.Egalitarian, (int)MathF.Clamp(egalitarian, DefaultTraits.Egalitarian.MinValue, DefaultTraits.Egalitarian.MaxValue));
        }

        private void InitPersonalityTraits(Hero hero)
        {
            foreach (TraitObject trait in BKTraits.Instance.PersonalityTraits)
            {
                float chance = 0.01f;
                int level = 1;

                if (MBRandom.RandomFloat < MBRandom.RandomFloat)
                {
                    level += MBRandom.RandomInt(-1, 1);
                }

                if (MBRandom.RandomFloat < MBRandom.RandomFloat)
                {
                    level += MBRandom.RandomInt(-1, 1);
                }

                if (hero.IsGangLeader)
                {
                    if (trait == BKTraits.Instance.Just)
                    {
                        level -= 2;
                    }
                }

                if (hero.IsPreacher)
                {
                    if (trait == BKTraits.Instance.Zealous)
                    {
                        chance += 0.13f;
                        level += 1;
                    }
                }

                if (BannerKingsConfig.Instance.ReligionsManager != null)
                {
                    var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
                    if (religion == null)
                    {
                        if (trait == BKTraits.Instance.Zealous)
                        {
                            chance = 0f;
                        }
                    }
                }

                if (MBRandom.RandomFloat < chance)
                {
                    hero.SetTraitLevel(trait, level);
                }
            }
        }
    }
}
