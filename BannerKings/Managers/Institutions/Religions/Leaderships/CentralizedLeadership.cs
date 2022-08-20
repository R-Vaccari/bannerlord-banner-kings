namespace BannerKings.Managers.Institutions.Religions.Leaderships;

public abstract class CentralizedLeadership : ReligiousLeadership
{
    private Clergyman clergyman;

    public Clergyman Leader
    {
        get
        {
            if (clergyman == null)
            {
                clergyman = DecideNewLeader();
            }

            return clergyman;
        }
    }

    public abstract Clergyman DecideNewLeader();
}