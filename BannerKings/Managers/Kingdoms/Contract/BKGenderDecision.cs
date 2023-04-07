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

namespace BannerKings.Managers.Kingdoms.Contract
{
    public class BKGenderDecision : BKContractDecision
    {
        public BKGenderDecision(Clan proposerClan, GenderLaw genderLaw, FeudalTitle title) : base(proposerClan, title)
        {
            this.genderLaw = genderLaw;
        }

        [SaveableProperty(100)] private GenderLaw genderLaw { get; set; }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            var newGovernment = (chosenOutcome as GenderLawDecisionOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
            {
                Title.ChangeContract(genderLaw);
            }
        }

        public override void ApplySecondaryEffects(MBReadOnlyList<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
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

        public override void DetermineSponsors(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
        {
            foreach (var decisionOutcome in possibleOutcomes)
            {
                if (((GenderLawDecisionOutcome) decisionOutcome).ShouldDecisionBeEnforced)
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
            var policyDecisionOutcome = possibleOutcome as GenderLawDecisionOutcome;
            var num = 0f;

            bool egalitarian = (genderLaw == GenderLaw.Cognatic && policyDecisionOutcome.ShouldDecisionBeEnforced)
                || (genderLaw == GenderLaw.Agnatic && !policyDecisionOutcome.ShouldDecisionBeEnforced);
            if (egalitarian)
            {
                num = 5f * clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian) -
                      2f * clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian) -
                      2f * clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic);

                if (clan.Leader.IsFemale)
                {
                    num += 30f;
                }
                else
                {
                    num -= 15f;
                }
            }
            else
            {
                num =  2f * clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian) +
                      2f * clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic) -
                      5f * clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian);
                if (!clan.Leader.IsFemale)
                {
                    num += 20f;
                }
                else
                {
                    num -= 35f;
                }
            }
 
            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Kingdom);
            if (sovereign != null && sovereign.Contract != null)
            {
                var government = sovereign.Contract.Government;
                if (government is GovernmentType.Tribal or GovernmentType.Republic)
                {
                    if (egalitarian)
                    {
                        num *= 1.5f;
                    }
                }
            }

            if (!policyDecisionOutcome.ShouldDecisionBeEnforced)
            {
                num *= -100f;
            }
            else
            {
                num *= 60f;
            }

            return num;
        }

        public override TextObject GetChooseDescription()
        {
            var textObject = new TextObject("{=mXcErd03}As {?IS_FEMALE}queen{?}king{\\?} you must decide whether to enforce the policy of {POLICY_NAME}.");
            textObject.SetTextVariable("IS_FEMALE", DetermineChooser().Leader.IsFemale ? 1 : 0);
            textObject.SetTextVariable("POLICY_NAME", genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            var textObject = new TextObject("{=iZ6VfHe6}Change government to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus,
            bool isShortVersion = false)
        {
            TextObject textObject;
            var newGovernment = ((GenderLawDecisionOutcome) chosenOutcome).ShouldDecisionBeEnforced;
            if (newGovernment)
            {
                textObject = new TextObject("{=u0mnqvO2}The {KINGDOM}'s government is now {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");
            }
            else
            {
                textObject = new TextObject("{=ikYX00pK}The {KINGDOM}'s government will continue to be {POLICY_DESCRIPTION}. {POLICY_SUPPORT}");
            }


            textObject.SetTextVariable("KINGDOM", Kingdom.InformalName);
            textObject.SetTextVariable("POLICY_DESCRIPTION",
                newGovernment 
                    ? genderLaw.ToString() 
                    : Title.Contract.Succession.ToString());
            if (isShortVersion || IsSingleClanDecision())
            {
                textObject.SetTextVariable("POLICY_SUPPORT", TextObject.Empty);
            }
            else
            {
                textObject.SetTextVariable("POLICY_SUPPORT", "{=74XdPqxc}This decision caused a split in the council.");
                switch (supportStatus)
                {
                    case SupportStatus.Majority:
                        textObject.SetTextVariable("POLICY_SUPPORT",
                            "{=VPH5EBOT}This decision had the support of the council.");
                        break;
                    case SupportStatus.Minority:
                        textObject.SetTextVariable("POLICY_SUPPORT",
                            "{=PnCak8E1}This decision was rejected by the support of the council.");
                        break;
                }
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

        public override DecisionOutcome GetQueriedDecisionOutcome(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
        {
            return possibleOutcomes.FirstOrDefault(t => ((GenderLawDecisionOutcome) t).ShouldDecisionBeEnforced);
        }

        public override TextObject GetSupportDescription()
        {
            var textObject = new TextObject("{=JjgGFcCt}{CLAN} proposes a change of government, currently {CURRENT} to {PROPOSED}. You can pick your stance regarding this decision.");

            textObject.SetTextVariable("CLAN", DetermineChooser().Leader.Name);
            textObject.SetTextVariable("CURRENT",
                Utils.Helpers.GetGovernmentString(Title.Contract.Government, Kingdom.Culture));
            textObject.SetTextVariable("PROPOSED", genderLaw.ToString());
            return textObject;
        }

        public override TextObject GetSupportTitle()
        {
            var textObject = new TextObject("{=uxYkvBrD}Vote to change of realm's gender law to {GOVERNMENT}");
            textObject.SetTextVariable("GOVERNMENT", genderLaw.ToString());
            return textObject;
        }

        public override bool IsAllowed()
        {
            var kingdom = ProposerClan.Kingdom;
            if (kingdom == null || FactionManager.GetEnemyKingdoms(kingdom).Any())
            {
                return false;
            }

            return base.IsAllowed();
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
                        support += DetermineSupport(clan, new GenderLawDecisionOutcome(true));
                    }

                    clans++;
                }
            }

            return MBMath.ClampFloat(support / clans, 0f, 100f);
        }

        public override void UpdateDecision(int value)
        {
            genderLaw = (GenderLaw)value;
        }

        public class GenderLawDecisionOutcome : DecisionOutcome
        {
            public GenderLawDecisionOutcome(bool shouldBeEnforced)
            {
                ShouldDecisionBeEnforced = shouldBeEnforced;
            }

            [SaveableProperty(200)] public bool ShouldDecisionBeEnforced { get; set; }


            public override TextObject GetDecisionTitle()
            {
                var textObject = new TextObject("{=DJRboTyg}{?SUPPORT}Yes{?}No{\\?}");
                textObject.SetTextVariable("SUPPORT", ShouldDecisionBeEnforced ? 1 : 0);
                return textObject;
            }

            public override TextObject GetDecisionDescription()
            {
                if (ShouldDecisionBeEnforced)
                {
                    return new TextObject("{=WEJOxOj8}We support this proposal");
                }

                return new TextObject("{=zfUqKqW2}We oppose this proposal");
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
}