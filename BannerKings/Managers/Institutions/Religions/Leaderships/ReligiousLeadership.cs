using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class ReligiousLeadership
    {
        private Religion religion;
        public abstract void Initialize(Religion religion);

        public abstract TextObject GetName();

        public abstract TextObject GetHint();
    }
}
