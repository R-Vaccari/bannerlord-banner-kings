using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions
{
    public abstract class LandedInstitution
    {
        protected Settlement settlement;

        public LandedInstitution(Settlement settlement)
        {
            this.settlement = settlement;
        }

        public abstract void Destroy();

        public Settlement Settlement => settlement;
    }
}
