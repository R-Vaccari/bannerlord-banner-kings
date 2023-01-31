
namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class RecurrentRite : Rite
    {
        public int DayOfTheSeason { get; protected set; }
        public int SeasonOfTheYear { get; protected set; }
    }
}
