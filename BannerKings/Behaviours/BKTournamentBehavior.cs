using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Behaviours
{
    public class BKTournamentBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.TournamentFinished.AddNonSerializedListener(this, OnTournamentFinished);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town,
            ItemObject prize)
        {
            if (BannerKingsConfig.Instance.PopulationManager == null)
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
            var tournament = data.TournamentData;
            if (tournament is {Active: true})
            {
                float price = town.MarketData.GetPrice(prize);
                var renown = -10f;
                if (price <= 10000)
                {
                    renown += price / 1000f;
                }
                else
                {
                    renown += price / 10000f;
                }

                GainRenownAction.Apply(Hero.MainHero, renown, true);
                InformationManager.DisplayMessage(new InformationMessage(string
                    .Format("Your prize of choice for the tournament at {0} has awarded you {1} renown", renown,
                        town.Name)));
                tournament.Active = false;
            }
        }
    }
}