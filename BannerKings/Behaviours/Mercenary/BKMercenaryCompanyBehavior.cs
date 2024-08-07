using BannerKings.Components;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours.Mercenary
{
    public class BKMercenaryCompanyBehavior : BannerKingsBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, (Town town) =>
            {
                //FreeCompanyComponent.CreateFreeCompany(town.Settlement);
            });

            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, (CampaignGameStarter game) =>
            {
                List<MobileParty> mobileParties = new List<MobileParty>();
                foreach (var party in MobileParty.All)
                {
                    if (party.PartyComponent is FreeCompanyComponent)
                    {
                        mobileParties.Add(party);
                    }
                }

                foreach (var party in mobileParties)
                    DestroyPartyAction.Apply(null, party);
            });
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
