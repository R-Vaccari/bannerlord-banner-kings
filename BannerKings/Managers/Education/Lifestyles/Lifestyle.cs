using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class Lifestyle : BannerKingsObject
    {
        private TextObject effects;

        private List<PerkObject> perks;

        public Lifestyle(string id) : base(id)
        {
            Progress = 0f;
            InvestedFocus = 0;
        }

        public float NecessarySkillForFocus => 80f * (InvestedFocus + 1f);


        [field: SaveableField(101)] public int InvestedFocus { get; private set; }

        [field: SaveableField(100)] public float Progress { get; private set; }

        public CultureObject Culture { get; private set; }

        public TextObject PassiveEffects => effects.SetTextVariable("EFFECT1", FirstEffect).SetTextVariable("EFFECT2", SecondEffect);

        public float FirstEffect { get; private set; }

        public float SecondEffect { get; private set; }

        public SkillObject FirstSkill { get; private set; }

        public SkillObject SecondSkill { get; private set; }

        public MBReadOnlyList<PerkObject> Perks => perks.GetReadOnlyList();

        public override bool Equals(object obj)
        {
            if (obj is Lifestyle lifestyle)
            {
                return StringId == lifestyle.StringId;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Lifestyle CreateLifestyle(Lifestyle lf)
        {
            var lifestyle = new Lifestyle(lf.StringId);
            lifestyle.Initialize(lf.Name, lf.Description, lf.FirstSkill, lf.SecondSkill, new List<PerkObject>(lf.Perks), lf.PassiveEffects, lf.FirstEffect, lf.SecondEffect, lf.Culture);
            return lifestyle;
        }

        public void Initialize(TextObject name, TextObject description, SkillObject firstSkill, SkillObject secondSkill, List<PerkObject> perks, TextObject effects, float firstEffect, float secondEffect, CultureObject culture = null)
        {
            Initialize(name, description);
            FirstSkill = firstSkill;
            SecondSkill = secondSkill;
            this.perks = perks;
            this.effects = effects;
            FirstEffect = firstEffect;
            SecondEffect = secondEffect;
            Culture = culture;
        }

        public bool CanLearn(Hero hero)
        {
            return (Culture == null || hero.Culture == Culture) && hero.GetSkillValue(FirstSkill) >= 15
                                                                && hero.GetSkillValue(SecondSkill) >= 15;
        }

        public bool CanInvestFocus(Hero hero)
        {
            return Progress >= 1f && perks.Count >= InvestedFocus + 1 &&
                   hero.GetSkillValue(FirstSkill) + hero.GetSkillValue(SecondSkill) >= NecessarySkillForFocus;
        }

        public void InvestFocus(EducationData data, Hero hero, bool cheat = false)
        {
            if (!cheat)
            {
                hero.HeroDeveloper.UnspentFocusPoints -= 1;
            }

            var perk = perks[InvestedFocus];
            data.AddPerk(perk);
            InvestedFocus += 1;
            Progress = 0f;
            if (hero == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=jAn2zxooy}You have received the {PERK} perk from the {LIFESTYLE} lifestyle.")
                        .SetTextVariable("PERK", perk.Name)
                        .SetTextVariable("LIFESTYLE", Name));
            }
        }

        public void AddProgress(float progress)
        {
            Progress = MBMath.ClampFloat(Progress + progress, 0f, 1f);
        }
    }
}