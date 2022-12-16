﻿using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals
{
    public abstract class Goal : BannerKingsObject
    {
        internal readonly GoalUpdateType goalUpdateType;
        protected Hero Fulfiller;

        internal Goal(string stringId, GoalUpdateType goalUpdateType, Hero fulfiller = null) : base(stringId)
        {
            this.goalUpdateType = goalUpdateType;
            Fulfiller = fulfiller;
        }

        internal List<TextObject> FailedReasons
        {
            get
            {
                IsFulfilled(out var failedReasons);
                return failedReasons;
            }
        }

        internal abstract bool IsAvailable();

        internal abstract bool IsFulfilled(out List<TextObject> failedReasons);

        internal Hero GetFulfiller()
        {
            if (Fulfiller != null) return Fulfiller;
            return Hero.MainHero;
        }

        internal abstract void ShowInquiry();

        internal abstract void ApplyGoal();

        public abstract void DoAiDecision();
    }
}