using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Contract;

public class BKGovernmentDecision : BKContractDecision
{
    public BKGovernmentDecision(Clan proposerClan, GovernmentType governmentType, FeudalTitle title) : base(
        proposerClan, title)
    {
        this.governmentType = governmentType;
    }

    [SaveableProperty(100)] private GovernmentType governmentType { get; }

    public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
    {
        var newGovernment = (chosenOutcome as GovernmentDecisionOutcome).ShouldDecisionBeEnforced;
        if (newGovernment)
        {
            Title.ChangeContract(governmentType);
        }
    }

    public override float CalculateKingdomSupport(Kingdom kingdom)
    {
        var support = 0f;
        float clans = 0;
        foreach (var clan in kingdom.Clans)
        {
            if (!clan.IsUnderMercenaryService)
            {
                if (clan == Clan.PlayerClan)
                {
                    support += 100f;
                }
                else
                {
                    support += DetermineSupport(clan, new GovernmentDecisionOutcome(true));
                }

                clans++;
            }
        }

        return MBMath.ClampFloat(support / clans, 0f, 100f);
    }

    public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
    {
        var newGovernment = (chosenOutcome as GovernmentDecisionOutcome).ShouldDecisionBeEnforced;
        if (newGovernment)
        {
            var succession = Title.contract.Succession;
            if (governmentType == GovernmentType.Imperial)
            {
                if (succession != SuccessionType.Imperial)
                {
                    succession = SuccessionType.Imperial;
                }
            }
            else if (governmentType == GovernmentType.Feudal)
            {
                if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
                {
                    succession = SuccessionType.Hereditary_Monarchy;
                }
            }
            else if (governmentType == GovernmentType.Tribal)
            {
                if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
                {
                    succession = SuccessionType.Elective_Monarchy;
                }
            }
            else if (governmentType == GovernmentType.Republic)
            {
                if (succession != SuccessionType.Republic)
                {
                    succession = SuccessionType.Republic;
                }
            }

            if (succession != Title.contract.Succession)
            {
                Title.ChangeContract(succession);
            }
        }
    }

    public override TextObject GetSecondaryEffects()
    {
        TextObject effects = null;
        var succession = Title.contract.Succession;
        if (governmentType == GovernmentType.Imperial)
        {
            if (succession != SuccessionType.Imperial)
            {
                effects = new TextObject("{=!}Succession will be changed to Imperial succession.");
            }
        }
        else if (governmentType == GovernmentType.Feudal)
        {
            if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
            {
                effects = new TextObject("{=!}Succession will be changed to Hereditary Monarchy succession.");
            }
        }
        else if (governmentType == GovernmentType.Tribal)
        {
            if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
            {
                effects = new TextObject("{=!}Succession will be changed to Elective Monarchy succession.");
            }
        }
        else if (governmentType == GovernmentType.Republic)
        {
            if (succession != SuccessionType.Republic)
            {
                effects = new TextObject("{=!}Succession will be changed to Republican succession.");
            }
        }

        return effects;
    }

    public override Clan DetermineChooser()
    {
        return Kingdom.RulingClan;
    }

    public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
    {
        yield return new GovernmentDecisionOutcome(true);
        yield return new GovernmentDecisionOutcome(false);
    }

    public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
    {
        foreach (var decisionOutcome in possibleOutcomes)
        {
            if (((GovernmentDecisionOutcome) decisionOutcome).ShouldDecisionBeEnforced)
            {
                decisionOutcome.SetSponsor(ProposerClan);
            }

            else
            {
                AssignDefaultSponsor(decisionOutcome);
            }
        }
    }

    public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
    {
        var policyDecisionOutcome = possibleOutcome as GovernmentDecisionOutcome;
        float authoritarian = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
        float egalitarian = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
        float oligarchic = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
        var weights = GetWeights();

        var num = weights[0] * authoritarian;
        var num2 = weights[1] * oligarchic;
        var num3 = weights[2] * egalitarian;

        var num4 = num + num3 + num2;

        if (clan == Kingdom.RulingClan)
        {
            if (governmentType == GovernmentType.Imperial && policyDecisionOutcome.ShouldDecisionBeEnforced)
            {
                num4 += 2f;
            }
            else if (governmentType == GovernmentType.Republic && policyDecisionOutcome.ShouldDecisionBeEnforced)
            {
                num4 -= 2f;
            }
        }

        float num5;
        if (policyDecisionOutcome.ShouldDecisionBeEnforced)
        {
            num5 = 60f;
        }
        else
        {
            num5 = -100f;
        }

        return num4 * num5;
    }

    private float[] GetWeights()
    {
        if (governmentType == GovernmentType.Imperial)
        {
            return new[] {3f, 1f, -2f};
        }

        if (governmentType == GovernmentType.Tribal)
        {
            return new[] {-1f, 2f, -1f};
        }

        if (governmentType == GovernmentType.Feudal)
        {
            return new[] {1f, 2f, -1f};
        }

        return new[] {-3f, 1.5f, 3f};
    }

