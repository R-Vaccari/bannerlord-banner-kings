using HarmonyLib;
using Populations.Behaviors;
using Populations.Models;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.VillageBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using static Populations.PopulationManager;

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
                    return false;
                }
                return true;
            }
        }

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
    }
}
