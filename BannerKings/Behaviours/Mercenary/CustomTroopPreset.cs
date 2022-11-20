using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class CustomTroopPreset : BannerKingsObject
    {
        public CustomTroopPreset(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, int level, int oneHanded, int twoHanded,
            int polearm, int riding, int athletics, int throwing, int bow, int crossbow)
        {
            Initialize(name, description);
            Level = level;
            OneHanded = oneHanded;
            TwoHanded = twoHanded;
            Polearm = polearm;
            Riding = riding;
            Athletics = athletics;
            Throwing = throwing;
            Bow = bow;
            Crossbow = crossbow;
        }

        public int Level { get; private set; }
        public int OneHanded { get; private set; }
        public int TwoHanded { get; private set; }
        public int Polearm { get; private set; }
        public int Riding { get; private set; }
        public int Athletics { get; private set; }
        public int Throwing { get; private set; }
        public int Bow { get; private set; }
        public int Crossbow { get; private set; }
    }
}
