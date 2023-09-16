using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Goals;
using BannerKings.Utils.Extensions;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers
{
    public class GoalManager
    {
        private Dictionary<Hero, Dictionary<Goal, CampaignTime>> heroGoals;

        public CampaignTime GetHeroGoalTime(Hero hero, Goal goal)
        {
            CampaignTime time = CampaignTime.Zero;
            if (heroGoals.ContainsKey(hero))
            {
                if (heroGoals[hero].ContainsKey(goal))
                {
                    time = heroGoals[hero][goal];
                }
            }

            return time;
        }

        public GoalManager()
        {
            heroGoals = new Dictionary<Hero, Dictionary<Goal, CampaignTime>>(50);
            PostInitialize();
        }

        private static IEnumerable<Goal> AvailableGoals => DefaultGoals.Instance.All;

        public void PostInitialize()
        {
            if (heroGoals == null) heroGoals = new Dictionary<Hero, Dictionary<Goal, CampaignTime>>(50);
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