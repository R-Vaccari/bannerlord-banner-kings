using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations.Villages
{
    public class VillageBuilding : Building
    {
        [SaveableProperty(1)]
        private Village village { get; set; }

        public VillageBuilding(BuildingType buildingType, Town town, Village village, float buildingProgress = 0f, int currentLevel = 0) : base(buildingType, town, buildingProgress, currentLevel)
        {
            this.village = village;
        }

        public void PostInitialize()
        {
            BuildingType type = DefaultVillageBuildings.Instance.GetById(BuildingType);
            BuildingType.Initialize(type.Name, 
                type.Explanation, 
                new int[] { type.GetProductionCost(0), type.GetProductionCost(1), type.GetProductionCost(2) }, 
                type.BuildingLocation, 
                new Tuple<BuildingEffectEnum, float, float, float>[] { },
                type.StartLevel);
        }

        public Village Village => village;
    }
}
