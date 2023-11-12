using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKRelationsBehavior : BannerKingsBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, (MapEvent mapEvent) =>
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
                (Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail) =>
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
        }
    }
}
