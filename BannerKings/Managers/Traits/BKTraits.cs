using System;
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
        public override IEnumerable<TraitObject> All => throw new NotImplementedException();

        public override void Initialize()
        {
            Zealous = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealot"));
            Zealous.Initialize(new TextObject("{=}Zealous"), 
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."), 
                false, -2, 2);

            Zealous = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealot"));
            Zealous.Initialize(new TextObject("{=}Zealous"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Zealous = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealot"));
            Zealous.Initialize(new TextObject("{=}Zealous"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Zealous = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealot"));
            Zealous.Initialize(new TextObject("{=}Zealous"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Zealous = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealot"));
            Zealous.Initialize(new TextObject("{=}Zealous"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);

            Zealous = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealot"));
            Zealous.Initialize(new TextObject("{=}Zealous"),
                new TextObject("{=!}Zealotry represents one's religious fervour. Zealous people will care to follow their faith's teachings and support it's expansion."),
                false, -2, 2);
        }
    }
}
