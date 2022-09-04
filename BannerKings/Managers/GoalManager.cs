using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Goals;
using BannerKings.Utils.Extensions;

namespace BannerKings.Managers
{
    public class GoalManager
    {
        public GoalManager()
        {
            PostInitialize();
        }

        private static IEnumerable<Goal> AvailableGoals => DefaultGoals.Instance.All;

        public void PostInitialize()
        {
            DefaultGoals.Instance.Initialize();
        }

        public static void UpdateHeroGoals()
        {
            var goals = AvailableGoals.Where(goal => goal.goalUpdateType == GoalUpdateType.Hero).ToList();
            UpdateGoals(goals);
        }

        public static void UpdateSettlementGoals()
        {
            var goals = AvailableGoals.Where(goal => goal.goalUpdateType == GoalUpdateType.Settlement).ToList();
            UpdateGoals(goals);
        }

        private static void UpdateGoals(IEnumerable<Goal> goals)
        {
            foreach (var goal in goals.Where(goal => goal.IsFulfilled(out var failedReasons)))
            {
                var fulfiller = goal.GetFulfiller();
                if (fulfiller is null)
                {
                    throw new BannerKingsException($"Could not resolve fulfiller for goal with id {goal.StringId}.");
                }

                if (fulfiller.IsPlayer())
                {
                    goal.ShowInquiry();
                }
                else
                {
                    goal.DoAiDecision();
                }
            }
        }
    }
}