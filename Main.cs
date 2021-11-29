using HarmonyLib;
using Populations.Behaviors;
using Populations.Models;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.AiBehaviors;
using TaleWorlds.Core;
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
                    } else if (originalOwnerCulture != newCulture && newCulture == settlementCulture) // new owner is same culture as settlement that was being assimilated by foreigner, invert the process
                    { 
                        float result = 1f - data.Assimilation;
                        data.Assimilation = result;
                    }
                }

                return true;
            }
        }

        /*
        [HarmonyPatch(typeof(DefaultSettlementFoodModel), "CalculateTownFoodChangeInternal")]
        class SettlementFoodPatch
        {
            static bool Prefix(ref ExplainedNumber __result, Town town, bool includeDescriptions)
            {
                if (PopulationManager.IsSettlementPopulated(town.Settlement))
                {
                    __result = new FoodModel().CalculateTownFoodChangeInternal(town, includeDescriptions);
                    return false;
                }

                return true;
            }
        }
        */
    }
}
