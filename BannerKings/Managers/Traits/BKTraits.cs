using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Traits
{
    public class BKTraits : DefaultTypeInitializer<BKTraits, TraitObject>
    {
        public TraitObject PresetScholar { get; } = new TraitObject("PresetScholar");
        public TraitObject PresetDruid { get; } = new TraitObject("PresetDruid");

        public TraitObject Just { get; } = new TraitObject("Just");
        public TraitObject Humble { get; } = new TraitObject("Humble");
        public TraitObject Diligent { get; } = new TraitObject("Diligent");
        public TraitObject Seductive { get; } = new TraitObject("Seductive");
        public TraitObject Ambitious { get; } = new TraitObject("Ambitious");
        public TraitObject Zealous { get; } = new TraitObject("Zealous");

        public TraitObject AptitudeViolence { get; } = new TraitObject("AptitudeViolence");
        public TraitObject AptitudeErudition { get; } = new TraitObject("AptitudeErudition");
        public TraitObject AptitudeSocializing { get; } = new TraitObject("AptitudeSocializing");

        public TraitObject Musician { get; } = new TraitObject("Musician");

        public TraitObject Castrated { get; } = new TraitObject("Castrated");

        public override IEnumerable<TraitObject> All => throw new NotImplementedException();

        public IEnumerable<TraitObject> PresetTraits
        {
            get
            {
                yield return PresetScholar;
            }
        }

        public IEnumerable<TraitObject> PoliticalTraits
        {
            get
            {
                yield return DefaultTraits.Oligarchic;
                yield return DefaultTraits.Authoritarian;
                yield return DefaultTraits.Egalitarian;
            }
        }

        public IEnumerable<TraitObject> PersonalityTraits
        {
            get
            {
                yield return DefaultTraits.Honor;
                yield return DefaultTraits.Calculating;
                yield return DefaultTraits.Mercy;
                yield return DefaultTraits.Valor;
                yield return DefaultTraits.Generosity;
                yield return Just;
                yield return Humble;
                yield return Diligent;
                yield return Ambitious;
                yield return Zealous;
            }
        }

        public IEnumerable<TraitObject> AptitudeTraits
        {
            get
            {
                yield return AptitudeViolence;
                yield return AptitudeErudition;
                yield return AptitudeSocializing;
            }
        }

        public IEnumerable<TraitObject> LifestyleTraits 
        { 
            get
            {
                yield return Musician;
            } 
        }

        public IEnumerable<TraitObject> MedicalTraits
        {
            get
            {
                yield return Castrated;
            }
        }

        public override void Initialize()
        {
            Just.Initialize(new TextObject("{=!}Justice"),
                new TextObject("{=!}Justice is the appropriate punishment and reward for a given deed. Just rulers are often respected by their vassals for delivering appropriate sentences, but also adequately rewarding loyalty."),
                true,
                -2,
                2);

            Humble.Initialize(new TextObject("{=!}Humility"),
                new TextObject("{=!}Justice is the appropriate punishment and reward for a given deed. Just rulers are often respected by their vassals for delivering appropriate sentences, but also adequately rewarding loyalty."),
                true,
                -2,
                2);

            Diligent.Initialize(new TextObject("{=!}Diligence"),
                new TextObject("{=!}Justice is the appropriate punishment and reward for a given deed. Just rulers are often respected by their vassals for delivering appropriate sentences, but also adequately rewarding loyalty."),
                true,
                -2,
                2);

            Ambitious.Initialize(new TextObject("{=!}Ambition"),
                new TextObject("{=!}Justice is the appropriate punishment and reward for a given deed. Just rulers are often respected by their vassals for delivering appropriate sentences, but also adequately rewarding loyalty."),
                true,
                -2,
                2);

            Zealous.Initialize(new TextObject("{=!}Zealotry"),
                new TextObject("{=!}Justice is the appropriate punishment and reward for a given deed. Just rulers are often respected by their vassals for delivering appropriate sentences, but also adequately rewarding loyalty."),
                true,
                -2,
                2);

            AptitudeViolence.Initialize(new TextObject("{=!}Violence"),
                new TextObject("{=!}Aptitude for violence describes how inclined one is towards hurting others. Violent persons are more inclined to learn combat skills."),
                true,
                -2,
                2);

            AptitudeErudition.Initialize(new TextObject("{=!}Erudition"),
                new TextObject("{=!}Aptitude for erudition describes how inclined one is towards intellectual tasks. Erudite persons are more inclined to learn intellectual skills such as Medicine, Scholarship, and others."),
                true,
                -2,
                2);

            AptitudeSocializing.Initialize(new TextObject("{=!}Socializing"),
                new TextObject("{=!}Aptitude for socializing describes how apt one is in expressing themselves and resolving conflicts. Social persons are more inclined to learn Leadership, Charm and Trade skills."),
                true,
                -2,
                2);
        }
    }
}
