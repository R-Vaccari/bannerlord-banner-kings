using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;

namespace BannerKings.Managers.Kingdoms.Succession
{
    public class FeudalElectiveDecision : BKKingElectionDecision
    {
        public FeudalElectiveDecision(Clan proposerClan, FeudalTitle title) : base(proposerClan, null, title)
        {
        }

        public override bool IsAllowed()
        {
            return BannerKingsConfig.Instance.TitleModel.GetSuccessionCandidates(PreviousRuler, Title).Count > 1;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            foreach (var candidate in BannerKingsConfig.Instance.TitleModel.CalculateSuccessionLine(Title, PreviousRuler.Clan, PreviousRuler, 3))
            {
                yield return new KingSelectionDecisionOutcome(candidate.Key);
            }
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            base.ApplyChosenOutcome(chosenOutcome);
            Hero heir = ((KingSelectionDecisionOutcome)chosenOutcome).King;
            if (heir.Clan == PreviousRuler.Clan && !heir.IsClanLeader())
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(heir.Clan, heir);
            }
        }
    }
}
