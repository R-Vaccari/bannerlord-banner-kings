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
        public TraitObject Patient { get; } = new TraitObject("Patient");
        public TraitObject Diligent { get; } = new TraitObject("Diligent");
        public TraitObject Seductive { get; } = new TraitObject("Seductive");
        public TraitObject Deceitful { get; } = new TraitObject("Deceitful");
        public TraitObject Ambitious { get; } = new TraitObject("Ambitious");
        public TraitObject Erudite { get; } = new TraitObject("Erudite");
        public TraitObject Zealous { get; } = new TraitObject("Zealous");

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

        public IEnumerable<TraitObject> PersonalityTraits
        {
            get
            {
                yield return Just;
                yield return Humble;
                yield return Patient;
                yield return Diligent;
                yield return Seductive;
                yield return Deceitful;
                yield return Ambitious;
                yield return Erudite;
                yield return Zealous;
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
            Just.Initialize(new TextObject("{=!}Just"),
                new TextObject("{=!}Justice is the appropriate punishment and reward for a given deed. Just rulers are often respected by their vassals for delivering appropriate sentences, but also adequately rewarding loyalty."),
                true,
                -2,
                2);
        }
    }
}
