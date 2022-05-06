using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class DescentralizedLeadership : ReligiousLeadership
    {

        private List<Hero> leaders;
        private Religion religion;
        public DescentralizedLeadership()
        {
            leaders = new List<Hero>();
            
        }

        public override void Initialize(Religion religion)
        {
            this.religion = religion;
        }

        public abstract List<Clergyman> GetLeaders();
    }
}
