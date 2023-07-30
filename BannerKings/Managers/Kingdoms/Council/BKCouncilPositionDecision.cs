using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Council
{
    public class BKCouncilPositionDecision : KingdomDecision
    {
        public BKCouncilPositionDecision(Clan proposerClan, CouncilData data, CouncilMember position, Hero suggested) : base(proposerClan)
        {
            Data = data;
            Position = position;
            Religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposerClan.Leader);
            IsEnforced = true;
            Suggested = suggested;
        }

        [SaveableProperty(100)] protected Hero Suggested { get; set; }
        [SaveableProperty(99)] protected CouncilData Data { get; set; }
        [SaveableProperty(98)] protected CouncilMember Position { get; set; }
        [SaveableProperty(97)] protected Religion Religion { get; set; }

        public override bool IsKingsVoteAllowed => true;

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            Hero candidate = ((CouncilPositionDecisionOutcome)chosenOutcome).Candidate;
            CouncilAction action = BannerKingsConfig.Instance.CouncilModel.GetAction(CouncilActionType.REQUEST,
                Data,
                candidate,
                Position,
                null,
                true);
            BannerKingsConfig.Instance.CourtManager.AddHeroToCouncil(action);
        }

        public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {
        }

        public override Clan DetermineChooser()
        {
            return Kingdom.RulingClan;
        }

        protected override int GetInfluenceCostOfSupportInternal(Supporter.SupportWeights supportWeight)
        {
            switch (supportWeight)
            {
                case Supporter.SupportWeights.Choose:
                case Supporter.SupportWeights.StayNeutral:
                    return 0;
                case Supporter.SupportWeights.SlightlyFavor:
                    return 50;
                case Supporter.SupportWeights.StronglyFavor:
                    return 100;
                case Supporter.SupportWeights.FullyPush:
                    return 150;
                default:
                    throw new ArgumentOutOfRangeException("supportWeight", supportWeight, null);
            }
        }


        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            foreach (var hero in Data.GetCourtMembers())
            {
                CouncilAction action = BannerKingsConfig.Instance.CouncilModel.GetAction(CouncilActionType.REQUEST,
                    Data,
                    hero,
                    Position,
                    null,
                    true);
                if (action.Possible)
                {
                    yield return new CouncilPositionDecisionOutcome(hero);
                }
            }
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            foreach (var decisionOutcome in possibleOutcomes)
            {
                var candidate = ((CouncilPositionDecisionOutcome) decisionOutcome).Candidate;
                if (candidate.IsLord && candidate.Clan != null)
                {
                    decisionOutcome.SetSponsor(candidate.Clan);
                }
                else
                {
                    decisionOutcome.SetSponsor(ProposerClan);
                }
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            var candidate = ((CouncilPositionDecisionOutcome)possibleOutcome).Candidate;
            var result = new ExplainedNumber(Position.CalculateCandidateCompetence(candidate).ResultNumber * 100f);
            if (candidate == Suggested)
            {
                float factor = 1f;
                if (clan != ProposerClan)
                {
                    factor = clan.Leader.GetRelation(ProposerClan.Leader) * 0.01f;
                }

                result.Add(20f * factor);
            }

            if (Religion != null)
            {
                var candidateReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(candidate);
                if (candidateReligion != Religion)
                {
                    var stance = FaithStance.Untolerated;
                    if (candidateReligion != null)
                    {
                        stance = Religion.Faith.GetStance(candidateReligion.Faith);
                    }

                    switch (stance)
                    {
                        case FaithStance.Untolerated:
                            result.AddFactor(-0.15f);
                            break;
                        case FaithStance.Hostile:
                            result.AddFactor(-0.4f);
                            break;
                    }
                }
            }

            float oligarchic = 100f - (MathF.Abs(candidate.GetTraitLevel(DefaultTraits.Oligarchic) -
                clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic)) * 12f);
            float egalitarian = 100f - (MathF.Abs(candidate.GetTraitLevel(DefaultTraits.Egalitarian) -
                clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian)) * 12f);
            float authoritarian = 100f - (MathF.Abs(candidate.GetTraitLevel(DefaultTraits.Authoritarian) -
                clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian)) * 12f);
            result.Add(oligarchic);
            result.Add(egalitarian);
            result.Add(authoritarian);

            result.AddFactor(clan.Leader.GetRelation(candidate) * 0.02f);
            return result.ResultNumber;
        }

        public override TextObject GetChooseDescription()
        {
            var textObject = new TextObject("{=atiwRMmv}As the sovereign of {KINGDOM}, you must decide whether to approve this contract change or not.");
            textObject.SetTextVariable("KINGDOM", Kingdom.Name);
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            return new TextObject("{=dqEO1Ug4}Choose the next council member to occupy the position of {POSITION}")
                .SetTextVariable("POSITION", Position.Name);
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus,
            bool isShortVersion = false)
        {
            var candidate = ((CouncilPositionDecisionOutcome) chosenOutcome).Candidate;
            var textObject = new TextObject("{=2GMvmKEb}The {KINGDOM} has chosen {NAME} as their new council member.")
                .SetTextVariable("KINGDOM", Kingdom.Name)
                .SetTextVariable("NAME", candidate.Name);
            return textObject;
        }

        public override TextObject GetGeneralTitle()
        {
            return new TextObject("{=mUaJDjqO}Council member for position {POSITION}")
                .SetTextVariable("POSITION", Position.Name);
        }

        public override int GetProposalInfluenceCost()
        {
            return 100;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return (from k in possibleOutcomes
                orderby k.Merit descending
                select k).ToList().FirstOrDefault();
        }

        public override TextObject GetSecondaryEffects()
        {
            return null;
        }

        protected override bool CanProposerClanChangeOpinion()
        {
            return true;
        }

        public override TextObject GetSupportDescription()
        {
            return new TextObject("{=Hj6NHtbc}{KINGDOM_NAME} will decide who will occupy the position of {POSITION}. You can pick your stance regarding this decision.")
                .SetTextVariable("KINGDOM_NAME", Kingdom.Name)
                .SetTextVariable("POSITION", Position.Name);
        }

        public override TextObject GetSupportTitle()
        {
            return new TextObject("{=dqEO1Ug4}Choose the next council member to occupy the position of {POSITION}")
                .SetTextVariable("POSITION", Position.Name);
        }

        public override bool IsAllowed()
        {
            return !Kingdom.UnresolvedDecisions.Any(x => x is BKCouncilPositionDecision && x != this);
        }

        public class CouncilPositionDecisionOutcome : DecisionOutcome
        {
            public CouncilPositionDecisionOutcome(Hero candidate)
            {
                Candidate = candidate;
            }

            [SaveableProperty(200)] public Hero Candidate { get; set; }

            public override TextObject GetDecisionTitle()
            {
                return Candidate.Name;
            }

            public override TextObject GetDecisionDescription()
            {
                return new TextObject("{=01waap2L}{NAME} should be appointed")
                    .SetTextVariable("NAME", Candidate.Name);
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