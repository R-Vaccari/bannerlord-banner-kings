using BannerKings.Components;
using BannerKings.Populations;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Behaviours
{
    public class BKPartyBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        private void HourlyTickParty(MobileParty party)
        {

            if (party != null && BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.PartyComponent;
                Settlement target = component._target;

                if (component is RetinueComponent)
                {
                    if (party.CurrentSettlement == null)
                        EnterSettlementAction.ApplyForParty(party, party.HomeSettlement);

                    party.SetMoveModeHold();
                    return;
                }

                if (component is MilitiaComponent)
                {
                    MilitiaComponent militiaComponent = (MilitiaComponent)component;
                    AiBehavior behavior = militiaComponent.Behavior;
                    if (behavior == AiBehavior.EscortParty)
                        party.SetMoveEscortParty(militiaComponent.Escort);
                    else party.SetMoveGoToSettlement(militiaComponent.OriginSettlement);
                    return;
                }

                if (target != null)
                {
                    float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(party, target);
                    if (distance <= 10f)
                        EnterSettlementAction.ApplyForParty(party, target);
                    
                    else
                    {
                        if (target.IsVillage)
                        {
                            party.Ai.SetAIState(AIState.VisitingVillage);
                            if (target.Village.VillageState == Village.VillageStates.Looted || target.Village.VillageState == Village.VillageStates.BeingRaided)
                                party.SetMoveModeHold();
                            else 
                            {
                                party.Ai.SetAIState(AIState.VisitingVillage);
                                party.SetMoveGoToSettlement(target);
                            }
                        }
                        else if (!target.IsVillage)
                        {
                            party.Ai.SetAIState(AIState.VisitingNearbyTown);
                            if (!target.IsUnderSiege)
                            {
                                party.Ai.SetAIState(AIState.VisitingNearbyTown);
                                party.SetMoveGoToSettlement(target);
                            }
                            else party.SetMoveModeHold();
                        }
                    }
                }
                else if (target == null)
                {
                    DestroyPartyAction.Apply(null, party);
                    BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
                }
            }
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement == null || BannerKingsConfig.Instance.PopulationManager == null) return;

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_slaves_export") && DecideSendSlaveCaravan(settlement))
            {
                Village target = null;
                MBReadOnlyList<Village> villages = settlement.BoundVillages;
                foreach (Village village in villages)
                    if (village.Settlement != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement) && !BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(village.Settlement, PopType.Slaves))
                    {
                        target = village;
                        break;
                    }

                if (target != null) SendSlaveCaravan(target);
            }

            // Send Travellers
            if (settlement.IsTown)
            {
                int random = MBRandom.RandomInt(1, 100);
                if (random <= 5)
                {
                    Settlement target = GetTownToTravel(settlement);
                    if (target != null)
                        if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target) &&
                            BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                            SendTravellerParty(settlement, target);
                }
            }
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party != null && BannerKingsConfig.Instance.PopulationManager != null)
            {
                if (BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target);
                    PopulationPartyComponent component = (PopulationPartyComponent)party.PartyComponent;

                    if (component is MilitiaComponent && target.IsVillage)
                    {
                        foreach (TroopRosterElement element in party.MemberRoster.GetTroopRoster())
                            target.MilitiaPartyComponent.MobileParty.MemberRoster.AddToCounts(element.Character, element.Number);
                        if (party.PrisonRoster.TotalRegulars > 0)
                            foreach (TroopRosterElement element in party.PrisonRoster.GetTroopRoster())
                                if (!element.Character.IsHero) data.UpdatePopType(PopType.Slaves, element.Number);
                    }

                    if (component.slaveCaravan)
                    {
                        int slaves = Helpers.Helpers.GetRosterCount(party.PrisonRoster);
                        data.UpdatePopType(PopType.Slaves, slaves);
                    }
                    else if (component.popType != PopType.None)
                    {
                        string filter = component.popType == PopType.Serfs ? "villager" : (component.popType == PopType.Craftsmen ? "craftsman" : "noble");
                        int pops = Helpers.Helpers.GetRosterCount(party.MemberRoster, filter);
                        data.UpdatePopType(component.popType, pops);
                    }

                    DestroyPartyAction.Apply(null, party);
                    BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
                }
            }
        }

        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
        {
            if (mobileParty != null && BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(mobileParty))
            {
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(mobileParty);
            }
        }

        private bool DecideSendSlaveCaravan(Settlement settlement)
        {

            if (settlement.IsTown && settlement.Town != null)
            {
                MBReadOnlyList<Village> villages = settlement.BoundVillages;
                if (villages != null && villages.Count > 0)
                    if (BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Slaves))
                        return true;
            }
            return false;
        }

        private Settlement GetTownToTravel(Settlement origin)
        {
            if (origin.OwnerClan != null)
            {
                Kingdom kingdom = origin.OwnerClan.Kingdom;
                if (kingdom != null)
                {
                    if (kingdom.Settlements != null && kingdom.Settlements.Count > 1)
                    {
                        List<ValueTuple<Settlement, float>> list = new List<ValueTuple<Settlement, float>>();
                        foreach (Settlement settlement in kingdom.Settlements)
                            if (settlement.IsTown && settlement != origin)
                                list.Add(new ValueTuple<Settlement, float>(settlement, 1f));

                        Settlement target = MBRandom.ChooseWeighted(list);
                        return target;
                    }
                }
            }

            return null;
        }

        private void SendTravellerParty(Settlement origin, Settlement target)
        {
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            int random = MBRandom.RandomInt(1, 100);
            CharacterObject civilian;
            PopType type;
            int count;
            string name;
            if (random < 60)
            {
                civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().FirstOrDefault(x => x.StringId == "villager_" + origin.Culture.StringId);
                count = MBRandom.RandomInt(30, 70);
                type = PopType.Serfs;
            }
            else if (random < 90)
            {
                civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().FirstOrDefault(x => x.StringId == "craftsman_" + origin.Culture.StringId);
                count = MBRandom.RandomInt(15, 30);
                type = PopType.Craftsmen;
            }
            else
            {
                civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().FirstOrDefault(x => x.StringId == "noble_" + origin.Culture.StringId);
                count = MBRandom.RandomInt(10, 15);
                type = PopType.Nobles;
            }

            name = "Travelling " + Helpers.Helpers.GetClassName(type, origin.Culture) + " from {0}";

            if (civilian != null)
                PopulationPartyComponent.CreateTravellerParty("travellers_", origin, target,
                  name, count, type, civilian);

        }

        private void SendSlaveCaravan(Village target)
        {
            Settlement origin = target.MarketTown.Settlement;
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            int slaves = (int)(data.GetTypeCount(PopType.Slaves) * 0.005d);
            data.UpdatePopType(PopType.Slaves, (int)(slaves * -1f));
            PopulationPartyComponent.CreateSlaveCaravan("slavecaravan_", origin, target.Settlement, "Slave Caravan from {0}", slaves);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialog(campaignGameStarter);
        }

        private void AddDialog(CampaignGameStarter starter)
        {

            starter.AddDialogLine("traveller_serf_party_start", "start", "traveller_party_greeting",
                "M'lord! We are humble folk, travelling between towns, looking for work and trade.",
                traveller_serf_start_on_condition, null);

            starter.AddDialogLine("traveller_craftsman_party_start", "start", "traveller_party_greeting",
                "Good day to you. We are craftsmen travelling for business purposes.",
                traveller_craftsman_start_on_condition, null);

            starter.AddDialogLine("traveller_noble_party_start", "start", "traveller_party_greeting",
                "Yes? Please do not interfere with our caravan.",
                traveller_noble_start_on_condition, null);


            starter.AddPlayerLine("traveller_party_loot", "traveller_party_greeting", "close_window",
                new TextObject("{=XaPMUJV0}Whatever you have, I'm taking it. Surrender or die!").ToString(),
                traveller_aggression_on_condition,
                delegate { PlayerEncounter.Current.IsEnemy = true; });

            starter.AddPlayerLine("traveller_party_leave", "traveller_party_greeting", "close_window",
                new TextObject("{=dialog_end_nice}Carry on, then. Farewell.").ToString(), null,
                delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddDialogLine("slavecaravan_friend_party_start", "start", "slavecaravan_party_greeting",
                "My lord, we are taking these rabble somewhere they can be put to good use.",
                slavecaravan_amicable_on_condition, null);

            starter.AddDialogLine("slavecaravan_neutral_party_start", "start", "slavecaravan_party_greeting",
                "If you're not planning to join those vermin back there, move away![rf:idle_angry][ib:aggressive]",
                slavecaravan_neutral_on_condition, null);

            starter.AddPlayerLine("slavecaravan_party_leave", "slavecaravan_party_greeting", "close_window",
               new TextObject("{=dialog_end_nice}Carry on, then. Farewell.").ToString(), null,
               delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddPlayerLine("slavecaravan_party_threat", "slavecaravan_party_greeting", "slavecaravan_threat",
               new TextObject("{=!}Give me your slaves and gear, or else!").ToString(),
               slavecaravan_neutral_on_condition,
               null);

            starter.AddDialogLine("slavecaravan_party_threat_response", "slavecaravan_threat", "close_window",
                "One more for the mines! Lads, get the whip![rf:idle_angry][ib:aggressive]",
                null, delegate { PlayerEncounter.Current.IsEnemy = true; });

            starter.AddDialogLine("raised_militia_party_start", "start", "raised_militia_greeting",
                "M'lord! We are ready to serve you.",
                raised_militia_start_on_condition, null);

            starter.AddPlayerLine("raised_militia_party_follow", "raised_militia_greeting", "raised_militia_order",
               new TextObject("{=!}Follow my company.").ToString(),
               raised_militia_order_on_condition,
               raised_militia_follow_on_consequence);

            starter.AddPlayerLine("raised_militia_party_retreat", "raised_militia_greeting", "raised_militia_order",
               new TextObject("{=!}You may go home.").ToString(),
               raised_militia_order_on_condition,
               raised_militia_retreat_on_consequence);

            starter.AddDialogLine("raised_militia_order_response", "raised_militia_order", "close_window",
                "Aye!",
                null, delegate { PlayerEncounter.LeaveEncounter = true; });
        }

        private bool IsTravellerParty(PartyBase party)
        {
            bool value = false;
            if (party != null && party.MobileParty != null)
                if (BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
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

        private void raised_militia_retreat_on_consequence()
        {
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                MilitiaComponent component = (MilitiaComponent)party.MobileParty.PartyComponent;
                component.Behavior = AiBehavior.GoToSettlement;
            }
        }

        private void raised_militia_follow_on_consequence()
        {
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                MilitiaComponent component = (MilitiaComponent)party.MobileParty.PartyComponent;
                component.Behavior = AiBehavior.EscortParty;
            }
        }

        private bool raised_militia_start_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
                if (party.MobileParty.PartyComponent is MilitiaComponent)
                    value = true;

            return value;
        }

        private bool raised_militia_order_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
                if (party.MobileParty.PartyComponent is MilitiaComponent && party.Owner == Hero.MainHero)
                    value = true;

            return value;
        }

        private bool traveller_craftsman_start_on_condition()
        {
            bool value = false;
            PartyBase party = PlayerEncounter.EncounteredParty;
            if (IsTravellerParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.MobileParty.PartyComponent;
                if (component.popType == PopType.Craftsmen)
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

        public override void SyncData(IDataStore dataStore)
        {
 
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(DestroyPartyAction), "Apply")]
        class ApplyPatch
        {
            static void Postfix(PartyBase destroyerParty, MobileParty destroyedParty)
            {
                Console.WriteLine(destroyedParty.Name);
            }
        }

        [HarmonyPatch(typeof(DestroyPartyAction), "ApplyForDisbanding")]
        class ApplyForDisbandingPatch
        {
            static void Postfix(MobileParty disbandedParty, Settlement relatedSettlement)
            {
                Console.WriteLine(disbandedParty.Name);
            }
        }
    }
}
