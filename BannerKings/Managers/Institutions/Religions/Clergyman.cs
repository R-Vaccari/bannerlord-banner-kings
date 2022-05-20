using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Clergyman
    {
        [SaveableField(1)]
        private Hero hero;

        [SaveableField(2)]
        private float piety;

        [SaveableField(3)]
        private int rank;

        public Clergyman(Hero hero, int rank)
        {
            this.hero = hero;
            this.rank = rank;
        }

        public Hero Hero => hero;
        public int Rank
        {
            get => rank;
            set => rank = value;
        }
    }
}
