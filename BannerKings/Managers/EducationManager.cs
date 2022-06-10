using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Training;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Managers
{
    public class EducationManager
    {
        private Dictionary<Hero, EducationData> Educations { get; set; }

        public EducationManager()
        {
            Educations = new Dictionary<Hero, EducationData>();
        }

        public void InitializeEducations()
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                Dictionary<Language, float> languages = new Dictionary<Language, float>();
                Language native = DefaultLanguages.Instance.All.FirstOrDefault(x => x.Culture == hero.Culture);
                if (native == null) native = DefaultLanguages.Instance.Calradian;
                languages.Add(native, 1f);

                Mastery mastery = null;
                float masteryProgress = 0f;

                if (hero.IsNotable)
                {
                    if (!languages.ContainsKey(DefaultLanguages.Instance.Calradian) && MBRandom.RandomFloat <= 0.15f)
                        languages.Add(DefaultLanguages.Instance.Calradian, MBRandom.RandomFloatRanged(0.5f, 1f));
                }

                if (hero.Occupation == Occupation.Lord || hero.Occupation == Occupation.Wanderer)
                {

                }


                Educations.Add(hero, new EducationData(languages));
            }
        }
    }
}
