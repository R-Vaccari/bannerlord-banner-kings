using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals
{
    internal abstract class Goal : BannerKingsObject
    {
        internal readonly GoalUpdateType goalUpdateType;

        internal Goal(string stringId, GoalUpdateType goalUpdateType) : base(stringId)
        {
            this.goalUpdateType = goalUpdateType;
        }

        internal abstract bool IsAvailable();

        internal abstract bool IsFulfilled(out List<TextObject> failedReasons);

        internal abstract Hero GetFulfiller();

        internal abstract void ShowInquiry();

        internal abstract void ApplyGoal();

        public abstract void DoAiDecision();
    }
}