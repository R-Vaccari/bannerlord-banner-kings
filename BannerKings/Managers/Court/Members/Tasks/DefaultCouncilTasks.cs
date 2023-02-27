using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members.Tasks
{
    public class DefaultCouncilTasks : DefaultTypeInitializer<DefaultCouncilTasks, CouncilTask>
    {
        public CouncilTask OrganizeMiltia { get; } = new CouncilTask("OrganizeMilitia");
        public CouncilTask EncourageMilitarism { get; } = new CouncilTask("OrganizeMilitia");
        public CouncilTask SuperviseGarrisons { get; } = new CouncilTask("OrganizeMilitia");

        public CouncilTask OverseeSecurity { get; } = new CouncilTask("OrganizeMilitia");

        public CouncilTask IntegrateTitles { get; } = new CouncilTask("OrganizeMilitia");
        public CouncilTask PromoteCulture { get; } = new CouncilTask("OrganizeMilitia");
        public override IEnumerable<CouncilTask> All
        {
            get
            {
                yield return OrganizeMiltia;
            }
        }

        public override void Initialize()
        {
            OrganizeMiltia.Initialize(new TextObject("{=!}Organize Militia"),
                new TextObject("{=!}Enact militia trainning drills and the obligation of households owning arms. Through stimulation of the state, more workforce members become active part of fief militias, as well as with generally improved trainning and equipment."),
                new TextObject("{=!}Increased militia production\nImproved militia quality"),
                0f);
        }
    }
}
