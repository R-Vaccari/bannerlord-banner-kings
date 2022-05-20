using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class AutonomousLeadership : DescentralizedLeadership
    {
        public override TextObject GetHint() => new TextObject("{=!}Autonomous religions do not have any sort of hierarchy for their spiritual guides. Even though different kinds of clergymen can exist, they are equally considered a 'head of faith' on their own right.");
        public override TextObject GetName() => new TextObject("{=!}Autonomous");

        public new List<Clergyman> GetLeaders() => null;
    }
}
