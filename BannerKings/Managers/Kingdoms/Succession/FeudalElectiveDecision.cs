using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;

namespace BannerKings.Managers.Kingdoms.Succession
{
    public class FeudalElectiveDecision : BKKingElectionDecision
    {
        public FeudalElectiveDecision(Clan proposerClan, FeudalTitle title) : base(proposerClan, null, title)
        {
        }

        public override bool IsAllowed()
        {
            return BannerKingsConfig.Instance.TitleModel.GetSuccessionCandidates(PreviousRuler, Title).Count > 1;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            foreach (var candidate in BannerKingsConfig.Instance.TitleModel.CalculateSuccessionLine(Title, PreviousRuler.Clan, PreviousRuler, 3))
            {
                yield return new KingSelectionDecisionOutcome(candidate.Key);
            }
        }

        public override float CalculateMeritOfOutcome(DecisionOutcome candidateOutcome)
        {
            float merit = 0f;
            foreach (Clan clan in Kingdom.Clans)
            {
                if (clan.Leader != Hero.MainHero)
                {
                    merit += CalculateMeritOfOutcomeForClan(clan, candidateOutcome);
                }
            }

            return merit;
        }

        public new float CalculateMeritOfOutcomeForClan(Clan clan, DecisionOutcome outcome)
        {
            float result = 0f;
            Hero candidate = ((KingSelectionDecisionOutcome)outcome).King;
            result += BannerKingsConfig.Instance.TitleModel.GetSuccessionHeirScore(PreviousRuler, candidate, Title).ResultNumber;
            if (clan.Leader != candidate)
            {
                result *= 2f * (clan.Leader.GetRelation(candidate) / 100f);
            }

            return result;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            base.ApplyChosenOutcome(chosenOutcome);
            Hero heir = ((KingSelectionDecisionOutcome)chosenOutcome).King;
            if (heir.Clan == PreviousRuler.Clan && !heir.IsClanLeader())
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(heir.Clan, heir);
            }
        }
    }
}
