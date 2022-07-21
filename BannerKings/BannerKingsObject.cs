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

        public override bool Equals(object obj)
        {
            if (obj is BannerKingsObject) return ((BannerKingsObject) obj).StringId == StringId;
            return base.Equals(obj);
        }
    }
}
