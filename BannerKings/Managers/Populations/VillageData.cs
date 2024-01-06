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
            buildings = new List<Building>(10);
            foreach (var type in DefaultVillageBuildings.VillageBuildings(village))
            {
                buildings.Add(new Building(type, village.Bound.Town));
            }

            inProgress = new Queue<Building>();
            LastPayment = 0;
        }

        [SaveableProperty(1)] private Village village { get; set; }
        [SaveableProperty(2)] private List<Building> buildings { get; set; }
        [SaveableProperty(3)] public int LastPayment { get; set; }
        [SaveableProperty(5)] private Queue<Building> inProgress { get; set; }

        public Village Village => village;

        public List<Building> Buildings => buildings;

        public ExplainedNumber ProductionsExplained => BannerKingsConfig.Instance.VillageProductionModel.CalculateProductionsExplained(village);

        public Building CurrentBuilding
        {
            get
            {
                Building building = null;

                if (inProgress != null && !inProgress.IsEmpty())
                {
                    var peek = inProgress.Peek();
                    if (peek == null || !peek.BuildingType.IsInitialized) inProgress.Dequeue();
                    else building = peek;
                }

                return building ?? CurrentDefault;
            }
        }

        public Building CurrentDefault
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
            if (type == null)
            {
                return 0;
            }

            Building building = buildings.FirstOrDefault(x => x.BuildingType.StringId == type.StringId);
            if (building != null)
            {
                return building.CurrentLevel;
            }

            return 0;
        }

        public void ReInitializeBuildings()
        {
            if (buildings == null) buildings = new List<Building>(10);
            foreach (var type in DefaultVillageBuildings.VillageBuildings(village))
                if (buildings.FirstOrDefault(x => x.BuildingType.StringId == type.StringId) == null)
                    buildings.Add(new Building(type, village.Bound.Town));
        }

        internal override void Update(PopulationData data)
        {
            foreach (var type in DefaultVillageBuildings.VillageBuildings(village))
                if (buildings.FirstOrDefault(x => x.BuildingType.StringId == type.StringId) == null)
                    buildings.Add(new Building(type, village.Bound.Town));

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