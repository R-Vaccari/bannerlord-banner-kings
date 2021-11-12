using HarmonyLib;
using Populations.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
                } catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
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
                {
                    List<TroopRosterElement> rosters = prisoners.GetTroopRoster();
                    int count = 0;
                    rosters.ForEach(roster =>
                    {
                        if (!roster.Character.IsHero)
                            count += roster.Number + roster.WoundedNumber;
                    });

                    Population.GetPopData(currentSettlement).UpdatePopType(Population.PopType.Slaves, count);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForSelectedPrisoners")]
        class ApplySelectedPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement)
            {
                if (Population.IsSettlementPopulated(currentSettlement))
                {
                    List<TroopRosterElement> rosters = prisoners.GetTroopRoster();
                    int count = 0;
                    rosters.ForEach(roster =>
                    {
                        if (!roster.Character.IsHero)
                            count += roster.Number + roster.WoundedNumber;
                    });
                    
                    Population.GetPopData(currentSettlement).UpdatePopType(Population.PopType.Slaves, count);
                }

                return true;
            }
        }
    }
}
