using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Contract
{
    public class BKDemesneLawDecision : KingdomDecision
    {
        public BKDemesneLawDecision(Clan proposerClan, FeudalTitle title, DemesneLaw currentLaw) : base(proposerClan)
        {
            Title = title;
            CurrentLaw = currentLaw;
        }

        public void UpdateDecision(DemesneLaw proposedLaw)
        {
            ProposedLaw = proposedLaw;
        }

        [SaveableProperty(99)] public FeudalTitle Title { get; set; }

        [SaveableProperty(100)] public DemesneLaw CurrentLaw { get; set; }

        [SaveableProperty(101)] public DemesneLaw ProposedLaw { get; set; }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            var outcome = chosenOutcome as DemesneLawDecisionOutcome;
            Title.EnactLaw(outcome.Law);
        }

        public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {
        }

        public override Clan DetermineChooser() => Kingdom.RulingClan;
        

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            yield return new DemesneLawDecisionOutcome(ProposedLaw);
            yield return new DemesneLawDecisionOutcome(CurrentLaw, true);
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            foreach (var decisionOutcome in possibleOutcomes)
            {
                if (((DemesneLawDecisionOutcome)decisionOutcome).Law == ProposedLaw)
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
            var outcome = possibleOutcome as DemesneLawDecisionOutcome;

            float egalitatian = 0.6f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian) - 0.9f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic);
            float oligarchic = 0.6f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic) - 0.9f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian) - 0.5f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian);
            float authoritarian = 0.8f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian) - 1.3f * (float)clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic);
            
            if (clan.Kingdom.RulingClan == clan)
            {
                authoritarian += 1.5f;
                oligarchic += 0.6f;
                egalitatian -= 0.6f;
            }

            if (clan.Culture == Kingdom.Culture)
            {
                oligarchic += 0.2f;
            }


            if (clan.Tier <= 2)
            {
                egalitatian += 0.4f;
                oligarchic += 0.1f;
                authoritarian -= 1f;
            }
            else if (clan.Tier <= 4)
            {
                oligarchic += 0.5f;
                authoritarian -= 0.3f;
                egalitatian -= 0.2f;
            }
            else
            {
                oligarchic += 1.2f;
                authoritarian += 0.5f;
                egalitatian -= 0.6f;
            }
            
            
            float support = outcome.Law.EgalitarianWeight * egalitatian + 
                outcome.Law.OligarchicWeight * oligarchic + 
                outcome.Law.AuthoritarianWeight * authoritarian;

            return support * 100;
        }

        public override TextObject GetChooseDescription()
        {
            var textObject = new TextObject("{=atiwRMmv}As the sovereign of {KINGDOM}, you must decide whether to approve this contract change or not.");
            textObject.SetTextVariable("KINGDOM", Kingdom.Name);
            return textObject;
        }


        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus,
            bool isShortVersion = false)
        {
            var law = (chosenOutcome as DemesneLawDecisionOutcome).Law;
            return new TextObject("{=17qaS6xu}The peers of {TITLE} have decided on the {LAW} law.")
                .SetTextVariable("TITLE", Title.FullName)
                .SetTextVariable("LAW", law.Name);
        }

        public override TextObject GetGeneralTitle() => new TextObject("{=p3VirLQH}Demesne Law");

        public override int GetProposalInfluenceCost() => ProposedLaw.InfluenceCost;

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes) => 
            (from k in possibleOutcomes orderby k.Merit descending select k).ToList().FirstOrDefault();

        public override TextObject GetSecondaryEffects() => new TextObject("{=bdTS2dAa}All supporters gains some relation with each other.", null);

        public override TextObject GetSupportDescription() => new TextObject("{=mn9edsZr}The peers of {TITLE} will decide the next {LAW} demesne law. You may pick your stance.")
            .SetTextVariable("TITLE", Title.FullName)
            .SetTextVariable("LAW", GameTexts.FindText("str_bk_demesne_law", CurrentLaw.LawType.ToString()));

        public override TextObject GetChooseTitle() => new TextObject("{=c7niULaT}Vote for the next {LAW} demesne law")
            .SetTextVariable("LAW", GameTexts.FindText("str_bk_demesne_law", CurrentLaw.LawType.ToString()));

        public override TextObject GetSupportTitle() => new TextObject("{=c7niULaT}Vote for the next {LAW} demesne law")
            .SetTextVariable("LAW", GameTexts.FindText("str_bk_demesne_law", CurrentLaw.LawType.ToString()));

        public override bool IsAllowed() => Title.contract != null && !ProposedLaw.Equals(CurrentLaw);
        

        public class DemesneLawDecisionOutcome : DecisionOutcome
        {
            public DemesneLawDecisionOutcome(DemesneLaw law, bool current = false)
            {
                Law = law;
            }

            [SaveableProperty(200)] public DemesneLaw Law { get; set; }
            [SaveableProperty(201)] public bool Current { get; set; }


            public override TextObject GetDecisionTitle() => Law.Name;

            public override TextObject GetDecisionDescription()
            {
                if (Current)
                {
                    return new TextObject("{=OMkKKOYG}We support the continuation of the demesne law {LAW}")
                        .SetTextVariable("LAW", Law.Name);
                }

                return new TextObject("{=H5yaNxnY}We support the enactment of the demesne law {LAW}")
                    .SetTextVariable("LAW", Law.Name);
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