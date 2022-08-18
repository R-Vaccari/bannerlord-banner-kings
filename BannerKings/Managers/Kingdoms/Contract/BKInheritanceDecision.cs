using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Contract
{
    public class BKInheritanceDecision : BKContractDecision
    {
        [SaveableProperty(100)]
        private InheritanceType inheritanceType { get; set; }

        public BKInheritanceDecision(Clan proposerClan, InheritanceType inheritanceType, FeudalTitle title) : base(proposerClan, title)
        {
            this.inheritanceType = inheritanceType;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            bool newGovernment = (chosenOutcome as InheritanceDecisionOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
            {
                Title.ChangeContract(inheritanceType);
            }
        }

        public override float CalculateKingdomSupport(Kingdom kingdom)
        {
            float support = 0f;
            float clans = 0;
            foreach (Clan clan in kingdom.Clans)
                if (!clan.IsUnderMercenaryService)
                {
                    if (clan == Clan.PlayerClan) support += 100f;
                    else support += DetermineSupport(clan, new InheritanceDecisionOutcome(true));
                    clans++;
                }

            return MBMath.ClampFloat(support / clans, 0f, 100f);
        }

        public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {
            
        }

        public override TextObject GetSecondaryEffects()
        {
            return null;
        }

        public override Clan DetermineChooser()
        {
            return Kingdom.RulingClan;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            yield return new InheritanceDecisionOutcome(true);
            yield return new InheritanceDecisionOutcome(false);
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            foreach (DecisionOutcome decisionOutcome in possibleOutcomes)
            {
                if (((InheritanceDecisionOutcome)decisionOutcome).ShouldDecisionBeEnforced)
                    decisionOutcome.SetSponsor(ProposerClan);

                else AssignDefaultSponsor(decisionOutcome);
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            InheritanceDecisionOutcome policyDecisionOutcome = possibleOutcome as InheritanceDecisionOutcome;
            float authoritarian = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float egalitarian = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float oligarchic = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float[] weights = GetWeights();

            float num = weights[0] * authoritarian;
            float num2 = weights[1] * oligarchic;
            float num3 = weights[2] * egalitarian;

            float num4 = num + num3 + num2;
            float num5;
            if (policyDecisionOutcome.ShouldDecisionBeEnforced)
                num5 = 60f;
            else num5 = -100f;

            return num4 * num5;
        }

        private float[] GetWeights()
        {
            if (inheritanceType == InheritanceType.Primogeniture)
                return new[] { 4f, 2f, -2f };
            if (inheritanceType == InheritanceType.Ultimogeniture)
                return new[] { -1f, 1f, 2f };
            return new[] { -3f, 2f, 5f };
        }

        public override TextObject GetChooseDescription()
        {
            TextObject textObject = new TextObject("{=0EqPRs21}As {?IS_FEMALE}queen{?}king{\\?} you must decide whether to enforce the policy of {POLICY_NAME}.");
            textObject.SetTextVariable("IS_FEMALE", DetermineChooser().Leader.IsFemale ? 1 : 0);
            textObject.SetTextVariable("POLICY_NAME", inheritanceType.ToString());
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            TextObject textObject = new TextObject("{=!}Change government to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", inheritanceType.ToString());
            return textObject;
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            TextObject textObject;
            bool newGovernment = ((InheritanceDecisionOutcome)chosenOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
                textObject = new TextObject("{=!}The {KINGDOM}'s government is now {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");
            else textObject = new TextObject("{=!}The {KINGDOM}'s government will continue to be {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");


            textObject.SetTextVariable("KINGDOM", Kingdom.InformalName);
            textObject.SetTextVariable("POLICY_DESCRIPTION", newGovernment ? inheritanceType.ToString() : Title.contract.Succession.ToString());
            if (isShortVersion || IsSingleClanDecision())
                textObject.SetTextVariable("POLICY_SUPPORT", TextObject.Empty);
            else
            {
                textObject.SetTextVariable("POLICY_SUPPORT", "{=bqEO389P}This decision caused a split in the council.");
                if (supportStatus == SupportStatus.Majority)
                    textObject.SetTextVariable("POLICY_SUPPORT", "{=3W67kdtc}This decision had the support of the council.");
                
                if (supportStatus == SupportStatus.Minority)
                    textObject.SetTextVariable("POLICY_SUPPORT", "{=b6MgRYlM}This decision was rejected by the support of the council.");
                
            }
            return textObject;
        }

        public override TextObject GetGeneralTitle()
        {
            return new TextObject(inheritanceType.ToString());
        }

        public override int GetProposalInfluenceCost()
        {
            return 200;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return possibleOutcomes.FirstOrDefault(t => ((InheritanceDecisionOutcome)t).ShouldDecisionBeEnforced);
        }

        public override TextObject GetSupportDescription()
        {
            TextObject textObject = new TextObject("{=!}{CLAN} proposes a change of government, currently {CURRENT} to {PROPOSED}. You can pick your stance regarding this decision.");

            textObject.SetTextVariable("CLAN", DetermineChooser().Leader.Name);
            textObject.SetTextVariable("CURRENT", Utils.Helpers.GetGovernmentString(Title.contract.Government, Kingdom.Culture));
            textObject.SetTextVariable("PROPOSED", inheritanceType.ToString());
            return textObject;
        }

        public override TextObject GetSupportTitle()
        {
            TextObject textObject = new TextObject("{=!}Vote to change realm clans' inheritance to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", inheritanceType.ToString());
            return textObject;
        }

        public override bool IsAllowed()
        {
            Kingdom kingdom = ProposerClan.Kingdom;
            if (kingdom == null || FactionManager.GetEnemyKingdoms(kingdom).Count() > 0) return false;

            return base.IsAllowed();
        }

        public class InheritanceDecisionOutcome : DecisionOutcome
        {

            [SaveableProperty(200)]
            public bool ShouldDecisionBeEnforced { get; private set; }


            public override TextObject GetDecisionTitle()
            {
                TextObject textObject = new TextObject("{=kakxnaN5}{?SUPPORT}Yes{?}No{\\?}");
                textObject.SetTextVariable("SUPPORT", ShouldDecisionBeEnforced ? 1 : 0);
                return textObject;
            }

            public override TextObject GetDecisionDescription()
            {
                if (ShouldDecisionBeEnforced)
                    return new TextObject("{=pWyxaauF}We support this proposal");

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

            public InheritanceDecisionOutcome(bool shouldBeEnforced)
            {
                ShouldDecisionBeEnforced = shouldBeEnforced;
            }
        }
    }
}
