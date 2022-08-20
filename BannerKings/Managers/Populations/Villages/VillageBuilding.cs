using System;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations.Villages;

public class VillageBuilding : Building
{
    public VillageBuilding(BuildingType buildingType, Town town, Village village, float buildingProgress = 0f,
        int currentLevel = 0) : base(buildingType, town, buildingProgress, currentLevel)
    {
        this.village = village;
    }

    [SaveableProperty(1)] private Village village { get; }

    public Village Village => village;

    public void PostInitialize()
    {
        var type = DefaultVillageBuildings.Instance.GetById(BuildingType);
        BuildingType.Initialize(type.Name,
            type.Explanation,
            new[] {type.GetProductionCost(0), type.GetProductionCost(1), type.GetProductionCost(2)},
            type.BuildingLocation,
            new Tuple<BuildingEffectEnum, float, float, float>[] { },
            type.StartLevel);
    }
}