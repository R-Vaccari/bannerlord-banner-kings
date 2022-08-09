using BannerKings.Managers.AI;
using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Lifestyles;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKLifestyleBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(OnWeeklyTick));
            CampaignEvents.HeroGainedSkill.AddNonSerializedListener(this, new Action<Hero, SkillObject, bool, int, bool>(OnHeroGainedSkill));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public void OnHeroGainedSkill(Hero hero, SkillObject skill, bool hasNewPerk, int change = 1, bool shouldNotify = true)
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
