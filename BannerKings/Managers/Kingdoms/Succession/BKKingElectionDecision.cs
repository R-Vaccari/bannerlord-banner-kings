using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Succession
{
    public class BKKingElectionDecision : KingSelectionKingdomDecision
    {
        [SaveableProperty(100)] protected FeudalTitle Title { get; set; }
        [SaveableProperty(101)] protected Hero PreviousRuler { get; set; }

        public BKKingElectionDecision(Clan proposerClan, Clan clanToExclude = null, FeudalTitle title = null, Hero previousRuler = null) 
            : base(proposerClan, clanToExclude)
        {
            Title = title;
            PreviousRuler = previousRuler;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            base.ApplyChosenOutcome(chosenOutcome);
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Kingdom);
            if (title != null)
            {
                var deJure = title.deJure;
                var king = ((KingSelectionDecisionOutcome)chosenOutcome).King;
                if (deJure != king)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(deJure, king, title);
                }
            }
        }
    }
}