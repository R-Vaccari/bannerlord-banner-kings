using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class Lifestyle : BannerKingsObject
    {
        private float progress;
        private CultureObject culture;
        private SkillObject firstSkill;
        private SkillObject secondSkill;
        private List<PerkObject> perks;

        public Lifestyle(string id, TextObject name, TextObject description) : base(id, name, description)
        {
        }

        public void Initialize(SkillObject firstSkill, SkillObject secondSkill,
            List<PerkObject> perks, CultureObject culture = null)
        { 
            this.firstSkill = firstSkill;
            this.secondSkill = secondSkill;
            this.perks = perks;
            this.culture = culture;
        }

        public bool CanLearn(Hero hero) => (culture == null ||hero.Culture == culture) && hero.GetSkillValue(firstSkill) >= 150 
            && hero.GetSkillValue(secondSkill) >= 150;

        public float Progress => progress;
        public CultureObject Culture => culture;
        public SkillObject FirstSkill => firstSkill;
        public SkillObject SecondSkill => secondSkill;
        public MBReadOnlyList<PerkObject> Perks => perks.GetReadOnlyList();
    }
}
