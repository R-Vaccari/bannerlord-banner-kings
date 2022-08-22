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

        internal abstract bool IsFulfilled();

        internal abstract Hero GetFulfiller();

        internal abstract TextObject GetDecisionText();

        internal abstract void ApplyGoal();
    }
}