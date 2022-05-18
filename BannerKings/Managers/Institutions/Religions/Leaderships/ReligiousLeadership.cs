using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class ReligiousLeadership
    {
        [SaveableField(1)]
        protected Religion religion;
        public abstract void Initialize(Religion religion);

        public abstract TextObject GetName();

        public abstract TextObject GetHint();
    }
}
