using BannerKings.Components;
using BannerKings.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Behaviours
{
    class BKPartyBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, new Action<MobileParty>(HourlyTickParty));
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, new Action<MobileParty, PartyBase>(OnMobilePartyDestroyed));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        private void HourlyTickParty(MobileParty party)
        {

            if (party != null && BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party))
            {
                PopulationPartyComponent component = (PopulationPartyComponent)party.PartyComponent;
                Settlement target = component._target;

                if (component is MilitiaComponent)
                {
                    MilitiaComponent militiaComponent = (MilitiaComponent)component;
                    AiBehavior behavior = militiaComponent.behavior;
                    if (behavior == AiBehavior.EscortParty)
                        party.SetMoveEscortParty(militiaComponent._escortTarget);
                    else party.SetMoveGoToSettlement(militiaComponent.OriginSettlement);
                    return;
                }

                if (target != null)
                {
                    float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(party, target);
                    if (distance <= 30f)
                        EnterSettlementAction.ApplyForParty(party, target);
                    
                    else
                    {
                        if (target.IsVillage)
                        {
                            party.Ai.SetAIState(AIState.VisitingVillage);
                            if (target.Village.VillageState == Village.VillageStates.Looted || target.Village.VillageState == Village.VillageStates.BeingRaided)
                                party.SetMoveModeHold();
                            else PartyKeepMoving(ref party, ref target);
                        }
                        else if (!target.IsVillage)
                        {
                            party.Ai.SetAIState(AIState.VisitingNearbyTown);
                            if (!target.IsUnderSiege)
                                PartyKeepMoving(ref party, ref target);
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

            /*
            if (party.StringId.Contains("slavecaravan") && party.Party != null && party.Party.NumberOfHealthyMembers == 0)
            {
                DestroyPartyAction.Apply(null, party);
                if (PopulationConfig.Instance.PopulationManager.IsPopulationParty(party))
                    PopulationConfig.Instance.PopulationManager.RemoveCaravan(party);
            }*/
        }

        private void PartyKeepMoving(ref MobileParty party, ref Settlement target)
        {
            if (target.IsVillage) party.Ai.SetAIState(AIState.VisitingVillage);
            else party.Ai.SetAIState(AIState.VisitingNearbyTown);
            party.SetMoveGoToSettlement(target);
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

                        Settlement target = MBRandom.ChooseWeighted<Settlement>(list);
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
                civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().FirstOrDefault(x => x.StringId == "villager_" + origin.Culture.StringId.ToString());
                count = MBRandom.RandomInt(30, 70);
                type = PopType.Serfs;
            }
            else if (random < 90)
            {
                civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().FirstOrDefault(x => x.StringId == "craftsman_" + origin.Culture.StringId.ToString());
                count = MBRandom.RandomInt(15, 30);
                type = PopType.Craftsmen;
            }
            else
            {
                civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().FirstOrDefault(x => x.StringId == "noble_" + origin.Culture.StringId.ToString());
                count = MBRandom.RandomInt(10, 15);
                type = PopType.Nobles;
            }

            name = "Travelling " + Helpers.Helpers.GetClassName(type, origin.Culture).ToString() + " from {0}";

            if (civilian != null)
                PopulationPartyComponent.CreateTravellerParty("travellers_", origin, target,
                  name, count, type, civilian);

        }

        private void SendSlaveCaravan(Village target)
        {
            Settlement origin = target.MarketTown.Settlement;
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            int slaves = (int)((double)data.GetTypeCount(PopType.Slaves) * 0.005d);
            data.UpdatePopType(PopType.Slaves, (int)((float)slaves * -1f));
            PopulationPartyComponent.CreateSlaveCaravan("slavecaravan_", origin, target.Settlement, "Slave Caravan from {0}", slaves);
        }

        public override void SyncData(IDataStore dataStore)
        {
 
        }
    }
}
