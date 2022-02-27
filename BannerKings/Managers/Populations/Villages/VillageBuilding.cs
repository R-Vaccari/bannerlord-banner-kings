using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Populations.Villages
{
    public class VillageBuilding : Building
    {

        private Village village;

        public VillageBuilding(BuildingType buildingType, Town town, Village village) : base(buildingType, town)
        {
            this.village = village;
        }

        public Village Village => this.village;
    }
}
