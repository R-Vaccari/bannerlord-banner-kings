using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings
{
    public abstract class BannerKingsObject : MBObjectBase
    {
        protected TextObject name;
        protected TextObject description;

        public BannerKingsObject(string id) : base(id)
        {
        }

        protected void Initialize(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
        }
        public TextObject Name => name;
        public TextObject Description => description;
    }
}
