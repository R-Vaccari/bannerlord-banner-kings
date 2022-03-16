using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Kingdoms
{
    public class KingdomGovernmentDecision : KingdomDecision
    {

        private GovernmentType governmentType;

        public KingdomGovernmentDecision(Clan proposerClan, GovernmentType governmentType) : base(proposerClan)
        {
            this.governmentType = governmentType;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            throw new NotImplementedException();
        }

        public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {
            throw new NotImplementedException();
        }

        public override Clan DetermineChooser()
        {
            return base.Kingdom.RulingClan;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            throw new NotImplementedException();
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            throw new NotImplementedException();
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetChooseDescription()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetChooseTitle()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetGeneralTitle()
        {
            throw new NotImplementedException();
        }

        public override int GetProposalInfluenceCost()
        {
            throw new NotImplementedException();
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetSecondaryEffects()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetSupportDescription()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetSupportTitle()
        {
            throw new NotImplementedException();
        }

        public override bool IsAllowed()
        {
            throw new NotImplementedException();
        }
    }
}
