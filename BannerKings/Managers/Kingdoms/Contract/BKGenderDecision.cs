using BannerKings.Managers.Titles;
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
                this.Title.ChangeContract(genderLaw);
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
            return base.Kingdom.RulingClan;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            yield return new GenderLawDecisionOutcome(true);
            yield return new GenderLawDecisionOutcome(false);
            yield break;
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            foreach (DecisionOutcome decisionOutcome in possibleOutcomes)
            {
                if (((GenderLawDecisionOutcome)decisionOutcome).ShouldDecisionBeEnforced)
                    decisionOutcome.SetSponsor(base.ProposerClan);

                else base.AssignDefaultSponsor(decisionOutcome);
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            GenderLawDecisionOutcome policyDecisionOutcome = possibleOutcome as GenderLawDecisionOutcome;
            float num = 2f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian) - 2f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float num2 = 0f;
            FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(base.Kingdom);
            if (sovereign != null && sovereign.contract != null)
            {
                GovernmentType government = sovereign.contract.Government;
                if (government == GovernmentType.Tribal || government == GovernmentType.Republic)
                    num2++;
                else num--;
            }
            

            return this.genderLaw == GenderLaw.Cognatic && policyDecisionOutcome.ShouldDecisionBeEnforced ? (num + num2) * 60f : (num + num2) * -100f;
        }

        public override TextObject GetChooseDescription()
        {
            TextObject textObject = new TextObject("{=0EqPRs21}As {?IS_FEMALE}queen{?}king{\\?} you must decide whether to enforce the policy of {POLICY_NAME}.", null);
            textObject.SetTextVariable("IS_FEMALE", this.DetermineChooser().Leader.IsFemale ? 1 : 0);
            textObject.SetTextVariable("POLICY_NAME", this.genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            TextObject textObject = new TextObject("{=!}Change government to {GOVERNMENT}", null);
            textObject.SetTextVariable("GOVERNMENT", this.genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            TextObject textObject;
            bool newGovernment = ((GenderLawDecisionOutcome)chosenOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
                textObject = new TextObject("{=!}The {KINGDOM}'s government is now {POLICY_DESCRIPTION}. {POLICY_SUPPORT}", null);
            else textObject = new TextObject("{=!}The {KINGDOM}'s government will continue to be {POLICY_DESCRIPTION}. {POLICY_SUPPORT}", null);


            textObject.SetTextVariable("KINGDOM", base.Kingdom.InformalName);
            textObject.SetTextVariable("POLICY_DESCRIPTION", newGovernment ? this.genderLaw.ToString() : this.Title.contract.Succession.ToString());
            if (isShortVersion || base.IsSingleClanDecision())
                textObject.SetTextVariable("POLICY_SUPPORT", TextObject.Empty);
            else
            {
                textObject.SetTextVariable("POLICY_SUPPORT", "{=bqEO389P}This decision caused a split in the council.");
                if (supportStatus == KingdomDecision.SupportStatus.Majority)
                    textObject.SetTextVariable("POLICY_SUPPORT", "{=3W67kdtc}This decision had the support of the council.");
                
                if (supportStatus == KingdomDecision.SupportStatus.Minority)
                    textObject.SetTextVariable("POLICY_SUPPORT", "{=b6MgRYlM}This decision was rejected by the support of the council.");
                
            }
            return textObject;
        }

        public override TextObject GetGeneralTitle()
        {
            return new TextObject(this.genderLaw.ToString());
        }

        public override int GetProposalInfluenceCost()
        {
            return 150;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return possibleOutcomes.FirstOrDefault((DecisionOutcome t) => ((GenderLawDecisionOutcome)t).ShouldDecisionBeEnforced);
        }

        public override TextObject GetSupportDescription()
        {
            TextObject textObject = new TextObject("{=!}{CLAN} proposes a change of government, currently {CURRENT} to {PROPOSED}. You can pick your stance regarding this decision.", null);

            textObject.SetTextVariable("CLAN", this.DetermineChooser().Leader.Name);
            textObject.SetTextVariable("CURRENT", BannerKings.Utils.Helpers.GetGovernmentString(this.Title.contract.Government, base.Kingdom.Culture));
            textObject.SetTextVariable("PROPOSED", this.genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetSupportTitle()
        {
            TextObject textObject = new TextObject("{=!}Vote for change of realm's succession to {GOVERNMENT}", null);
            textObject.SetTextVariable("GOVERNMENT", this.genderLaw.ToString());
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
                    else support += this.DetermineSupport(clan, new GenderLawDecisionOutcome(true));
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

            public GenderLawDecisionOutcome(bool shouldBeEnforced)
            {
                this.ShouldDecisionBeEnforced = shouldBeEnforced;
            }
        }
    }
}
