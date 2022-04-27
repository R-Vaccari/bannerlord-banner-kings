using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class ReligiousLeadership
    {

        public abstract Hero GetLeader();
        public abstract void DecideNewLeader();
    }
}
