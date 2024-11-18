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
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Roster;
using BannerKings.CampaignContent;
using BannerKings.CampaignContent.Economy.Markets;

namespace BannerKings.Patches
{
    internal class EconomyPatches
    {
        [HarmonyPatch(typeof(DefaultVillageTypes))]
        internal class DefaultVillageTypesPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("InitializeAll")]
            private static void InitializeAll()
            {
                BKVillageTypes.Instance.Initialize();
            }
        }

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

        [HarmonyPatch(typeof(ClanVariablesCampaignBehavior))]
        internal class ClanVariablesPatches
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

            [HarmonyPrefix]
            [HarmonyPatch("MakeClanFinancialEvaluation", MethodType.Normal)]
            private static bool Prefix2(Clan clan)
            {
                int num = MBRandom.RoundRandomized((clan.IsMinorFaction ? 10000 : 30000) * BannerKingsSettings.Instance.BaseWage);
                int num2 = MBRandom.RoundRandomized((clan.IsMinorFaction ? 30000 : 90000) * BannerKingsSettings.Instance.BaseWage);
                if (clan.Leader.Gold > num2)
                {
                    using (List<WarPartyComponent>.Enumerator enumerator = clan.WarPartyComponents.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            WarPartyComponent warPartyComponent = enumerator.Current;
                            warPartyComponent.MobileParty.SetWagePaymentLimit(TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyWageModel.MaxWage);
                        }
                        return false;
                    }
                }
                if (clan.Leader.Gold > num)
                {
                    using (List<WarPartyComponent>.Enumerator enumerator = clan.WarPartyComponents.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            WarPartyComponent warPartyComponent2 = enumerator.Current;
                            float num3 = 600f + (float)(clan.Leader.Gold - num) / (float)(num2 - num) * 600f;
                            if (warPartyComponent2.MobileParty.LeaderHero == clan.Leader)
                            {
                                num3 *= 1.5f;
                            }
                            warPartyComponent2.MobileParty.SetWagePaymentLimit((int)num3);
                        }
                        return false;
                    }
                }
                foreach (WarPartyComponent warPartyComponent3 in clan.WarPartyComponents)
                {
                    float num4 = 200f + (float)clan.Leader.Gold / (float)num * ((float)clan.Leader.Gold / (float)num) * 400f;
                    if (warPartyComponent3.MobileParty.LeaderHero == clan.Leader)
                    {
                        num4 *= 1.5f;
                    }
                    warPartyComponent3.MobileParty.SetWagePaymentLimit((int)num4);
                }

                return false;
                /*if (clan.IsMinorFaction)
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
                    var income = TaleWorlds.CampaignSystem.Campaign.Current.Models.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber *
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
                                        limit = TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementTaxModel
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

                return true;*/
            }
        }

        [HarmonyPatch(typeof(DefaultClanFinanceModel))]
        internal class ClanFinancesPatches
        {
            private static DefaultClanFinanceModel DefaultClanFinanceModel = new DefaultClanFinanceModel();
            /*private static MethodInfo AddExpensesFromPartiesAndGarrisons => DefaultClanFinanceModel
                .GetType()
                .GetMethod("AddExpensesFromPartiesAndGarrisons", BindingFlags.Instance | BindingFlags.NonPublic);

            private static MethodInfo AddExpensesForHiredMercenaries => DefaultClanFinanceModel
                .GetType()
                .GetMethod("AddExpensesForHiredMercenaries", BindingFlags.Instance | BindingFlags.NonPublic);

            private static MethodInfo AddExpensesForTributes => DefaultClanFinanceModel
                .GetType()
                .GetMethod("AddExpensesForTributes", BindingFlags.Instance | BindingFlags.NonPublic);

            private static MethodInfo AddExpensesForAutoRecruitment => DefaultClanFinanceModel
                .GetType()
                .GetMethod("AddExpensesForAutoRecruitment", BindingFlags.Instance | BindingFlags.NonPublic);

            private static MethodInfo AddPaymentForDebts => DefaultClanFinanceModel
                .GetType()
                .GetMethod("AddPaymentForDebts", BindingFlags.Instance | BindingFlags.NonPublic);

            private static MethodInfo AddPlayerExpenseForWorkshops => DefaultClanFinanceModel
                .GetType()
                .GetMethod("AddPlayerExpenseForWorkshops", BindingFlags.Instance | BindingFlags.NonPublic);*/

            [HarmonyPrefix]
            [HarmonyPatch("CalculateClanExpensesInternal")]
            private static bool CalculateClanExpensesInternalPrefix(DefaultClanFinanceModel __instance, Clan clan, 
                ref ExplainedNumber goldChange, bool applyWithdrawals = false, bool includeDetails = false)
            {
                bool payBudget = clan.Gold > 100000 && clan.Kingdom != null && clan.Leader != Hero.MainHero && !clan.IsUnderMercenaryService;
                if (payBudget) 
                {
                    MethodInfo AddExpensesFromPartiesAndGarrisons = DefaultClanFinanceModel
                        .GetType()
                        .GetMethod("AddExpensesFromPartiesAndGarrisons", BindingFlags.Instance | BindingFlags.NonPublic);
                    var partiesArr = new object[] { clan, goldChange, applyWithdrawals, includeDetails };
                    AddExpensesFromPartiesAndGarrisons.Invoke(__instance, partiesArr);
                    goldChange = (ExplainedNumber)partiesArr[1];
                    if (!clan.IsUnderMercenaryService)
                    {
                        MethodInfo AddExpensesForHiredMercenaries = DefaultClanFinanceModel
                            .GetType()
                            .GetMethod("AddExpensesForHiredMercenaries", BindingFlags.Instance | BindingFlags.NonPublic);
                        MethodInfo AddExpensesForTributes = DefaultClanFinanceModel
                           .GetType()
                           .GetMethod("AddExpensesForTributes", BindingFlags.Instance | BindingFlags.NonPublic);
                        var mercsArr = new object[] { clan, goldChange, applyWithdrawals };
                        AddExpensesForHiredMercenaries.Invoke(__instance, mercsArr);
                        goldChange = (ExplainedNumber)mercsArr[1];

                        var tributesArr = new object[] { clan, goldChange, applyWithdrawals };
                        AddExpensesForTributes.Invoke(__instance, tributesArr);
                        goldChange = (ExplainedNumber)tributesArr[1];
                    }

                    MethodInfo AddExpensesForAutoRecruitment = DefaultClanFinanceModel
                        .GetType()
                        .GetMethod("AddExpensesForAutoRecruitment", BindingFlags.Instance | BindingFlags.NonPublic);
                    var recruitmentArr = new object[] { clan, goldChange, applyWithdrawals };
                    AddExpensesForAutoRecruitment.Invoke(__instance, recruitmentArr);
                    goldChange = (ExplainedNumber)recruitmentArr[1];

                    if (clan.Gold > 100000 && clan.Kingdom != null && clan.Leader != Hero.MainHero && !clan.IsUnderMercenaryService)
                    {
                        int num = (int)(((float)clan.Gold - 100000f) * 0.001f);
                        if (applyWithdrawals)
                        {
                            clan.Kingdom.KingdomBudgetWallet += num;
                        }
                        goldChange.Add((float)(-(float)num), new TextObject("{=7uzvI8e8}Kingdom Budget Expense"));
                    }
                    if (clan.DebtToKingdom > 0)
                    {
                        MethodInfo AddPaymentForDebts = DefaultClanFinanceModel
                            .GetType()
                            .GetMethod("AddPaymentForDebts", BindingFlags.Instance | BindingFlags.NonPublic);
                        var debtArr = new object[] { clan, goldChange, applyWithdrawals };
                        AddPaymentForDebts.Invoke(__instance, debtArr);
                        goldChange = (ExplainedNumber)debtArr[1];
                    }
                    
                    if (Clan.PlayerClan == clan)
                    {
                        MethodInfo AddPlayerExpenseForWorkshops = DefaultClanFinanceModel
                            .GetType()
                            .GetMethod("AddPlayerExpenseForWorkshops", BindingFlags.Instance | BindingFlags.NonPublic);
                        var workshopArr = new object[] { goldChange };
                        AddPlayerExpenseForWorkshops.Invoke(__instance, workshopArr);
                        goldChange = (ExplainedNumber)workshopArr[0];
                    }
                    
                    return false;
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddIncomeFromKingdomBudget", MethodType.Normal)]
            private static bool KingdomBudgetPrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    /*var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                    return title is { Contract: { } } &&
                           title.Contract.Rights.Contains(FeudalRights.Assistance_Rights); */
                    return false;
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
                        int num2 = (int)TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(town, false).ResultNumber;
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
                        goldChange.Add(explainedNumber.ResultNumber, new TextObject("{=!}Walled Demesnes"), null);
                        return false;
                    }
                    goldChange.AddFromExplainedNumber(explainedNumber, new TextObject("{=!}Walled Demesnes"));
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

        [HarmonyPatch(typeof(InventoryManager))]
        internal class InventoryManagerPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("GetCurrentMarketData")]
            private static void GetPricePostfix(ref IMarketData __result)
            {
                if (TaleWorlds.CampaignSystem.Campaign.Current.GameMode == CampaignGameMode.Campaign)
                {
                    Settlement settlement = MobileParty.MainParty.CurrentSettlement;
                    if (settlement != null && settlement.IsCastle)
                    {
                        __result = settlement.Town.MarketData;
                    }
                }    
            }
        }

        [HarmonyPatch(typeof(MapEvent), "LootDefeatedParties")]
        public class SiegePatch
        {
            static bool Prefix(ref bool playerCaptured, ref ItemRoster __state, object lootCollector, MapEvent __instance)
            {
                __state = new ItemRoster();
                var mapEventSide = __instance.GetMapEventSide(__instance.DefeatedSide);
                foreach (var party in mapEventSide.Parties
                             .Select(mapEventParty => mapEventParty.Party)
                             .Where(party => party?.IsSettlement == true && party.Settlement?.IsCastle == true))
                {

                    __state.Add(party.ItemRoster);
                    party.ItemRoster.Clear();
                }
                return true;
            }

            static void Postfix(ref bool playerCaptured, object lootCollector, MapEvent __instance, ItemRoster __state)
            {
                var mapEventSide = __instance.GetMapEventSide(__instance.DefeatedSide);
                foreach (var party in mapEventSide.Parties
                             .Select(mapEventParty => mapEventParty.Party)
                             .Where(party => party?.IsSettlement == true && party.Settlement?.IsCastle == true))
                {
                    party.ItemRoster.Add(__state);
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
                    popData.EconomicData.ConsumedValue = 0;
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
                            int finalCost = (int)(finalAmount * (float)price);
                            popData.EconomicData.ConsumedValue += finalCost;
                            town.ChangeGold(finalCost);
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

            [HarmonyPrefix]
            [HarmonyPatch("DeleteOverproducedItems", MethodType.Normal)]
            private static bool Prefix(Town town)
            {
                return false;
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
            [HarmonyPostfix]
            [HarmonyPatch("InitializeWorkshops", MethodType.Normal)]
            private static void InitializeWorkshopsPatch()
            {
                foreach (Town town in Town.AllCastles)
                {
                    town.InitializeWorkshops((int)(TaleWorlds.CampaignSystem.Campaign.Current.Models.WorkshopModel
                        .DefaultWorkshopCountInSettlement / 2f));
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("BuildWorkshopsAtGameStart", MethodType.Normal)]
            private static void BuildWorkshopsAtGameStartPatch(WorkshopsCampaignBehavior __instance)
            {
                MethodInfo artisans = __instance.GetType().GetMethod("BuildArtisanWorkshop", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo build = __instance.GetType().GetMethod("BuildWorkshopForHeroAtGameStart", BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (Town town in Town.AllCastles)
                {
                    artisans.Invoke(__instance, new object[] { town });
                    for (int i = 1; i < town.Workshops.Length; i++)
                    {
                        Hero notableOwnerForWorkshop = TaleWorlds.CampaignSystem.Campaign.Current.Models.WorkshopModel
                            .GetNotableOwnerForWorkshop(town.Workshops[i]);
                        build.Invoke(__instance, new object[] { notableOwnerForWorkshop });
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("IsItemPreferredForTown", MethodType.Normal)]
            public static void ItemPreferredPostfix(ref bool __result, ItemObject item, Town townComponent)
            {
                if (!__result && item.Culture != null)
                {
                    MarketGroup market = DefaultMarketGroups.Instance.GetMarket(townComponent.Culture);
                    if (market != null && MBRandom.RandomFloat < market.GetSpawn((CultureObject)item.Culture))
                    {
                        __result = true;
                    }
                }

                if (__result)
                {
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
            [HarmonyPatch("ProduceAnOutputToTown", MethodType.Normal)]
            private static bool ProduceOutputPrefix(EquipmentElement outputItem, Workshop workshop, bool effectCapital)
            {
                Town town = workshop.Settlement.Town;
                int count = 1;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if (data == null)
                {
                    return true;
                }

                ItemModifierGroup modifierGroup = Utils.Helpers.GetItemModifierGroup(outputItem.Item);
                if (workshop.WorkshopType.StringId == "artisans")
                {
                    float craftsmenFactor = data.GetTypeCount(PopType.Craftsmen) / 45f;
                    count = (int)MathF.Max(1f, count + (craftsmenFactor / outputItem.ItemValue));
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

                if (TaleWorlds.CampaignSystem.Campaign.Current.GameStarted && effectCapital)
                {
                    var num = totalValue;
                    workshop.ChangeGold(num);
                    town.ChangeGold(-num);
                }

                CampaignEventDispatcher.Instance.OnItemProduced(outputItem.Item, town.Owner.Settlement, count);
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

        [HarmonyPatch(typeof(VillagerCampaignBehavior))]
        internal class VillagerMoveItemsPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("SendVillagerPartyToTradeBoundTown", MethodType.Normal)]
            private static bool SendVillagerPartyToTradeBoundTown(MobileParty villagerParty)
            {
                Settlement bound = villagerParty.HomeSettlement.Village.Bound;
                if (!bound.IsUnderSiege) villagerParty.Ai.SetMoveGoToSettlement(bound);
                else
                {
                    Settlement tradeBound = villagerParty.HomeSettlement.Village.TradeBound;
                    if (tradeBound != null)
                    {
                        if (!tradeBound.IsUnderSiege) villagerParty.Ai.SetMoveGoToSettlement(tradeBound);
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
                if (mobileParty is { IsActive: true, IsVillager: true })
                {
                    if (____previouslyChangedVillagerTargetsDueToEnemyOnWay.ContainsKey(mobileParty))
                    {
                        ____previouslyChangedVillagerTargetsDueToEnemyOnWay[mobileParty].Clear();
                    }

                    if (settlement.Town != null)
                    {
                        SellGoodsForTradeAction.ApplyByVillagerTrade(settlement, mobileParty);
                    }

                    if (settlement.IsVillage)
                    {
                        var tax = TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementTaxModel.CalculateVillageTaxFromIncome(
                            mobileParty.HomeSettlement.Village, mobileParty.PartyTradeGold);
                        float remainder = mobileParty.PartyTradeGold - tax;
                        mobileParty.HomeSettlement.Village.ChangeGold((int)(remainder * 0.5f));
                        mobileParty.PartyTradeGold = 0;
                        if (mobileParty.HomeSettlement.Village.TradeTaxAccumulated < 0)
                        {
                            mobileParty.HomeSettlement.Village.TradeTaxAccumulated = 0;
                        }

                        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                        if (data != null && data.EstateData != null) data.EstateData.AccumulateTradeTax(data, tax);
                    }

                    if (settlement.Town != null && settlement.Town.Governor != null &&
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

        // Added productions
        [HarmonyPatch(typeof(VillageGoodProductionCampaignBehavior))]
        internal class TickGoodProductionPatch
        {
            private static Dictionary<Village, Dictionary<ItemObject, float>> productionCache = new Dictionary<Village, Dictionary<ItemObject, float>>();

            [HarmonyPrefix]
            [HarmonyPatch("TickGoodProduction", MethodType.Normal)]
            private static bool TickGoodProduction(Village village, bool initialProductionForTowns)
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
                        var result = TaleWorlds.CampaignSystem.Campaign.Current.Models.VillageProductionCalculatorModel.CalculateDailyProductionAmount(
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

            [HarmonyPrefix]
            [HarmonyPatch("DistributeInitialItemsToTowns", MethodType.Normal)]
            private static bool DistributeInitialItemsToTowns()
            {
                int num = 25;
                foreach (SettlementComponent settlementComponent in Town.AllTowns)
                {
                    float num2 = 0f;
                    Settlement settlement = settlementComponent.Settlement;
                    foreach (Village village in Village.All)
                    {
                        float num3 = 0f;
                        if (village.TradeBound == settlement)
                        {
                            num3 += 1f;
                        }
                        else
                        {
                            float distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, village.Settlement);
                            float num4 = 0.5f * (600f / MathF.Pow(distance, 1.5f));
                            if (num4 > 0.5f)
                            {
                                num4 = 0.5f;
                            }
                            if (village.TradeBound == null)
                            {
                                PropertyInfo bound = village.GetType().GetProperty("TradeBound", BindingFlags.Instance | BindingFlags.Public);
                                bound.SetValue(village, village.Bound);
                            }

                            float distance2 = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, village.TradeBound);
                            float num5 = 0.5f * (600f / MathF.Pow(distance2, 1.5f));
                            if (num5 > 0.5f)
                            {
                                num5 = 0.5f;
                            }
                            num3 = ((village.Settlement.MapFaction == settlement.MapFaction) ? 1f : 0.6f) * 0.5f * ((num4 + num5) / 2f);
                        }
                        num2 += num3;
                    }
                    foreach (Village village2 in Village.All)
                    {
                        float num6 = 0f;
                        if (village2.TradeBound == settlement)
                        {
                            num6 += 1f;
                        }
                        else
                        {
                            float distance3 = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, village2.Settlement);
                            float num7 = 0.5f * (600f / MathF.Pow(distance3, 1.5f));
                            if (num7 > 0.5f)
                            {
                                num7 = 0.5f;
                            }
                            float distance4 = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, village2.TradeBound);
                            float num8 = 0.5f * (600f / MathF.Pow(distance4, 1.5f));
                            if (num8 > 0.5f)
                            {
                                num8 = 0.5f;
                            }
                            num6 = ((village2.Settlement.MapFaction == settlement.MapFaction) ? 1f : 0.6f) * 0.5f * ((num7 + num8) / 2f);
                        }
                        foreach (ValueTuple<ItemObject, float> valueTuple in village2.VillageType.Productions)
                        {
                            ItemObject item = valueTuple.Item1;
                            float item2 = valueTuple.Item2;
                            num6 *= 0.12244235f;
                            int num9 = MBRandom.RoundRandomized(item2 * num6 * ((float)num * (12f / num2)));
                            for (int i = 0; i < num9; i++)
                            {
                                ItemModifier itemModifier = null;
                                EquipmentElement rosterElement = new EquipmentElement(item, itemModifier, null, false);
                                settlement.ItemRoster.AddToCounts(rosterElement, 1);
                            }
                        }
                    }
                }

                return false;
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
