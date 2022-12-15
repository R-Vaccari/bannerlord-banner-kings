using BannerKings.Extensions;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Behaviours
{
    public class BKBuildingsBehavior : CampaignBehaviorBase
    {
        private Dictionary<Town, int> miningRevenues;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlement);
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCreationOver);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnTownDailyTick);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData)
            {
                miningRevenues = null;
            }

            dataStore.SyncData("bannerkings-mining-revenues", ref miningRevenues);

            if (miningRevenues == null)
            {
                miningRevenues = new Dictionary<Town, int>();
            }
        }

        public int GetMiningRevenue(Town town)
        {
            if (miningRevenues == null)
            {
                miningRevenues = new Dictionary<Town, int>();
            }

            if (town == null)
            {
                return 0;
            }

            if (miningRevenues.ContainsKey(town))
            {
                return miningRevenues[town];
            }

            return 0;
        }

        private void OnCreationOver()
        {
            miningRevenues = new Dictionary<Town, int>();
        }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            if (!settlement.IsVillage || settlement.Village.VillageState != Village.VillageStates.Normal)
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data == null || data.VillageData == null)
            {
                return;
            }

            RunMarketplace(settlement.Village, data);
        }

        private void RunMarketplace(Village village, PopulationData data)
        {
            ExceptionUtils.TryCatch(() =>
            {
                var villageData = data.VillageData;
                float marketplace = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Marketplace);
                if (marketplace > 0)
                {
                    var town = village.Bound?.Town;
                    if (town == null)
                    {
                        return;
                    }

                    var marketData = town.MarketData;
                    var productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(data);

                    int budget = (int)(village.Gold * 0.05f);
                    for (int i = town.Owner.ItemRoster.Count - 1; i >= 0; i--)
                    {
                        if (budget <= 10)
                        {
                            break;
                        }

                        ItemRosterElement elementCopyAtIndex = town.Owner.ItemRoster.GetElementCopyAtIndex(i);
                        ItemObject item = elementCopyAtIndex.EquipmentElement.Item;
                        if (productions.Any(x => x.Item1 == item))
                        {
                            continue;
                        }

                        var itemData = marketData.GetCategoryData(item.ItemCategory);
                        if (itemData.Supply > itemData.Demand)
                        {
                            int price = marketData.GetPrice(elementCopyAtIndex.EquipmentElement, null, false, null);
                            if (budget >= price)
                            {
                                village.ChangeGold(-price);
                                town.ChangeGold(price);
                                town.Owner.ItemRoster.AddToCounts(elementCopyAtIndex.EquipmentElement, -1);
                            }
                        }
                    }
                }
            }, GetType().Name);
        }

        private void OnTownDailyTick(Town town)
        {
            RunMines(town);
            RunStuds(town);
        }

        private void RunStuds(Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (!town.IsCastle || town.Villages.Any(x => x.IsRanchVillage()))
                {
                    return;
                }

                var studs = town.Buildings.FirstOrDefault(x => x.BuildingType == BKBuildings.Instance.WarhorseStuds);
                if (studs == null || studs.CurrentLevel == 0)
                {
                    return;
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var pastures = data.LandData.Pastureland;
                var horseLimit = (pastures / 10000f) + studs.CurrentLevel * 2f;
                var currentHorses = 0;

                foreach (var element in town.Settlement.Stash)
                {
                    if (element.EquipmentElement.Item.ItemCategory == DefaultItemCategories.WarHorse)
                    {
                        currentHorses += element.Amount;
                    }
                }

                if (currentHorses < horseLimit && MBRandom.RandomFloat < 0.02f)
                {
                    var horse = Campaign.Current.ObjectManager.GetObjectTypeList<ItemObject>()
                        .FirstOrDefault(x => x.ItemCategory == DefaultItemCategories.WarHorse && x.Culture == town.Culture);
                    town.Settlement.Stash.AddToCounts(horse, 1);
                }

            }, GetType().Name);
        }

        private void RunMines(Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                var building = town.Buildings.FirstOrDefault(x => x.BuildingType.StringId == BKBuildings.Instance.Mines.StringId ||
                                        x.BuildingType.StringId == BKBuildings.Instance.CastleMines.StringId);
                if (building == null)
                {
                    return;
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if ((building.CurrentLevel > 0) && data != null && data.MineralData != null)
                {
                    if (miningRevenues.ContainsKey(town))
                    {
                        miningRevenues[town] = 0;
                    }

                    foreach (var pair in data.MineralData.GetLocalMinerals())
                    {
                        if (MBRandom.RandomFloat * ((int)data.MineralData.Richness + 0.5f) < pair.Item2)
                        {
                            var quantity = building.CurrentLevel;
                            var item = pair.Item1;

                            var itemPrice = town.GetItemPrice(item);
                            var finalPrice = (int)(itemPrice * (float)quantity);
                            if (town.Gold >= finalPrice)
                            {
                                town.Owner.ItemRoster.AddToCounts(item, quantity);
                                town.ChangeGold(-finalPrice);
                                AddRevenue(town, finalPrice);
                            }
                            else
                            {
                                town.Settlement.Stash.AddToCounts(item, quantity);
                            }
                        }
                    }
                }
            },
            GetType().Name,
            false);
        }

        private void AddRevenue(Town town, int revenue)
        {
            if (miningRevenues.ContainsKey(town))
            {
                miningRevenues[town] += revenue;
            }
            else
            {
                miningRevenues.Add(town, revenue);
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            if (BannerKingsConfig.Instance.PopulationManager == null)
            {
                return;
            }

            foreach (var settlement in Settlement.All)
            {
                if (settlement.Town != null)
                {
                    var buildings = settlement.Town.Buildings;
                    foreach (var type in BKBuildings.Instance.All)
                    {
                        if (settlement.IsTown && type.BuildingLocation == BuildingLocation.Settlement &&
                            buildings.FirstOrDefault(x => x.BuildingType == type) == null)
                        {
                            buildings.Add(new Building(type, settlement.Town));
                        }
                        else if (settlement.IsCastle && type.BuildingLocation == BuildingLocation.Castle &&
                            buildings.FirstOrDefault(x => x.BuildingType == type) == null)
                        {
                            buildings.Add(new Building(type, settlement.Town));
                        }
                    }
                }
            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(BuildingType), "All", MethodType.Getter)]
        internal class BuildingsPatch
        {
            private static bool Prefix(ref MBReadOnlyList<BuildingType> __result)
            {
                var list = new List<BuildingType>(BKBuildings.AllBuildings);
                foreach (var type in DefaultVillageBuildings.Instance.All)
                {
                    if (list.Contains(type))
                    {
                        list.Remove(type);
                    }
                }
                __result = list.GetReadOnlyList();
                return false;
            }
        }
    }
}
