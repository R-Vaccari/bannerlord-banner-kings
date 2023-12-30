using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;

namespace BannerKings.Campaign
{
    public class BKTournamentManager : TournamentManager
    {
        public new void GivePrizeToWinner(TournamentGame tournament, Hero winner, bool isPlayerParticipated)
        {
            if (tournament.Prize == null)
            {
                tournament.UpdateTournamentPrize(isPlayerParticipated, true);
            }

            if (!isPlayerParticipated)
            {
                tournament.UpdateTournamentPrize(isPlayerParticipated, false);
            }
            if (winner.PartyBelongedTo == MobileParty.MainParty)
            {
                winner.PartyBelongedTo.ItemRoster.AddToCounts(tournament.Prize, 1);
                return;
            }
            if (winner.Clan != null)
            {
                GiveGoldAction.ApplyBetweenCharacters(null, winner.Clan.Leader, tournament.Town.MarketData.GetPrice(tournament.Prize, null, false, null), false);
            }
        }
    }
}
