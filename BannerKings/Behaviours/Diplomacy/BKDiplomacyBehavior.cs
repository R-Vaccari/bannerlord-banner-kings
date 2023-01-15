using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;

namespace BannerKings.Behaviours.Diplomacy
{
    public class BKDiplomacyBehavior : BannerKingsBehavior
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
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.KingdomCreatedEvent.AddNonSerializedListener(this, OnKingdomCreated);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            InitializeDiplomacies();
        }

        private void InitializeDiplomacies()
        {
            foreach (var kingdom in Kingdom.All)
            {
                if (!kingdomDiplomacies.ContainsKey(kingdom))
                {
                    kingdomDiplomacies.Add(kingdom, new KingdomDiplomacy(kingdom));
                }
            }
        }

        private void OnKingdomCreated(Kingdom kingdom)
        {
            kingdomDiplomacies.Add(kingdom, new KingdomDiplomacy(kingdom));
        }

        private void OnDailyTickClan(Clan clan)
        {
            if (clan.Kingdom == null || clan == Clan.PlayerClan || clan != clan.Kingdom.RulingClan)
            {
                return;
            }

            RunWeekly(() =>
            {
                DiplomacyModel diplomacyModel = Campaign.Current.Models.DiplomacyModel;
                if (clan.Influence > (float)diplomacyModel.GetInfluenceCostOfProposingWar(clan.Kingdom))
                {
                    return;
                }
            },
            GetType().Name,
            false);
        }

        private bool ConsiderWar(Clan clan, Kingdom kingdom, IFaction otherFaction)
        {
            int num = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfProposingWar(kingdom) / 2;
            if (clan.Influence < (float)num)
            {
                return false;
            }
            DeclareWarDecision declareWarDecision = new DeclareWarDecision(clan, otherFaction);
            if (declareWarDecision.CalculateSupport(clan) > 50f)
            {
                float kingdomSupportForDecision = this.GetKingdomSupportForDecision(declareWarDecision);
                if (MBRandom.RandomFloat < 1.4f * kingdomSupportForDecision - 0.55f)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2)
        {
            if (faction1.IsKingdomFaction && faction2.IsKingdomFaction)
            {

            }
        }
    }
}
