
namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class RecurrentRite : Rite
    {
        public abstract int DayOfTheSeason { get; }
        public abstract int SeasonOfTheYear { get; }
    }
}
