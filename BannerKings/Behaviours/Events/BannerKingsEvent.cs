using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Events
{
    public abstract class BannerKingsEvent : BannerKingsObject
    {
        public BannerKingsEvent(string id) : base(id)
        {
        }

        public Hero Hero { get; protected set; }

        public abstract BannerKingsEvent GetCopy(Hero hero);
        protected abstract void SetTexts();
        public abstract TextObject Name { get; }
        public TextObject Description { get; protected set; }

        protected abstract void SetUp();
        public abstract void ShowPlayerPrompt();
        public abstract IEnumerable<EventResolution> Resolutions { get; }

        public abstract bool IsPossible(Hero hero);

        public void AiResolve()
        {
            List<ValueTuple<EventResolution, float>> options = new List<(EventResolution, float)>();
            foreach (var resolution in Resolutions)
            {
                float score = resolution.CalculateAiLikelihood(Hero);
                options.Add(new(resolution, score));
            }

            EventResolution result = MBRandom.ChooseWeighted(options);
            if (result == null)
            {
                result = Resolutions.GetRandomElementInefficiently();
            }

            Fulfill(result);
        }

        public void Fulfill(EventResolution resolution)
        {
            resolution.Fulfill(Hero);
            if (resolution.Trait != null && MBRandom.RandomFloat < resolution.TraitChance)
            {
                Hero.SetTraitLevel(resolution.Trait, Hero.GetTraitLevel(resolution.Trait) + resolution.TraitValue);
            }

            if (resolution.Skill != null) 
            {
                Hero.AddSkillXp(resolution.Skill, resolution.SKillXp);
            }
        }

        public class EventResolution
        {
            private Action<Hero> fulfill;
            private Func<Hero, bool> isAdequate;
            private Func<Hero, float> calculateAiLikelihood;

            public EventResolution(TextObject name, TextObject description, TraitObject gainedTrait, int traitValue, float traitChance,
                SkillObject skill, int skillXp,
                Func<Hero, bool> isAdequate, Func<Hero, float> calculateAiLikelihood, Action<Hero> fulfill)
            {
                Name = name;
                Description = description;
                Skill = skill;
                SKillXp = skillXp;
                Trait = gainedTrait;
                TraitValue = traitValue;
                TraitChance = traitChance;
                this.fulfill = fulfill;
                this.calculateAiLikelihood = calculateAiLikelihood;
                this.fulfill = fulfill;
            }
            public TextObject Name { get; private set; }
            public TextObject Description { get; private set; }
            public TraitObject Trait { get; private set; }
            public int TraitValue { get; private set; }
            public float TraitChance { get; private set; }
            public SkillObject Skill { get; private set; }
            public int SKillXp { get; private set; }

            public float CalculateAiLikelihood(Hero fulfiller) => calculateAiLikelihood(fulfiller);
            public bool IsAdequate(Hero fulfiller) => isAdequate(fulfiller);
            public void Fulfill(Hero fulfiller) => fulfill(fulfiller);
        }
    }
}
