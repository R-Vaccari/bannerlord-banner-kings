using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using System.Linq;

namespace BannerKings.Managers.Kingdoms.Council
{
    public class BKCouncilPositionDecision : KingdomDecision
    {
        [SaveableProperty(99)]
        protected CouncilData Data { get; set; }

        [SaveableProperty(98)]
        protected CouncilMember Position { get; set; }

        [SaveableProperty(97)]
        protected Religion Religion { get; set; }

        public BKCouncilPositionDecision(Clan proposerClan, CouncilData data, CouncilMember position) : base(proposerClan)
        {
            Data = data;
            Position = position;
            Religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposerClan.Leader);
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            Position.Member = ((CouncilPositionDecisionOutcome)chosenOutcome).Candidate;
        }

        public override void ApplySecondaryEffects(List<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {

        }

        public override Clan DetermineChooser()
        {
            return Kingdom.RulingClan;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            foreach (Hero hero in Data.GetAvailableHeroes())
                if (Position.IsValidCandidate(hero))
                    yield return new CouncilPositionDecisionOutcome(hero);
        }

        public override void DetermineSponsors(List<DecisionOutcome> possibleOutcomes)
        {
            foreach (DecisionOutcome decisionOutcome in possibleOutcomes)
            {
                Hero candidate = ((CouncilPositionDecisionOutcome)decisionOutcome).Candidate;
                if (candidate.IsNoble && candidate.Clan != null)
                    decisionOutcome.SetSponsor(candidate.Clan);
                else decisionOutcome.SetSponsor(ProposerClan);
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            float result = -1f;
            Hero candidate = ((CouncilPositionDecisionOutcome)possibleOutcome).Candidate;
            
            result += Data.GetCompetence(candidate, Position.Position) * 4f;
            if (Religion != null)
            {
                Religion candidateReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(candidate);
                if (candidateReligion != Religion)
                {
                    FaithStance stance = Religion.Faith.GetStance(candidateReligion.Faith);
                    if (stance == FaithStance.Untolerated)
                        result -= 1.5f;
                    else if (stance == FaithStance.Hostile)
                        result -= 4f;
                }               
            }

            result += ProposerClan.Leader.GetRelation(candidate) * 0.02f;
            if (!candidate.IsNoble)
                result -= 1f;

            return MathF.Clamp(result, -3f, 8f);
        }

        public override TextObject GetChooseDescription()
        {
            TextObject textObject = new TextObject("{=!}As the sovereign of {KINGDOM}, you must decide whether to approve this contract change or not.");
            textObject.SetTextVariable("KINGDOM", Kingdom.Name);
            return textObject;
        }

        public override TextObject GetChooseTitle() => new TextObject("{=!}Choose the next council member to occupy the position of {POSITION}", null)
                .SetTextVariable("POSITION", Position.GetName());

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            Hero candidate = ((CouncilPositionDecisionOutcome)chosenOutcome).Candidate;
            TextObject textObject = new TextObject("{=!}The{KINGDOM} has chosen {NAME} as their new council member.", null)
                .SetTextVariable("KINGDOM", base.Kingdom.Name)
                .SetTextVariable("NAME", candidate.Name);
            return textObject;
        }

        public override TextObject GetGeneralTitle() => new TextObject("{=!}Council member for position {POSITION}")
            .SetTextVariable("POSITION", Position.GetName());
        

        public override int GetProposalInfluenceCost()
        {
            return 0;
        }

        public override DecisionOutcome GetQueriedDecisionOutcome(List<DecisionOutcome> possibleOutcomes)
        {
            return (from k in possibleOutcomes
                    orderby k.Merit descending
                    select k).ToList<DecisionOutcome>().FirstOrDefault<DecisionOutcome>();
        }

        public override TextObject GetSecondaryEffects()
        {
            return null;
        }

        public override TextObject GetSupportDescription() => new TextObject("{=!}{KINGDOM_NAME} will decide who will occupy the position of {POSITION}. You can pick your stance regarding this decision.", null)
                .SetTextVariable("KINGDOM_NAME", base.Kingdom.Name)
                .SetTextVariable("POSITION", Position.GetName());

        public override TextObject GetSupportTitle() => new TextObject("{=!}Choose the next council member to occupy the position of {POSITION}", null)
                .SetTextVariable("POSITION", Position.GetName());

        public override bool IsKingsVoteAllowed => true;

        public override bool IsAllowed()
        {
            return Data != null && Position != null;
        }

        public class CouncilPositionDecisionOutcome : DecisionOutcome
        {
            [SaveableProperty(200)]
            public Hero Candidate { get; private set; }

            public override TextObject GetDecisionTitle() => Candidate.Name;
            
            public override TextObject GetDecisionDescription() => new TextObject("{=!}{NAME} should be appointed")
                .SetTextVariable("NAME", Candidate.Name);
            
            public override string GetDecisionLink() => null;

            public override ImageIdentifier GetDecisionImageIdentifier() => null;

            public CouncilPositionDecisionOutcome(Hero candidate)
            {
                Candidate = candidate;
            }
        }
    }
}
