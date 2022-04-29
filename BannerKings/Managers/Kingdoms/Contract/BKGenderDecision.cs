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
    public class BKGenderDecision : BKContractDecision
    {

        [SaveableProperty(100)]
        private GenderLaw genderLaw { get; set; }

        public BKGenderDecision(Clan proposerClan, GenderLaw genderLaw, FeudalTitle title) : base(proposerClan, title)
        {
            this.genderLaw = genderLaw;
;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            bool newGovernment = (chosenOutcome as GenderLawDecisionOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
            {
                Title.ChangeContract(genderLaw);
            }
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
            yield return new GenderLawDecisionOutcome(true);
            yield return new GenderLawDecisionOutcome(false);
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            foreach (DecisionOutcome decisionOutcome in possibleOutcomes)
            {
                if (((GenderLawDecisionOutcome)decisionOutcome).ShouldDecisionBeEnforced)
                    decisionOutcome.SetSponsor(ProposerClan);

                else AssignDefaultSponsor(decisionOutcome);
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            GenderLawDecisionOutcome policyDecisionOutcome = possibleOutcome as GenderLawDecisionOutcome;
            float num = 2f * clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian) - 2f * clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float num2 = 0f;
            FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Kingdom);
            if (sovereign != null && sovereign.contract != null)
            {
                GovernmentType government = sovereign.contract.Government;
                if (government == GovernmentType.Tribal || government == GovernmentType.Republic)
                    num2++;
                else num--;
            }
            

            return genderLaw == GenderLaw.Cognatic && policyDecisionOutcome.ShouldDecisionBeEnforced ? (num + num2) * 60f : (num + num2) * -100f;
        }

        public override TextObject GetChooseDescription()
        {
            TextObject textObject = new TextObject("{=0EqPRs21}As {?IS_FEMALE}queen{?}king{\\?} you must decide whether to enforce the policy of {POLICY_NAME}.");
            textObject.SetTextVariable("IS_FEMALE", DetermineChooser().Leader.IsFemale ? 1 : 0);
            textObject.SetTextVariable("POLICY_NAME", genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            TextObject textObject = new TextObject("{=!}Change government to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            TextObject textObject;
            bool newGovernment = ((GenderLawDecisionOutcome)chosenOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
                textObject = new TextObject("{=!}The {KINGDOM}'s government is now {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");
            else textObject = new TextObject("{=!}The {KINGDOM}'s government will continue to be {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");


            textObject.SetTextVariable("KINGDOM", Kingdom.InformalName);
            textObject.SetTextVariable("POLICY_DESCRIPTION", newGovernment ? genderLaw.ToString() : Title.contract.Succession.ToString());
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
            return new TextObject(genderLaw.ToString());
        }

        public override int GetProposalInfluenceCost()
        {
            return 150;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return possibleOutcomes.FirstOrDefault(t => ((GenderLawDecisionOutcome)t).ShouldDecisionBeEnforced);
        }

        public override TextObject GetSupportDescription()
        {
            TextObject textObject = new TextObject("{=!}{CLAN} proposes a change of government, currently {CURRENT} to {PROPOSED}. You can pick your stance regarding this decision.");

            textObject.SetTextVariable("CLAN", DetermineChooser().Leader.Name);
            textObject.SetTextVariable("CURRENT", BannerKings.Helpers.Helpers.GetGovernmentString(Title.contract.Government, Kingdom.Culture));
            textObject.SetTextVariable("PROPOSED", genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetSupportTitle()
        {
            TextObject textObject = new TextObject("{=!}Vote for change of realm's succession to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", genderLaw.ToString());
            return textObject;
        }

        public override bool IsAllowed()
        {
            Kingdom kingdom = ProposerClan.Kingdom;
            if (kingdom == null || FactionManager.GetEnemyKingdoms(kingdom).Count() > 0) return false;

            return base.IsAllowed();
        }

        public override float CalculateKingdomSupport(Kingdom kingdom)
        {
            float support = 0f;
            float clans = 0;
            foreach (Clan clan in kingdom.Clans)
                if (!clan.IsUnderMercenaryService)
                {
                    if (clan == Clan.PlayerClan) support += 100f;
                    else support += DetermineSupport(clan, new GenderLawDecisionOutcome(true));
                    clans++;
                }

            return MBMath.ClampFloat(support / clans, 0f, 100f);
        }

        public class GenderLawDecisionOutcome : DecisionOutcome
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

            public GenderLawDecisionOutcome(bool shouldBeEnforced)
            {
                ShouldDecisionBeEnforced = shouldBeEnforced;
            }
        }
    }
}
