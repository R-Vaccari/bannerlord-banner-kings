using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class Lifestyle : BannerKingsObject
    {
        [SaveableField(100)]
        private float progress;
        private int investedFocus;
        private CultureObject culture;
        private SkillObject firstSkill;
        private SkillObject secondSkill;
        private List<PerkObject> perks;

        public Lifestyle(string id) : base(id)
        {
            progress = 0f;
            investedFocus = 0;
        }

        public void Initialize(TextObject name, TextObject description, SkillObject firstSkill, SkillObject secondSkill,
            List<PerkObject> perks, CultureObject culture = null)
        {
            Initialize(name, description);
            this.firstSkill = firstSkill;
            this.secondSkill = secondSkill;
            this.perks = perks;
            this.culture = culture;
        }

        public float NecessarySkillForFocus => 75f * (investedFocus + 1f);
        public bool CanInvestFocus(Hero hero) => progress >= 1f && perks.Count >= investedFocus + 1 && 
            (hero.GetSkillValue(firstSkill) >= NecessarySkillForFocus || hero.GetSkillValue(secondSkill) >= NecessarySkillForFocus);
        public PerkObject InvestFocus(Hero hero) 
        {
            PerkObject perk = perks[investedFocus];
            investedFocus += 1;
            InformationManager.AddQuickInformation(new TextObject("{=!}You have received the {PERK} from the {LIFESTYLE} lifestyle.")
                            .SetTextVariable("PERK", perk.Name)
                            .SetTextVariable("LIFESTYLE", Name));
            return perk;
        } 

        public bool CanLearn(Hero hero) => (culture == null ||hero.Culture == culture) && hero.GetSkillValue(firstSkill) >= 75 
            && hero.GetSkillValue(secondSkill) >= 75;

        public void AddProgress(float progress) => this.progress = MBMath.ClampFloat(this.progress + progress, 0f, 1f);
        

        public int InvestedFocus => investedFocus;
        public float Progress => progress;
        public CultureObject Culture => culture;
        public SkillObject FirstSkill => firstSkill;
        public SkillObject SecondSkill => secondSkill;
        public MBReadOnlyList<PerkObject> Perks => perks.GetReadOnlyList();
    }
}
