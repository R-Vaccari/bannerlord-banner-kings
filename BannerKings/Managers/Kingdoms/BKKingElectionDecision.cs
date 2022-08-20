using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;

namespace BannerKings.Managers.Kingdoms
{
    public class BKKingElectionDecision : KingSelectionKingdomDecision
    {
        public BKKingElectionDecision(Clan proposerClan, Clan clanToExclude = null) : base(proposerClan, clanToExclude)
        {
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            base.ApplyChosenOutcome(chosenOutcome);
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Kingdom);
            if (title != null)
            {
                var deJure = title.deJure;
                var king = ((KingSelectionDecisionOutcome) chosenOutcome).King;
                if (deJure != king)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(deJure, king, title);
                }
            }
        }
    }
}