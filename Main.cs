using HarmonyLib;
using Populations.Behaviors;
using Populations.Models;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.AiBehaviors;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

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
                if (Population.IsSettlementPopulated(currentSettlement))
                    Population.GetPopData(currentSettlement).UpdatePopType(
                        Population.PopType.Slaves, Helpers.Helpers.GetPrisionerCount(prisoners));

                return true;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForSelectedPrisoners")]
        class ApplySelectedPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement)
            {
                if (Population.IsSettlementPopulated(currentSettlement))
                    Population.GetPopData(currentSettlement).UpdatePopType(
                        Population.PopType.Slaves, Helpers.Helpers.GetPrisionerCount(prisoners));

                return true;
            }
        }

        [HarmonyPatch(typeof(AiPatrollingBehavior), "AiHourlyTick")]
        class AiPatrolPatch
        {
            static bool Prefix(MobileParty mobileParty, PartyThinkParams p)
            {
                if (Population.CARAVANS.ContainsKey(mobileParty))
                    return false;

                return true;
            }
        }
    }
}
