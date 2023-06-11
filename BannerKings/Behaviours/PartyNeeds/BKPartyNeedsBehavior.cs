using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours.PartyNeeds
{
    public class BKPartyNeedsBehavior : BannerKingsBehavior
    {
        private Dictionary<MobileParty, PartySupplies> partyNeeds = new Dictionary<MobileParty, PartySupplies>();

        public PartySupplies GetPartySupplies(MobileParty party)
        {
            PartySupplies supplies;
            if (!partyNeeds.TryGetValue(party, out supplies))
            {
                supplies = null;
            }

            return supplies;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, OnPartyDailyTick);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnPartyDestroyed);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-party-supplies", ref partyNeeds);

            if (partyNeeds == null)
            {
                partyNeeds = new Dictionary<MobileParty, PartySupplies>();
            }
        }
        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (target == null || party == null || hero == null || !party.IsLordParty ||
                hero == Hero.MainHero || (!target.IsVillage && target.IsFortification))
            {
                return;
            }

            if (partyNeeds.ContainsKey(party))
            {
                partyNeeds[party].BuyItems();
            }
        }

        private void OnPartyDailyTick(MobileParty party)
        {
            AddPartyNeeds(party);
            if (partyNeeds.ContainsKey(party))
            {
                partyNeeds[party].Tick();
            }    
        }

        private void OnPartyDestroyed(MobileParty party, PartyBase destroyer)
        {
            if (partyNeeds.ContainsKey(party))
            {
                partyNeeds.Remove(party);
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            foreach (var party in MobileParty.AllLordParties)
            {
                AddPartyNeeds(party);
            }
        }

        private void AddPartyNeeds(MobileParty party)
        {
            if (!party.IsLordParty)
            {
                return;
            }

            if (!partyNeeds.ContainsKey(party))
            {
                partyNeeds.Add(party, new PartySupplies(party));
            }
        }
    }
}
