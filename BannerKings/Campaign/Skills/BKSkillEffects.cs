using BannerKings.Managers.Skills;
using BannerKings.Settings;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Campaign.Skills
{
    public class BKSkillEffects : DefaultTypeInitializer<BKSkillEffects, SkillEffect>
    {
        public SkillEffect PietyGain { get; set; }
        public SkillEffect FaithPresence { get; set; }
        public SkillEffect LanguageSpeed { get; set; }
        public SkillEffect ReadingSpeed { get; set; }
        public SkillEffect LifestyleSpeed { get; set; }
        public SkillEffect ResearchSpeed { get; set; }
        public SkillEffect DemesneLimit { get; set; }
        public SkillEffect VassalLimit { get; set; }
        public SkillEffect Legitimacy { get; set; }
        public SkillEffect Stability { get; set; }
        public SkillEffect SpouseScore { get; set; }
        public SkillEffect TradePower { get; set; }
        public SkillEffect ProductionQuality { get; set; }
        public SkillEffect ProductionEfficiency { get; set; }
        public SkillEffect SupplyEfficiency { get; set; }

        public override IEnumerable<SkillEffect> All => throw new NotImplementedException();

        public override void Initialize()
        {
            PietyGain = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("PietyGain"));
            PietyGain.Initialize(new TextObject("{=3MDmvuVf}Daily piety gain: +{a0}"),
                new SkillObject[]
                {
                    BKSkills.Instance.Theology
                },
                SkillEffect.PerkRole.Personal,
                0.01f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.Add,
                0f,
                0f);

            FaithPresence = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("FaithPresence"));
            FaithPresence.Initialize(new TextObject("{=vTyRD6cM}Faith presence in fiefs: +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Theology
                },
                SkillEffect.PerkRole.Governor,
                0.1f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            SpouseScore = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("SpouseScore"));
            SpouseScore.Initialize(new TextObject("{=Jh0vPbET}Spouse score improvement (half for other clan members): +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Lordship
                },
                SkillEffect.PerkRole.Personal,
                0.1f,
                SkillEffect.PerkRole.ClanLeader,
                0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            Legitimacy = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("Legitimacy"));
            Legitimacy.Initialize(new TextObject("{=Ojp1qZdC}Legitimacy (as ruler): +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Lordship
                },
                SkillEffect.PerkRole.Personal,
                0.1f,
                SkillEffect.PerkRole.None,
                0.0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            DemesneLimit = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("DemesneLimit"));
            DemesneLimit.Initialize(new TextObject("{=yEbrBMJC}Demesne limit: +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Lordship
                },
                SkillEffect.PerkRole.ClanLeader,
                0.1f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            VassalLimit = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("VassalLimit"));
            VassalLimit.Initialize(new TextObject("{=UkiSUHE6}Vassal limit: +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Lordship
                },
                SkillEffect.PerkRole.ClanLeader,
                0.1f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);


            LanguageSpeed = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("LanguageSpeed"));
            LanguageSpeed.Initialize(new TextObject("{=7oP8Hj7c}Language learning speed: +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Scholarship
                },
                SkillEffect.PerkRole.Personal,
                0.15f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            ReadingSpeed = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("ReadingSpeed"));
            ReadingSpeed.Initialize(new TextObject("{=GuYLFezW}Book reading speed: +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Scholarship
                },
                SkillEffect.PerkRole.Personal,
                0.2f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            LifestyleSpeed = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("LifestyleSpeed"));
            LifestyleSpeed.Initialize(new TextObject("{=zwd8fwK7}Lifestyle progress speed: +{a0}%"),
                new SkillObject[]
                {
                    BKSkills.Instance.Scholarship
                },
                SkillEffect.PerkRole.Personal,
                0.15f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            ResearchSpeed = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("ResearchSpeed"));
            ResearchSpeed.Initialize(new TextObject("{=Zyao3x8F}Personal research progress: +{a0}"),
                new SkillObject[]
                {
                    BKSkills.Instance.Scholarship
                },
                SkillEffect.PerkRole.Personal,
                0.0333f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.Add,
                0f,
                0f);
        }

        public void AddVanilla()
        {
            ProductionEfficiency = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("ProductionEfficiency"));
            ProductionEfficiency.Initialize(new TextObject("{=ft4CKf5O}Fief production efficiency: +{a0}%"),
                new SkillObject[]
                {
                    DefaultSkills.Crafting
                },
                SkillEffect.PerkRole.Governor,
                0.15f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);

            ProductionQuality = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("ProductionQuality"));
            ProductionQuality.Initialize(new TextObject("{=H8jSy770}Fief production quality: +{a0}%"),
                new SkillObject[]
                {
                    DefaultSkills.Crafting
                },
                SkillEffect.PerkRole.Governor,
                0.085f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);


            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardSkills && BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
            {
                SupplyEfficiency = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("SupplyEfficiency"));
                SupplyEfficiency.Initialize(new TextObject("{=wMcipFaM}Party supply efficiency: +{a0}%"),
                    new SkillObject[]
                    {
                    DefaultSkills.Steward
                    },
                    SkillEffect.PerkRole.Quartermaster,
                    0.15f,
                    SkillEffect.PerkRole.PartyMember,
                    0.015f,
                    SkillEffect.EffectIncrementType.AddFactor,
                    0f,
                    0f);

                Stability = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("Stability"));
                Stability.Initialize(new TextObject("{=dSjTJUjU}Fief stability: +{a0}"),
                    new SkillObject[]
                    {
                    DefaultSkills.Steward
                    },
                    SkillEffect.PerkRole.Governor,
                    0.001f,
                    SkillEffect.PerkRole.ClanLeader,
                    0.0001f,
                    SkillEffect.EffectIncrementType.Add,
                    0f,
                    0f);
            }
            else
            {
                SupplyEfficiency = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("SupplyEfficiency"));
                SupplyEfficiency.Initialize(new TextObject("{=wMcipFaM}Party supply efficiency: +{a0}%"),
                    new SkillObject[]
                    {
                    DefaultSkills.Steward
                    },
                    SkillEffect.PerkRole.Quartermaster,
                    0.15f,
                    SkillEffect.PerkRole.None,
                    0f,
                    SkillEffect.EffectIncrementType.AddFactor,
                    0f,
                    0f);

                Stability = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("Stability"));
                Stability.Initialize(new TextObject("{=dSjTJUjU}Fief stability: +{a0}"),
                    new SkillObject[]
                    {
                    DefaultSkills.Steward
                    },
                    SkillEffect.PerkRole.Governor,
                    0.001f,
                    SkillEffect.PerkRole.None,
                    0f,
                    SkillEffect.EffectIncrementType.Add,
                    0f,
                    0f);

            }
            TradePower = Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect("TradePower"));
            TradePower.Initialize(new TextObject("{=vSqWjxNU}Fief trade power: +{a0}%"),
                new SkillObject[]
                {
                    DefaultSkills.Trade
                },
                SkillEffect.PerkRole.Governor,
                0.12f,
                SkillEffect.PerkRole.None,
                0f,
                SkillEffect.EffectIncrementType.AddFactor,
                0f,
                0f);
        }
    }
}
