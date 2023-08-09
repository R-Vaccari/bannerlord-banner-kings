using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals
{
    public abstract class Goal : BannerKingsObject
    {
        public readonly GoalUpdateType goalUpdateType;
        public readonly GoalCategory goalType;
        protected Hero Fulfiller;

        public Goal(string stringId, GoalCategory goalType, GoalUpdateType goalUpdateType, Hero fulfiller = null) : base(stringId)
        {
            this.goalType = goalType;
            this.goalUpdateType = goalUpdateType;
            Fulfiller = fulfiller;
        }

        public List<TextObject> FailedReasons
        {
            get
            {
                IsFulfilled(out var failedReasons);
                return failedReasons;
            }
        }

        public abstract bool IsAvailable();
        public abstract bool IsFulfilled(out List<TextObject> failedReasons);
        public Hero GetFulfiller()
        {
            if (Fulfiller != null) return Fulfiller;
            return Hero.MainHero;
        }

        public abstract void ShowInquiry();
        public abstract void ApplyGoal();
        public abstract void DoAiDecision();
    }
}