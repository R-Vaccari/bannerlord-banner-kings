using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class KinshipLeadership : DescentralizedLeadership
    {
        public override TextObject GetHint() => new TextObject("{=!}Kinship religions have their religions leaders dictated by the landed clans that adhere to the faith. Each clan is responsable for appointing a proeminent preacher from among their feifs.");

        public override List<Clergyman> GetLeaders()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetName() => new TextObject("{=!}Kinship");
    }
}
