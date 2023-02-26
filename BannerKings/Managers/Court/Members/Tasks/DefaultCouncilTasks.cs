using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members.Tasks
{
    public class DefaultCouncilTasks : DefaultTypeInitializer<DefaultCouncilTasks, CouncilTask>
    {
        public CouncilTask OrganizeMiltia { get; } = new CouncilTask("OrganizeMilitia");
        public override IEnumerable<CouncilTask> All
        {
            get
            {
                yield return OrganizeMiltia;
            }
        }

        public override void Initialize()
        {
            OrganizeMiltia.Initialize(new TextObject(),
                new TextObject(),
                new TextObject(),
                0f);
        }
    }
}
