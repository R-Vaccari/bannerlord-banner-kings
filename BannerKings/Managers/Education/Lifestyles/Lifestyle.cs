using BannerKings.Managers.Innovations;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class Lifestyle : BannerKingsObject
    {
        private EducationData data;
        private TextObject effects;

        private List<PerkObject> perks;

        public Lifestyle(string id) : base(id)
        {
        }

        public float NecessarySkillForFocus => 80f * (InvestedFocus + 1f);


        public CultureObject Culture { get; private set; }

        public TextObject PassiveEffects => effects.SetTextVariable("EFFECT1", FirstEffect).SetTextVariable("EFFECT2", SecondEffect);

        public float FirstEffect { get; private set; }

        public float SecondEffect { get; private set; }

        public SkillObject FirstSkill { get; private set; }

        public SkillObject SecondSkill { get; private set; }

        public MBReadOnlyList<PerkObject> Perks => new MBReadOnlyList<PerkObject>(perks);

        public override bool Equals(object obj)
        {
            if (obj is Lifestyle lifestyle)
            {
                return StringId == lifestyle.StringId;
            }

            return base.Equals(obj);
        }

        public static Lifestyle CreateLifestyle(Lifestyle lf, EducationData data)
        {
            var lifestyle = new Lifestyle(lf.StringId);
            lifestyle.Initialize(lf.Name, lf.Description, lf.FirstSkill, lf.SecondSkill, new List<PerkObject>(lf.Perks), 
                lf.PassiveEffects, lf.FirstEffect, lf.SecondEffect, data, lf.Culture);
            return lifestyle;
        }

        public void Initialize(TextObject name, TextObject description, SkillObject firstSkill, SkillObject secondSkill, 
            List<PerkObject> perks, TextObject effects, float firstEffect, float secondEffect,
            EducationData data = null, CultureObject culture = null)
        {
            Initialize(name, description);
            FirstSkill = firstSkill;
            SecondSkill = secondSkill;
            this.perks = perks;
            this.effects = effects;
            FirstEffect = firstEffect;
            SecondEffect = secondEffect;
            Culture = culture;
            this.data = data;
        }

        public int InvestedFocus
        {
            get
            {
                int result = 0;
                foreach (var perk in perks)
                {
                    if (data.HasPerk(perk))
                    {
                        result++;
                    }
                }

                return result;
            }
        }

        public bool CanLearn(Hero hero)
        {
            return (Culture == null || hero.Culture == Culture) && hero.GetSkillValue(FirstSkill) >= 15 && hero.GetSkillValue(SecondSkill) >= 15;
        }

        public bool CanInvestFocus(Hero hero)
        {
            return data.LifestyleProgress >= 1f && perks.Count >= InvestedFocus + 1 && hero.GetSkillValue(FirstSkill) + hero.GetSkillValue(SecondSkill) >= NecessarySkillForFocus;
        }

        public void InvestFocus(EducationData data, Hero hero, bool cheat = false)
        {
            if (!cheat)
            {
                hero.HeroDeveloper.UnspentFocusPoints -= 1;
            }

            var perk = perks[InvestedFocus];
            data.AddPerk(perk);
            data.ResetProgress();
            if (hero == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=xtrVWoWG}You have received the {PERK} perk from the {LIFESTYLE} lifestyle.")
                        .SetTextVariable("PERK", perk.Name)
                        .SetTextVariable("LIFESTYLE", Name));
            }
        }
    }
}