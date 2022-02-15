using BannerKings.Managers.Populations.Tournament;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKTournamentModel : DefaultTournamentModel
    {
        public override TournamentGame CreateTournament(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if (data != null)
                {
                    TournamentData tournamentData = data.TournamentData;
                    if (tournamentData != null)
                        return new BannerKingsTournament(town, tournamentData);
                }
            }
            return base.CreateTournament(town);
        }
    }
}
