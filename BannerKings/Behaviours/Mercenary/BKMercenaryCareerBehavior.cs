using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Behaviours.Mercenary
{
    internal class BKMercenaryCareerBehavior : CampaignBehaviorBase
    {
        private Dictionary<Clan, MercenaryCareer> careers = new Dictionary<Clan, MercenaryCareer>();

        public MercenaryCareer GetCareer(Clan clan) 
        {
            if (careers.ContainsKey(clan))
            {
                return careers[clan];
            }

            return null;
        } 

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnClanDailyTick);
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, OnClanChangedKingdom);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-mercenary-careers", ref careers);

            if (careers == null)
            {
                careers = new Dictionary<Clan, MercenaryCareer>();
            }
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, 
            ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
        {
            if (detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinAsMercenary)
            {
                AddCareer(clan, newKingdom);
            }
        }

        private void OnClanDailyTick(Clan clan)
        {
            if (clan.IsUnderMercenaryService)
            {
                if (!careers.ContainsKey(clan))
                {
                    AddCareer(clan, clan.Kingdom);
                }

                careers[clan].Tick();
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
