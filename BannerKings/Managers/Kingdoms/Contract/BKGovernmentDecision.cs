using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Kingdoms.Contract
{
    public class BKGovernmentDecision : BKContractDecision
    {

        private GovernmentType governmentType;
        private FeudalTitle title;

        public BKGovernmentDecision(Clan proposerClan, GovernmentType governmentType, FeudalTitle title) : base(proposerClan, title)
        {
            this.governmentType = governmentType;
            this.title = title;
        }

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
            yield return new GovernmentDecisionOutcome(true);
            yield return new GovernmentDecisionOutcome(false);
            yield break;
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            foreach (DecisionOutcome decisionOutcome in possibleOutcomes)
            {
                if (((GovernmentDecisionOutcome)decisionOutcome).ShouldDecisionBeEnforced)
                    decisionOutcome.SetSponsor(base.ProposerClan);

                else base.AssignDefaultSponsor(decisionOutcome);
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            GovernmentDecisionOutcome policyDecisionOutcome = possibleOutcome as GovernmentDecisionOutcome;
            float num = 0.1f;
            float num2 = 0.1f;
            float num3 = 0.1f;
            if (clan.Kingdom != null && clan.Kingdom.RulingClan == clan)
            {
                num2 += 1f;
                num3 -= 1.5f;
                num -= 0.4f;
            }
            else if (clan.IsMinorFaction)
            {
                num += 1f;
                num3 -= 1.5f;
                num2 -= 0.3f;
            }
            else if (clan.Tier >= 3)
            {
                num -= 1.3f;
                num3 += (float)clan.Tier * 0.2f;
                num2 -= 1.3f;
            }
            else if (clan.Tier == 2)
            {
                num2 -= 0.1f;
                num3 += 0.4f;
                num -= 0.5f;
            }
            CultureObject culture = clan.Culture;
            num += 0.6f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian) - 0.9f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic);
            num3 += 0.6f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic) - 0.9f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian) - 0.5f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            num2 += 0.8f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian) - 1.3f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic);
            float num4 = num + num3 + num2;
            float num5;
            if (policyDecisionOutcome.ShouldDecisionBeEnforced)
                num5 = 60f;
            else num5 = -100f;

            return num4 * num5;
        }

        public override TextObject GetChooseDescription()
        {
            TextObject textObject = new TextObject("{=0EqPRs21}As {?IS_FEMALE}queen{?}king{\\?} you must decide whether to enforce the policy of {POLICY_NAME}.", null);
            textObject.SetTextVariable("IS_FEMALE", this.DetermineChooser().Leader.IsFemale ? 1 : 0);
            textObject.SetTextVariable("POLICY_NAME", this.governmentType.ToString());
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            TextObject textObject = new TextObject("{=!}{GOVERNMENT}", null);
            textObject.SetTextVariable("GOVERNMENT", this.governmentType.ToString());
            return textObject;
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            TextObject textObject;
            if (((GovernmentDecisionOutcome)chosenOutcome).ShouldDecisionBeEnforced)
            {
                if (this.title.contract.government != this.governmentType)
                {
                    textObject = new TextObject("{=Lbs2bNlg}The {KINGDOM} will start {POLICY_DESCRIPTION} ({POLICY}). {POLICY_SUPPORT}", null);
                }
                else
                {
                    textObject = new TextObject("{=jnYwiCAz}The {KINGDOM} will continue {POLICY_DESCRIPTION} ({POLICY}). {POLICY_SUPPORT}", null);
                }
            }
            else if (this.title.contract.government == this.governmentType)
            {
                textObject = new TextObject("{=2BVDp7Tg}The {KINGDOM} will stop {POLICY_DESCRIPTION} ({POLICY}). {POLICY_SUPPORT}", null);
            }
            else
            {
                textObject = new TextObject("{=faUooB7V}The {KINGDOM} will not be {POLICY_DESCRIPTION} ({POLICY}). {POLICY_SUPPORT}", null);
            }
            textObject.SetTextVariable("KINGDOM", base.Kingdom.InformalName);
            textObject.SetTextVariable("POLICY", this.governmentType.ToString());
            textObject.SetTextVariable("POLICY_DESCRIPTION", this.governmentType.ToString());
            if (isShortVersion || base.IsSingleClanDecision())
            {
                textObject.SetTextVariable("POLICY_SUPPORT", TextObject.Empty);
            }
            else
            {
                textObject.SetTextVariable("POLICY_SUPPORT", "{=bqEO389P}This decision caused a split in the council.");
                if (supportStatus == KingdomDecision.SupportStatus.Majority)
                {
                    textObject.SetTextVariable("POLICY_SUPPORT", "{=3W67kdtc}This decision had the support of the council.");
                }
                if (supportStatus == KingdomDecision.SupportStatus.Minority)
                {
                    textObject.SetTextVariable("POLICY_SUPPORT", "{=b6MgRYlM}This decision was rejected by the support of the council.");
                }
            }
            return textObject;
        }

        public override TextObject GetGeneralTitle()
        {
            return new TextObject(this.governmentType.ToString());
        }

        public override int GetProposalInfluenceCost()
        {
            return 200;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return possibleOutcomes.FirstOrDefault((DecisionOutcome t) => ((GovernmentDecisionOutcome)t).ShouldDecisionBeEnforced);
        }

        public override TextObject GetSecondaryEffects()
        {
            return new TextObject("Secondary effects");
        }

        public override TextObject GetSupportDescription()
        {
            TextObject textObject = new TextObject("{=!}{CLAN} proposes a change of government, currently {CURRENT} to {PROPOSED}. You can pick your stance regarding this decision.", null);

            textObject.SetTextVariable("CLAN", this.DetermineChooser().Leader.Name);
            textObject.SetTextVariable("CURRENT", this.title?.contract?.government.ToString());
            textObject.SetTextVariable("PROPOSED", governmentType.ToString());
            return textObject;
        }

        public override TextObject GetSupportTitle()
        {
            TextObject textObject = new TextObject("{=!}Vote for change of government to {GOVERNMENT}", null);
            textObject.SetTextVariable("GOVERNMENT", this.governmentType.ToString());
            return textObject;
        }

        public override bool IsAllowed()
        {
            if (this.title == null || this.title.contract == null ||
                this.governmentType == this.title.contract.government) return false;
            Kingdom kingdom = ProposerClan.Kingdom;
            if (kingdom == null || FactionManager.GetEnemyKingdoms(kingdom).Count() > 0) return false;

            return true;
        }

        public class GovernmentDecisionOutcome : DecisionOutcome
        {

            [SaveableProperty(200)]
            public bool ShouldDecisionBeEnforced { get; private set; }


            public override TextObject GetDecisionTitle()
            {
                TextObject textObject = new TextObject("{=kakxnaN5}{?SUPPORT}Yes{?}No{\\?}", null);
                textObject.SetTextVariable("SUPPORT", this.ShouldDecisionBeEnforced ? 1 : 0);
                return textObject;
            }

            public override TextObject GetDecisionDescription()
            {
                if (this.ShouldDecisionBeEnforced)
                    return new TextObject("{=pWyxaauF}We support this proposal", null);

                return new TextObject("{=BktSNgY4}We oppose this proposal", null);
            }

            public override string GetDecisionLink()
            {
                return null;
            }

            public override ImageIdentifier GetDecisionImageIdentifier()
            {
                return null;
            }

            public GovernmentDecisionOutcome(bool shouldBeEnforced)
            {
                this.ShouldDecisionBeEnforced = shouldBeEnforced;
            }
        }
    }
}
