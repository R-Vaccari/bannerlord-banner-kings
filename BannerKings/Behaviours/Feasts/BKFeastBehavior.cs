using BannerKings.Behaviours.Marriage;
using BannerKings.Managers.Goals.Decisions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Feasts
{
    public class BKFeastBehavior : CampaignBehaviorBase
    {
        private Dictionary<Town, Feast> feasts = new Dictionary<Town, Feast>();
        private Dictionary<Hero, CampaignTime> heroRecords = new Dictionary<Hero, CampaignTime>();

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnDailyTickTown);
            CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, OnSettlementHourlyTick);
            CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, OnMissionStarted);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-feasts", ref feasts);
            dataStore.SyncData("bannerkings-feast-records", ref heroRecords);

            if (feasts == null)
            {
                feasts = new Dictionary<Town, Feast>();
            }

            if (heroRecords == null)
            {
                heroRecords = new Dictionary<Hero, CampaignTime>();
            }
        }

        public void LaunchFeast(Town town, List<Clan> guests, MarriageContract marriage = null)
        {
            var feast = new Feast(town.OwnerClan.Leader,
                guests,
                town,
                CampaignTime.WeeksFromNow(1f),
                town.OwnerClan != Clan.PlayerClan,
                marriage);

            if (town.MapFaction == Hero.MainHero.MapFaction)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=Bj1LjLgm}The {CLAN} is now hosting a feast at {TOWN}!")
                                   .SetTextVariable("TOWN", town.Name)
                                   .SetTextVariable("CLAN", town.OwnerClan.Name));
            }

            feasts.Add(town, feast); 
            AddRecord(feast);
        }

        public void EndFeast(Feast feast, TextObject reason) 
        {
            feast.Finish(reason);
            feasts.Remove(feast.Town);  
        }

        private void AddRecord(Feast feast)
        {
            var hero = feast.Host;
            if (heroRecords.ContainsKey(hero))
            {
                heroRecords[hero] = CampaignTime.Now;
            }
            else
            {
                heroRecords.Add(hero, CampaignTime.Now);
            }
        }

        public CampaignTime LastHeroFeast(Hero hero)
        {
            if (heroRecords.ContainsKey(hero))
            {
                return heroRecords[hero];
            }

            return CampaignTime.Zero;
        }

        private void OnMissionStarted(IMission mission)
        {
            if (Hero.MainHero.CurrentSettlement != null)
            {
                var settlement = Hero.MainHero.CurrentSettlement;
                if (settlement.Town != null && feasts.ContainsKey(settlement.Town))
                {

                    SoundEvent.PlaySound2D(string.Format("event:/music/musicians/{0}/0{1}",
                        settlement.Culture.StringId,
                        MBRandom.RandomInt(1, 4)));
                }
            }
        }

        private void HourlyTickParty(MobileParty party)
        {
            if (!party.IsLordParty || party.LeaderHero == null || party.LeaderHero == Hero.MainHero)
            {
                return;
            }

            var clan = party.LeaderHero.Clan;
            if (clan == null)
            {
                return;
            }

            var kingdom = clan.Kingdom;
            if (kingdom == null)
            {
                return;
            }

            foreach (var town in kingdom.Fiefs)
            {
                if (feasts.ContainsKey(town) && (feasts[town].Guests.Contains(clan) || feasts[town].Host.Clan == clan))
                {
                    if (party.CurrentSettlement != town.Settlement)
                    {
                        party.Ai.DisableAi();
                        party.SetMoveGoToSettlement(town.Settlement);
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

            //Utils.Helpers.AddMusicianToKeep(target);
            foreach (var notable in target.Notables)
            {
                Utils.Helpers.AddNotableToKeep(notable, target);
            }
        }

        private void OnSettlementHourlyTick(Settlement settlement)
        {
            if (!IsFeastTown(settlement))
            {
                return;
            }

            foreach (var notable in settlement.Notables)
            {
                Utils.Helpers.AddNotableToKeep(notable, settlement);
            }

            Feast feast = feasts[settlement.Town];
            feast.Tick();
            if (feast.EndDate.IsPast)
            {
                EndFeast(feast, new TextObject());
            }

            if (settlement.OwnerClan != null && settlement.MapFaction.IsKingdomFaction)
            {
                if (FactionManager.GetEnemyKingdoms(settlement.MapFaction as Kingdom).Count() > 0)
                {
                    EndFeast(feast, new TextObject("{=!}The kingdom is at war!"));
                }
            }
        }

        private void OnDailyTickTown(Town town)
        {
            if (!IsFeastTown(town.Settlement))
            {
                return;
            }

            Feast feast = feasts[town];
            feast.Tick(false);
        }

        private void OnDailyTickClan(Clan clan)
        {
            if (clan.Kingdom != null && clan != Clan.PlayerClan && MBRandom.RandomFloat <= 0.05f)
            {
                var decision = new OrganizeFeastDecision(clan.Leader);
                decision.DoAiDecision();
            }
        }

        public bool IsFeastTown(Settlement settlement) => settlement.Town != null && feasts.ContainsKey(settlement.Town);
    }
}
