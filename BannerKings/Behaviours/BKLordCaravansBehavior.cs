using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours
{
    public class BKLordCaravansBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party == null || party.LeaderHero == null || !party.IsLordParty) return;

            Hero lord = party.LeaderHero;
            Kingdom kingdom = lord.Clan.Kingdom;
            if (kingdom == null || target.OwnerClan.Kingdom != kingdom) return;

            if (ShouldHaveCaravan(lord))
            {
                lord.ChangeHeroGold(-15000);
                CaravanPartyComponent.CreateCaravanParty(lord, target, false, null, null, 0);
            }
        }

        private bool ShouldHaveCaravan(Hero hero) => hero == hero.Clan.Leader && hero.Clan.Gold > 100000 && 
            hero.OwnedCaravans.Count < (int)(hero.Clan.Tier / 3f);
    }
}
