using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.Source.TournamentGames;
using TaleWorlds.Core;

namespace BannerKings.Managers.Populations.Tournament
{
    public class BannerKingsTournament : FightTournamentGame
    {

        private TournamentData data;

        public BannerKingsTournament(Town town, TournamentData data) : base(town)
        {
            this.data = data;
        }

        protected override ItemObject GetTournamentPrize(bool includePlayer, int lastRecordedNobleCountForTournamentPrize)
        {
            PopulationData popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(base.Town.Settlement);
            TournamentData tournament = popData.TournamentData;
            if (tournament != null)
            {
                this.data = tournament;
                return tournament.Prize;
            } else  
                return base.GetTournamentPrize(includePlayer, lastRecordedNobleCountForTournamentPrize);
        }
    }
}
