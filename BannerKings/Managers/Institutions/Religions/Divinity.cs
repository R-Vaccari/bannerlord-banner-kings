using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity
    {
        [SaveableField(1)]
        private TextObject name;

        [SaveableField(2)]
        private TextObject description;

        [SaveableField(3)]
        private TextObject effects;

        [SaveableField(4)]
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
