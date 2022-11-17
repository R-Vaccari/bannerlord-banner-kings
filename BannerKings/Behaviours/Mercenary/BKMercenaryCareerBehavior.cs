using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Behaviours.Mercenary
{
    internal class BKMercenaryCareerBehavior : CampaignBehaviorBase
    {
        private Dictionary<Clan, MercenaryCareer> careers;

        public override void RegisterEvents()
        {
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, OnClanChangedKingdom);
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, 
            ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
        {
            if (detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinAsMercenary)
            {
                AddCareer(clan, newKingdom);
            }
        }

        private void AddCareer(Clan clan, Kingdom kingdom)
        {
            if (!careers.ContainsKey(clan))
            {
                careers.Add(clan, new MercenaryCareer(clan, kingdom));
            }

            careers[clan].AddKingdom(kingdom);
        }

        private void InitCareers()
        {
            foreach (var clan in Clan.All)
            {
                if (clan.IsUnderMercenaryService)
                {
                    AddCareer(clan, clan.Kingdom);
                }
            }
        }
    }
}
