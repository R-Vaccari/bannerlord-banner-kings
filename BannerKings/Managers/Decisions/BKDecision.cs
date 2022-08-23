using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Decisions
{
    public abstract class BKDecision<T> where T : MBObjectBase
    {
        protected BKDecision(T referencedObject, bool enabled)
        {
            ReferencedObject = referencedObject;
            Enabled = enabled;
        }

        [SaveableProperty(1)] public T ReferencedObject { get; private set; }

        [SaveableProperty(2)] public bool Enabled { get; set; }

        public abstract string GetHint();
        public abstract string GetName();
        public abstract string GetIdentifier();

        public void OnChange(bool value)
        {
            Enabled = value;
        }
    }
}