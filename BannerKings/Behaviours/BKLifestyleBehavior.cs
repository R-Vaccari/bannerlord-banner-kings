using BannerKings.Managers.AI;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKLifestyleBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.HeroGainedSkill.AddNonSerializedListener(this, OnHeroGainedSkill);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnConversationEnded(CharacterObject character)
        {
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
                lf.AddProgress(education.StandartLifestyleProgress / 10f);
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
                    education.SetCurrentLifestyle(new AIBehavior().ChooseLifestyle(hero));
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

        private void GainSuzerainRelation(Hero hero)
        {
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(hero);
            if (title != null && title.deJure != null && title.deJure != hero && title.deJure.IsAlive)
            {
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, title.deJure, 5);
            }
        }
    }
}