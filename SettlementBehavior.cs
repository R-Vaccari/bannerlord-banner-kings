using Populations.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static Populations.Population;

namespace Populations.Behaviors
{
    public class SettlementBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailyTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameCreated));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void DailyTick(Settlement settlement)
        {
            if (settlement != null)
            {
                Population.UpdateSettlementPops(settlement);
                if (DecideSendSlaveCaravan(settlement))
                {
                    Village target = null;
                    MBReadOnlyList<Village> villages = settlement.BoundVillages;
                    foreach (Village village in villages)
                        if (village.Settlement != null && SlaveSurplusExists(village.Settlement)) 
                        {
                            target = village;
                            break;
                        }

                    if (target != null) SendSlaveCaravan(target.Settlement);
                }
            }
        }

        private bool DecideSendSlaveCaravan(Settlement settlement)
        {
            if (settlement.IsTown && settlement.Town != null)
            {
                MBReadOnlyList<Village> villages = settlement.BoundVillages;
                if (villages != null && villages.Count > 0)
                    if (SlaveSurplusExists(settlement))
                        return true;
            }
            return false;
        }

        private void SendSlaveCaravan(Settlement target)
        {
            DeductPopulation(target, PopType.Slaves, 50);
            MobileParty caravan = new MobileParty();
            TroopRoster guardRoster = new TroopRoster(target.Party);
            guardRoster.AddToCounts(CharacterObject.All.FirstOrDefault(x => x.StringId == "caravan_guard_empire"), 20);
            TroopRoster slaveRoster = new TroopRoster(target.Party);
            guardRoster.AddToCounts(CharacterObject.All.FirstOrDefault(x => x.StringId == "battanian_volunteer"), 50);

            caravan.InitializeMobileParty(guardRoster, slaveRoster, target.GatePosition, 0f);

            caravan.SetMoveGoToSettlement(target);
            caravan.Ai.SetAIState(AIState.VisitingVillage);
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

       
    }
}
