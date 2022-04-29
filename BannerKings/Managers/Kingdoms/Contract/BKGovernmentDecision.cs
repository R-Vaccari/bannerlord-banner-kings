﻿using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Contract
{
    public class BKGovernmentDecision : BKContractDecision
    {
        [SaveableProperty(100)]
        private GovernmentType governmentType { get; set; }

        public BKGovernmentDecision(Clan proposerClan, GovernmentType governmentType, FeudalTitle title) : base(proposerClan, title)
        {
            this.governmentType = governmentType;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            bool newGovernment = (chosenOutcome as GovernmentDecisionOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
            {
                Title.ChangeContract(governmentType);
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
                    else support += DetermineSupport(clan, new GovernmentDecisionOutcome(true));
                    clans++;
                }

            return MBMath.ClampFloat(support / clans, 0f, 100f);
        }

        public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {
            bool newGovernment = (chosenOutcome as GovernmentDecisionOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
            {
                SuccessionType succession = Title.contract.Succession;
                if (governmentType == GovernmentType.Imperial)
                {
                    if (succession != SuccessionType.Imperial)
                        succession = SuccessionType.Imperial;
                }
                else if (governmentType == GovernmentType.Feudal)
                {
                    if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
                        succession = SuccessionType.Hereditary_Monarchy;
                }
                else if (governmentType == GovernmentType.Tribal)
                {
                    if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
                        succession = SuccessionType.Elective_Monarchy;
                }
                else if (governmentType == GovernmentType.Republic)
                {
                    if (succession != SuccessionType.Republic)
                        succession = SuccessionType.Republic;
                }

                if (succession != Title.contract.Succession)
                    Title.ChangeContract(succession);
            } 
        }

        public override TextObject GetSecondaryEffects()
        {
            TextObject effects = null;
            SuccessionType succession = Title.contract.Succession;
            if (governmentType == GovernmentType.Imperial)
            {
                if (succession != SuccessionType.Imperial)
                    effects = new TextObject("{=!}Succession will be changed to Imperial succession.");
            }
            else if(governmentType == GovernmentType.Feudal)
            {
                if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
                    effects = new TextObject("{=!}Succession will be changed to Hereditary Monarchy succession.");
            }
            else if(governmentType == GovernmentType.Tribal)
            {
                if (succession != SuccessionType.Elective_Monarchy && succession != SuccessionType.Hereditary_Monarchy)
                    effects = new TextObject("{=!}Succession will be changed to Elective Monarchy succession.");
            }
            else if(governmentType == GovernmentType.Republic)
            {
                if (succession != SuccessionType.Republic)
                    effects = new TextObject("{=!}Succession will be changed to Republican succession.");
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
            foreach (DecisionOutcome decisionOutcome in possibleOutcomes)
            {
                if (((GovernmentDecisionOutcome)decisionOutcome).ShouldDecisionBeEnforced)
                    decisionOutcome.SetSponsor(ProposerClan);

                else AssignDefaultSponsor(decisionOutcome);
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            GovernmentDecisionOutcome policyDecisionOutcome = possibleOutcome as GovernmentDecisionOutcome;
            float authoritarian = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float egalitarian = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float oligarchic = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float[] weights = GetWeights();

            float num = weights[0] * authoritarian;
            float num2 = weights[1] * oligarchic;
            float num3 = weights[2] * egalitarian;

            float num4 = num + num3 + num2;

            if (clan == Kingdom.RulingClan)
                if (governmentType == GovernmentType.Imperial && policyDecisionOutcome.ShouldDecisionBeEnforced)
                {
                    num4 += 2f;
                } else if (governmentType == GovernmentType.Republic && policyDecisionOutcome.ShouldDecisionBeEnforced)
                    num4 -= 2f;

            float num5;
            if (policyDecisionOutcome.ShouldDecisionBeEnforced)
                num5 = 60f;
            else num5 = -100f;

            return num4 * num5;
        }

        private float[] GetWeights()
        {
            if (governmentType == GovernmentType.Imperial)
                return new[] { 3f, 1f, -2f };
            if (governmentType == GovernmentType.Tribal)
                return new[] { -1f, 2f, -1f };
            if (governmentType == GovernmentType.Feudal)
                return new[] { 1f, 2f, -1f };
            return new[] { -3f, 1.5f, 3f };
        }

        public override TextObject GetChooseDescription()
        {
            TextObject textObject = new TextObject("{=0EqPRs21}As {?IS_FEMALE}queen{?}king{\\?} you must decide whether to enforce the policy of {POLICY_NAME}.");
            textObject.SetTextVariable("IS_FEMALE", DetermineChooser().Leader.IsFemale ? 1 : 0);
            textObject.SetTextVariable("POLICY_NAME", governmentType.ToString());
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            TextObject textObject = new TextObject("{=!}Change government to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", governmentType.ToString());
            return textObject;
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            TextObject textObject;
            bool newGovernment = ((GovernmentDecisionOutcome)chosenOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
                textObject = new TextObject("{=!}The {KINGDOM}'s government is now {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");
            else textObject = new TextObject("{=!}The {KINGDOM}'s government will continue to be {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");


            textObject.SetTextVariable("KINGDOM", Kingdom.InformalName);
            textObject.SetTextVariable("POLICY_DESCRIPTION", BannerKings.Helpers.Helpers
                .GetGovernmentString(newGovernment ? governmentType : Title.contract.Government));
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
            return new TextObject(BannerKings.Helpers.Helpers
                .GetGovernmentString(governmentType));
        }

        public override int GetProposalInfluenceCost()
        {
            return 300;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return possibleOutcomes.FirstOrDefault(t => ((GovernmentDecisionOutcome)t).ShouldDecisionBeEnforced);
        }

        public override TextObject GetSupportDescription()
        {
            TextObject textObject = new TextObject("{=!}{CLAN} proposes a change of government, currently {CURRENT} to {PROPOSED}. You can pick your stance regarding this decision.");

            textObject.SetTextVariable("CLAN", DetermineChooser().Leader.Name);
            textObject.SetTextVariable("CURRENT", BannerKings.Helpers.Helpers.GetGovernmentString(Title.contract.Government, Kingdom.Culture));
            textObject.SetTextVariable("PROPOSED", BannerKings.Helpers.Helpers.GetGovernmentString(governmentType, Kingdom.Culture));
            return textObject;
        }

        public override TextObject GetSupportTitle()
        {
            TextObject textObject = new TextObject("{=!}Vote for change of government to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", governmentType.ToString());
            return textObject;
        }

        public override bool IsAllowed()
        {
            Kingdom kingdom = ProposerClan.Kingdom;
            if (kingdom == null || FactionManager.GetEnemyKingdoms(kingdom).Count() > 0) return false;

            return base.IsAllowed();
        }

        public class GovernmentDecisionOutcome : DecisionOutcome
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

            public GovernmentDecisionOutcome(bool shouldBeEnforced)
            {
                ShouldDecisionBeEnforced = shouldBeEnforced;
            }
        }
    }
}
