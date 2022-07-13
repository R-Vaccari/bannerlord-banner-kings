using TaleWorlds.Localization;

namespace BannerKings
{
    public abstract class BannerKingsObject
    {
        private string id;
        protected TextObject name;
        protected TextObject description;

        public BannerKingsObject(string id)
        {
            this.id = id;
        }

        protected void Initialize(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
        }

        public string Id => id;
        public TextObject Name => name;
        public TextObject Description => description;
    }
}
