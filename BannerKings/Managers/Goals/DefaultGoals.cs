using System.Collections.Generic;
using BannerKings.Managers.Goals.Decisions;

namespace BannerKings.Managers.Goals
{
    internal class DefaultGoals : DefaultTypeInitializer<DefaultGoals, Goal>
    {
        public override IEnumerable<Goal> All
        {
            get { yield return GreaterBattania; }
        }

        public Goal GreaterBattania { get; private set; }

        public override void Initialize()
        {
            GreaterBattania = new GreaterBattaniaGoal();
        }
    }
}