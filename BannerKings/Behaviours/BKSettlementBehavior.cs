using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Components;
using BannerKings.Extensions;
using BannerKings.Managers;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Models.Vanilla;
using BannerKings.Utils;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
        private GoalManager goalsManager;
        private bool firstUse = BannerKingsConfig.Instance.FirstUse;

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationOver);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnGameCreated);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSiegeAftermathAppliedEvent.AddNonSerializedListener(this, OnSiegeAftermath);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
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
                goalsManager = BannerKingsConfig.Instance.GoalManager;
                firstUse = BannerKingsConfig.Instance.FirstUse;

                educationsManager.CleanEntries();
                religionsManager.CleanEntries();
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
                goalsManager = null;
            }

            dataStore.SyncData("bannerkings-populations", ref populationManager);
            dataStore.SyncData("bannerkings-titles", ref titleManager);
            dataStore.SyncData("bannerkings-courts", ref courtManager);
            dataStore.SyncData("bannerkings-policies", ref policyManager);
            dataStore.SyncData("bannerkings-religions", ref religionsManager);
            dataStore.SyncData("bannerkings-educations", ref educationsManager);
            dataStore.SyncData("bannerkings-innovations", ref innovationsManager);
            dataStore.SyncData("bannerkings-goals", ref goalsManager);
            dataStore.SyncData("bannerkings-first-use", ref firstUse);

            if (dataStore.IsLoading)
            {
                if (firstUse)
                {
                    BannerKingsConfig.Instance.InitializeManagersFirstTime();
                }
                else
                {
                    BannerKingsConfig.Instance.InitManagers(populationManager, policyManager, titleManager, courtManager, religionsManager, educationsManager, innovationsManager, goalsManager);
                }
            }
        }

        private void OnGameCreated(CampaignGameStarter starter)
        {
            if (firstUse)
            {
                BannerKingsConfig.Instance.InitializeManagersFirstTime();
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            if (!firstUse)
            {
                BannerKingsConfig.Instance.PopulationManager.PostInitialize();
                BannerKingsConfig.Instance.EducationManager.PostInitialize();
                BannerKingsConfig.Instance.InnovationsManager.PostInitialize();
                BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
                BannerKingsConfig.Instance.GoalManager.PostInitialize();
                BannerKingsConfig.Instance.CourtManager.PostInitialize();
            } 
            else
            {
                BannerKingsConfig.Instance.InitializeManagersFirstTime();
            }

            BannerKingsConfig.Instance.TitleManager.PostInitialize();
            BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
        }

        private void OnCharacterCreationOver()
        {
            BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
            BannerKingsConfig.Instance.TitleManager.PostInitialize();
        }

        private void TickSettlementData(Settlement settlement)
        {
            UpdateSettlementPops(settlement);
            BannerKingsConfig.Instance.PolicyManager.InitializeSettlement(settlement);
        }

        private void OnSiegeAftermath(MobileParty attackerParty, Settlement settlement, SiegeAftermathCampaignBehavior.SiegeAftermath aftermathType, Clan previousSettlementOwner, Dictionary<MobileParty, float> partyContributions)
        {
            if (aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.ShowMercy || settlement?.Town == null ||
                BannerKingsConfig.Instance.PopulationManager == null ||
                !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var shareToKill = aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.Pillage ? MBRandom.RandomFloatRanged(0.1f, 0.16f) : MBRandom.RandomFloatRanged(0.16f, 0.24f);

            var killTotal = (int) (data.TotalPop * shareToKill);
            var lognum = killTotal;
            var weights = GetDesiredPopTypes(settlement).Select(pair => new ValueTuple<PopType, float>(pair.Key, pair.Value[0])).ToList();

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
                new TextObject("{=ocT5sL1n}{NUMBER} people have been killed in the siege aftermath of {SETTLEMENT}.")
                    .SetTextVariable("NUMBER", lognum)
                    .SetTextVariable("SETTLEMENT", settlement.Name)
                    .ToString()));
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party is not {IsLordParty: true} || target.OwnerClan == null || party.LeaderHero != target.OwnerClan.Leader)
            {
                return;
            }

            if ((!target.IsVillage && target.Town.Governor == null) ||
                (target.IsVillage && target.Village.Bound.Town.Governor == null))
            {
                BannerKingsConfig.Instance.AI.SettlementManagement(target);
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

            TickTown(settlement);
            TickCastle(settlement);
            TickVillage(settlement);
        }

        private void TickTown(Settlement settlement)
        {
            if (settlement.Town != null)
            {
                var town = settlement.Town;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).LandData;
                HandleItemAvailability(town);
                //HandleExcessWorkforce(data, town);
                HandleExcessFood(data, town);
                HandleMarketGold(town);
            }
        }

        private void HandleMarketGold(Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (!town.IsTown)
                {
                    return;
                }

                if (town.Gold < 50000)
                {
                    var notable = town.Settlement.Notables.FirstOrDefault(x => x.Gold >= 30000);
                    if (notable != null)
                    {
                        town.ChangeGold(1000);
                        notable.ChangeHeroGold(-1000);
                        notable.AddPower(10f);
                    }
                }
            }, GetType().Name);
        }

        private void HandleItemAvailability(Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (!town.IsTown)
                {
                    return;
                }

                Dictionary<ItemCategory, int> desiredAmounts = new Dictionary<ItemCategory, int>();
                ItemRoster itemRoster = town.Owner.ItemRoster;

                desiredAmounts.Add(DefaultItemCategories.UltraArmor, (int)(town.Prosperity / 750f));
                desiredAmounts.Add(DefaultItemCategories.HeavyArmor, (int)(town.Prosperity / 400f));
                desiredAmounts.Add(DefaultItemCategories.MeleeWeapons5, (int)(town.Prosperity / 750f));
                desiredAmounts.Add(DefaultItemCategories.MeleeWeapons4, (int)(town.Prosperity / 400f));
                desiredAmounts.Add(DefaultItemCategories.RangedWeapons5, (int)(town.Prosperity / 1500f));
                desiredAmounts.Add(DefaultItemCategories.RangedWeapons4, (int)(town.Prosperity / 1000f));

                var behavior = Campaign.Current.GetCampaignBehavior<WorkshopsCampaignBehavior>();
                var getItem = AccessTools.Method(behavior.GetType(), "GetRandomItemAux", new Type[] { typeof(ItemCategory), typeof(Town) });

                foreach (var pair in desiredAmounts)
                {
                    var quantity = 0;
                    foreach (ItemRosterElement element in itemRoster)
                    {
                        var category = element.EquipmentElement.Item.ItemCategory;
                        if (category == pair.Key)
                        {
                            quantity++;
                        }
                    }

                    if (quantity < desiredAmounts[pair.Key])
                    {
                        EquipmentElement item = (EquipmentElement)getItem.Invoke(behavior, new object[] { pair.Key, town });
                        if (item.Item != null)
                        {
                            itemRoster.AddToCounts(item, 1);
                        }
                    }
                }
            }, GetType().Name);
        }

        private void HandleExcessWorkforce(LandData data, Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (data.WorkforceSaturation > 1f)
                {
                    var workers = data.AvailableWorkForce * (data.WorkforceSaturation - 1f);
                    var items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                    {
                        foreach (var tuple in from vil in town.Villages select BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement) into popData from tuple in BannerKingsConfig.Instance.PopulationManager.GetProductions(popData) where tuple.Item1.IsTradeGood && !tuple.Item1.IsFood select tuple)
                        {
                            items.Add(tuple.Item1);
                        }
                    }

                    if (items.Count > 0)
                    {
                        var random = items.GetRandomElementInefficiently();
                        var itemCount = (int)(workers * 0.01f);
                        BuyOutput(town, random, itemCount, town.GetItemPrice(random));
                    }
                }
            }, GetType().Name); 
        }

        private void HandleExcessFood(LandData data, Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (town.FoodStocks >= town.FoodStocksUpperLimit() - 10)
                {
                    var items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                    {
                        foreach (var tuple in town.Villages.Select(vil => BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement)).SelectMany(data => BannerKingsConfig.Instance.PopulationManager.GetProductions(data)))
                        {
                            items.Add(tuple.Item1);
                        }
                    }

                    var foodModel = (BKFoodModel)Campaign.Current.Models.SettlementFoodModel;
                    var popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                    if (popData == null)
                    {
                        return;
                    }

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
                                ? (int)MBMath.ClampFloat(farmFood * MBRandom.RandomFloat, 0f, farmFood)
                                : (int)farmFood;
                            if (count == 0)
                            {
                                break;
                            }

                            BuyOutput(town, item, count, town.GetItemPrice(item));
                            farmFood -= count;
                        }
                    }
                }
            }, GetType().Name);
        }

        private void TickCastle(Settlement settlement)
        {
            if (settlement.IsCastle)
            {
                if (settlement.Town?.GarrisonParty == null)
                {
                    return;
                }

                foreach (var garrison in from castleBuilding in settlement.Town.Buildings where BKBuildings.Instance.CastleRetinue != null && castleBuilding.BuildingType == BKBuildings.Instance.CastleRetinue let garrison = settlement.Town.GarrisonParty where garrison.MemberRoster != null && garrison.MemberRoster.Count > 0 let elements = garrison.MemberRoster.GetTroopRoster() let currentRetinue = elements.Where(soldierElement => Utils.Helpers.IsRetinueTroop(soldierElement.Character)).Sum(soldierElement => soldierElement.Number) let maxRetinue = castleBuilding.CurrentLevel == 1 ? 20 : castleBuilding.CurrentLevel == 2 ? 40 : 60 where currentRetinue < maxRetinue where garrison.MemberRoster.Count < garrison.Party.PartySizeLimit select garrison)
                {
                    garrison.MemberRoster.AddToCounts(settlement.Culture.EliteBasicTroop, 1);
                }

                if (settlement.Town.FoodStocks <= settlement.Town.FoodStocksUpperLimit() * 0.05f &&
                    settlement.Town.Settlement.Stash != null)
                {
                    ConsumeStash(settlement);
                }
            }
        }

        private void TickVillage(Settlement settlement)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (settlement.IsVillage)
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    if (data == null)
                    {
                        return;
                    }

                    var villageData = data.VillageData;
                    if (villageData == null)
                    {
                        return;
                    }

                    float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Manor);
                    if (!(manor > 0))
                    {
                        return;
                    }

                    var toRemove = new List<MobileParty>();
                    foreach (var party in settlement.Parties)
                    {
                        if (party.PartyComponent is RetinueComponent)
                        {
                            toRemove.Add(party);
                        }
                    }

                    if (toRemove.Count > 0)
                    {
                        toRemove.RemoveAt(0);
                        foreach (var party in toRemove)
                        {
                            DestroyPartyAction.Apply(null, party);
                        }
                    }

                    var retinue = RetinueComponent.CreateRetinue(settlement);
                    if (retinue != null)
                    {
                        (retinue.PartyComponent as RetinueComponent).DailyTick(manor);
                    }
                }
            }, this.GetType().Name);
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
                        new TextObject("{=OeCpEGzz}You have been charged {GOLD} for the excess production of {ITEM}, now in your stash at {CASTLE}.")
                            .SetTextVariable("GOLD", $"{itemFinalPrice:n0}")
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
            var elements = settlement.Stash.Where(element => element.EquipmentElement.Item != null && element.EquipmentElement.Item.CanBeConsumedAsFood()).ToList();

            float food = 0;
            foreach (var element in elements)
            {
                food += element.Amount * (float)element.EquipmentElement.Item.GetItemFoodValue();
                settlement.Stash.Remove(element);
            }

            if (food > 0)
            {
                settlement.Town.FoodStocks += (int)food;
            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(SellPrisonersAction))]
        internal class ApplyAllPrisionersPatch
        {
            private static void SendOffPrisoners(TroopRoster prisoners, Settlement currentSettlement)
            {
                var policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
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
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("ApplyForAllPrisoners", MethodType.Normal)]
            private static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement, bool applyGoldChange = true)
            {
                if (currentSettlement == null || (!currentSettlement.IsCastle && !currentSettlement.IsTown) || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    return true;
                }

                if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                {
                    return true;
                }

                SendOffPrisoners(prisoners, currentSettlement);

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("ApplyForSelectedPrisoners", MethodType.Normal)]
            private static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement)
            {
                if (currentSettlement == null || (!currentSettlement.IsCastle && !currentSettlement.IsTown) || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    return true;
                }

                if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                {
                    return true;
                }

                SendOffPrisoners(prisoners, currentSettlement);

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
            }
        }
    }
}