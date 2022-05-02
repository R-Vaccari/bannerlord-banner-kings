using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class DescentralizedLeadership : ReligiousLeadership
    {
        public override void Initialize(Religion religion)
        {
            religion = religion;
        }

        public abstract List<Clergyman> GetLeaders();
    }
}
