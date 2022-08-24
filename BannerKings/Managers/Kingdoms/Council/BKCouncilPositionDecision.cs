using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Council
{
    public class BKCouncilPositionDecision : KingdomDecision
    {
        public BKCouncilPositionDecision(Clan proposerClan, CouncilData data, CouncilMember position) : base(proposerClan)
        {
            Data = data;
            Position = position;
            Religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposerClan.Leader);
            IsEnforced = true;
        }

        [SaveableProperty(99)] protected CouncilData Data { get; set; }

        [SaveableProperty(98)] protected CouncilMember Position { get; set; }

        [SaveableProperty(97)] protected Religion Religion { get; set; }

        public override bool IsKingsVoteAllowed => false;

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            Position.Member = ((CouncilPositionDecisionOutcome) chosenOutcome).Candidate;
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
                    return 10;
                case Supporter.SupportWeights.StronglyFavor:
                    return 30;
                case Supporter.SupportWeights.FullyPush:
                    return 50;
                default:
                    throw new ArgumentOutOfRangeException("supportWeight", supportWeight, null);
            }
        }


        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            foreach (var hero in Data.GetAvailableHeroes())
            {
                if (Position.IsValidCandidate(hero))
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
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            var result = 2f;
            var candidate = ((CouncilPositionDecisionOutcome) possibleOutcome).Candidate;

            result += Data.GetCompetence(candidate, Position.Position) * 4f;
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
                            result -= 1.5f;
                            break;
                        case FaithStance.Hostile:
                            result -= 4f;
                            break;
                    }
                }
            }

            result += ProposerClan.Leader.GetRelation(candidate) * 0.02f;
            if (!candidate.IsLord)
            {
                result -= 1f;
            }

            return MathF.Clamp(result, -3f, 8f);
        }

        public override TextObject GetChooseDescription()
        {
            var textObject =
                new TextObject(
                    "{=QWtE9Q9PK}As the sovereign of {KINGDOM}, you must decide whether to approve this contract change or not.");
            textObject.SetTextVariable("KINGDOM", Kingdom.Name);
            return textObject;
        }

        public override TextObject GetChooseTitle()
        {
            return new TextObject("{=ar0YnUsbq}Choose the next council member to occupy the position of {POSITION}")
                .SetTextVariable("POSITION", Position.GetName());
        }

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus,
            bool isShortVersion = false)
        {
            var candidate = ((CouncilPositionDecisionOutcome) chosenOutcome).Candidate;
            var textObject = new TextObject("{=p1ORXqXqf}The{KINGDOM} has chosen {NAME} as their new council member.")
                .SetTextVariable("KINGDOM", Kingdom.Name)
                .SetTextVariable("NAME", candidate.Name);
            return textObject;
        }

        public override TextObject GetGeneralTitle()
        {
            return new TextObject("{=pHvJNFY3c}Council member for position {POSITION}")
                .SetTextVariable("POSITION", Position.GetName());
        }


        public override int GetProposalInfluenceCost()
        {
            return 0;
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
            return new TextObject(
                    "{=H5LjaS6nf}{KINGDOM_NAME} will decide who will occupy the position of {POSITION}. You can pick your stance regarding this decision.")
                .SetTextVariable("KINGDOM_NAME", Kingdom.Name)
                .SetTextVariable("POSITION", Position.GetName());
        }

        public override TextObject GetSupportTitle()
        {
            return new TextObject("{=ar0YnUsbq}Choose the next council member to occupy the position of {POSITION}")
                .SetTextVariable("POSITION", Position.GetName());
        }

        public override bool IsAllowed()
        {
            return Data != null && Position != null;
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
                return new TextObject("{=vp931xMbr}{NAME} should be appointed")
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