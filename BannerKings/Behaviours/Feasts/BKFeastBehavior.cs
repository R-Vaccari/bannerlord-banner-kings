using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours.Feasts
{
    public class BKFeastBehavior : CampaignBehaviorBase
    {

        private Dictionary<Town, Feast> feasts = new Dictionary<Town, Feast>();

        public override void RegisterEvents()
        {
            
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public void LaunchFeast(Town town)
        {

            var list = new List<Clan>();
            foreach (var clan in town.OwnerClan.Kingdom.Clans)
            {
                list.Add(clan);
            }

            feasts.Add(town, new Feast(town.OwnerClan.Leader, 
                list,
                town, 
                CampaignTime.WeeksFromNow(1f)));
        }

        private void HourlyTickParty(MobileParty party)
        {
            if (!party.IsLordParty || party.LeaderHero == null || party.LeaderHero.Clan == Clan.PlayerClan)
            {
                return;
            }

            var clan = party.LeaderHero.Clan;
            var kingdom = clan.Kingdom;
            if (kingdom == null)
            {
                return;
            }

            foreach (var town in kingdom.Fiefs)
            {
                if (feasts.ContainsKey(town) && feasts[town].Guests.Contains(clan))
                {
                    if (party.CurrentSettlement != town.Settlement)
                    {
                        party.SetMoveGoToSettlement(town.Settlement);
                    }
                    else
                    {
                        party.Ai.DisableAi();
                    }
                }
            }
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null || !feasts.ContainsKey(target.Town))
            {
                return;
            }

            Utils.Helpers.AddMusicianToKeep(target);
            foreach (var notable in target.Notables)
            {
                Utils.Helpers.AddNotableToKeep(notable, target);
            }
        }
    }
}
