using System.Collections.Generic;
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

        [SaveableField(101)]
        private int investedFocus;
        private CultureObject culture;
        private SkillObject firstSkill;
        private SkillObject secondSkill;
        private List<PerkObject> perks;
        private TextObject effects;
        private float firstEffect, secondEffect;

        public Lifestyle(string id) : base(id)
        {
            progress = 0f;
            investedFocus = 0;
        }

        public static Lifestyle CreateLifestyle(Lifestyle lf)
        {
            Lifestyle lifestyle = new Lifestyle(lf.StringId);
            lifestyle.Initialize(lf.Name, lf.Description, lf.FirstSkill, lf.SecondSkill, new List<PerkObject>(lf.Perks), lf.PassiveEffects,
                    lf.FirstEffect, lf.SecondEffect, lf.Culture);
            return lifestyle;
        }

        public void Initialize(TextObject name, TextObject description, SkillObject firstSkill, SkillObject secondSkill,
            List<PerkObject> perks, TextObject effects, float firstEffect, float secondEffect, CultureObject culture = null)
        {
            Initialize(name, description);
            this.firstSkill = firstSkill;
            this.secondSkill = secondSkill;
            this.perks = perks;
            this.effects = effects;
            this.firstEffect = firstEffect;
            this.secondEffect = secondEffect;
            this.culture = culture;

        }

        public void loadFix(float progress = 0f, int focus = 0)
        {
            this.progress = progress;
            this.investedFocus = focus;
        }

        public float NecessarySkillForFocus => 80f * (investedFocus + 1f);

        public bool CanLearn(Hero hero) => (culture == null || hero.Culture == culture) && hero.GetSkillValue(firstSkill) >= 15
            && hero.GetSkillValue(secondSkill) >= 15;
        public bool CanInvestFocus(Hero hero) => progress >= 1f && perks.Count >= investedFocus + 1 && 
            (hero.GetSkillValue(firstSkill) + hero.GetSkillValue(secondSkill) >= NecessarySkillForFocus);
        public void InvestFocus(EducationData data, Hero hero) 
        {
            hero.HeroDeveloper.UnspentFocusPoints -= 1;
            PerkObject perk = perks[investedFocus];
            data.AddPerk(perk);
            investedFocus += 1;
            progress = 0f;
            if (hero == Hero.MainHero) 
                InformationManager.AddQuickInformation(new TextObject("{=!}You have received the {PERK} perk from the {LIFESTYLE} lifestyle.")
                            .SetTextVariable("PERK", perk.Name)
                            .SetTextVariable("LIFESTYLE", Name));
        } 

        public void AddProgress(float progress) => this.progress = MBMath.ClampFloat(this.progress + progress, 0f, 1f);
        

        public int InvestedFocus => investedFocus;
        public float Progress => progress;
        public CultureObject Culture => culture;
        public TextObject PassiveEffects => effects.SetTextVariable("EFFECT1", firstEffect).SetTextVariable("EFFECT2", secondEffect);
        public float FirstEffect => firstEffect;
        public float SecondEffect => secondEffect;
        public SkillObject FirstSkill => firstSkill;
        public SkillObject SecondSkill => secondSkill;
        public MBReadOnlyList<PerkObject> Perks => perks.GetReadOnlyList();
    }
}
