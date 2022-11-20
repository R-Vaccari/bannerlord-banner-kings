using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class DefaultCustomTroopPresets : DefaultTypeInitializer<DefaultCustomTroopPresets, CustomTroopPreset>
    {

        public CustomTroopPreset SargeantLevy { get; } = new CustomTroopPreset("sargeant_levy");
        public CustomTroopPreset LineBreakerLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset DefenderLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset Skirmisher { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset ArcherLevy { get; } = new CustomTroopPreset("archer_levy");
        public CustomTroopPreset CrossbowmanLevy { get; } = new CustomTroopPreset("crossbowman_levy");
        public CustomTroopPreset HorsemanLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset HorseArcherLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset MountedSkirmisherLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public override IEnumerable<CustomTroopPreset> All => throw new NotImplementedException();

        public override void Initialize()
        {
            SargeantLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            LineBreakerLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            DefenderLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            Skirmisher.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            ArcherLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            CrossbowmanLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            HorsemanLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            HorseArcherLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);

            MountedSkirmisherLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10);
        }
    }
}
