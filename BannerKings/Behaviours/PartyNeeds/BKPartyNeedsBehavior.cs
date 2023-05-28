using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Behaviours.PartyNeeds
{
    public class BKPartyNeedsBehavior : BannerKingsBehavior
    {
        private Dictionary<MobileParty, PartyNeeds> partyNeeds = new Dictionary<MobileParty, PartyNeeds>();

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, OnPartyDailyTick);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnPartyDestroyed);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {
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
                partyNeeds.Add(party, new PartyNeeds(party));
            }
        }
    }
}
