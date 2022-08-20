using System.Collections.Generic;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Contract;

public abstract class BKContractDecision : KingdomDecision
{
    public BKContractDecision(Clan proposerClan, FeudalTitle title) : base(proposerClan)
    {
        Title = title;
    }

    [SaveableProperty(99)] protected FeudalTitle Title { get; set; }

    public abstract float CalculateKingdomSupport(Kingdom kingdom);

    public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
    {
    }

    public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
    {
    }

    public override Clan DetermineChooser()
    {
        return Kingdom.RulingClan;
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
        var textObject =
            new TextObject(
                "{=!}As the sovereign of {KINGDOM}, you must decide whether to approve this contract change or not.");
        textObject.SetTextVariable("KINGDOM", Kingdom.Name);
        return textObject;
    }

    public override TextObject GetChooseTitle()
    {
        return null;
    }

    public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus,
        bool isShortVersion = false)
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
        return Title != null && Title.contract != null;
    }
}