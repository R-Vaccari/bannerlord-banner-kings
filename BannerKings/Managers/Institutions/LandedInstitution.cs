using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Managers.Institutions
{
    public abstract class LandedInstitution : Institution
    {
        protected Settlement settlement;

        public LandedInstitution(string id) : base(id)
        {
        }

        public Settlement Settlement => settlement;
    }
}