using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Goals;
using BannerKings.Utils.Extensions;

namespace BannerKings.Managers
{
    public class GoalManager
    {
        private static IEnumerable<Goal> AvailableGoals => DefaultGoals.Instance.All;

        public void UpdateHeroGoals()
        {
            var goals = AvailableGoals.Where(goal => goal.goalUpdateType == GoalUpdateType.Hero).ToList();
            UpdateGoals(goals);
        }

        public void UpdateSettlementGoals()
        {
            var goals = AvailableGoals.Where(goal => goal.goalUpdateType == GoalUpdateType.Settlement).ToList();
            UpdateGoals(goals);
        }

        private void UpdateGoals(IEnumerable<Goal> goals)
        {
            foreach (var goal in goals.Where(goal => goal.IsFulfilled()))
            {
                var fulfiller = goal.GetFulfiller();
                if (fulfiller is null)
                {
                    throw new BannerKingsException($"Could not resolve fulfiller for goal with id {goal.StringId}.");
                }

                if (fulfiller.IsPlayer())
                {
                    ShowDecisionUI(goal);
                }
                else
                {
                    DoAiDecision(goal);
                }
            }
        }

        private void ShowDecisionUI(Goal goal)
        {
            //TODO: Show UI, so Player can make his decision.
            var decisionText = goal.GetDecisionText();
            goal.ApplyGoal();
        }

        private void DoAiDecision(Goal goal)
        {
            //TODO: Let AI make his decision.
            goal.ApplyGoal();
        }
    }
}