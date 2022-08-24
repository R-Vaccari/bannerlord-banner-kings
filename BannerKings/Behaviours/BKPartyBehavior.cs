using System;
using System.Linq;
using BannerKings.Components;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
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
            if (party == null || BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party))
            {
                return;
            }

            var component = (PopulationPartyComponent) party.PartyComponent;
            var target = component._target;

            switch (component)
            {
                case RetinueComponent:
                {
                    if (party.CurrentSettlement == null)
                    {
                        EnterSettlementAction.ApplyForParty(party, party.HomeSettlement);
                    }

                    party.SetMoveModeHold();
                    return;
                }
                case MilitiaComponent militiaComponent:
                {
                    var behavior = militiaComponent.Behavior;
                    if (behavior == AiBehavior.EscortParty)
                    {
                        party.SetMoveEscortParty(militiaComponent.Escort);
                    }
                    else
                    {
                        party.SetMoveGoToSettlement(militiaComponent.OriginSettlement);
                    }

                    return;
                }
            }

            if (target != null)
            {
                var distance = Campaign.Current.Models.MapDistanceModel.GetDistance(party, target);
                if (distance <= 10f)
                {
                    EnterSettlementAction.ApplyForParty(party, target);
                }

                else
                {
                    switch (target.IsVillage)
                    {
                        case true:
                        {
                            party.Ai.SetAIState(AIState.VisitingVillage);
                            if (target.Village.VillageState is Village.VillageStates.Looted or Village.VillageStates.BeingRaided)
                            {
                                party.SetMoveModeHold();
                            }
                            else
                            {
                                party.Ai.SetAIState(AIState.VisitingVillage);
                                party.SetMoveGoToSettlement(target);
                            }

                            break;
                        }
                        case false:
                        {
                            party.Ai.SetAIState(AIState.VisitingNearbyTown);
                            if (!target.IsUnderSiege)
                            {
                                party.Ai.SetAIState(AIState.VisitingNearbyTown);
                                party.SetMoveGoToSettlement(target);
                            }
                            else
                            {
                                party.SetMoveModeHold();
                            }

                            break;
                        }
                    }
                }
            }
            else if (target == null)
            {
                DestroyPartyAction.Apply(null, party);
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
            }
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement == null || BannerKingsConfig.Instance.PopulationManager == null)
            {
                return;
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_slaves_export") &&
                DecideSendSlaveCaravan(settlement))
            {
                var villages = settlement.BoundVillages;
                var target = villages.FirstOrDefault(village => village.Settlement != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement) && !BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(village.Settlement, PopType.Slaves));

                if (target != null)
                {
                    SendSlaveCaravan(target);
                }
            }

            // Send Travellers
            if (!settlement.IsTown)
            {
                return;
            }

            {
                var random = MBRandom.RandomInt(1, 100);
                if (random > 5)
                {
                    return;
                }

                var target = GetTownToTravel(settlement);
                if (target == null)
                {
                    return;
                }

                if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target) &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    SendTravellerParty(settlement, target);
                }
            }
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party == null || BannerKingsConfig.Instance.PopulationManager == null)
            {
                return;
            }

            if (!BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party))
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target);
            var component = (PopulationPartyComponent) party.PartyComponent;

            if (component is MilitiaComponent && target.IsVillage)
            {
                foreach (var element in party.MemberRoster.GetTroopRoster())
                {
                    target.MilitiaPartyComponent.MobileParty.MemberRoster.AddToCounts(element.Character,
                        element.Number);
                }

                if (party.PrisonRoster.TotalRegulars > 0)
                {
                    foreach (var element in party.PrisonRoster.GetTroopRoster().Where(element => !element.Character.IsHero))
                    {
                        data.UpdatePopType(PopType.Slaves, element.Number);
                    }
                }
            }

            if (component.slaveCaravan)
            {
                var slaves = Utils.Helpers.GetRosterCount(party.PrisonRoster);
                data.UpdatePopType(PopType.Slaves, slaves);
            }
            else if (component.popType != PopType.None)
            {
                var filter = component.popType == PopType.Serfs ? "villager" :
                    component.popType == PopType.Craftsmen ? "craftsman" : "noble";
                var pops = Utils.Helpers.GetRosterCount(party.MemberRoster, filter);
                data.UpdatePopType(component.popType, pops);
            }

            DestroyPartyAction.Apply(null, party);
            BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
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
            if (!settlement.IsTown || settlement.Town == null)
            {
                return false;
            }

            var villages = settlement.BoundVillages;
            return villages is {Count: > 0} && BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Slaves);
        }

        private Settlement GetTownToTravel(Settlement origin)
        {
            var kingdom = origin.OwnerClan?.Kingdom;
            if (kingdom?.Settlements == null || kingdom.Settlements.Count <= 1)
            {
                return null;
            }

            var list = (from settlement in kingdom.Settlements where settlement.IsTown && settlement != origin select new ValueTuple<Settlement, float>(settlement, 1f)).ToList();

            return MBRandom.ChooseWeighted(list);
        }

        private void SendTravellerParty(Settlement origin, Settlement target)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            var random = MBRandom.RandomInt(1, 100);
            CharacterObject civilian;
            PopType type;
            int count;
            switch (random)
            {
                case < 60:
                    civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()
                        .FirstOrDefault(x => x.StringId == "villager_" + origin.Culture.StringId);
                    count = MBRandom.RandomInt(30, 70);
                    type = PopType.Serfs;
                    break;
                case < 90:
                    civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()
                        .FirstOrDefault(x => x.StringId == "craftsman_" + origin.Culture.StringId);
                    count = MBRandom.RandomInt(15, 30);
                    type = PopType.Craftsmen;
                    break;
                default:
                    civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()
                        .FirstOrDefault(x => x.StringId == "noble_" + origin.Culture.StringId);
                    count = MBRandom.RandomInt(10, 15);
                    type = PopType.Nobles;
                    break;
            }

            var name = "Travelling " + Utils.Helpers.GetClassName(type, origin.Culture) + " from {0}";

            if (civilian != null)
            {
                PopulationPartyComponent.CreateTravellerParty("travellers_", origin, target,
                    name, count, type, civilian);
            }
        }

        private void SendSlaveCaravan(Village target)
        {
            var origin = target.Bound;
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            var slaves = (int) (data.GetTypeCount(PopType.Slaves) * 0.005d);
            data.UpdatePopType(PopType.Slaves, (int) (slaves * -1f));
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
                new TextObject("{=tOgaryTeT}Give me your slaves and gear, or else!").ToString(),
                slavecaravan_neutral_on_condition,
                null);

            starter.AddDialogLine("slavecaravan_party_threat_response", "slavecaravan_threat", "close_window",
                "One more for the mines! Lads, get the whip![rf:idle_angry][ib:aggressive]",
                null, delegate { PlayerEncounter.Current.IsEnemy = true; });

            starter.AddDialogLine("raised_militia_party_start", "start", "raised_militia_greeting",
                "M'lord! We are ready to serve you.",
                raised_militia_start_on_condition, null);

            starter.AddPlayerLine("raised_militia_party_follow", "raised_militia_greeting", "raised_militia_order",
                new TextObject("{=Y0vGy2wxM}Follow my company.").ToString(),
                raised_militia_order_on_condition,
                raised_militia_follow_on_consequence);

            starter.AddPlayerLine("raised_militia_party_retreat", "raised_militia_greeting", "raised_militia_order",
                new TextObject("{=BzMpGOgnT}You may go home.").ToString(),
                raised_militia_order_on_condition,
                raised_militia_retreat_on_consequence);

            starter.AddDialogLine("raised_militia_order_response", "raised_militia_order", "close_window",
                "Aye!",
                null, delegate { PlayerEncounter.LeaveEncounter = true; });
        }

        private bool IsTravellerParty(PartyBase party)
        {
            var value = false;
            if (party is not {MobileParty: { }})
            {
                return false;
            }

            if (BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
            {
                value = true;
            }

            return value;
        }

        private bool traveller_serf_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            if (component.popType == PopType.Serfs)
            {
                value = true;
            }

            return value;
        }

        private void raised_militia_retreat_on_consequence()
        {
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return;
            }

            var component = (MilitiaComponent) party.MobileParty.PartyComponent;
            component.Behavior = AiBehavior.GoToSettlement;
        }

        private void raised_militia_follow_on_consequence()
        {
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return;
            }

            var component = (MilitiaComponent) party.MobileParty.PartyComponent;
            component.Behavior = AiBehavior.EscortParty;
        }

        private bool raised_militia_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return value;
            }

            if (party.MobileParty.PartyComponent is MilitiaComponent)
            {
                value = true;
            }

            return value;
        }

        private bool raised_militia_order_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            if (party.MobileParty.PartyComponent is MilitiaComponent && party.Owner == Hero.MainHero)
            {
                value = true;
            }

            return value;
        }

        private bool traveller_craftsman_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            if (component.popType == PopType.Craftsmen)
            {
                value = true;
            }

            return value;
        }

        private bool traveller_noble_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            if (component.popType == PopType.Nobles)
            {
                value = true;
            }

            return value;
        }

        private bool traveller_aggression_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            var partyKingdom = component.OriginSettlement.OwnerClan.Kingdom;
            if (partyKingdom != null)
            {
                if (Hero.MainHero.Clan.Kingdom == null ||
                    component.OriginSettlement.OwnerClan.Kingdom != Hero.MainHero.Clan.Kingdom)
                {
                    value = true;
                }
            }

            return value;
        }

        private bool slavecaravan_neutral_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            var partyKingdom = component.OriginSettlement.OwnerClan.Kingdom;
            if (partyKingdom == null || !component.slaveCaravan)
            {
                return false;
            }

            if (Hero.MainHero.Clan.Kingdom == null ||
                component.OriginSettlement.OwnerClan.Kingdom != Hero.MainHero.Clan.Kingdom)
            {
                value = true;
            }

            return value;
        }

        private bool slavecaravan_amicable_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            var partyKingdom = component.OriginSettlement.OwnerClan.Kingdom;
            var heroKingdom = Hero.MainHero.Clan.Kingdom;
            if (component.slaveCaravan && ((partyKingdom != null && heroKingdom != null && partyKingdom == heroKingdom) || component.OriginSettlement.OwnerClan == Hero.MainHero.Clan))
            {
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
        internal class ApplyPatch
        {
            private static void Postfix(PartyBase destroyerParty, MobileParty destroyedParty)
            {
                Console.WriteLine(destroyedParty.Name);
            }
        }

        [HarmonyPatch(typeof(DestroyPartyAction), "ApplyForDisbanding")]
        internal class ApplyForDisbandingPatch
        {
            private static void Postfix(MobileParty disbandedParty, Settlement relatedSettlement)
            {
                Console.WriteLine(disbandedParty.Name);
            }
        }
    }
}