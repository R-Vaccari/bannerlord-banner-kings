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

        public void AddGoal(Hero fulfiller, Goal goal)
        {
            if (!heroGoals.ContainsKey(fulfiller)) heroGoals[fulfiller] = new Dictionary<Goal, CampaignTime>(1);
            heroGoals[fulfiller].Add(goal, CampaignTime.Now);
        }

        public GoalManager()
        {
            heroGoals = new Dictionary<Hero, Dictionary<Goal, CampaignTime>>(50);
            PostInitialize();
        }

        public void PostInitialize()
        {
            if (heroGoals == null) heroGoals = new Dictionary<Hero, Dictionary<Goal, CampaignTime>>(50);
            DefaultGoals.Instance.Initialize();
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