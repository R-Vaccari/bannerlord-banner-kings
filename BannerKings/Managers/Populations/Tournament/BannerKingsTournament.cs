using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.Source.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations.Tournament
{
    public class BannerKingsTournament : FightTournamentGame
    {
        [SaveableField(100)]
        private TournamentData data;

        public BannerKingsTournament(Town town, TournamentData data) : base(town)
        {
            this.data = data;
        }

        protected override ItemObject GetTournamentPrize(bool includePlayer, int lastRecordedNobleCountForTournamentPrize)
        {
            PopulationData popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(Town.Settlement);
            TournamentData tournament = popData.TournamentData;
            if (tournament != null)
            {
                data = tournament;
                return tournament.Prize;
            }

            return base.GetTournamentPrize(includePlayer, lastRecordedNobleCountForTournamentPrize);
        }
    }
}
