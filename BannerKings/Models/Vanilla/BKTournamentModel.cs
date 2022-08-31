using BannerKings.Managers.Education;
using BannerKings.Managers.Populations.Tournament;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
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

        public override int GetInfluenceReward(Hero winner, Town town)
        {
            var result = base.GetInfluenceReward(winner, town);
            if (winner != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(winner);
                if (education.HasPerk(BKPerks.Instance.GladiatorCrowdsFavorite))
                {
                    result += 10;
                }
            }

            return result;
        }

        public override int GetRenownReward(Hero winner, Town town)
        {
            var result =  base.GetRenownReward(winner, town);
            if (winner != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(winner);
                if (education.HasPerk(BKPerks.Instance.GladiatorCrowdsFavorite))
                {
                    result += 3;
                }
            }

            return result;
        }
    }
}