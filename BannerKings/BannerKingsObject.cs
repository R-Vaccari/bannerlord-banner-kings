using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings
{
    public abstract class BannerKingsObject : MBObjectBase
    {
        protected TextObject description;
        protected TextObject name;

        protected BannerKingsObject(string id) : base(id)
        {
        }

        public TextObject Name => name;
        public TextObject Description => description;

        protected void Initialize(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
        }

        public override bool Equals(object obj)
        {
            if (obj is BannerKingsObject kingsObject)
            {
                return kingsObject.StringId == StringId;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}