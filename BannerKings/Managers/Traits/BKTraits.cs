using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Traits
{
    public class BKTraits : DefaultTypeInitializer<BKTraits, TraitObject>
    {
        public TraitObject Zealous { get; private set; }
        public TraitObject Conservative { get; private set; }
        public TraitObject Ambitious { get; private set; }
        public TraitObject Forgiving { get; private set;  }
        public TraitObject Arrogant { get; private set; }
        public TraitObject Patient { get; private set; }
        public override IEnumerable<TraitObject> All
        {
            get
            {
                yield return Zealous;
                yield return Conservative;
                yield return Ambitious;
                yield return Forgiving;
                yield return Arrogant;
                yield return Patient;
            }
        }

        public override void Initialize()
        {
            Zealous = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealot"));
            Zealous.Initialize(new TextObject("{=}Zealous"), 
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."), 
                false, -2, 2);

            Conservative = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Conservative"));
            Conservative.Initialize(new TextObject("{=}Conservative"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Ambitious = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Ambitious"));
            Ambitious.Initialize(new TextObject("{=}Ambitious"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Forgiving = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Forgiving"));
            Forgiving.Initialize(new TextObject("{=}Forgiving"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Arrogant = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Arrogant"));
            Arrogant.Initialize(new TextObject("{=}Arrogant"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Patient = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Patient"));
            Patient.Initialize(new TextObject("{=}Patient"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);
        }
    }
}
