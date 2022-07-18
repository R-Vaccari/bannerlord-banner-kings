using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity
    {
        [SaveableField(1)]
        private string id;

        private TextObject name;
        private TextObject description;
        private TextObject effects;
        private TextObject secondaryTitle;
        private int blessingCost;
        public Divinity(string id, TextObject name, TextObject description, TextObject effects, TextObject secondaryTitle = null, int blessingCost = 500)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.effects = effects;
            this.secondaryTitle = secondaryTitle != null ? secondaryTitle : new TextObject();
            this.blessingCost = blessingCost;
        }

        public int BlessingCost => blessingCost; 

        public TextObject Name => name;
        public TextObject Description => description;
        public TextObject Effects => effects;
        public TextObject SecondaryTitle => secondaryTitle;
    }
}
