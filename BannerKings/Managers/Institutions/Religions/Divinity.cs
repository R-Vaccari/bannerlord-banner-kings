using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity
    {
        private TextObject name;
        private TextObject description;
        private TextObject effects;

        public Divinity(TextObject name, TextObject description, TextObject effects)
        {
            this.name = name;
            this.description = description;
            this.effects = effects;
        }

        public TextObject Name => name;
        public TextObject Description => description;
        public TextObject Effects => effects;
    }
}
