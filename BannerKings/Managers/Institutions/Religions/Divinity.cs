using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity
    {
        private TextObject name;
        private TextObject description;
        private TextObject effects;
        private TextObject secondaryTitle;
        public Divinity(TextObject name, TextObject description, TextObject effects, TextObject secondaryTitle = null)
        {
            this.name = name;
            this.description = description;
            this.effects = effects;
            this.secondaryTitle = secondaryTitle != null ? secondaryTitle : new TextObject();
        }

        public TextObject Name => name;
        public TextObject Description => description;
        public TextObject Effects => effects;
        public TextObject SecondaryTitle => secondaryTitle;
    }
}
