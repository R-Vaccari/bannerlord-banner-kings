using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Components;
using BannerKings.Managers;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Models.Vanilla;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Behaviours
{
    public class BKSettlementBehavior : CampaignBehaviorBase
    {
        private CourtManager courtManager;
        private EducationManager educationsManager;
        private InnovationsManager innovationsManager;
        private PolicyManager policyManager;
        private PopulationManager populationManager;
        private ReligionsManager religionsManager;
        private TitleManager titleManager;

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSiegeAftermathAppliedEvent.AddNonSerializedListener(this, OnSiegeAftermath);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving)
            {
                populationManager = BannerKingsConfig.Instance.PopulationManager;
                policyManager = BannerKingsConfig.Instance.PolicyManager;
                titleManager = BannerKingsConfig.Instance.TitleManager;
                courtManager = BannerKingsConfig.Instance.CourtManager;
                religionsManager = BannerKingsConfig.Instance.ReligionsManager;
                educationsManager = BannerKingsConfig.Instance.EducationManager;
                innovationsManager = BannerKingsConfig.Instance.InnovationsManager;
            }

            if (BannerKingsConfig.Instance.wipeData)
            {
                populationManager = null;
                policyManager = null;
                titleManager = null;
                courtManager = null;
                religionsManager = null;
                educationsManager = null;
                innovationsManager = null;
            }

            dataStore.SyncData("bannerkings-populations", ref populationManager);
            dataStore.SyncData("bannerkings-titles", ref titleManager);
            dataStore.SyncData("bannerkings-courts", ref courtManager);
            dataStore.SyncData("bannerkings-policies", ref policyManager);
            dataStore.SyncData("bannerkings-religions", ref religionsManager);
            dataStore.SyncData("bannerkings-educations", ref educationsManager);
            dataStore.SyncData("bannerkings-innovations", ref innovationsManager);

            if (dataStore.IsLoading && populationManager != null)
            {
                BannerKingsConfig.Instance.InitManagers(populationManager, policyManager,
                    titleManager, courtManager, religionsManager, educationsManager, innovationsManager);
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            BannerKingsConfig.Instance.PopulationManager.PostInitialize();
        }

        private void TickSettlementData(Settlement settlement)
        {
            UpdateSettlementPops(settlement);
            BannerKingsConfig.Instance.PolicyManager.InitializeSettlement(settlement);
        }

        private void OnSiegeAftermath(MobileParty attackerParty, Settlement settlement,
            SiegeAftermathCampaignBehavior.SiegeAftermath aftermathType, Clan previousSettlementOwner,
            Dictionary<MobileParty, float> partyContributions)
        {
            if (aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.ShowMercy || settlement == null ||
                settlement.Town == null ||
                BannerKingsConfig.Instance.PopulationManager == null ||
                !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float shareToKill;
            if (aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.Pillage)
            {
                shareToKill = MBRandom.RandomFloatRanged(0.1f, 0.16f);
            }
            else
            {
                shareToKill = MBRandom.RandomFloatRanged(0.16f, 0.24f);
            }

            var killTotal = (int) (data.TotalPop * shareToKill);
            var lognum = killTotal;
            var weights = new List<(PopType, float)>();
            foreach (var pair in GetDesiredPopTypes(settlement))
            {
                weights.Add(new ValueTuple<PopType, float>(pair.Key, pair.Value[0]));
            }

            if (killTotal <= 0)
            {
                return;
            }

            while (killTotal > 0)
            {
                var random = MBRandom.RandomInt(10, 20);
                var target = MBRandom.ChooseWeighted(weights);
                var finalNum = MBMath.ClampInt(random, 0, data.GetTypeCount(target));
                data.UpdatePopType(target, -finalNum);
                killTotal -= finalNum;
            }

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}{NUMBER} people have been killed in the siege aftermath of {SETTLEMENT}.")
                    .SetTextVariable("NUMBER", lognum)
                    .SetTextVariable("SETTLEMENT", settlement.Name)
                    .ToString()));
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party != null && party.IsLordParty && target.OwnerClan != null &&
                party.LeaderHero == target.OwnerClan.Leader)
            {
                if ((!target.IsVillage && target.Town.Governor == null) ||
                    (target.IsVillage && target.Village.Bound.Town.Governor == null))
                {
                    BannerKingsConfig.Instance.AI.SettlementManagement(target);
                }
            }
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement == null || settlement.StringId.Contains("tutorial") || settlement.StringId.Contains("Ruin"))
            {
                return;
            }

            TickSettlementData(settlement);
            TickRotting(settlement);

            BannerKingsConfig.Instance.AI.SettlementManagement(settlement);

            if (settlement.Town != null)
            {
                var town = settlement.Town;
                var wkModel = (BKWorkshopModel) Campaign.Current.Models.WorkshopModel;
                foreach (var wk in town.Workshops)
                {
                    if (!wk.IsRunning || !wk.Owner.IsNotable)
                    {
                        continue;
                    }

                    var gold = Campaign.Current.Models.ClanFinanceModel.CalculateOwnerIncomeFromWorkshop(wk);
                    gold -= (int) (wkModel.CalculateWorkshopTax(wk.Settlement, wk.Owner).ResultNumber * gold);
                    wk.Owner.ChangeHeroGold(gold);
                    wk.ChangeGold(-gold);
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).LandData;

                if (data.WorkforceSaturation > 1f)
                {
                    var workers = data.AvailableWorkForce * (data.WorkforceSaturation - 1f);
                    var items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                    {
                        foreach (var tuple in from vil in town.Villages select BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement).VillageData into vilData from tuple in BannerKingsConfig.Instance.PopulationManager.GetProductions(vilData) where tuple.Item1.IsTradeGood && !tuple.Item1.IsFood select tuple)
                        {
                            items.Add(tuple.Item1);
                        }
                    }

                    if (items.Count > 0)
                    {
                        var random = items.GetRandomElementInefficiently();
                        var itemCount = (int) (workers * 0.01f);
                        BuyOutput(town, random, itemCount, town.GetItemPrice(random));
                    }
                }


                if (town.FoodStocks >= town.FoodStocksUpperLimit() - 10)
                {
                    var items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                    {
                        foreach (var tuple in town.Villages.Select(vil => BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement).VillageData).SelectMany(vilData => BannerKingsConfig.Instance.PopulationManager.GetProductions(vilData)))
                        {
                            items.Add(tuple.Item1);
                        }
                    }

                    var foodModel = (BKFoodModel) Campaign.Current.Models.SettlementFoodModel;
                    var popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    var excess = foodModel.GetPopulationFoodProduction(popData, town).ResultNumber - 10 - foodModel.GetPopulationFoodConsumption(popData).ResultNumber;
                    //float pasturePorportion = data.Pastureland / data.Acreage;

                    var farmFood = MBMath.ClampFloat(data.Farmland * data.GetAcreOutput("farmland"), 0f, excess);
                    if (town.IsCastle)
                    {
                        farmFood *= 0.1f;
                    }

                    while (farmFood > 1f)
                    {
                        foreach (var item in items)
                        {
                            if (!item.IsFood)
                            {
                                continue;
                            }

                            var count = farmFood > 10f
                                ? (int) MBMath.ClampFloat(farmFood * MBRandom.RandomFloat, 0f, farmFood)
                                : (int) farmFood;
                            if (count == 0)
                            {
                                break;
                            }

                            BuyOutput(town, item, count, town.GetItemPrice(item));
                            farmFood -= count;
                        }
                    }
                }
            }


            if (settlement.IsCastle)
            {
                //SetNotables(settlement);
                //UpdateVolunteers(settlement);
                if (settlement.Town == null || settlement.Town.GarrisonParty == null)
                {
                    return;
                }

                foreach (var garrison in from castleBuilding in settlement.Town.Buildings where Utils.Helpers._buildingCastleRetinue != null && castleBuilding.BuildingType == Utils.Helpers._buildingCastleRetinue let garrison = settlement.Town.GarrisonParty where garrison.MemberRoster != null && garrison.MemberRoster.Count > 0 let elements = garrison.MemberRoster.GetTroopRoster() let currentRetinue = elements.Where(soldierElement => Utils.Helpers.IsRetinueTroop(soldierElement.Character, settlement.Culture)).Sum(soldierElement => soldierElement.Number) let maxRetinue = castleBuilding.CurrentLevel == 1 ? 20 : castleBuilding.CurrentLevel == 2 ? 40 : 60 where currentRetinue < maxRetinue where garrison.MemberRoster.Count < garrison.Party.PartySizeLimit select garrison)
                {
                    garrison.MemberRoster.AddToCounts(settlement.Culture.EliteBasicTroop, 1);
                }

                if (settlement.Town.FoodStocks <= settlement.Town.FoodStocksUpperLimit() * 0.05f &&
                    settlement.Town.Settlement.Stash != null)
                {
                    ConsumeStash(settlement);
                }
            }
            else if (settlement.IsVillage)
            {
                var villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).VillageData;
                if (villageData == null)
                {
                    return;
                }

                float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Manor);
                if (!(manor > 0))
                {
                    return;
                }

                var retinues = BannerKingsConfig.Instance.PopulationManager.AllParties;
                MobileParty retinue = null;
                if (retinues.Count > 0)
                {
                    retinue = retinues.FirstOrDefault(x =>
                        x.StringId.Contains($"bk_retinue_{settlement.Name}"));
                }

                retinue ??= RetinueComponent.CreateRetinue(settlement);

                (retinue.PartyComponent as RetinueComponent).DailyTick(manor);
            }
        }

        private void BuyOutput(Town town, ItemObject item, int count, int price)
        {
            var itemFinalPrice = (int) (price * (float) count);
            if (town.IsTown)
            {
                town.Owner.ItemRoster.AddToCounts(item, count);
                town.ChangeGold(-itemFinalPrice);
            }
            else
            {
                town.Settlement.Stash.AddToCounts(item, count);
                town.OwnerClan.Leader.ChangeHeroGold(-itemFinalPrice);
                if (town.OwnerClan.Leader == Hero.MainHero)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject(
                                "You have been charged {GOLD} for the excess production of {ITEM}, now in your stash at {CASTLE}.")
                            .SetTextVariable("GOLD", itemFinalPrice)
                            .SetTextVariable("ITEM", item.Name)
                            .SetTextVariable("CASTLE", town.Name)
                            .ToString()));
                }
            }
        }

        private void TickRotting(Settlement settlement)
        {
            var party = settlement.Party;

            var roster = party?.ItemRoster;
            if (roster == null)
            {
                return;
            }

            var maxStorage = 1000f;
            if (settlement.Town != null)
            {
                maxStorage += settlement.Town.Buildings.Where(b => b.BuildingType == DefaultBuildingTypes.CastleGranary || b.BuildingType == DefaultBuildingTypes.SettlementGranary).Sum(b => b.CurrentLevel * 5000f);
            }

            RotRosterFood(roster, maxStorage);
            if (settlement.Stash != null)
            {
                RotRosterFood(settlement.Stash, settlement.IsCastle ? maxStorage : 1000f);
            }
        }

        private void RotRosterFood(ItemRoster roster, float maxStorage)
        {
            if (!(roster.TotalFood > maxStorage))
            {
                return;
            }

            var toRot = (int) (roster.TotalFood * 0.01f);
            foreach (var element in roster.ToList().FindAll(x => x.EquipmentElement.Item != null &&
                                                                 x.EquipmentElement.Item.ItemCategory.Properties ==
                                                                 ItemCategory.Property.BonusToFoodStores))
            {
                if (toRot <= 0)
                {
                    break;
                }

                var result = (int) MathF.Min(MBRandom.RandomFloatRanged(10f, toRot), (float) element.Amount);
                roster.AddToCounts(element.EquipmentElement, -result);
                toRot -= result;
            }
        }

        private void ConsumeStash(Settlement settlement)
        {
            var elements = settlement.Stash.Where(element => element.EquipmentElement.Item != null && element.EquipmentElement.Item.ItemCategory.Properties == ItemCategory.Property.BonusToFoodStores).ToList();

            var food = 0;
            foreach (var element in elements)
            {
                food += element.Amount;
                settlement.Stash.Remove(element);
            }

            if (food > 0)
            {
                settlement.Town.FoodStocks += food;
            }
        }


        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            var retinueType = MBObjectManager.Instance.GetObjectTypeList<BuildingType>().FirstOrDefault(x => x == Utils.Helpers._buildingCastleRetinue);
            if (retinueType == null)
            {
                Utils.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks"),
                    new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level)."),
                    new[]
                    {
                        1000,
                        1500,
                        2000
                    }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                    {
                    });
            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForAllPrisoners")]
        internal class ApplyAllPrisionersPatch
        {
            private static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement, bool applyGoldChange = true)
            {
                if (currentSettlement == null || (!currentSettlement.IsCastle && !currentSettlement.IsTown) || BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    return true;
                }

                if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                {
                    return true;
                }

                var policy = (BKCriminalPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                switch (policy.Policy)
                {
                    case BKCriminalPolicy.CriminalPolicy.Enslavement:
                    {
                        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                        data?.UpdatePopType(PopType.Slaves, Utils.Helpers.GetRosterCount(prisoners));
                        break;
                    }
                    case BKCriminalPolicy.CriminalPolicy.Forgiveness:
                    {
                        var dic = new Dictionary<CultureObject, int>();
                        foreach (var element in prisoners.GetTroopRoster())
                        {
                            if (element.Character.Occupation == Occupation.Bandit)
                            {
                                continue;
                            }

                            var culture = element.Character.Culture;
                            if (culture == null || culture.IsBandit)
                            {
                                continue;
                            }

                            if (dic.ContainsKey(culture))
                            {
                                dic[culture] += element.Number;
                            }
                            else
                            {
                                dic.Add(culture, element.Number);
                            }
                        }

                        foreach (var pair in dic)
                        {
                            if (!Settlement.All.Any(x => x.Culture == pair.Key))
                            {
                                continue;
                            }

                            {
                                var random = Settlement.All.FirstOrDefault(x => x.Culture == pair.Key);
                                if (random != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(random))
                                {
                                    BannerKingsConfig.Instance.PopulationManager.GetPopData(random).UpdatePopType(PopType.Serfs, pair.Value);
                                }
                            }
                        }

                        break;
                    }
                    default:
                        return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForSelectedPrisoners")]
        internal class ApplySelectedPrisionersPatch
        {
            private static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement)
            {
                if (currentSettlement != null && (currentSettlement.IsCastle || currentSettlement.IsTown) & (BannerKingsConfig.Instance.PopulationManager != null) &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                    {
                        return true;
                    }

                    var policy = (BKCriminalPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                    switch (policy.Policy)
                    {
                        case BKCriminalPolicy.CriminalPolicy.Enslavement:
                        {
                            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                            data?.UpdatePopType(PopType.Slaves, Utils.Helpers.GetRosterCount(prisoners));
                            break;
                        }
                        case BKCriminalPolicy.CriminalPolicy.Forgiveness:
                        {
                            var dic = new Dictionary<CultureObject, int>();
                            foreach (var element in prisoners.GetTroopRoster())
                            {
                                if (element.Character.Occupation == Occupation.Bandit)
                                {
                                    continue;
                                }

                                var culture = element.Character.Culture;
                                if (culture == null)
                                {
                                    continue;
                                }

                                if (dic.ContainsKey(culture))
                                {
                                    dic[culture] += element.Number;
                                }
                                else
                                {
                                    dic.Add(culture, element.Number);
                                }
                            }

                            foreach (var pair in dic)
                            {
                                if (!Settlement.All.Any(x => x.Culture == pair.Key))
                                {
                                    continue;
                                }

                                {
                                    var random = Settlement.All.FirstOrDefault(x => x.Culture == pair.Key);
                                    if (random == null ||
                                        !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(random))
                                    {
                                        continue;
                                    }

                                    var data =
                                        BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                                    data?.UpdatePopType(PopType.Serfs, pair.Value);
                                }
                            }

                            break;
                        }
                        default:
                            return false;
                    }
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(Town), "FoodStocksUpperLimit")]
        internal class FoodStockPatch
        {
            private static bool Prefix(ref Town __instance, ref int __result)
            {
                if (BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(__instance.Settlement))
                {
                    return true;
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(__instance.Settlement);
                var total = data.TotalPop;
                var result = (int) (total / 6.5f);

                __result = (int) (Campaign.Current.Models.SettlementFoodModel.FoodStocksUpperLimit + (__instance.IsCastle ? Campaign.Current.Models.SettlementFoodModel.CastleFoodStockUpperLimitBonus : 0) + __instance.GetEffectOfBuildings(BuildingEffectEnum.Foodstock) + result);
                return false;

            }
        }

        [HarmonyPatch(typeof(DefaultBuildingTypes), "InitializeAll")]
        internal class InitializeBuildingsPatch
        {
            private static void Postfix()
            {
                Utils.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks"),
                    new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level)."),
                    new[]
                    {
                        800,
                        1200,
                        1500
                    }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                    {
                    });
            }
        }
    }
}