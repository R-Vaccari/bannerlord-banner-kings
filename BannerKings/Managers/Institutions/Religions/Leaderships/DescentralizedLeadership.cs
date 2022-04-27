using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DescentralizedLeadership : ReligiousLeadership
    {

        private List<Hero> leaders;
        private Religion religion;
        public DescentralizedLeadership() : base()
        {
            this.leaders = new List<Hero>();
            
        }

        public override void Initialize(Religion religion)
        {
            this.religion = religion;
        }

        public override void DecideNewLeader()
        {
            throw new System.NotImplementedException();
        }

        public override Hero GetLeader()
        {
            throw new System.NotImplementedException();
        }
    }
}
