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
        public CustomTroopPreset SkirmisherLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset ArcherLevy { get; } = new CustomTroopPreset("archer_levy");
        public CustomTroopPreset CrossbowmanLevy { get; } = new CustomTroopPreset("crossbowman_levy");
        public CustomTroopPreset HorsemanLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset HorseArcherLevy { get; } = new CustomTroopPreset("line_breaker_levy");
        public CustomTroopPreset MountedSkirmisherLevy { get; } = new CustomTroopPreset("line_breaker_levy");


        public CustomTroopPreset SargeantProfessional { get; } = new CustomTroopPreset("sargeant_professional");
        public CustomTroopPreset LineBreakerProfessional { get; } = new CustomTroopPreset("line_breaker_professional");
        public CustomTroopPreset DefenderProfessional { get; } = new CustomTroopPreset("line_breaker_professional");
        public CustomTroopPreset SkirmisherProfessional { get; } = new CustomTroopPreset("line_breaker_professional");
        public CustomTroopPreset ArcherProfessional { get; } = new CustomTroopPreset("archer_professional");
        public CustomTroopPreset CrossbowmanProfessional { get; } = new CustomTroopPreset("crossbowman_professional");
        public CustomTroopPreset HorsemanProfessional { get; } = new CustomTroopPreset("line_breaker_professional");
        public CustomTroopPreset HorseArcherProfessional { get; } = new CustomTroopPreset("line_breaker_professional");
        public CustomTroopPreset MountedSkirmisherProfessional { get; } = new CustomTroopPreset("line_breaker_professional");
        public override IEnumerable<CustomTroopPreset> All
        {
            get
            {
                yield return SargeantLevy;
                yield return LineBreakerLevy;
                yield return DefenderLevy;
                yield return SkirmisherLevy;
                yield return ArcherLevy;
                yield return CrossbowmanLevy;
                yield return HorsemanLevy;
                yield return HorseArcherLevy;
                yield return MountedSkirmisherLevy;
            }
        }

        public IEnumerable<CustomTroopPreset> Levies => All.ToList().FindAll(x => x.Level == 16);

        public IEnumerable<CustomTroopPreset> GetAdequatePresets(int level)
        {

        }
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
                new TextObject("{=!}Archers have strong ranged skills focused on Bows, while maintaining decent speed and ability to defend themselves in close quarters."),
                16,
                70,
                10,
                50,
                10,
                90,
                10,
                110,
                50,
                "lowland_longbow");

            CrossbowmanLevy.Initialize(new TextObject("{=!}Crossbowman"),
                new TextObject("{=!}Crossbowmen have strong ranged skills focused on Crossbows, while maintaining decent speed and ability to defend themselves in close quarters."),
                16,
                70,
                10,
                50,
                10,
                90,
                10,
                50,
                110,
                "crossbow_b");

            HorsemanLevy.Initialize(new TextObject("{=!}Horseman"),
                new TextObject("{=!}Quite similar to the Sargeant, but mounted. Horseman focuses on polearm and riding trainning, with a good amount of one-handed for melee combat. Slowest cavalry, but with the strongest melee."),
                16,
                100,
                20,
                120,
                90,
                20,
                20,
                10,
                10,
                "empire_horse");

            HorseArcherLevy.Initialize(new TextObject("{=ugJfuabA}Horse Archer"),
                new TextObject("{=!}Ranged cavalry, mainly focused on Bows, but also capable with Crossbows. Very limited melee capacities."),
                16,
                50,
                10,
                30,
                100,
                20,
                20,
                110,
                70,
                "khuzait_horse");

            MountedSkirmisherLevy.Initialize(new TextObject("{=!}Mounted Skirmisher"),
                new TextObject("{=!}The nimblest form of cavalry, made for hit-and-run combat with javelins. Sub-par melee capabilities."),
                16,
                70,
                10,
                50,
                120,
                20,
                120,
                10,
                10,
                "camel");
        }
    }
}
