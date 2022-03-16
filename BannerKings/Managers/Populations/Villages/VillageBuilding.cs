using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations.Villages
{
    public class VillageBuilding : Building
    {
        [SaveableProperty(1)]
        private Village village { get; set; }

        public VillageBuilding(BuildingType buildingType, Town town, Village village) : base(buildingType, town)
        {
            this.village = village;
        }

        public Village Village => this.village;
    }
}
