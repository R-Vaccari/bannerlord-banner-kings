using System;
using System.Collections.Generic;
using System.Linq;
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
        public override IEnumerable<CustomTroopPreset> All
        {
            get
            {
                yield return SargeantLevy;
                yield return LineBreakerLevy;
                yield return DefenderLevy;
                yield return Skirmisher;
                yield return ArcherLevy;
                yield return CrossbowmanLevy;
                yield return HorsemanLevy;
                yield return HorseArcherLevy;
                yield return MountedSkirmisherLevy;
            }
        }

        public IEnumerable<CustomTroopPreset> Levies => All.ToList().FindAll(x => x.Level == 16);

        public override void Initialize()
        {
            SargeantLevy.Initialize(new TextObject("{=!}Sargeant"),
                new TextObject("{=!}The backbone of infantry armies, the sargeant specializes in one-handed combat and foot movement. Handles well an extra polearm but poorly any type of ranged weapons. Better with a shield."),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10,
                "empire_sword_2_t3");

            LineBreakerLevy.Initialize(new TextObject("{=!}Line Breaker"),
                new TextObject("{=!}In opposition to the sargeant, line breakers focus on two-handed combat and thus are better off without shields. Their role is maximum damage output, but are vulnerable to cavalry or ranged attacks. Handles well secondary throwing weapons."),
                16,
                40,
                100,
                40,
                10,
                120,
                70,
                10,
                10,
                "bearded_axe_t3");

            DefenderLevy.Initialize(new TextObject("{=!}Defender"),
                new TextObject("{=!}The defender is a slow moving infantry, that focuses on polearm combat, but also capable with one-handed and throwing. They are best equipped with a good shield."),
                16,
                70,
                40,
                120,
                10,
                60,
                80,
                10,
                10,
                "western_spear_4_t3");

            Skirmisher.Initialize(new TextObject("{=s9oED1IR}Skirmisher"),
                new TextObject("{=!}Skirmishers specialize in moving fast and hitting hard with thrown weapons. Their close-combat skills are just enough to give them a chance to defend themselves if needed."),
                16,
                60,
                20,
                60,
                10,
                120,
                120,
                10,
                10,
                "western_javelin_2_t3");

            ArcherLevy.Initialize(new TextObject("{=!}Archer"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10,
                "lowland_longbow");

            CrossbowmanLevy.Initialize(new TextObject("{=!}Crossbowman"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10,
                "crossbow_b");

            HorsemanLevy.Initialize(new TextObject("{=!}Horseman"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10,
                "empire_horse");

            HorseArcherLevy.Initialize(new TextObject("{=ugJfuabA}Horse Archer"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10,
                "khuzait_horse");

            MountedSkirmisherLevy.Initialize(new TextObject("{=!}Mounted Skirmisher"),
                new TextObject("{=!}"),
                16,
                120,
                40,
                80,
                10,
                100,
                30,
                10,
                10,
                "camel");
        }
    }
}
