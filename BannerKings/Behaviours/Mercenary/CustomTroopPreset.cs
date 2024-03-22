using TaleWorlds.Core;
using TaleWorlds.Localization;
using static TaleWorlds.CampaignSystem.CharacterDevelopment.DefaultPerks;

namespace BannerKings.Behaviours.Mercenary
{
    internal class CustomTroopPreset : BannerKingsObject
    {
        public CustomTroopPreset(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, FormationClass formation,
            int level, int oneHanded, int twoHanded,
            int polearm, int riding, int athletics, int throwing, int bow, int crossbow, string itemId)
        {
            Initialize(name, description);
            Formation = formation;
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
            Initialize(copy.Name, copy.Description, copy.Formation, copy.Level, copy.OneHanded, copy.TwoHanded, copy.Polearm,
                copy.Riding, copy.Athletics, copy.Throwing, copy.Bow, copy.Crossbow, copy.ItemId);
        }

        public TextObject GetExplanation() => new TextObject("{=3azYg7UM}{DESCRIPTION}{newline}{newline}One-Handed: {ONEHANDED}{newline}Two-Handed: {TWOHANDED}{newline}Polearm: {POLEARM}{newline}Riding: {RIDING}{newline}Athletics: {ATHLETICS}{newline}Throwing: {THROWING}{newline}Bow: {BOW}{newline}Crossbow: {CROSSBOW}")
            .SetTextVariable("DESCRIPTION", Description)
            .SetTextVariable("ONEHANDED", OneHanded)
            .SetTextVariable("TWOHANDED", TwoHanded)
            .SetTextVariable("POLEARM", Polearm)
            .SetTextVariable("RIDING", Riding)
            .SetTextVariable("ATHLETICS", Athletics)
            .SetTextVariable("THROWING", Throwing)
            .SetTextVariable("BOW", Bow)
            .SetTextVariable("CROSSBOW", Crossbow);

        public FormationClass Formation { get; private set; }
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