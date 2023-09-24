using BannerKings.Managers.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Shipping
{
    public class BKShippingBehavior : BannerKingsBehavior
    {
        private Dictionary<MobileParty, Travel> sailing = new Dictionary<MobileParty, Travel>(20);

        private void AddParty(MobileParty party, Settlement destination, CampaignTime time)
        {
            sailing[party] = new Travel(party, time, destination);
        }

        public void RemoveParty(MobileParty party)
        {
            if (sailing.ContainsKey(party))
            {
                sailing.Remove(party);
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, AfterSettlementEntered);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, TickParty);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-travels", ref sailing);

            if (sailing == null) sailing = new Dictionary<MobileParty, Travel>(20);
        }

        public bool HasLanes(Settlement settlement) => DefaultShippingLanes.Instance.GetSettlementLanes(settlement).Count() > 0;
        public bool CanTravel(Settlement settlement, MobileParty party)
        {
            bool fief = settlement.Town != null ? !settlement.IsUnderSiege : settlement.Village.VillageState == Village.VillageStates.Normal;
            int price = CalculatePrice(settlement, party);
            bool gold = price <= party.PartyTradeGold || price <= party.LeaderHero?.Gold;

            return fief && gold && !sailing.ContainsKey(party);
        }

        public int CalculatePrice(Settlement settlement, MobileParty party)
        {
            float result = 0f;
            float distance = party.CurrentSettlement.GatePosition.Distance(settlement.GatePosition);
            result += distance * 10f;

            return MBRandom.RoundRandomized(result);
        }

        public CampaignTime CalculateArrival(Settlement settlement, MobileParty party)
        {
            float distance = party.CurrentSettlement.GatePosition.Distance(settlement.GatePosition);
            float days = distance / 75f;

            return CampaignTime.DaysFromNow(days);
        }

        public void SetTravel(MobileParty party, Settlement destination)
        {
            int price = CalculatePrice(destination, party);
            if (party.LeaderHero?.Gold >= price) party.LeaderHero.ChangeHeroGold(price);
            else party.PartyTradeGold -= price;

            Settlement current = party.CurrentSettlement;
            CampaignTime arrival = CalculateArrival(destination, party);
            LeaveSettlementAction.ApplyForParty(party);
            party.Party.UpdateVisibilityAndInspected(0f, true);
            party.IsActive = false;
            party.Ai.DisableAi();

            AddParty(party, destination, arrival);
            if (party.MemberRoster.Contains(Hero.MainHero.CharacterObject))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=!}{PARTY} is at {PLACE} and travelling to {FIEF} on water until {DATE}.")
                    .SetTextVariable("PARTY", party.Name)
                    .SetTextVariable("FIEF", destination.Name)
                    .SetTextVariable("PLACE", current.Name)
                    .SetTextVariable("DATE", arrival.ToString())
                    .ToString(),
                    Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
            }
        }

        private void FinishTravel(Travel travel)
        {
            bool teleportOutside = false;
            if (travel.Destination.Town != null && travel.Destination.IsUnderSiege) teleportOutside = true;
            else if (travel.Destination.IsVillage && travel.Destination.Village.VillageState != Village.VillageStates.Normal) teleportOutside = true;

            MobileParty party = travel.Party;
            if (teleportOutside) travel.Party.Position2D = travel.Destination.GatePosition;
            else EnterSettlementAction.ApplyForParty(party, travel.Destination);

            party.Party.UpdateVisibilityAndInspected(0f, false);
            party.IsActive = true;
            party.Ai.EnableAi();

            RemoveParty(travel.Party);
        }

        private void OnWeeklyTick()
        {
            foreach (ShippingLane lane in DefaultShippingLanes.Instance.All)
            {
                if (lane.Culture == null) continue;
                
                foreach (Settlement port in lane.Ports)
                {
                    if (!port.IsTown) continue;
                        
                    if (!port.Notables.Any(x => x.Culture == lane.Culture))
                    {
                        var merchant = lane.Culture.NotableAndWandererTemplates.FirstOrDefault(x => x.Occupation == Occupation.Merchant);
                        EnterSettlementAction.ApplyForCharacterOnly(HeroCreator
                            .CreateSpecialHero(merchant, port, null, null, 30), port);
                    }
                }
            }
        }

        private void AfterSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party == null || !party.IsCaravan) return;

            var lanes = DefaultShippingLanes.Instance.GetSettlementLanes(settlement);
            if (!lanes.Any()) return;

            Town town = null;
            try
            {
                CaravansCampaignBehavior behavior = Campaign.Current.GetCampaignBehavior<CaravansCampaignBehavior>();
                var thinkMethod = behavior.GetType().GetMethod("ThinkNextDestination",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                town = (Town)thinkMethod.Invoke(behavior, new object[] { party });
            } 
            catch (Exception e)
            {

            }
            
            if (town == null) return;

            party.Ai.SetMoveGoToSettlement(town.Settlement);
            if (town.Settlement == settlement) return;

            foreach (ShippingLane lane in lanes)
            {
                if (lane.Ports.Contains(party.TargetSettlement))
                {
                    if (CanTravel(party.TargetSettlement, party))
                    {
                        SetTravel(party, party.TargetSettlement);
                    }
                }
            }
        }

        private void TickParty(MobileParty party)
        {
            if (!party.IsCaravan) return;

            if (sailing.ContainsKey(party))
            {
                Travel travel = sailing[party];
                if (travel.Arrival.IsPast || travel.Arrival.IsNow)
                {
                    FinishTravel(travel);
                }
            }
        }
    }
}
