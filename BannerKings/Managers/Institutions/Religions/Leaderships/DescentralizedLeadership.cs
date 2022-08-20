using System.Collections.Generic;
using System.Linq;

namespace BannerKings.Managers.Institutions.Religions.Leaderships;

public abstract class DescentralizedLeadership : ReligiousLeadership
{
    public override void Initialize(Religion religion)
    {
    }

    public List<Clergyman> GetLeaders(Religion religion)
    {
        var list = new List<Clergyman>();
        var max = religion.Faith.GetMaxClergyRank();
        foreach (var clergyman in religion.Clergy.Values.ToList())
        {
            if (clergyman.Rank == max)
            {
                list.Add(clergyman);
            }
        }

        return list;
    }

    public override bool IsLeader(Clergyman clergyman)
    {
        return false;
    }
}