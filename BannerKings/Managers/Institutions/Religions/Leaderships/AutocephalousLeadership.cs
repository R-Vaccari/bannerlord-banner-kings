using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class AutocephalousLeadership : DescentralizedLeadership
    {
        public override TextObject GetHint()
        {
            throw new NotImplementedException();
        }

        public override List<Clergyman> GetLeaders()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetName() => new TextObject("{=!}Autocephalous");
    }
}
