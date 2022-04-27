using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class ReligiousLeadership
    {
        public abstract void Initialize(Religion religion);
        public abstract Hero GetLeader();
        public abstract void DecideNewLeader();
    }
}
