using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Clergyman
    {
        private Hero hero;
        private float piety;
        private int rank;

        public Clergyman(Hero hero, int rank)
        {
            this.hero = hero;
            this.rank = rank;
        }

        public Hero Hero => this.hero;
        public int Rank => this.rank;
    }
}
