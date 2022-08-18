using BannerKings.Managers.AI;
using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Lifestyles;
using System;
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
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(OnConversationEnded));
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(OnWeeklyTick));
            CampaignEvents.HeroGainedSkill.AddNonSerializedListener(this, new Action<Hero, SkillObject, int, bool>(OnHeroGainedSkill));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnConversationEnded(CharacterObject character)
        {
            EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
            if (education.Lifestyle == DefaultLifestyles.Instance.Outlaw && character.IsHero)
            {
                if (character.Occupation != Occupation.GangLeader || character.HeroObject.GetTraitLevel(DefaultTraits.Honor) >= 0)
                {
                    float random = MBRandom.RandomFloat;
                    if (random < 0.05f) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, character.HeroObject, -3);
                }
            }
        }

        public void OnHeroGainedSkill(Hero hero, SkillObject skill, int change = 1, bool shouldNotify = true)
        {
            if (BannerKingsConfig.Instance.EducationManager == null) return;

            EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
            if (education == null || education.Lifestyle == null) return;

            Lifestyle lf = education.Lifestyle;
            if (skill == lf.FirstSkill || skill == lf.SecondSkill) lf.AddProgress(education.StandartLifestyleProgress);
        }

        private void OnWeeklyTick()
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero.Clan == Clan.PlayerClan || hero.IsChild) continue;

                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                if (education.Lifestyle == null) education.SetCurrentLifestyle(new AIBehavior().ChooseLifestyle(hero));
                else if (education.Lifestyle.CanInvestFocus(hero)) education.Lifestyle.InvestFocus(education, hero);
            }
        }
    }
}
