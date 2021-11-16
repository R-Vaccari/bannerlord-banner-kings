using Populations.Components;
using Populations.UI;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static Populations.PopulationManager;

namespace Populations.Behaviors
{
    public class SettlementBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, new Action<MobileParty>(HourlyTickParty));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameCreated));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void HourlyTickParty(MobileParty caravan)
        {
            try
            {
                if (caravan != null && CARAVANS.ContainsKey(caravan) && caravan.MapEvent == null)
                {
                    Settlement target = CARAVANS[caravan];
                    if (caravan.Ai.AiState == AIState.VisitingVillage)
                    {
                        if (target != null && caravan.MapEvent == null && target.IsVillage && target.Village != null && target.Village.VillageState == Village.VillageStates.Normal)
                        {
                            if (Campaign.Current.Models.MapDistanceModel.GetDistance(caravan, target) <= 1.5f)
                            {
                                EnterSettlementAction.ApplyForParty(caravan, target);
                                int slaves = Helpers.Helpers.GetPrisionerCount(caravan.PrisonRoster);
                                PopulationData data = GetPopData(target);
                                data.UpdatePopType(PopType.Slaves, slaves);
                                DestroyPartyAction.Apply(null, caravan);
                                CARAVANS.Remove(caravan);
                                InformationManager.DisplayMessage(new InformationMessage(String.Format("{0} has received {1} slaves.",
                                    target.Name.ToString(), slaves)));
                            }
                            else caravan.SetMoveGoToSettlement(target);
                        }
                        else
                        {
                            DestroyPartyAction.Apply(null, caravan);
                            CARAVANS.Remove(caravan);
                        }

                    } else
                    {
                        if (Campaign.Current.Models.MapDistanceModel.GetDistance(caravan, target) <= 1.5f)
                        {
                            EnterSettlementAction.ApplyForParty(caravan, target);
                            int slaves = Helpers.Helpers.GetPrisionerCount(caravan.PrisonRoster);
                            PopulationData data = GetPopData(target);
                            data.UpdatePopType(PopType.Slaves, slaves);
                            DestroyPartyAction.Apply(null, caravan);
                            CARAVANS.Remove(caravan);
                            InformationManager.DisplayMessage(new InformationMessage(String.Format("{0} has received {1} slaves.",
                                target.Name.ToString(), slaves)));
                        } else
                        {
                            caravan.Ai.SetAIState(AIState.VisitingVillage);
                            caravan.SetMoveGoToSettlement(target);
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement != null)
            {
                UpdateSettlementPops(settlement);
                if (PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.EXPORT_SLAVES) && DecideSendSlaveCaravan(settlement))
                {
                    Village target = null;
                    MBReadOnlyList<Village> villages = settlement.BoundVillages;
                    foreach (Village village in villages)
                        if (village.Settlement != null && IsSettlementPopulated(village.Settlement) && !SlaveSurplusExists(village.Settlement)) 
                        {
                            target = village;
                            break;
                        }

                    if (target != null) SendSlaveCaravan(target);
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

        private void SendSlaveCaravan(Village target)
        {
            Settlement origin = target.MarketTown.Settlement;
            PopulationData data = GetPopData(origin);
            int slaves = (int)((double)data.GetTypeCount(PopType.Slaves) * 0.005d);
            data.UpdatePopType(PopType.Slaves, slaves * -1);

            MobileParty caravan = PopulationPartyComponent.CreateParty("slavecaravan_", origin, target.Settlement, "Slave Caravan from {0}");

            caravan.AddPrisoner(CharacterObject.All.FirstOrDefault(x => x.StringId == "vlandian_recruit_new"), slaves);

            caravan.InitializeMobileParty(origin.Culture.EliteCaravanPartyTemplate, origin.GatePosition, 0f, 0f, -1);
            caravan.Party.Visuals.SetMapIconAsDirty();
            //caravan.SetCustomName(new TaleWorlds.Localization.TextObject(
            //   String.Format("Slave Caravan from {0}", origin.Name.ToString())
            //   ));
            caravan.Party.Visuals.SetMapIconAsDirty();
            caravan.SetInititave(0f, 1f, float.MaxValue);
            caravan.ShouldJoinPlayerBattles = false;
            caravan.Aggressiveness = 0f;
            caravan.SetMoveGoToSettlement(target.Settlement);
            CARAVANS.Add(caravan, target.Settlement);
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
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && Populations.PopulationManager.IsSettlementPopulated(currentSettlement);
        }

        public static void game_menu_town_manage_town_on_consequence(MenuCallbackArgs args) => UIManager.instance.InitializeReligionWindow();
        

       
    }
}
