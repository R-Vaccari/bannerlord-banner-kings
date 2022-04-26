using BannerKings.Managers.Titles;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Kingdoms.Contract
{
    public abstract class BKContractDecision : KingdomDecision
    {
        [SaveableProperty(99)]
        protected FeudalTitle Title { get; set; }

        public BKContractDecision(Clan proposerClan, FeudalTitle title) : base(proposerClan)
        {
            this.Title = title;
        }

        public abstract float CalculateKingdomSupport(Kingdom kingdom);

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {

        }

        public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {

        }

        public override Clan DetermineChooser()
        {
            return base.Kingdom.RulingClan;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            yield break;
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            return 0;
        }

        public override TextObject GetChooseDescription()
        {
            TextObject textObject = new TextObject("{=!}As the sovereign of {KINGDOM}, you must decide whether to approve this contract change or not.", null);
            textObject.SetTextVariable("KINGDOM", this.Kingdom.Name);
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            return null;
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            return null;
        }

        public override TextObject GetGeneralTitle()
        {
            return null;
        }

        public override int GetProposalInfluenceCost()
        {
            return 0;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return null;
        }

        public override TextObject GetSecondaryEffects()
        {
            return null;
        }

        public override TextObject GetSupportDescription()
        {
            return null;
        }

        public override TextObject GetSupportTitle()
        {
            return null;
        }

        public override bool IsAllowed()
        {
            return this.Title != null && this.Title.contract != null;
        }
    }
}
