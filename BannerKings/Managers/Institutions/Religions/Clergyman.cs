using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Clergyman
    {
        public Clergyman(Hero hero, int rank)
        {
            Hero = hero;
            Rank = rank;
        }

        [field: SaveableField(1)] public Hero Hero { get; }

        [field: SaveableField(3)] public int Rank { get; set; }
    }
}