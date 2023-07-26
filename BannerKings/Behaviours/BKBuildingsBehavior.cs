using BannerKings.Extensions;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Utils;
using HarmonyLib;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKBuildingsBehavior : CampaignBehaviorBase
    {
        private Dictionary<Town, int> miningRevenues;
        private Dictionary<Town, int> materialExpenses;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlement);
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCreationOver);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnTownDailyTick);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnBuildingLevelChangedEvent.AddNonSerializedListener(this, OnBuildingChanged);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData)
            {
                miningRevenues = null;
                materialExpenses = null;
            }

            dataStore.SyncData("bannerkings-mining-revenues", ref miningRevenues);
            dataStore.SyncData("bannerkings-material-expenses", ref materialExpenses);

            if (miningRevenues == null)
            {
                miningRevenues = new Dictionary<Town, int>();
                
            }

            if (materialExpenses == null)
            {
                materialExpenses = new Dictionary<Town, int>();
            }
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

        private void AddExpense(Town town, int expense)
        {
            if (materialExpenses.ContainsKey(town))
            {
                materialExpenses[town] += expense;
            }
            else
            {
                materialExpenses.Add(town, expense);
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

        public int GetMaterialExpenses(Town town)
        {
            if (materialExpenses == null)
            {
                materialExpenses = new Dictionary<Town, int>();
            }

            if (town == null)
            {
                return 0;
            }

            if (materialExpenses.ContainsKey(town))
            {
                return materialExpenses[town];
            }

            return 0;
        }

        private void OnCreationOver()
        {
            miningRevenues = new Dictionary<Town, int>();
            materialExpenses = new Dictionary<Town, int>();
        }

        private void OnBuildingChanged(Town town, Building building, int levelChange)
        {
            if (levelChange > 0)
            {
                float totalCost = 0;
                var requirements = BannerKingsConfig.Instance.ConstructionModel.GetMaterialRequirements(building);
                foreach (var requirement in requirements)
                {
                    int consumed = 0;
                    foreach (ItemRosterElement element in town.Settlement.Stash)
                    {
                        if (consumed < requirement.Item2 && element.EquipmentElement.Item == requirement.Item1)
                        {
                            int toConsume = MathF.Min(element.Amount, requirement.Item2 - consumed);
                            consumed += toConsume;
                        }
                    }

                    town.Settlement.Stash.AddToCounts(requirement.Item1, -consumed);

                    if (consumed < requirement.Item2)
                    {
                        int diff = requirement.Item2 - consumed;
                        int bought = 0;
                        foreach (ItemRosterElement element in town.Settlement.ItemRoster)
                        {
                            if (diff > 0 && element.EquipmentElement.Item == requirement.Item1)
                            {
                                int toBuy = MathF.Min(element.Amount, diff);
                                diff -= toBuy;
                                bought += toBuy;
                                totalCost += town.GetItemPrice(element.EquipmentElement) * (float)toBuy;
                            }
                        }

                        town.Settlement.Stash.AddToCounts(requirement.Item1, -bought);
                    }
                }

                int toDeduct = MathF.Min((int)totalCost, town.OwnerClan.Gold);
                town.OwnerClan.Leader.ChangeHeroGold(-toDeduct);
                if (town.OwnerClan == Clan.PlayerClan && toDeduct > 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=G3WXaS0w}The {PROJECT} project at {TOWN} has finished and the remaining materials costed {DENARS}{GOLD_ICON}.")
                        .SetTextVariable("PROJECT", building.BuildingType.Name)
                        .SetTextVariable("TOWN", town.Name)
                        .SetTextVariable("DENARS", toDeduct.ToString())
                        .ToString(),
                        Color.UIntToColorString(TextHelper.COLOR_LIGHT_YELLOW)));
                }

                if (toDeduct < totalCost)
                {
                    float influence = toDeduct * 0.5f;
                    ChangeClanInfluenceAction.Apply(town.OwnerClan, -influence);
                    if (town.OwnerClan == Clan.PlayerClan && influence > 0f)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=R3MCYNKK}You did not have all the denars to pay for materials, and thus lost {INFLUENCE} influence.")
                           .SetTextVariable("INFLUENCE", influence.ToString("0.00"))
                           .ToString(),
                           Color.UIntToColorString(TextHelper.COLOR_LIGHT_RED)));
                    }
                }
            }
        }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data == null)
            {
                return;
            }

            HandleVillage(data);
        }

        private void HandleVillage(PopulationData data)
        {
            Settlement settlement = data.Settlement;
            if (!settlement.IsVillage || settlement.Village.VillageState != Village.VillageStates.Normal)
            {
                return;
            }

            Village village = settlement.Village;
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
            RunMaterials(town);
        }

        private void RunMaterials(Town town)
        {
            if (materialExpenses == null)
            {
                return;
            }

            if (materialExpenses.ContainsKey(town))
            {
                materialExpenses[town] = 0;
            }

            if (town.Governor != null && town.CurrentBuilding != null)
            {
                Town materialSource = town.IsTown ? town : SettlementHelper
                    .FindNearestTown(x => !x.MapFaction.IsAtWarWith(town.MapFaction)).Town;

                if (materialSource == null)
                {
                    return;
                }

                var requirements = BannerKingsConfig.Instance.ConstructionModel.GetMaterialRequirements(town.CurrentBuilding);
                foreach (var requirement in requirements)
                {
                    int stashCount = 0;
                    foreach (ItemRosterElement element in materialSource.Settlement.Stash)
                    {
                        if (element.EquipmentElement.Item == requirement.Item1)
                        {
                            stashCount += element.Amount;
                        }
                    }

                    if (stashCount < requirement.Item2)
                    {
                        int toBuy = MathF.Min(requirement.Item2 - stashCount, 5);
                        foreach (ItemRosterElement element in town.Settlement.ItemRoster)
                        {
                            if (element.EquipmentElement.Item == requirement.Item1)
                            {
                                int bought = MathF.Min(toBuy, element.Amount);
                                toBuy -= bought;
                                town.Settlement.Stash.AddToCounts(element.EquipmentElement, bought);
                                materialSource.Settlement.ItemRoster.AddToCounts(element.EquipmentElement, -bought);

                                int price = (int)(materialSource.GetItemPrice(element.EquipmentElement) * (float)bought);
                                if (materialSource != town)
                                {
                                    price = (int)(price * 1.2f);
                                }

                                AddExpense(town, price);
                                materialSource.ChangeGold(price);
                            }
                        }
                    }
                }
            }
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
                                AddRevenue(town, (int)(finalPrice * 0.5f));
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
                __result = new MBReadOnlyList<BuildingType>(list);
                return false;
            }
        }
    }
}
