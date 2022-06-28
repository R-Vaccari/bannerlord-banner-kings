using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Training
{
    public class Lifestyle : BannerKingsObject
    {
        private CultureObject culture;
        private SkillObject firstSkill;
        private SkillObject secondSkill;
        private List<PerkObject> perks;

        public Lifestyle(TextObject name, TextObject description, SkillObject firstSkill, SkillObject secondSkill,
            CultureObject culture = null) : base(name, description)
        {
            this.firstSkill = firstSkill;
            this.secondSkill = secondSkill;
            this.culture = culture;
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public CultureObject Culture => culture;
        public SkillObject FirstSkill => firstSkill;
        public SkillObject SecondSkill => secondSkill;
        public MBReadOnlyList<PerkObject> Perks => perks.GetReadOnlyList();
    }
}