    public override TextObject GetChooseDescription()
    {
        var textObject =
            new TextObject(
                "{=0EqPRs21}As {?IS_FEMALE}queen{?}king{\\?} you must decide whether to enforce the policy of {POLICY_NAME}.");
        textObject.SetTextVariable("IS_FEMALE", DetermineChooser().Leader.IsFemale ? 1 : 0);
        textObject.SetTextVariable("POLICY_NAME", governmentType.ToString());
        return textObject;
    }

    public override TextObject GetChooseTitle()
    {
        var textObject = new TextObject("{=!}Change government to {GOVERNMENT}");
        textObject.SetTextVariable("GOVERNMENT", governmentType.ToString());
        return textObject;
    }

    public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus,
        bool isShortVersion = false)
    {
        TextObject textObject;
        var newGovernment = ((GovernmentDecisionOutcome) chosenOutcome).ShouldDecisionBeEnforced;
        if (newGovernment)
        {
            textObject = new TextObject("{=!}The {KINGDOM}'s government is now {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");
        }
        else
        {
            textObject =
                new TextObject(
                    "{=!}The {KINGDOM}'s government will continue to be {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");
        }


        textObject.SetTextVariable("KINGDOM", Kingdom.InformalName);
        textObject.SetTextVariable("POLICY_DESCRIPTION", Utils.Helpers
            .GetGovernmentString(newGovernment ? governmentType : Title.contract.Government));
        if (isShortVersion || IsSingleClanDecision())
        {
            textObject.SetTextVariable("POLICY_SUPPORT", TextObject.Empty);
        }
        else
        {
            textObject.SetTextVariable("POLICY_SUPPORT", "{=bqEO389P}This decision caused a split in the council.");
            if (supportStatus == SupportStatus.Majority)
            {
                textObject.SetTextVariable("POLICY_SUPPORT",
                    "{=3W67kdtc}This decision had the support of the council.");
            }

            if (supportStatus == SupportStatus.Minority)
            {
                textObject.SetTextVariable("POLICY_SUPPORT",
                    "{=b6MgRYlM}This decision was rejected by the support of the council.");
            }
        }

        return textObject;
    }

    public override TextObject GetGeneralTitle()
    {
        return new TextObject(Utils.Helpers
            .GetGovernmentString(governmentType));
    }

    public override int GetProposalInfluenceCost()
    {
        return 300;
    }

    public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
    {
        return possibleOutcomes.FirstOrDefault(t => ((GovernmentDecisionOutcome) t).ShouldDecisionBeEnforced);
    }

    public override TextObject GetSupportDescription()
    {
        var textObject =
            new TextObject(
                "{=!}{CLAN} proposes a change of government, currently {CURRENT} to {PROPOSED}. You can pick your stance regarding this decision.");

        textObject.SetTextVariable("CLAN", DetermineChooser().Leader.Name);
        textObject.SetTextVariable("CURRENT",
            Utils.Helpers.GetGovernmentString(Title.contract.Government, Kingdom.Culture));
        textObject.SetTextVariable("PROPOSED", Utils.Helpers.GetGovernmentString(governmentType, Kingdom.Culture));
        return textObject;
    }

    public override TextObject GetSupportTitle()
    {
        var textObject = new TextObject("{=!}Vote to change form of government to {GOVERNMENT}");
        textObject.SetTextVariable("GOVERNMENT", governmentType.ToString());
        return textObject;
    }

    public override bool IsAllowed()
    {
        var kingdom = ProposerClan.Kingdom;
        if (kingdom == null || FactionManager.GetEnemyKingdoms(kingdom).Count() > 0)
        {
            return false;
        }

        return base.IsAllowed();
    }

    public class GovernmentDecisionOutcome : DecisionOutcome
    {
        public GovernmentDecisionOutcome(bool shouldBeEnforced)
        {
            ShouldDecisionBeEnforced = shouldBeEnforced;
        }

        [SaveableProperty(200)] public bool ShouldDecisionBeEnforced { get; }


        public override TextObject GetDecisionTitle()
        {
            var textObject = new TextObject("{=kakxnaN5}{?SUPPORT}Yes{?}No{\\?}");
            textObject.SetTextVariable("SUPPORT", ShouldDecisionBeEnforced ? 1 : 0);
            return textObject;
        }

        public override TextObject GetDecisionDescription()
        {
            if (ShouldDecisionBeEnforced)
            {
                return new TextObject("{=pWyxaauF}We support this proposal");
            }

            return new TextObject("{=BktSNgY4}We oppose this proposal");
        }

        public override string GetDecisionLink()
        {
            return null;
        }

        public override ImageIdentifier GetDecisionImageIdentifier()
        {
            return null;
        }
    }
}