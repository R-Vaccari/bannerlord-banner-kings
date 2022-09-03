using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations.Tournament
{
    public class BannerKingsTournament : FightTournamentGame
    {
        [SaveableField(100)] private TournamentData data;

        public BannerKingsTournament(Town town, TournamentData data) : base(town)
        {
            this.data = data;
        }

        protected override ItemObject GetTournamentPrize(bool includePlayer, int lastRecordedNobleCountForTournamentPrize)
        {
            var popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(Town.Settlement);
            var tournament = popData.TournamentData;
            if (tournament != null)
            {
                data = tournament;
                return tournament.Prize;
            }

            return base.GetTournamentPrize(includePlayer, lastRecordedNobleCountForTournamentPrize);
        }
    }
}