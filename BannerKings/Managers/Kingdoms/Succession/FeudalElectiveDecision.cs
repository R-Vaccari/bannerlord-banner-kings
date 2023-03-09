using BannerKings.Managers.Titles;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
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
            return BannerKingsConfig.Instance.TitleModel.GetSuccessionCandidates(PreviousRuler, Title.contract).Count > 1;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            BannerKingsConfig.Instance.TitleModel.GetSuccessionCandidates(PreviousRuler, Title.contract);
            return base.DetermineInitialCandidates();
        }

        public override float CalculateMeritOfOutcome(DecisionOutcome candidateOutcome)
        {
            return base.CalculateMeritOfOutcome(candidateOutcome);
        }
    }
}
