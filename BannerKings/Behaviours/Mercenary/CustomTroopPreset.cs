using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class CustomTroopPreset : BannerKingsObject
    {
        public CustomTroopPreset(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, int level, int oneHanded, int twoHanded,
            int polearm, int riding, int athletics, int throwing, int bow, int crossbow, string itemId)
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
            ItemId = itemId;
        }

        public void PostInitialize()
        {
            var copy = DefaultCustomTroopPresets.Instance.GetById(StringId);
            Initialize(copy.Name, copy.Description, copy.Level, copy.OneHanded, copy.TwoHanded, copy.Polearm,
                copy.Riding, copy.Athletics, copy.Throwing, copy.Bow, copy.Crossbow, copy.ItemId);
        }

        public TextObject GetExplanation() => new TextObject("{=!}{DESCRIPTION}\n\nOne-Handed: {1H}\nTwo-Handed: {2H}")
            .SetTextVariable("DESCRIPTION", Description)
            .SetTextVariable("1H", OneHanded)
            .SetTextVariable("2h", TwoHanded);

        public int Level { get; private set; }
        public int OneHanded { get; private set; }
        public int TwoHanded { get; private set; }
        public int Polearm { get; private set; }
        public int Riding { get; private set; }
        public int Athletics { get; private set; }
        public int Throwing { get; private set; }
        public int Bow { get; private set; }
        public int Crossbow { get; private set; }
        public string ItemId { get; private set; }
    }
}
