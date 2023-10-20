using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles.Governments
{
    public class BKContractChangeDecision : KingdomDecision
    {
        [SaveableProperty(100)] public FeudalTitle Title { get; private set; }
        [SaveableProperty(101)] public FeudalContract Proposed { get; private set; }

        public BKContractChangeDecision(FeudalTitle title, FeudalContract proposed, Clan proposerClan) : base(proposerClan)
        {
            Title = title;
            Proposed = proposed;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            if ((chosenOutcome as ContractDecisionOutcome).ShouldDecisionBeEnforced)
            {
                Title.ChangeContract(Proposed.Government);
                Title.ChangeContract(Proposed.Succession);
                Title.ChangeContract(Proposed.Inheritance);
                Title.ChangeContract(Proposed.GenderLaw);
            }
        }

        public override void ApplySecondaryEffects(MBReadOnlyList<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {
            if ((chosenOutcome as ContractDecisionOutcome).ShouldDecisionBeEnforced)
            {
                if (!Proposed.Government.Successions.Contains(Proposed.Succession))
                {
                    Succession s = Proposed.Government.Successions.First();
                    Title.ChangeContract(s);
                }
            }  
        }

        public override Clan DetermineChooser() => Kingdom.RulingClan;

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            yield return new ContractDecisionOutcome(true);
            yield return new ContractDecisionOutcome(false);
            yield break;
        }

        public override void DetermineSponsors(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
        {
            foreach (DecisionOutcome decisionOutcome in possibleOutcomes)
            {
                if (((ContractDecisionOutcome)decisionOutcome).ShouldDecisionBeEnforced)
                {
                    decisionOutcome.SetSponsor(base.ProposerClan);
                }
                else
                {
                    AssignDefaultSponsor(decisionOutcome);
                }
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            ContractDecisionOutcome outcome = possibleOutcome as ContractDecisionOutcome;
            float result = 0f;

            Hero hero = clan.Leader;
            ContractAspect aspect = GetAspect();
            result += aspect.Authoritarian * (hero.GetTraitLevel(DefaultTraits.Authoritarian) - 10f);
            result += aspect.Egalitarian * (hero.GetTraitLevel(DefaultTraits.Egalitarian) - 10f);
            result += aspect.Oligarchic * (hero.GetTraitLevel(DefaultTraits.Oligarchic) - 10f);

            if (aspect is Government)
            {
                if (clan == Kingdom.RulingClan && outcome.ShouldDecisionBeEnforced)
                {
                    result -= 5000f;
                }
            }

            if (!outcome.ShouldDecisionBeEnforced) result *= -1f;

            return result * 100f;
        }

        public override TextObject GetChooseDescription()
            => new TextObject("{=7XXDexLV}As ruler, you must decide whether to enforce the law {LAW}.")
            .SetTextVariable("LAW", GetDifference());

        public override TextObject GetChooseTitle() => new TextObject("{=iyyXeYam}Implement the {LAW} law")
            .SetTextVariable("LAW", GetDifference());

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, 
            bool isShortVersion = false)
        {
            bool enforce = (chosenOutcome as ContractDecisionOutcome).ShouldDecisionBeEnforced;
            TextObject result = enforce ? new TextObject("{=EaWBQYbX}The {CHANGE} law will be made part of the realm's demesne, replacing the {CURRENT} law.") :
                new TextObject("{=Zcd8IQKR}The {CHANGE} law will not be made part of the realm's demesne, and the {CURRENT} law is upheld.");

            return result.SetTextVariable("CHANGE", GetDifference())
                .SetTextVariable("CURRENT", GetCurrent());
        }

        public override TextObject GetGeneralTitle() => new TextObject("{=iKl3Ol2D}Contract Structure Change");

        public override int GetProposalInfluenceCost() => 500;

        public override DecisionOutcome GetQueriedDecisionOutcome(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
            => possibleOutcomes.FirstOrDefault((DecisionOutcome t) => ((ContractDecisionOutcome)t).ShouldDecisionBeEnforced);

        public override TextObject GetSecondaryEffects()
        {
            if (!Proposed.Government.Successions.Contains(Proposed.Succession))
            {
                Succession s = Proposed.Government.Successions.First();
                return new TextObject("{=1x2KUJ3j}Succession law will be changed to {NAME}.")
                    .SetTextVariable("NAME", s.Name);
            }
            return TextObject.Empty;
        }

        public override TextObject GetSupportDescription() =>
            new TextObject("{=42uf705u}{FACTION_LEADER} proposes the legal implementation of {CHANGE} throughout the realm's demesne, replacing its current law, {CURRENT}. You can pick your stance regarding this decision.")
            .SetTextVariable("FACTION_LEADER", Kingdom.RulingClan.Leader.Name)
            .SetTextVariable("CHANGE", GetDifference())
            .SetTextVariable("CURRENT", GetCurrent());

        public override TextObject GetSupportTitle() => new TextObject("{=rMH701T0}Vote for the implementation of {CHANGE}")
            .SetTextVariable("CHANGE", GetDifference());

        public override bool IsAllowed() => Title != null && Proposed != null;

        private ContractAspect GetAspect()
        {
            if (Title.Contract.Government != Proposed.Government) return Proposed.Government;
            if (Title.Contract.Succession != Proposed.Succession) return Proposed.Succession;
            if (Title.Contract.Inheritance != Proposed.Inheritance) return Proposed.Inheritance;
            else return Proposed.GenderLaw;
        }

        private TextObject GetCurrent()
        {
            if (Title.Contract.Government != Proposed.Government) return Title.Contract.Government.Name;
            if (Title.Contract.Succession != Proposed.Succession) return Title.Contract.Succession.Name;
            if (Title.Contract.Inheritance != Proposed.Inheritance) return Title.Contract.Inheritance.Name;
            else return Title.Contract.GenderLaw.Name;
        }

        private TextObject GetDifference() => GetAspect().Name;

        public class ContractDecisionOutcome : DecisionOutcome
        {
            [SaveableProperty(200)]
            public bool ShouldDecisionBeEnforced { get; private set; }

            public override TextObject GetDecisionTitle()
            {
                TextObject textObject = new TextObject("{=5b71bMh1}{?SUPPORT}Change contract{?}Maintain contract{\\?}", null);
                textObject.SetTextVariable("SUPPORT", ShouldDecisionBeEnforced ? 1 : 0);
                return textObject;
            }

            public override TextObject GetDecisionDescription()
            {
                if (ShouldDecisionBeEnforced)
                {
                    return new TextObject("{=pWyxaauF}We support this proposal", null);
                }
                return new TextObject("{=zfUqKqW2}We oppose this proposal", null);
            }

            public override string GetDecisionLink() => null;

            public override ImageIdentifier GetDecisionImageIdentifier() => null;

            public ContractDecisionOutcome(bool shouldBeEnforced)
            {
                ShouldDecisionBeEnforced = shouldBeEnforced;
            }
        }
    }
}
