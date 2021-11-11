using Helpers;
using Populations.UI;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;

namespace Behaviors
{
    public class SettlementBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailyTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameCreated));
        }

        private void DailyTick(Settlement settlement)
        {
            if (settlement != null) Populations.Population.UpdateSettlementPops(settlement);
        }

        private void OnGameCreated(CampaignGameStarter campaignGameStarter)
        {

                campaignGameStarter.AddGameMenuOption("town", "manage_population", "{=!}Manage population",
                    new GameMenuOption.OnConditionDelegate(game_menu_town_manage_town_on_condition),
                    new GameMenuOption.OnConsequenceDelegate(game_menu_town_manage_town_on_consequence), false, 5, false);
          
        }

        private static bool game_menu_town_manage_town_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && Populations.Population.IsSettlementPopulated(currentSettlement);
        }

        public static void game_menu_town_manage_town_on_consequence(MenuCallbackArgs args)
        {
            UIManager.instance.InitializeReligionWindow();
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
