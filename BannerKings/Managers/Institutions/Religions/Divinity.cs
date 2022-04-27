using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity
    {
        private TextObject name;
        private TextObject description;
        
        public Divinity(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
        }

        public TextObject Name => this.name;
        public TextObject Description => this.description;
    }
}
