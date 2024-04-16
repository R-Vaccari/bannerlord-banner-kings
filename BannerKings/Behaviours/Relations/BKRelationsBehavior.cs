using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Traits;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace BannerKings.Behaviours.Relations
{
    public class BKRelationsBehavior : BannerKingsBehavior
    {
        private Dictionary<Hero, HeroRelations> relations;
        private Dictionary<Hero, CampaignTime> lastUpdated;

        public HeroRelations GetRelations(Hero hero)
        {
            if (!relations.ContainsKey(hero)) relations.Add(hero, new HeroRelations(hero));
            return relations[hero];
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, (starter) =>
            {
                SetRelations();
            });

            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, (starter) =>
            {
                SetRelations();
            });

            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, (Hero hero) =>
            {
                if (lastUpdated == null) lastUpdated = new Dictionary<Hero, CampaignTime>(Hero.AllAliveHeroes.Count);
                if (lastUpdated.ContainsKey(hero) && lastUpdated[hero].ElapsedWeeksUntilNow < 1f) return;
                RunWeekly(() =>
                {
                    HeroRelations relations = GetRelations(hero);
                    relations.UpdateRelations();
                    lastUpdated[hero] = CampaignTime.Now;
                },
                 GetType().Name,
                false);
            });

            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, (mapEvent) =>
            {
                if (mapEvent.HasWinner)
                {
                    foreach (var eventParty in mapEvent.PartiesOnSide(mapEvent.DefeatedSide))
                    {
                        PartyBase party = eventParty.Party;
                        if (party != null && party.IsMobile && party.LeaderHero != null)
                        {
                            int random = MBRandom.RandomInt(-8, -2);
                            random += (int)(party.LeaderHero.GetTraitLevel(BKTraits.Instance.Humble) * 2f);
                            random += party.LeaderHero.GetTraitLevel(DefaultTraits.Honor);

                            if (party.MobileParty.Army != null)
                            {
                                MobileParty leader = party.MobileParty.Army.LeaderParty;
                                if (leader != null && leader != party.MobileParty && leader.LeaderHero != null)
                                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(party.LeaderHero,
                                        leader.LeaderHero,
                                        random);
                            }
                        }
                    }
                }
            });

            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this,
                (settlement, openToClaim, newOwner, oldOwner, capturerHero, detail) =>
                {
                    if (detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege) return;

                    Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(newOwner);
                    foreach (Hero notable in settlement.Notables)
                    {
                        float relation = 0;
                        Religion notableRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(notable);
                        FaithStance stance = FaithStance.Tolerated;
                        if (notableRel != null)
                        {
                            stance = FaithStance.Untolerated;
                            if (rel != null)
                            {
                                stance = notableRel.GetStance(rel.Faith);
                            }
                        }

                        if (stance == FaithStance.Untolerated) relation -= 15f;
                        else if (stance == FaithStance.Hostile) relation -= 30f;

                        if (oldOwner.Culture == notable.Culture)
                        {
                            if (newOwner.Culture != notable.Culture)
                            {
                                relation -= 20f;
                            }
                        }
                        else if (newOwner.Culture == notable.Culture)
                        {
                            relation += 15f;
                        }

                        relation *= MBRandom.RandomFloatRanged(0.75f, 1.25f);
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(capturerHero, notable, (int)relation);
                    }
                });
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bk_hero_relations", ref relations);

            if (relations == null) relations = new Dictionary<Hero, HeroRelations>(500);
            if (lastUpdated == null) lastUpdated = new Dictionary<Hero, CampaignTime>(500);
        }

        private void SetRelations()
        {
            if (relations == null) relations = new Dictionary<Hero, HeroRelations>(Hero.AllAliveHeroes.Count);
            if (lastUpdated == null) lastUpdated = new Dictionary<Hero, CampaignTime>(Hero.AllAliveHeroes.Count);
            foreach (Hero hero in Hero.AllAliveHeroes)
                if (!relations.ContainsKey(hero)) relations.Add(hero, new HeroRelations(hero));

            foreach (Hero hero in Hero.DeadOrDisabledHeroes)
                if (hero.IsDead && relations.ContainsKey(hero)) relations.Remove(hero);
        }
    }
}
