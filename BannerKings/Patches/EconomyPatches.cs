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
using BannerKings.Managers.Populations;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Settings;
using BannerKings.Utils.Extensions;
using System.Reflection;
using TaleWorlds.CampaignSystem.GameComponents;
using BannerKings.Managers.Innovations;

namespace BannerKings.Patches
{
    internal class EconomyPatches
    {
        //Clan members should not raise parties on peace, except leader and knights
        [HarmonyPatch(typeof(HeroSpawnCampaignBehavior))]
        internal class HeroSpawnCampaignBehaviorPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("GetBestAvailableCommander")]
            private static void GetBestAvailableCommanderPrefix(Clan clan, ref Hero __result)
            {
                if (__result != null)
                {
                    Kingdom kingdom = clan.Kingdom;
                    if (clan != Clan.PlayerClan && kingdom != null && FactionManager.GetEnemyKingdoms(kingdom).Count() == 0)
                    {
                        if (!__result.IsClanLeader() && BannerKingsConfig.Instance.TitleManager.GetAllDeJure(__result).Count == 0)
                        {
                            __result = null;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WorkshopsCampaignBehavior), "IsItemPreferredForTown")]
        class IsItemPreferredPatch
        {
            public static void Postfix(ref bool __result, ItemObject item, Town townComponent)
            {
                if (!__result) return;

                InnovationData data = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(townComponent.Settlement.Culture);
                if (data != null)
                {
                    if (item.HasWeaponComponent && (item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Crossbow ||
                                       item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Bolt))
                    {
                        __result = data.HasFinishedInnovation(DefaultInnovations.Instance.Crossbows);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ClanVariablesCampaignBehavior), "MakeClanFinancialEvaluation")]
        internal class MakeClanFinancialEvaluationPatch
        {
            private static bool Prefix(Clan clan)
            {
                if (clan.IsMinorFaction)
                {
                    return true;
                }
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var war = false;
                    if (clan.Kingdom != null)
                    {
                        war = FactionManager.GetEnemyKingdoms(clan.Kingdom).Any();
                    }
                    var income = Campaign.Current.Models.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber *
                    (war ? 0.45f : 0.15f);
                    if (war)
                    {
                        income += clan.Gold * 0.005f;
                    }

                    if (income > 0f)
                    {
                        var knights = 0f;
                        foreach (var partyComponent in clan.WarPartyComponents)
                        {
                            if (partyComponent.Leader != null && partyComponent.Leader != clan.Leader)
                            {
                                var title =
                                    BannerKingsConfig.Instance.TitleManager.GetHighestTitle(partyComponent.Leader);
                                if (title is { Fief: { } })
                                {
                                    knights++;
                                    var limit = 0f;
                                    if (title.Fief.IsVillage)
                                    {
                                        limit = BannerKingsConfig.Instance.TaxModel.CalculateVillageTaxFromIncome(title.Fief.Village).ResultNumber;
                                    }
                                    else if (title.Fief.Town != null)
                                    {
                                        limit = Campaign.Current.Models.SettlementTaxModel
                                            .CalculateTownTax(title.Fief.Town).ResultNumber;
                                    }

                                    partyComponent.MobileParty.SetWagePaymentLimit((int)(50f + limit));
                                }
                            }
                        }

                        foreach (var partyComponent in clan.WarPartyComponents)
                        {
                            var share = MathF.Min(8000f, income / clan.WarPartyComponents.Count - knights);
                            partyComponent.MobileParty.SetWagePaymentLimit((int)(300f + share));
                        }

                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ClanVariablesCampaignBehavior))]
        internal class AutoRecruitmentPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateClanSettlementAutoRecruitment", MethodType.Normal)]
            private static bool Prefix1(Clan clan)
            {
                if (clan.MapFaction is { IsKingdomFaction: true })
                {
                    var enemies = FactionManager.GetEnemyKingdoms(clan.Kingdom);
                    foreach (var settlement in clan.Settlements)
                    {
                        if (settlement.IsFortification && settlement.Town.GarrisonParty != null)
                        {
                            if (enemies.Count() >= 0 && settlement.Town.GarrisonParty.MemberRoster.TotalManCount < 500)
                            {
                                settlement.Town.GarrisonAutoRecruitmentIsEnabled = true;
                            }

                            settlement.Town.GarrisonAutoRecruitmentIsEnabled = false;
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(DefaultClanFinanceModel))]
        internal class ClanFinancesPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("AddIncomeFromKingdomBudget", MethodType.Normal)]
            private static bool KingdomBudgetPrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                    return title is { Contract: { } } &&
                           title.Contract.Rights.Contains(FeudalRights.Assistance_Rights);
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddIncomeFromParty", MethodType.Normal)]
            private static bool AddIncomeFromPartyPrefix(MobileParty party, Clan clan, ref ExplainedNumber goldChange,
                bool applyWithdrawals)
            {
                if (BannerKingsConfig.Instance.TitleManager != null && party.LeaderHero != null &&
                    party.LeaderHero != clan.Leader)
                {
                    return BannerKingsConfig.Instance.TitleManager.GetHighestTitle(party.LeaderHero) == null;
                }

                if (party.IsCaravan)
                {
                    return !BannerKingsSettings.Instance.RealisticCaravanIncome;
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddExpensesFromPartiesAndGarrisons", MethodType.Normal)]
            private static bool PartyExpensesPrefix(Clan clan, ref ExplainedNumber goldChange,
                bool applyWithdrawals, bool includeDetails)
            {
                var model = new DefaultClanFinanceModel();
                var calculatePartyWageFunction = model.GetType()
                    .GetMethod("CalculatePartyWage", BindingFlags.Instance | BindingFlags.NonPublic);

                ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions, null);

                Hero leader = clan.Leader;
                MobileParty mainParty = (leader != null) ? leader.PartyBelongedTo : null;
                if (mainParty != null)
                {
                    int budget = clan.Gold + (int)goldChange.ResultNumber + (int)goldChange.ResultNumber;
                    object[] array = { mainParty, budget, applyWithdrawals };
                    int expense = (int)calculatePartyWageFunction.Invoke(model, array);
                    explainedNumber.Add(-expense, new TextObject("{=YkZKXsIn}Main party wages"));
                }

                List<MobileParty> list = new List<MobileParty>();
                foreach (var hero in clan.Lords)
                    foreach (var caravanPartyComponent in hero.OwnedCaravans)
                        list.Add(caravanPartyComponent.MobileParty);
                        
                foreach (var hero2 in clan.Companions)
                    foreach (var caravanPartyComponent2 in hero2.OwnedCaravans)
                        list.Add(caravanPartyComponent2.MobileParty);     

                foreach (var warPartyComponent in clan.WarPartyComponents)
                    if (warPartyComponent.MobileParty != mainParty)
                        list.Add(warPartyComponent.MobileParty);
                    
                foreach (Town town in clan.Fiefs)
                    if (town.GarrisonParty != null && town.GarrisonParty.IsActive)
                        list.Add(town.GarrisonParty);
  
                foreach (var party in list)
                {
                    int budget = clan.Gold + (int)goldChange.ResultNumber + (int)goldChange.ResultNumber;
                    object[] array = { party, budget, applyWithdrawals };
                    int expense = (int)calculatePartyWageFunction.Invoke(model, array);

                    if (applyWithdrawals)
                    {
                        if (party.IsLordParty)
                        {
                            if (party.LeaderHero != null)
                            {
                                party.LeaderHero.Gold -= expense;
                            }
                            else
                            {
                                party.ActualClan.Leader.Gold -= expense;
                            }
                        }
                        else
                        {
                            party.PartyTradeGold -= expense;
                        }
                    }

                    if (party.LeaderHero != null && party.LeaderHero != clan.Leader)
                    {
                        if (BannerKingsConfig.Instance.TitleManager.GetAllDeJure(party.LeaderHero)
                            .Any(x => x.TitleType == TitleType.Lordship))
                        {
                            continue;
                        }
                    }

                    if (applyWithdrawals)
                    {
                        if (party.LeaderHero != null && party.LeaderHero.IsClanLeader())
                        {
                            continue;
                        }

                        bool needsExtra = false;
                        if (party.IsLordParty && party.LeaderHero != null)
                        {
                            needsExtra = party.LeaderHero.Gold < 5000;
                        }
                        else
                        {
                            needsExtra = party.PartyTradeGold < 5000;
                        }

                        if (needsExtra && (expense + 200) < budget)
                        {
                            expense += 200;
                        }

                        int refund = MathF.Min(expense, budget);
                        if (party.IsLordParty)
                        {
                            if (party.LeaderHero != null)
                            {
                                party.LeaderHero.Gold += refund;
                            }
                            else
                            {
                                party.ActualClan.Leader.Gold += refund;
                            }
                        }
                        else
                        {
                            party.PartyTradeGold += refund;
                        }
                    }

                    explainedNumber.Add(-expense, new TextObject("{=tqCSk7ya}Party wages {A0}"), party.Name);
                }

                if (!includeDetails)
                {
                    goldChange.Add(explainedNumber.ResultNumber, new TextObject("{=ChUDSiJw}Garrison and Party Expense", null), null);
                    return false;
                }

                goldChange.AddFromExplainedNumber(explainedNumber, new TextObject("{=ChUDSiJw}Garrison and Party Expense", null));
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddSettlementIncome", MethodType.Normal)]
            private static bool VillageIncomePrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions, null);
                    foreach (Town town in clan.Fiefs)
                    {
                        ExplainedNumber explainedNumber2 = new ExplainedNumber((float)((int)((float)town.TradeTaxAccumulated / 5f)), false, null);
                        int num = MathF.Round(explainedNumber2.ResultNumber);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.ContentTrades, town, ref explainedNumber2);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Crossbow.Steady, town, ref explainedNumber2);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Roguery.SaltTheEarth, town, ref explainedNumber2);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.GivingHands, town, ref explainedNumber2);
                        if (applyWithdrawals)
                        {
                            town.TradeTaxAccumulated -= num;
                            if (clan == Clan.PlayerClan)
                            {
                                CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(DefaultClanFinanceModel.AssetIncomeType.Taxes, (int)explainedNumber2.ResultNumber);
                            }
                        }
                        int num2 = (int)Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(town, false).ResultNumber;
                        explainedNumber.Add((float)num2, new TextObject("{=TLuaPAIO}{A0} Taxes", null), town.Name);
                        explainedNumber.Add(explainedNumber2.ResultNumber, new TextObject("{=wVMPdc8J}{A0}'s tariff", null), town.Name);
                        if (town.CurrentDefaultBuilding != null && town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Engineering.ArchitecturalCommisions))
                        {
                            explainedNumber.Add(DefaultPerks.Engineering.ArchitecturalCommisions.SecondaryBonus, new TextObject("{=uixuohBp}Settlement Projects", null), null);
                        }
                    }

                    int villageTotal = 0;
                    foreach (Village village in clan.GetActualVillages())
                    {
                        FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
                        var income = CalculateVillageIncome(village);
                        if (title != null && title.deJure != clan.Leader && applyWithdrawals)
                        {
                            ApplyWithdrawal(village, income, title.deJure);
                        }
                        else
                        {
                            villageTotal += income;
                            if (applyWithdrawals)
                            {
                                ApplyWithdrawal(village, income);
                            }
                        }
                    }

                    goldChange.Add(villageTotal, new TextObject("{=GikQuojv}Village Demesnes"));
                    if (!includeDetails)
                    {
                        goldChange.Add(explainedNumber.ResultNumber, new TextObject("{=AewK9qME}Settlement Income", null), null);
                        return false;
                    }
                    goldChange.AddFromExplainedNumber(explainedNumber, new TextObject("{=AewK9qME}Settlement Income", null));
                    return false;
                }

                return true;
            }

            private static void ApplyWithdrawal(Village village, int income, Hero payTo = null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
                data.VillageData.LastPayment = income;
                village.TradeTaxAccumulated -= MathF.Min(village.TradeTaxAccumulated, income);
                if (payTo != null)
                {
                    payTo.Gold += income;
                }
            }

            private static int CalculateVillageIncome(Village village) => (int)BannerKingsConfig.Instance.TaxModel
                .CalculateVillageTaxFromIncome(village,
                    false,
                    false)
                    .ResultNumber;
        }

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
                if (__instance.Monster != null && __instance.Monster.StringId == "chicken" ||
                    __instance.Monster.StringId == "goose")
                {
                    __result = 0;
                }

                if (__instance.Item != null && __instance.Item.Weight < 10)
                {
                    __result = 0;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("HideCount", MethodType.Getter)]
            private static void HideCountPostfix(HorseComponent __instance, ref int __result)
            {
                if (__instance.Monster != null && __instance.Monster.StringId == "chicken" ||
                   __instance.Monster.StringId == "goose")
                {
                    __result = 0;
                }

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

        [HarmonyPatch(typeof(WorkshopsCampaignBehavior))]
        internal class WorkshopsCampaignBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("InitializeWorkshops", MethodType.Normal)]
            private static bool InitializeWorkshopsPrefix()
            {
                foreach (Town town in Town.AllTowns)
                {
                    town.InitializeWorkshops(6);
                }

                return false;
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(methodName: "DecideBestWorkshopType", MethodType.Normal)]
            private static void DecideBestWorkshopTypePostfix(ref WorkshopType __result, 
                Settlement currentSettlement, bool atGameStart, WorkshopType workshopToExclude = null)
            {
                if (__result != null && currentSettlement != null && currentSettlement.Town != null)
                {
                    var id = __result.StringId;
                    if (currentSettlement.Town.Workshops.Any(x => x != null && x.WorkshopType != null && x.WorkshopType.StringId == id))
                    {
                        if (MBRandom.RandomFloat <= 0.3f)
                        {
                            __result = WorkshopType.Find("mines");
                        }
                        else
                        {
                            List<WorkshopType> list = new List<WorkshopType>();
                            list.Add(WorkshopType.Find("smithy"));
                            list.Add(WorkshopType.Find("fletcher"));
                            list.Add(WorkshopType.Find("barding-smithy"));
                            list.Add(WorkshopType.Find("armorsmithy"));
                            list.Add(WorkshopType.Find("weaponsmithy"));
                            var random = list.GetRandomElement();
                            if (currentSettlement.Town.Workshops.Any(x => x != null && x.WorkshopType != null && 
                            x.WorkshopType.StringId == random.StringId))
                            {
                                __result = list.GetRandomElement();
                            }
                            else
                            {
                                __result = random;
                            }
                        }
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("ProduceOutput", MethodType.Normal)]
            private static bool ProduceOutputPrefix(EquipmentElement outputItem, Town town, Workshop workshop, int count,
                bool doNotEffectCapital)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if (data == null)
                {
                    return true;
                }

                ItemModifierGroup modifierGroup = Utils.Helpers.GetItemModifierGroup(outputItem.Item);
                if (workshop.WorkshopType.StringId == "artisans")
                {
                    float prosperityFactor = town.Prosperity / 10000f;
                    float craftsmenFactor = data.GetTypeCount(PopType.Craftsmen) / 50f;
                    count = (int)MathF.Max(1f, count + ((craftsmenFactor / outputItem.ItemValue) * prosperityFactor));
                }
                else if (workshop.WorkshopType.StringId == "mines" && data.MineralData != null)
                {
                    MineralData mineralData = data.MineralData;
                    outputItem = new EquipmentElement(mineralData.GetRandomItem());
                    if (mineralData.Richness == MineralRichness.RICH) count += 2;
                    else if (mineralData.Richness == MineralRichness.ADEQUATE) count += 1;
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
                        ItemObject item = valueTuple.Item1;
                        var result = Campaign.Current.Models.VillageProductionCalculatorModel.CalculateDailyProductionAmount(
                                village, item);

                        var num = MathF.Floor(result);
                        var diff = result - num;
                        num += GetDifferential(village, item, diff);

                        if (num > 0)
                        {
                            ItemModifierGroup modifierGroup = Utils.Helpers.GetItemModifierGroup(item);
                            ItemModifier modifier = null;
                            if (modifierGroup != null)
                            {
                                modifier = modifierGroup.GetRandomModifierWithTarget(data.EconomicData.ProductionQuality.ResultNumber, 
                                    0.2f);
                            }

                            EquipmentElement element = new EquipmentElement(item, modifier);
                            if (!initialProductionForTowns)
                            {
                                village.Owner.ItemRoster.AddToCounts(element, num);
                                CampaignEventDispatcher.Instance.OnItemProduced(item, village.Owner.Settlement,
                                    num);
                            }
                            else
                            {
                                village.Bound.ItemRoster.AddToCounts(element, num);
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
