using Populations.UI;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, new Action<MobileParty>(this.HourlyTickParty));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameCreated));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void HourlyTickParty(MobileParty caravan)
        {
            if (CARAVANS.ContainsKey(caravan))
            {
                Settlement target = CARAVANS[caravan];
                if (Campaign.Current.Models.MapDistanceModel.GetDistance(caravan, target) < 1f)
                {
                    EnterSettlementAction.ApplyForParty(caravan, target);
                    caravan.PrisonRoster.
                    PopulationData data = GetPopData(target);

                }
            }
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement != null)
            {
                UpdateSettlementPops(settlement);
                if (DecideSendSlaveCaravan(settlement))
                {
                    Village target = null;
                    MBReadOnlyList<Village> villages = settlement.BoundVillages;
                    foreach (Village village in villages)
                        if (village.Settlement != null && IsSettlementPopulated(village.Settlement) && SlaveSurplusExists(village.Settlement)) 
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
            int slaves = MBRandom.RandomInt(25, 60);
            PopulationData data = GetPopData(target);
            data.UpdatePopType(PopType.Slaves, slaves);

            MobileParty caravan = new MobileParty();
            TroopRoster guardRoster = new TroopRoster(target.Party);
            guardRoster.AddToCounts(CharacterObject.All.FirstOrDefault(x => x.StringId == "caravan_guard_empire"), MBRandom.RandomInt(15, 25));
            TroopRoster slaveRoster = new TroopRoster(target.Party);
            slaveRoster.AddToCounts(CharacterObject.All.FirstOrDefault(x => x.StringId == "battanian_volunteer"), slaves);

            caravan.Aggressiveness = 0f;
            caravan.SetCustomName(new TaleWorlds.Localization.TextObject(
                String.Format("Slave Caravan from {0}", target.Name.ToString())
                ));
            caravan.InitializeMobileParty(guardRoster, slaveRoster, target.GatePosition, 0f);
            caravan.SetMoveGoToSettlement(target);
            caravan.Ai.SetAIState(AIState.VisitingVillage);
            CARAVANS.Add(caravan, target);
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

        public static void game_menu_town_manage_town_on_consequence(MenuCallbackArgs args) => UIManager.instance.InitializeReligionWindow();
        

       
    }
}
