using HarmonyLib;
using System.Collections.Generic;
using static BannerKings.Managers.PopulationManager;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Extensions;
using BannerKings.Extensions;
using Helpers;
using System;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Library;
using BannerKings.Managers.Policies;

namespace BannerKings.Patches
{
    internal class EconomyPatches
    {
        [HarmonyPatch(typeof(TownMarketData))]
        internal class TownMarketDataPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("GetPrice", new Type[] { typeof(EquipmentElement), typeof(MobileParty),
            typeof(bool), typeof(PartyBase)})]
            private static void GetPricePostfix(TownMarketData __instance, ref int __result, EquipmentElement itemRosterElement, 
                MobileParty tradingParty = null, bool isSelling = false, PartyBase merchantParty = null)
            {
                var item = itemRosterElement.Item;
                if (item != null && item.HasHorseComponent)
                {
                    int minimumPrice = 0;
                    if (item.HorseComponent.MeatCount > 0)
                    {
                        int meatPrice = __instance.GetPrice(DefaultItems.Meat, tradingParty, isSelling, merchantParty);
                        minimumPrice += (int)(meatPrice * (float)item.HorseComponent.MeatCount);
                    }

                    if (item.HorseComponent.HideCount > 0)
                    {
                        int hidePrice = __instance.GetPrice(DefaultItems.Hides, tradingParty, isSelling, merchantParty);
                        minimumPrice += (int)(hidePrice * (float)item.HorseComponent.HideCount);
                    }

                    if (__result < minimumPrice)
                    {
                        __result += minimumPrice;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(VillageMarketData))]
        internal class VillageMarketDataPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("GetPrice", new Type[] { typeof(EquipmentElement), typeof(MobileParty),
            typeof(bool), typeof(PartyBase)})]
            private static void GetPricePostfix(VillageMarketData __instance, ref int __result, EquipmentElement itemRosterElement,
                MobileParty tradingParty, bool isSelling, PartyBase merchantParty)
            {
                var item = itemRosterElement.Item;
                if (item != null && item.HasHorseComponent)
                {
                    int minimumPrice = 0;
                    if (item.HorseComponent.MeatCount > 0)
                    {
                        int meatPrice = __instance.GetPrice(DefaultItems.Meat, tradingParty, isSelling, merchantParty);
                        minimumPrice += (int)(meatPrice * (float)item.HorseComponent.MeatCount);
                    }

                    if (item.HorseComponent.HideCount > 0)
                    {
                        int hidePrice = __instance.GetPrice(DefaultItems.Hides, tradingParty, isSelling, merchantParty);
                        minimumPrice += (int)(hidePrice * (float)item.HorseComponent.HideCount);
                    }

                    if (__result < minimumPrice)
                    {
                        __result += minimumPrice;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HorseComponent))]
        internal class HorseComponentPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("MeatCount", MethodType.Getter)]
            private static void MeatCountPostfix(HorseComponent __instance, ref int __result)
            {
                if (__instance.Item != null && __instance.Item.Weight < 10)
                {
                    __result = 0;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("HideCount", MethodType.Getter)]
            private static void HideCountPostfix(HorseComponent __instance, ref int __result)
            {
                if (__instance.Item != null && __instance.Item.Weight < 10)
                {
                    __result = 0;
                }
            }
        }

        [HarmonyPatch(typeof(ItemConsumptionBehavior))]
        internal class ItemConsumptionPatch
        {
            // Retain behavior of original while updating satisfaction parameters
            [HarmonyPrefix]
            [HarmonyPatch("MakeConsumption", MethodType.Normal)]
            private static bool Prefix(Town town, Dictionary<ItemCategory, float> categoryDemand,
                Dictionary<ItemCategory, int> saleLog)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                {
                    saleLog.Clear();
                    var marketData = town.MarketData;
                    var itemRoster = town.Owner.ItemRoster;
                    var popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                    for (var i = itemRoster.Count - 1; i >= 0; i--)
                    {
                        var elementCopyAtIndex = itemRoster.GetElementCopyAtIndex(i);
                        var item = elementCopyAtIndex.EquipmentElement.Item;
                        var amount = elementCopyAtIndex.Amount;
                        var itemCategory = item.GetItemCategory();

                        if (!categoryDemand.ContainsKey(itemCategory))
                        {
                            continue;
                        }

                        var demand = categoryDemand[itemCategory];
                        var budget = CalculateBudget(town, demand, itemCategory);

                        if (budget > 0.01f)
                        {
                            var price = marketData.GetPrice(item);
                            var desiredAmount = budget / price;
                            if (desiredAmount > amount)
                            {
                                desiredAmount = amount;
                            }

                            if (item.IsFood && town.FoodStocks <= town.FoodStocksUpperLimit() * 0.1f)
                            {
                                var requiredFood = town.FoodChange * -1f;
                                if (amount > requiredFood)
                                {
                                    desiredAmount += requiredFood + 1f;
                                }
                                else
                                {
                                    desiredAmount += amount;
                                }
                            }

                            var finalAmount = MBRandom.RoundRandomized(desiredAmount);
                            var type = Utils.Helpers.GetTradeGoodConsumptionType(item);
                            if (finalAmount > amount)
                            {
                                finalAmount = amount;
                                if (type != ConsumptionType.None)
                                {
                                    popData.EconomicData.UpdateSatisfaction(type, -0.0015f);
                                }
                            }
                            else if (type != ConsumptionType.None)
                            {
                                popData.EconomicData.UpdateSatisfaction(type, 0.001f);
                            }

                            itemRoster.AddToCounts(elementCopyAtIndex.EquipmentElement, -finalAmount);
                            categoryDemand[itemCategory] = budget - desiredAmount * price;
                            town.ChangeGold(finalAmount * price);
                            var num4 = 0;
                            saleLog.TryGetValue(itemCategory, out num4);
                            saleLog[itemCategory] = num4 + finalAmount;
                        }
                    }

                    if (town.FoodStocks <= town.FoodStocksUpperLimit() * 0.05f && town.Settlement.Stash != null)
                    {
                        var elements = new List<ItemRosterElement>();
                        foreach (var element in town.Settlement.Stash)
                        {
                            if (element.EquipmentElement.Item.CanBeConsumedAsFood())
                            {
                                elements.Add(element);
                            }
                        }

                        foreach (var element in elements)
                        {
                            var item = element.EquipmentElement.Item;
                            var category = item.ItemCategory;
                            if (saleLog.ContainsKey(category))
                            {
                                saleLog[category] += element.Amount;
                            }
                            else
                            {
                                saleLog.Add(category, element.Amount);
                            }

                            town.Settlement.Stash.Remove(element);
                            if (item.HasHorseComponent)
                            {
                                town.FoodStocks += item.GetItemFoodValue();
                            }
                        }
                    }

                    var list = new List<Town.SellLog>();
                    foreach (var keyValuePair in saleLog)
                    {
                        if (keyValuePair.Value > 0)
                        {
                            list.Add(new Town.SellLog(keyValuePair.Key, keyValuePair.Value));
                        }
                    }

                    town.SetSoldItems(list);
                    return false;
                }

                return true;
            }
        }

        private static float CalculateBudget(Town town, float demand, ItemCategory category)
        {
            BKTaxPolicy policy = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax");
            float factor = 0.15f;
            if (policy.Policy == BKTaxPolicy.TaxType.High)
            {
                factor = 0.1f;
            }
            else if (policy.Policy == BKTaxPolicy.TaxType.Low)
            {
                factor = 0.2f;
            }

            float priceIndex = town.GetItemCategoryPriceIndex(category);
            float prosperity = (town.Prosperity / 1000f) / priceIndex;

            return demand * MathF.Pow(priceIndex, factor) + prosperity;
        }

        [HarmonyPatch(typeof(NotablesCampaignBehavior), "BalanceGoldAndPowerOfNotable")]
        internal class BalanceGoldAndPowerOfNotablePatch
        {
            private static bool Prefix(Hero notable)
            {
                if (notable.Gold > 10500)
                {
                    var num = (notable.Gold - 10000) / 500;
                    GiveGoldAction.ApplyBetweenCharacters(notable, null, num * 500, true);
                    notable.AddPower(num);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(WorkshopsCampaignBehavior), "ProduceOutput")]
        internal class WorkshopProduceOutputPatch
        {
            private static bool Prefix(EquipmentElement outputItem, Town town, Workshop workshop, int count,
                bool doNotEffectCapital)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if (data == null)
                {
                    return true;
                }

                var category = outputItem.Item.ItemCategory;
                ItemModifierGroup modifierGroup = null;
                if (outputItem.Item.ArmorComponent != null)
                {
                    modifierGroup = outputItem.Item.ArmorComponent.ItemModifierGroup;
                }
                else if (outputItem.Item.WeaponComponent != null)
                {
                    modifierGroup = outputItem.Item.WeaponComponent.ItemModifierGroup;
                }
                else if (outputItem.Item.IsFood)
                {
                    modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("consumables");
                }
                else if (outputItem.Item.IsAnimal)
                {
                    modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("animals");
                }
                else if (!outputItem.Item.HasHorseComponent && category != DefaultItemCategories.Iron)
                {
                    modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("goods");
                }

                if (workshop.WorkshopType.StringId == "artisans")
                {
                    float prosperityFactor = town.Prosperity / 10000f;
                    float craftsmenFactor = data.GetTypeCount(PopType.Craftsmen) / 50f;
                    count = (int)MathF.Max(1f, count + ((craftsmenFactor / outputItem.ItemValue) * prosperityFactor));
                }

                var result = BannerKingsConfig.Instance.WorkshopModel.GetProductionQuality(workshop).ResultNumber;
                var itemPrice = town.GetItemPrice(new EquipmentElement(outputItem));
                int totalValue = 0;
                for (int i = 0; i < count; i++)
                {
                    ItemModifier modifier = null;
                    if (modifierGroup != null)
                    {
                        modifier = modifierGroup.GetRandomModifierWithTarget(result, 0.2f);
                        if (modifier != null)
                        {
                            itemPrice = (int)(itemPrice * modifier.PriceMultiplier);
                        }
                    }
                    var element = new EquipmentElement(outputItem.Item, modifier);
                    totalValue += itemPrice;
                    town.Owner.ItemRoster.AddToCounts(element, 1);
                }

                if (Campaign.Current.GameStarted && !doNotEffectCapital)
                {
                    var num = totalValue;
                    workshop.ChangeGold(num);
                    town.ChangeGold(-num);
                }

                CampaignEventDispatcher.Instance.OnItemProduced(outputItem.Item, town.Owner.Settlement, count);
                return false;
            }
        }

        [HarmonyPatch(typeof(Workshop), "Expense", MethodType.Getter)]
        internal class WorkshopExpensePatch
        {
            private static bool Prefix(Workshop __instance, ref int __result)
            {
                __result = (int)BannerKingsConfig.Instance.WorkshopModel.GetDailyExpense(__instance).ResultNumber;
                return false;
            }
        }

        [HarmonyPatch(typeof(HeroHelper))]
        internal class StartRecruitingMoneyLimitPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("StartRecruitingMoneyLimit", MethodType.Normal)]
            private static bool Prefix1(Hero hero, ref float __result)
            {
                __result = 50f;
                if (hero.PartyBelongedTo != null)
                {
                    __result += 1000f;
                }

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetVolunteerTroopsOfHeroForRecruitment", MethodType.Normal)]
            private static bool Prefix2(Hero hero, ref List<CharacterObject> __result)
            {
                List<CharacterObject> list = new List<CharacterObject>();
                for (int i = 0; i < hero.VolunteerTypes.Length; i++)
                {
                    list.Add(hero.VolunteerTypes[i]);
                }
                __result = list;
                return false;
            }
        }

        [HarmonyPatch(typeof(CaravansCampaignBehavior), "GetTradeScoreForTown")]
        internal class GetTradeScoreForTownPatch
        {
            private static void Postfix(ref float __result, MobileParty caravanParty, Town town,
                CampaignTime lastHomeVisitTimeOfCaravan,
                float caravanFullness, bool distanceCut)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null)
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                    if (data != null)
                    {
                        if (__result > 0f)
                        {
                            __result *= data.EconomicData.CaravanAttraction.ResultNumber;
                        }
                        
                        __result -= data.EconomicData.CaravanFee(caravanParty) / 10f;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SellGoodsForTradeAction), "ApplyInternal")]
        internal class SellGoodsPatch
        {
            private static bool Prefix(object[] __args)
            {
                var settlement = (Settlement)__args[0];
                var mobileParty = (MobileParty)__args[1];
                if (settlement != null && mobileParty != null)
                {
                    if (settlement.IsTown && mobileParty.IsVillager)
                    {
                        var component = settlement.Town;
                        var list = new List<ValueTuple<EquipmentElement, int>>();
                        var num = 10000;
                        ItemObject itemObject = null;
                        for (var i = 0; i < mobileParty.ItemRoster.Count; i++)
                        {
                            var elementCopyAtIndex = mobileParty.ItemRoster.GetElementCopyAtIndex(i);
                            if (elementCopyAtIndex.EquipmentElement.Item.ItemCategory ==
                                DefaultItemCategories.PackAnimal &&
                                elementCopyAtIndex.EquipmentElement.Item.Value < num)
                            {
                                num = elementCopyAtIndex.EquipmentElement.Item.Value;
                                itemObject = elementCopyAtIndex.EquipmentElement.Item;
                            }
                        }

                        for (var j = mobileParty.ItemRoster.Count - 1; j >= 0; j--)
                        {
                            var elementCopyAtIndex2 = mobileParty.ItemRoster.GetElementCopyAtIndex(j);
                            var itemPrice = component.GetItemPrice(elementCopyAtIndex2.EquipmentElement,
                                mobileParty, true);
                            var num2 = mobileParty.ItemRoster.GetElementNumber(j);
                            if (elementCopyAtIndex2.EquipmentElement.Item == itemObject)
                            {
                                var num3 = (int)(0.5f * mobileParty.MemberRoster.TotalManCount) + 2;
                                num2 -= num3;

                                if (itemObject.StringId == "mule")
                                {
                                    num2 -= (int)(mobileParty.MemberRoster.TotalManCount * 0.1f);
                                }
                            }

                            if (num2 > 0)
                            {
                                var num4 = MathF.Min(num2, component.Gold / itemPrice);
                                if (num4 > 0)
                                {
                                    mobileParty.PartyTradeGold += num4 * itemPrice;
                                    var equipmentElement = elementCopyAtIndex2.EquipmentElement;
                                    component.ChangeGold(-num4 * itemPrice);
                                    settlement.ItemRoster.AddToCounts(equipmentElement, num4);
                                    mobileParty.ItemRoster.AddToCounts(equipmentElement, -num4);
                                    list.Add(new ValueTuple<EquipmentElement, int>(
                                        new EquipmentElement(equipmentElement.Item), num4));
                                }
                            }
                        }

                        if (!list.IsEmpty())
                        {
                            CampaignEventDispatcher.Instance.OnCaravanTransactionCompleted(mobileParty, component,
                                list);
                        }

                        return false;
                    }
                }

                return true;
            }
        }

        //Mules
        [HarmonyPatch(typeof(VillagerCampaignBehavior), "MoveItemsToVillagerParty")]
        internal class VillagerMoveItemsPatch
        {
            private static bool Prefix(Village village, MobileParty villagerParty)
            {
                var mule = MBObjectManager.Instance.GetObject<ItemObject>(x => x.StringId == "mule");
                var muleCount = (int)(villagerParty.MemberRoster.TotalManCount * 0.1f);
                villagerParty.ItemRoster.AddToCounts(mule, muleCount);
                var itemRoster = village.Settlement.ItemRoster;
                var num = villagerParty.InventoryCapacity - villagerParty.ItemRoster.TotalWeight;
                for (var i = 0; i < 4; i++)
                {
                    foreach (var itemRosterElement in itemRoster)
                    {
                        var item = itemRosterElement.EquipmentElement.Item;
                        var num2 = MBRandom.RoundRandomized(itemRosterElement.Amount * 0.2f);
                        if (num2 > 0)
                        {
                            if (!item.HasHorseComponent && item.Weight * num2 > num)
                            {
                                num2 = MathF.Ceiling(num / item.Weight);
                                if (num2 <= 0)
                                {
                                    continue;
                                }
                            }

                            if (!item.HasHorseComponent)
                            {
                                num -= num2 * item.Weight;
                            }

                            villagerParty.Party.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, num2);
                            itemRoster.AddToCounts(itemRosterElement.EquipmentElement, -num2);
                        }
                    }
                }

                return false;
            }
        }

        //Add gold to village and consume some of it, do not reset gold
        [HarmonyPatch(typeof(VillagerCampaignBehavior), "OnSettlementEntered")]
        internal class VillagerSettlementEnterPatch
        {
            private static bool Prefix(
                ref Dictionary<MobileParty, List<Settlement>> ____previouslyChangedVillagerTargetsDueToEnemyOnWay,
                MobileParty mobileParty, Settlement settlement, Hero hero)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (mobileParty is { IsActive: true, IsVillager: true } && data != null && data.EstateData != null)
                {

                    if (____previouslyChangedVillagerTargetsDueToEnemyOnWay.ContainsKey(mobileParty))
                    {
                        ____previouslyChangedVillagerTargetsDueToEnemyOnWay[mobileParty].Clear();
                    }

                    if (settlement.IsTown)
                    {
                        SellGoodsForTradeAction.ApplyByVillagerTrade(settlement, mobileParty);
                    }

                    if (settlement.IsVillage)
                    {
                        var tax = Campaign.Current.Models.SettlementTaxModel.CalculateVillageTaxFromIncome(
                            mobileParty.HomeSettlement.Village, mobileParty.PartyTradeGold);
                        float remainder = mobileParty.PartyTradeGold - tax;
                        mobileParty.HomeSettlement.Village.ChangeGold((int)(remainder * 0.5f));
                        mobileParty.PartyTradeGold = 0;
                        if (mobileParty.HomeSettlement.Village.TradeTaxAccumulated < 0)
                        {
                            mobileParty.HomeSettlement.Village.TradeTaxAccumulated = 0;
                        }

                        data.EstateData.AccumulateTradeTax(data, tax);
                    }

                    if (settlement.IsTown && settlement.Town.Governor != null &&
                        settlement.Town.Governor.GetPerkValue(DefaultPerks.Trade.DistributedGoods))
                    {
                        settlement.Town.TradeTaxAccumulated +=
                            MathF.Round(DefaultPerks.Trade.DistributedGoods.SecondaryBonus);
                    }

                    return false;
                }

                return true;
            }
        }


        // Impact prosperity
        [HarmonyPatch(typeof(ChangeOwnerOfWorkshopAction), "ApplyInternal")]
        internal class BankruptcyPatch
        {
            private static void Postfix(Workshop workshop, Hero newOwner, WorkshopType workshopType, int capital,
                bool upgradable, int cost, TextObject customName,
                ChangeOwnerOfWorkshopAction.ChangeOwnerOfWorkshopDetail detail)
            {
                var settlement = workshop.Settlement;
                settlement.Prosperity -= 50f;
            }
        }

        // Added productions
        [HarmonyPatch(typeof(VillageGoodProductionCampaignBehavior), "TickGoodProduction")]
        internal class TickGoodProductionPatch
        {
            private static Dictionary<Village, Dictionary<ItemObject, float>> productionCache = new Dictionary<Village, Dictionary<ItemObject, float>>();

            private static bool Prefix(Village village, bool initialProductionForTowns)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null)
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);

                    if (data == null)
                    {
                        return true;
                    }

                    var productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(data);
                    foreach (var valueTuple in productions)
                    {
                        var item = valueTuple.Item1;
                        var result = Campaign.Current.Models.VillageProductionCalculatorModel.CalculateDailyProductionAmount(
                                village, item);
                        var num = MathF.Floor(result);
                        var diff = result - num;
                        num += GetDifferential(village, item, diff);

                        if (num > 0)
                        {
                            if (!initialProductionForTowns)
                            {
                                village.Owner.ItemRoster.AddToCounts(item, num);
                                CampaignEventDispatcher.Instance.OnItemProduced(item, village.Owner.Settlement,
                                    num);
                            }
                            else
                            {
                                village.Bound.ItemRoster.AddToCounts(item, num);
                            }
                        }
                    }

                    return false;
                }

                return true;
            }

            private static int GetDifferential(Village village, ItemObject item, float diff)
            {
                int result = 0;
                if (productionCache.ContainsKey(village))
                {
                    var dic = productionCache[village];
                    if (dic.ContainsKey(item))
                    {
                        dic[item] += diff;
                        result = MathF.Floor(dic[item]);
                        if (result >= 1)
                        {
                            dic[item] -= result;
                        }
                    }
                    else
                    {
                        dic.Add(item, diff);
                    }
                }
                else
                {
                    productionCache.Add(village, new Dictionary<ItemObject, float>() { { item, diff } });
                }

                return result;
            }
        }
    }
}
