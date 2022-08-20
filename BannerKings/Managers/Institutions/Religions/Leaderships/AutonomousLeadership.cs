using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class AutonomousLeadership : DescentralizedLeadership
    {
        public override TextObject GetHint()
        {
            return new(
                "{=!}Autonomous religions do not have any sort of hierarchy for their spiritual guides. Even though different kinds of clergymen can exist, they are equally considered a 'head of faith' on their own right.");
        }

        public override TextObject GetName()
        {
            return new("{=!}Autonomous");
        }

        public List<Clergyman> GetLeaders()
        {
            return null;
        }
    }
}