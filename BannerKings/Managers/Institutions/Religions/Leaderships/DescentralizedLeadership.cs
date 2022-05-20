using System.Collections.Generic;
using System.Linq;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class DescentralizedLeadership : ReligiousLeadership
    {
        public DescentralizedLeadership()
        {
            
        }

        public override void Initialize(Religion religion)
        {

        }

        public List<Clergyman> GetLeaders(Religion religion)
        {
            List<Clergyman> list = new List<Clergyman>();
            int max = religion.Faith.GetMaxClergyRank();
            foreach (Clergyman clergyman in religion.Clergy.Values.ToList())
                if (clergyman.Rank == max)
                    list.Add(clergyman);
            return list;
        }

        public override bool IsLeader(Clergyman clergyman) => false;
    }
}
