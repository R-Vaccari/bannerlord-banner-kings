using TaleWorlds.Localization;

namespace BannerKings
{
    public abstract class BannerKingsObject
    {
        private TextObject name;
        private TextObject description;

        public BannerKingsObject(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
        }

        public TextObject Name => name;
        public TextObject Description => description;

        public abstract void Initialize();
    }
}
