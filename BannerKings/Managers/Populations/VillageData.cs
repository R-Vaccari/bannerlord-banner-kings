using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations
{
    public class VillageData : BannerKingsData
    {
        public VillageData(Village village)
        {
            this.village = village;
            buildings = new List<VillageBuilding>();
            foreach (var type in DefaultVillageBuildings.VillageBuildings(village))
            {
                buildings.Add(new VillageBuilding(type, village.Bound.Town, village));
            }

            inProgress = new Queue<Building>();
        }

        [SaveableProperty(1)] private Village village { get; set; }

        [SaveableProperty(2)] private List<VillageBuilding> buildings { get; set; }

        [SaveableProperty(5)] private Queue<Building> inProgress { get; set; }

        public Village Village => village;

        public List<VillageBuilding> Buildings => buildings;

        public ExplainedNumber ProductionsExplained => BannerKingsConfig.Instance.VillageProductionModel.CalculateProductionsExplained(village);

        public VillageBuilding CurrentBuilding
        {
            get
            {
                VillageBuilding building = null;

                if (inProgress != null && !inProgress.IsEmpty())
                {
                    building = (VillageBuilding?) inProgress.Peek();
                }

                return building ?? CurrentDefault;
            }
        }

        public VillageBuilding CurrentDefault
        {
            get
            {
                var building = buildings.FirstOrDefault(x => x.IsCurrentlyDefault);
                if (building == null)
                {
                    var dailyProd =
                        buildings.FirstOrDefault(x => x.BuildingType.StringId == "bannerkings_daily_production");
                    dailyProd.IsCurrentlyDefault = true;
                    building = dailyProd;
                }

                return building;
            }
        }

        public Queue<Building> BuildingsInProgress
        {
            get => inProgress;
            set => inProgress = value;
        }

        public bool IsCurrentlyBuilding => BuildingsInProgress.Any();

        public float Construction =>
            new BKConstructionModel().CalculateVillageConstruction(village.Settlement).ResultNumber;

        public void StartRandomProject()
        {
            if (inProgress.IsEmpty())
            {
                inProgress.Enqueue(buildings.GetRandomElementWithPredicate(x =>
                    x.BuildingType.BuildingLocation != BuildingLocation.Daily));
            }
        }

        public int GetBuildingLevel(BuildingType type)
        {
            Building building = buildings.FirstOrDefault(x => x.BuildingType == type);
            if (building != null)
            {
                return building.CurrentLevel;
            }

            return 0;
        }

        public void ReInitializeBuildings()
        {
            foreach (var building in buildings)
            {
                building.PostInitialize();
            }

            foreach (VillageBuilding building in inProgress)
            {
                building.PostInitialize();
            }
        }

        internal override void Update(PopulationData data)
        {

            foreach (var type in DefaultVillageBuildings.VillageBuildings(village))
            {
                if (buildings.FirstOrDefault(x => x.BuildingType.StringId == type.StringId) == null)
                {
                    buildings.Add(new VillageBuilding(type, village.Bound.Town, village));
                }
            }

            var current = CurrentBuilding;
            if (current != null && BuildingsInProgress.Any())
            {
                if (BuildingsInProgress.Peek().BuildingType.StringId == current.BuildingType.StringId)
                {
                    current.BuildingProgress += Construction;
                    if (current.GetConstructionCost() <= current.BuildingProgress)
                    {
                        if (current.CurrentLevel < 3)
                        {
                            current.LevelUp();
                        }

                        if (current.CurrentLevel == 3)
                        {
                            current.BuildingProgress = current.GetConstructionCost();
                        }

                        BuildingsInProgress.Dequeue();
                    }
                }
            }
        }
    }
}