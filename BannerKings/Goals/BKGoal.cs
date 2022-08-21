using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Goals
{
    internal abstract class BKGoal : BannerKingsObject
    {
        public bool IsDone { get; protected set; }

        protected BKGoal(string stringId) : base(stringId)
        {
        }

        public abstract void Update();

        public abstract bool IsFulfilled(out List<TextObject> failedReasons);
    }
}