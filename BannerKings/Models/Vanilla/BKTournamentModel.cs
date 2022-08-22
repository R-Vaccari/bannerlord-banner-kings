using BannerKings.Managers.Populations.Tournament;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;

namespace BannerKings.Models.Vanilla
{
    public class BKTournamentModel : DefaultTournamentModel
    {
        public override TournamentGame CreateTournament(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var tournamentData = data?.TournamentData;
                if (tournamentData != null)
                {
                    return new BannerKingsTournament(town, tournamentData);
                }
            }

            return base.CreateTournament(town);
        }
    }
}