using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals
{
    public abstract class Goal : BannerKingsObject
    {
        public Goal(string stringId, Hero fulfiller = null) : base(stringId)
        {
            Fulfiller = fulfiller;
        }

        protected void FinishGoal()
        {
            BannerKingsConfig.Instance.GoalManager.AddGoal(Fulfiller, this);
        }

        public abstract Goal GetCopy(Hero fulfiller);

        protected Hero Fulfiller { get; set; }
        public abstract bool TickClanLeaders { get; }
        public abstract bool TickClanMembers { get; }
        public abstract bool TickNotables { get; }
        public abstract GoalCategory Category { get; }

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