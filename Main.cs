using HarmonyLib;
using Populations.Behaviors;
using Populations.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.VillageBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using static Populations.Managers.TitleManager;
using static Populations.PopulationManager;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Reflection;

namespace Populations
{
    public class Main : MBSubModuleBase
    {
        public static Harmony patcher = new Harmony("Patcher");

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign)
            {
                try
                {
                    CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
                    campaignStarter.AddBehavior(new SettlementBehavior());

                    campaignStarter.AddModel(new ProsperityModel());
                    campaignStarter.AddModel(new TaxModel());
                    campaignStarter.AddModel(new FoodModel());
                    campaignStarter.AddModel(new ConstructionModel());
                    campaignStarter.AddModel(new MilitiaModel());
                    campaignStarter.AddModel(new InfluenceModel());
                    campaignStarter.AddModel(new LoyaltyModel());
                    campaignStarter.AddModel(new VillageProductionModel());
                    campaignStarter.AddModel(new SecurityModel());
                    campaignStarter.AddModel(new PartyLimitModel());
                    campaignStarter.AddModel(new ClanIncomeModel());
                    campaignStarter.AddModel(new EconomyModel());
                    campaignStarter.AddModel(new PriceFactorModel());
                    campaignStarter.AddModel(new FeudalWorkshopModel());

                } catch (Exception e)
                {
                }
            }
        }

        protected override void OnSubModuleLoad()
        {
            new Harmony("Populations").PatchAll();
            base.OnSubModuleLoad();
        }
    }

    namespace Patches
    {

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForAllPrisoners")]
        class ApplyAllPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement, bool applyGoldChange = true)
            {
                if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                    PopulationConfig.Instance.PopulationManager.GetPopData(currentSettlement).UpdatePopType(
                        PopulationManager.PopType.Slaves, Helpers.Helpers.GetRosterCount(prisoners));

                return true;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForSelectedPrisoners")]
        class ApplySelectedPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement)
            {
                if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                    PopulationConfig.Instance.PopulationManager.GetPopData(currentSettlement).UpdatePopType(
                        PopulationManager.PopType.Slaves, Helpers.Helpers.GetRosterCount(prisoners));

                return true;
            }
        }

        [HarmonyPatch(typeof(ChangeOwnerOfSettlementAction), "ApplyInternal")]
        class ChangeOnwerPatch
        {
            static bool Prefix(Settlement settlement, Hero newOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
            {
                if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
                    CultureObject settlementCulture = settlement.Culture;
                    CultureObject originalOwnerCulture = settlement.Owner.Culture;
                    CultureObject newCulture = newOwner.Culture;

                    if ((settlementCulture == originalOwnerCulture && settlementCulture != newCulture) ||
                        (settlementCulture != originalOwnerCulture && settlementCulture != newCulture
                        && originalOwnerCulture != newCulture)) // previous owner as assimilated or everybody has a different culture
                    {
                        data.Assimilation = 0f;
                    }
                    else if (originalOwnerCulture != newCulture && newCulture == settlementCulture) // new owner is same culture as settlement that was being assimilated by foreigner, invert the process
                    {
                        float result = 1f - data.Assimilation;
                        data.Assimilation = result;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Town), "FoodStocksUpperLimit")]
        class FoodStockPatch
        {
            static bool Prefix(ref Town __instance, ref int __result)
            {
                if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(__instance.Settlement))
                {
                    PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(__instance.Settlement);
                    int total = data.TotalPop;
                    int result = (int)((float)total / 10f);

                    __result = (int)((float)(Campaign.Current.Models.SettlementFoodModel.FoodStocksUpperLimit + 
                        (__instance.IsCastle ? Campaign.Current.Models.SettlementFoodModel.CastleFoodStockUpperLimitBonus : 0)) +
                        __instance.GetEffectOfBuildings(BuildingEffectEnum.Foodstock) +
                        result); 
                    return false;
                }
                else return true;
            }
        }

        [HarmonyPatch(typeof(DefaultBuildingTypes), "InitializeAll")]
        class InitializeBuildingsPatch
        {
            static void Postfix()
            {
                Helpers.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks", null), new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level).", null), new int[]
                {
                     800,
                     1200,
                     1500
                }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                }, 0);
            }
        }

        /*
        [HarmonyPatch(typeof(Village), "DailyTick")]
        class VillageTickPatch
        {
            static bool Prefix(ref Village __instance)
            {
                if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(__instance.Settlement))
                {
                    int hearthLevel = __instance.GetHearthLevel();
                    __instance.Hearth += __instance.HearthChange;
                    if (hearthLevel != __instance.GetHearthLevel())
                        __instance.Settlement.Party.Visuals.RefreshLevelMask(__instance.Settlement.Party);
                    
                    if (__instance.Hearth < 10f)
                        __instance.Hearth = 10f;

                    __instance.Owner.Settlement.Militia += __instance.MilitiaChange;
                    
                    if (PopulationConfig.Instance.PopulationManager != null 
                        && __instance.Settlement.MilitiaPartyComponent != null
                        && __instance.Settlement.MilitiaPartyComponent.MobileParty != null
                        && !PopulationConfig.Instance.PopulationManager.IsPopulationParty(__instance.Settlement.MilitiaPartyComponent.MobileParty))
                        __instance.Owner.Settlement.Militia += __instance.MilitiaChange;    
                    
                    return false;
                }
                return true;
            }
        }*/

        [HarmonyPatch(typeof(VillagerCampaignBehavior), "OnSettlementEntered")]
        class VillagerSettlementEnterPatch
        {
            static bool Prefix(ref Dictionary<MobileParty, List<Settlement>>  ____previouslyChangedVillagerTargetsDueToEnemyOnWay, MobileParty mobileParty, Settlement settlement, Hero hero)
            {
                if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {

                    if (mobileParty != null && mobileParty.IsActive && mobileParty.IsVillager)
                    {
                        ____previouslyChangedVillagerTargetsDueToEnemyOnWay[mobileParty].Clear();
                        if (settlement.IsTown)
                            SellGoodsForTradeAction.ApplyByVillagerTrade(settlement, mobileParty);
                        
                        if (settlement.IsVillage)
                        {
                            int tax = Campaign.Current.Models.SettlementTaxModel.CalculateVillageTaxFromIncome(mobileParty.HomeSettlement.Village, mobileParty.PartyTradeGold);
                            float remainder = mobileParty.PartyTradeGold - tax;
                            mobileParty.HomeSettlement.Village.ChangeGold((int)(remainder * 0.5f));
                            mobileParty.PartyTradeGold = 0;
                            mobileParty.HomeSettlement.Village.TradeTaxAccumulated += tax;
                        }
                        if (settlement.IsTown && settlement.Town.Governor != null && settlement.Town.Governor.GetPerkValue(DefaultPerks.Trade.DistributedGoods))
                            settlement.Town.TradeTaxAccumulated += MathF.Round(DefaultPerks.Trade.DistributedGoods.SecondaryBonus);    
                    }
                    return false;
                }
                else return true;  
            }
        }


        [HarmonyPatch(typeof(Hero), "SetHeroEncyclopediaTextAndLinks")]
        class HeroDescriptionPatch
        {
            static void Postfix(ref string __result, Hero o)
            {
                HashSet<FeudalTitle> titles = TitleConfig.Instance.TitleManager.GetTitles(o);
                if (titles != null && titles.Count > 0)
                {
                    string desc = "";
                    FeudalTitle current = null;
                    List<FeudalTitle> finalList = titles.OrderBy(x => (int)x.type).ToList();
                    foreach (FeudalTitle title in finalList)
                    {
                        if (current == null)
                            desc += string.Format("{0} of {1}", Helpers.Helpers.GetTitleHonorary(title.type, false), title.shortName);
                        else if (current.type == title.type)
                            desc += ", " + title.shortName;
                        else if (current.type != title.type)
                            desc += string.Format(" and {0} of {1}", Helpers.Helpers.GetTitleHonorary(title.type, false), title.shortName);
                        current = title;
                    }
                    __result = __result + Environment.NewLine + desc;
                }
            }
        }

        namespace Economy
        {
            // Pass on settlement party as parameter
            [HarmonyPatch(typeof(Town))]
            class TownItemPricePatch
            {

                [HarmonyPrefix]
                [HarmonyPatch("GetItemPrice", new Type[] { typeof(ItemObject), typeof(MobileParty), typeof(bool) })]
                static bool Prefix1(Town __instance, ref int __result, ItemObject item, MobileParty tradingParty = null, bool isSelling = false)
                {
                    __result = __instance.MarketData.GetPrice(item, tradingParty, isSelling, __instance.GarrisonParty.Party);
                    return false;
                }

                
                [HarmonyPrefix]
                [HarmonyPatch("GetItemPrice", new Type[] { typeof(EquipmentElement), typeof(MobileParty), typeof(bool) })]
                static bool Prefix2(Town __instance, ref int __result, EquipmentElement itemRosterElement, MobileParty tradingParty = null, bool isSelling = false)
                {
                    __result = __instance.MarketData.GetPrice(itemRosterElement, tradingParty, isSelling, __instance.GarrisonParty.Party);
                    return false;
                }
            }

            // Pass on settlement party as consumer
            [HarmonyPatch(typeof(WorkshopsCampaignBehavior), "ConsumeInput")]
            class ConsumeInputPatch
            {
                static bool Prefix(ItemCategory productionInput, Town town, Workshop workshop, bool doNotEffectCapital)
                {
                    if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                    {
                        ItemRoster itemRoster = town.Owner.ItemRoster;
                        int num = itemRoster.FindIndex((ItemObject x) => x.ItemCategory == productionInput);
                        if (num >= 0)
                        {
                            ItemObject itemAtIndex = itemRoster.GetItemAtIndex(num);
                            itemRoster.AddToCounts(itemAtIndex, -1);
                            if (Campaign.Current.GameStarted && !doNotEffectCapital)
                            {
                                ItemData categoryData = town.MarketData.GetCategoryData(itemAtIndex.GetItemCategory());
                                int itemPrice = new PriceFactorModel().GetPrice(new EquipmentElement(itemAtIndex, null, null, false), town.GarrisonParty, town.GarrisonParty.Party, false, categoryData.InStoreValue,
                                    categoryData.Supply, categoryData.Demand);
                                workshop.ChangeGold(-itemPrice);
                                town.ChangeGold(itemPrice);
                            }
                            CampaignEventDispatcher.Instance.OnItemConsumed(itemAtIndex, town.Owner.Settlement, 1);
                        }
                        return false;
                    }
                    else return true;
                }
            }

            // Impact prosperity
            [HarmonyPatch(typeof(ChangeOwnerOfWorkshopAction), "ApplyInternal")]
            class BankruptcyPatch
            {
                static void Postfix(Workshop workshop, Hero newOwner, WorkshopType workshopType, int capital, bool upgradable, int cost, TextObject customName, ChangeOwnerOfWorkshopAction.ChangeOwnerOfWorkshopDetail detail)
                {
                    Settlement settlement = workshop.Settlement;
                    settlement.Prosperity -= 50f;
                }
            }



            // Retain behavior of original while updating satisfaction parameters
            [HarmonyPatch(typeof(ItemConsumptionBehavior), "MakeConsumption")]
            class ItemConsumptionPatch
            {
                static bool Prefix(Town town, Dictionary<ItemCategory, float> categoryDemand, Dictionary<ItemCategory, int> saleLog)
                {
                    if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                    {
                        saleLog.Clear();
                        TownMarketData marketData = town.MarketData;
                        ItemRoster itemRoster = town.Owner.ItemRoster;
                        PopulationData popData = PopulationConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                        for (int i = itemRoster.Count - 1; i >= 0; i--)
                        {
                            ItemRosterElement elementCopyAtIndex = itemRoster.GetElementCopyAtIndex(i);
                            ItemObject item = elementCopyAtIndex.EquipmentElement.Item;
                            int amount = elementCopyAtIndex.Amount;
                            ItemCategory itemCategory = item.GetItemCategory();
                            float demand = categoryDemand[itemCategory];

                            IEnumerable<ItemConsumptionBehavior> behaviors = Campaign.Current.GetCampaignBehaviors<ItemConsumptionBehavior>();
                            MethodInfo dynMethod = behaviors.First().GetType().GetMethod("CalculateBudget", BindingFlags.NonPublic | BindingFlags.Static);
                            float num = (float)dynMethod.Invoke(null, new object[] { town, demand, itemCategory });
                            if (num > 0.01f)
                            {
                                int price = marketData.GetPrice(item, null, false, null);
                                float desiredAmount = num / (float)price;
                                if (desiredAmount > (float)amount)
                                    desiredAmount = (float)amount;

                                int finalAmount = MBRandom.RoundRandomized(desiredAmount);
                                ConsumptionType type = Helpers.Helpers.GetTradeGoodConsumptionType(item);
                                if (finalAmount > amount)
                                {
                                    finalAmount = amount;
                                    if (type != ConsumptionType.None) popData.UpdateSatisfaction(type, -0.001f);
                                }
                                else
                                {
           
                                    if (type != ConsumptionType.None) popData.UpdateSatisfaction(type, 0.001f);
                                }
                                itemRoster.AddToCounts(elementCopyAtIndex.EquipmentElement, -finalAmount);
                                categoryDemand[itemCategory] = num - desiredAmount * (float)price;
                                town.ChangeGold(finalAmount * price);
                                int num4 = 0;
                                saleLog.TryGetValue(itemCategory, out num4);
                                saleLog[itemCategory] = num4 + finalAmount;
                            }
                        }
                        List<Town.SellLog> list = new List<Town.SellLog>();
                        foreach (KeyValuePair<ItemCategory, int> keyValuePair in saleLog)
                        {
                            if (keyValuePair.Value > 0)
                            {
                                list.Add(new Town.SellLog(keyValuePair.Key, keyValuePair.Value));
                            }
                        }
                        town.SetSoldItems(list);
                        return false;
                    }
                    else return true;
                }
            }
        }     
    }
}
