using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Diplomacy
{
    public class BKDiplomacyBehavior : CampaignBehaviorBase
    {
        private Dictionary<Kingdom, KingdomDiplomacy> kingdomDiplomacies;

        public KingdomDiplomacy GetKingdomDiplomacy(Kingdom kingdom)
        {
            if (kingdom == null)
            {
                return null;
            }

            if (kingdomDiplomacies.ContainsKey(kingdom))
            {
                return kingdomDiplomacies[kingdom];
            }

            return null;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2)
        {
            if (faction1.IsKingdomFaction && faction2.IsKingdomFaction)
            {

            }
        }
    }
}
