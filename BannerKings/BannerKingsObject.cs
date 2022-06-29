using TaleWorlds.Localization;

namespace BannerKings
{
    public abstract class BannerKingsObject
    {
        private string id;
        protected TextObject name;
        protected TextObject description;

        public BannerKingsObject(string id, TextObject name, TextObject description)
        {
            this.id = id;
        }

        public string Id => id;
        public TextObject Name => name;
        public TextObject Description => description;
    }
}
