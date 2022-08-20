using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Managers.Institutions
{
    public abstract class LandedInstitution : Institution
    {
        protected Settlement settlement;

        public LandedInstitution(Settlement settlement)
        {
            this.settlement = settlement;
        }

        public Settlement Settlement => settlement;
    }
}