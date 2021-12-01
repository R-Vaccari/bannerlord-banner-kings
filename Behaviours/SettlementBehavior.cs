using Populations.Components;
using Populations.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

namespace Populations.Behaviors
{
    public class SettlementBehavior : CampaignBehaviorBase
    {

        private PopulationManager populationManager = null;
        private PolicyManager policyManager = null;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, new Action<MobileParty>(HourlyTickParty));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameCreated));
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, new Action<MobileParty, PartyBase>(this.OnMobilePartyDestroyed));
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving)
            {
                if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PolicyManager != null)
                {
                    populationManager = PopulationConfig.Instance.PopulationManager;
                    policyManager = PopulationConfig.Instance.PolicyManager;
                }
            }

            dataStore.SyncData("pops", ref populationManager);
            dataStore.SyncData("policies", ref policyManager);

            if (dataStore.IsLoading)
            {
                if (populationManager == null && policyManager == null)
                {
                    PopulationConfig.Instance.InitManagers(new Dictionary<Settlement, PopulationData>(), new List<MobileParty>(),
                    new Dictionary<Settlement, List<PolicyManager.PolicyElement>>(), new Dictionary<Settlement, PolicyManager.TaxType>(),
                    new Dictionary<Settlement, PolicyManager.MilitiaPolicy>(), new Dictionary<Settlement, WorkforcePolicy>());
                }
                else
                {
                    PopulationConfig.Instance.InitManagers(populationManager, policyManager);
                }
            }
        }

        private void HourlyTickParty(MobileParty party)
        {

            if (party != null && PopulationConfig.Instance.PopulationManager != null && 
                PopulationConfig.Instance.PopulationManager.IsPopulationParty(party) && party.MapEvent == null)
            {
                Settlement target = party.HomeSettlement;
                PopulationPartyComponent component = (PopulationPartyComponent)party.PartyComponent;

                if (target != null && party.MapEvent == null)
                {
                    if (component.slaveCaravan)
                    {
                        if (target.IsVillage && target.Village.VillageState == Village.VillageStates.Normal)
                        {
                            if (Campaign.Current.Models.MapDistanceModel.GetDistance(party, target) <= 1f)
                                PartyEnterSettlement(ref party, target, component.popType, component.slaveCaravan);
                            else PartyKeepMoving(ref party, target);
                        }
                        else party.SetMoveModeHold();

                    }
                    else if (!target.IsVillage && !target.IsUnderSiege)
                    {
                        if (Campaign.Current.Models.MapDistanceModel.GetDistance(party, target) <= 1f)
                            PartyEnterSettlement(ref party, target, component.popType, component.slaveCaravan);
                        else PartyKeepMoving(ref party, target);
                    }
                    else party.SetMoveModeHold();

                }
                else if (target == null)
                {
                    DestroyPartyAction.Apply(null, party);
                    PopulationConfig.Instance.PopulationManager.RemoveCaravan(party);
                }
            }
        }

        private void PartyKeepMoving(ref MobileParty party, Settlement target)
        {
            if (target.IsVillage) party.Ai.SetAIState(AIState.VisitingVillage);
            else party.Ai.SetAIState(AIState.VisitingNearbyTown);
            party.SetMoveGoToSettlement(target);
        }
        private void PartyEnterSettlement(ref MobileParty party, Settlement target, PopType popType, bool slaveCaravan)
        {
            EnterSettlementAction.ApplyForParty(party, target);
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(target);
            if (slaveCaravan)
            {
                int slaves = Helpers.Helpers.GetRosterCount(party.PrisonRoster);
                data.UpdatePopType(PopType.Slaves, slaves);
            }
            else if (popType != PopType.None)
            {
                int pops = Helpers.Helpers.GetRosterCount(party.MemberRoster);
                data.UpdatePopType(popType, pops);
            }

            DestroyPartyAction.Apply(null, party);
            PopulationConfig.Instance.PopulationManager.RemoveCaravan(party);
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement != null)
            {
                if (PopulationConfig.Instance.PopulationManager == null)
                    PopulationConfig.Instance.InitManagers(new Dictionary<Settlement, PopulationData>(), new List<MobileParty>(),
                    new Dictionary<Settlement, List<PolicyManager.PolicyElement>>(), new Dictionary<Settlement, PolicyManager.TaxType>(),
                    new Dictionary<Settlement, PolicyManager.MilitiaPolicy>(), new Dictionary<Settlement, WorkforcePolicy>());

                UpdateSettlementPops(settlement);
                InitializeSettlementPolicies(settlement);
                // Send Slaves
                if (PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.EXPORT_SLAVES) && DecideSendSlaveCaravan(settlement))
                {
                    Village target = null;
                    MBReadOnlyList<Village> villages = settlement.BoundVillages;
                    foreach (Village village in villages)
                        if (village.Settlement != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement) && !PopulationConfig.Instance.PopulationManager.PopSurplusExists(village.Settlement, PopType.Slaves))
                        {
                            target = village;
                            break;
                        }

                    if (target != null) SendSlaveCaravan(target);
                }

                // Send Travellers
                if (!settlement.IsVillage)
                {
                    int random = MBRandom.RandomInt(1, 100);
                    if (random == 1)
                    {
                        Settlement target = GetTownToTravel(settlement);
                        if (target != null)
                            if (PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(target) &&
                             PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                                SendTravellerParty(settlement, target);
                    }
                }
            }
        }

        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
        {
            if (mobileParty != null && PopulationConfig.Instance.PopulationManager != null &&
                PopulationConfig.Instance.PopulationManager.IsPopulationParty(mobileParty))
            {
                PopulationConfig.Instance.PopulationManager.RemoveCaravan(mobileParty);
            }
        }

        private bool DecideSendSlaveCaravan(Settlement settlement)
        {

            if (settlement.IsTown && settlement.Town != null)
            {
                MBReadOnlyList<Village> villages = settlement.BoundVillages;
                if (villages != null && villages.Count > 0)
                    if (PopulationConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Slaves))
                        return true;
            }
            return false;
        }

        private Settlement GetTownToTravel(Settlement origin)
        {
            Kingdom kingdom = origin.OwnerClan.Kingdom;
            if (kingdom != null)
            {
                if (kingdom.Settlements.Count > 1)
                {
                    Settlement target = MBRandom.ChooseWeighted<Settlement>(kingdom.Settlements, delegate (Settlement settlement)
                    {
                        if (settlement.IsTown && settlement != origin) return 1f;
                        else return 0f;
                    });
                    return target;
                }
            }

            return null;
        }

        private void SendTravellerParty(Settlement origin, Settlement target)
        {
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(origin);
            int random = MBRandom.RandomInt(1, 100);
            CharacterObject peasant = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().FirstOrDefault(x => x.StringId == "villager_" + origin.Culture.StringId.ToString());
            if (peasant != null)
            {
                PopType type;
                int count;
                string name;
                count = MBRandom.RandomInt(20, 50);
                type = PopType.Serfs;
                name = "Travelling " + Helpers.Helpers.GetCulturalClassName(type, origin.Culture) + " from {0}";
                /*
                if (random < 65)
                {

                }
                
                else if (random < 95)
                {
                    count = MBRandom.RandomInt(15, 30);
                    type = PopType.Craftsmen;
                    name = "Travelling craftsmen from {0}";
                } else
                {
                    count = MBRandom.RandomInt(5, 15);
                    type = PopType.Craftsmen;
                    name = "Travelling nobles from {0}";
                } */

                PopulationPartyComponent.CreateTravellerParty("travellers_", origin, target,
                    name, count, type, peasant);
            }
        }

        private void SendSlaveCaravan(Village target)
        {
            Settlement origin = target.MarketTown.Settlement;
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(origin);
            int slaves = (int)((double)data.GetTypeCount(PopType.Slaves) * 0.005d);
            data.UpdatePopType(PopType.Slaves, slaves * -1);

            PopulationPartyComponent.CreateSlaveCaravan("slavecaravan_", origin, target.Settlement, "Slave Caravan from {0}", slaves);

        }

        private void OnGameCreated(CampaignGameStarter campaignGameStarter)
        {
            AddDialog(campaignGameStarter);
            AddMenus(campaignGameStarter);

            if (PopulationConfig.Instance.PopulationManager != null)
                foreach (Settlement settlement in Settlement.All)
                    if (PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {
                        PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
                        if (data.Assimilation >= 1f)
                            settlement.Culture = settlement.Owner.Culture;
                    }
            
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town", "manage_population", "{=!}Manage population",
                new GameMenuOption.OnConditionDelegate(game_menu_town_manage_town_on_condition),
                new GameMenuOption.OnConsequenceDelegate(game_menu_town_manage_town_on_consequence), false, 5, false);

            campaignGameStarter.AddGameMenuOption("castle", "manage_population", "{=!}Manage population",
               new GameMenuOption.OnConditionDelegate(game_menu_town_manage_town_on_condition),
               new GameMenuOption.OnConsequenceDelegate(game_menu_town_manage_town_on_consequence), false, 3, false);

            //campaignGameStarter.AddGameMenuOption("village", "manage_population", "{=!}Manage population",
            //   new GameMenuOption.OnConditionDelegate(game_menu_town_manage_town_on_condition),
            //   new GameMenuOption.OnConsequenceDelegate(game_menu_town_manage_town_on_consequence), false, 5, false);
        }

        private static bool game_menu_town_manage_town_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement);
        }

        public static void game_menu_town_manage_town_on_consequence(MenuCallbackArgs args) => UIManager.instance.InitializePopulationWindow();

        private void AddDialog(CampaignGameStarter starter)
        {

            starter.AddDialogLine("traveller_serf_party_start", "start", "traveller_party_greeting", 
                "M'lord! We are humble folk, travelling between towns, looking for work and trade.", 
                new ConversationSentence.OnConditionDelegate(this.traveller_serf_start_on_condition), null, 100, null);

            starter.AddDialogLine("traveller_craftsman_party_start", "start", "traveller_party_greeting",
                "Good day to you. We are craftsmen travelling for business purposes.",
                new ConversationSentence.OnConditionDelegate(this.traveller_craftsman_start_on_condition), null, 100, null);

            starter.AddDialogLine("traveller_noble_party_start", "start", "traveller_party_greeting",
                "Yes? Please do not interfere with our caravan.",
                new ConversationSentence.OnConditionDelegate(this.traveller_noble_start_on_condition), null, 100, null);



            starter.AddPlayerLine("traveller_party_loot", "traveller_party_greeting", "close_window", 
                new TextObject("{=XaPMUJV0}Whatever you have, I'm taking it. Surrender or die!", null).ToString(),
                new ConversationSentence.OnConditionDelegate(this.traveller_aggression_on_condition), 
                delegate { PlayerEncounter.Current.IsEnemy = true; }, 
                100, null, null);

            starter.AddPlayerLine("traveller_party_leave", "traveller_party_greeting", "close_window",
                new TextObject("{=dialog_end_nice}Carry on, then. Farewell.", null).ToString(), null,
                delegate { PlayerEncounter.LeaveEncounter = true; },
                100, null, null);

            starter.AddDialogLine("slavecaravan_friend_party_start", "start", "slavecaravan_party_greeting",
                "My lord, we are taking these rabble somewhere they can be put to good use.",
                new ConversationSentence.OnConditionDelegate(this.slavecaravan_amicable_on_condition), null, 100, null);

            starter.AddDialogLine("slavecaravan_neutral_party_start", "start", "slavecaravan_party_greeting",
                "If you're not planning to join those vermin back there, move away![rf:idle_angry][ib:aggressive]",
                new ConversationSentence.OnConditionDelegate(this.slavecaravan_neutral_on_condition), null, 100, null);

            starter.AddPlayerLine("slavecaravan_party_leave", "slavecaravan_party_greeting", "close_window",
               new TextObject("{=dialog_end_nice}Carry on, then. Farewell.", null).ToString(), null,
               delegate { PlayerEncounter.LeaveEncounter = true; },
               100, null, null);

            starter.AddPlayerLine("slavecaravan_party_threat", "slavecaravan_party_greeting", "slavecaravan_threat",
               new TextObject("{=!}Give me your slaves and gear, or else!", null).ToString(),
               new ConversationSentence.OnConditionDelegate(this.slavecaravan_neutral_on_condition),
               null, 100, null, null);

            starter.AddDialogLine("slavecaravan_party_threat_response", "slavecaravan_threat", "close_window",
                "One more for the mines! Lads, get the whip![rf:idle_angry][ib:aggressive]",
                null, delegate { PlayerEncounter.Current.IsEnemy = true; }, 100, null);
        }

        private bool IsTravellerParty(PartyBase party)
        {
            bool value = false;
            if (party != null && party.MobileParty != null)
                if (PopulationConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
                    value = true;
            return value;
        }

        private bool traveller_serf_start_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.MobileParty.PartyComponent;
                if (component.popType == PopType.Serfs)
                    value = true;
            }
    
            return value;
        }

        private bool traveller_craftsman_start_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.MobileParty.PartyComponent;
                if (component.popType == PopType.Serfs)
                    value = true;
            }

            return value;
        }

        private bool traveller_noble_start_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.MobileParty.PartyComponent;
                if (component.popType == PopType.Nobles)
                    value = true;
            }

            return value;
        }

        private bool traveller_aggression_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.MobileParty.PartyComponent;
                Kingdom partyKingdom = component.OriginSettlement.OwnerClan.Kingdom;
                if (partyKingdom != null)
                    if (Hero.MainHero.Clan.Kingdom == null || component.OriginSettlement.OwnerClan.Kingdom != Hero.MainHero.Clan.Kingdom)
                        value = true;
            }

            return value;
        }

        private bool slavecaravan_neutral_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.MobileParty.PartyComponent;
                Kingdom partyKingdom = component.OriginSettlement.OwnerClan.Kingdom;
                if (partyKingdom != null && component.slaveCaravan)
                    if (Hero.MainHero.Clan.Kingdom == null || component.OriginSettlement.OwnerClan.Kingdom != Hero.MainHero.Clan.Kingdom)
                        value = true;
            }

            return value;
        }

        private bool slavecaravan_amicable_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.MobileParty.PartyComponent;
                Kingdom partyKingdom = component.OriginSettlement.OwnerClan.Kingdom;
                Kingdom heroKingdom = Hero.MainHero.Clan.Kingdom;
                if (component.slaveCaravan && ((partyKingdom != null && heroKingdom != null && partyKingdom == heroKingdom) 
                    || (component.OriginSettlement.OwnerClan == Hero.MainHero.Clan))) 
                    value = true;
            }

            return value;
        }
    }
}
