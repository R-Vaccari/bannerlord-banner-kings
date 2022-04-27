using BannerKings.Populations;
using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class ReligionData : BannerKingsData
    {
        private Settlement settlement;
        private Clergyman clergyman;
        private Religion religion;

        public ReligionData(Religion religion, Settlement settlement)
        {
            this.religion = religion;
            this.settlement = settlement;
        }

        public Religion Religion => this.religion;
        public Settlement Settlement => this.settlement;

        public Clergyman Clergyman
        {
            get
            {
                if (this.clergyman == null) this.clergyman = this.religion.GenerateClergyman(this.settlement);
                return this.clergyman;
            }
        }

        internal override void Update(PopulationData data)
        {
     
        }
    }
}
