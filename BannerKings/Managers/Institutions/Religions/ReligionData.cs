using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class ReligionData : BannerKingsData
    {
        [SaveableField(1)]
        private Settlement settlement;

        [SaveableField(2)]
        private Clergyman clergyman;

        [SaveableField(3)]
        private Religion religion;

        public ReligionData(Religion religion, Settlement settlement)
        {
            this.religion = religion;
            this.settlement = settlement;
        }

        public Religion Religion => religion;
        public Settlement Settlement => settlement;

        public Clergyman Clergyman
        {
            get
            {
                if (clergyman == null) clergyman = religion.GenerateClergyman(settlement);
                return clergyman;
            }
        }

        internal override void Update(PopulationData data)
        {
            clergyman = religion.GetClergyman(data.Settlement);
            if (clergyman == null)
                clergyman = religion.GenerateClergyman(settlement);
        }
    }
}
