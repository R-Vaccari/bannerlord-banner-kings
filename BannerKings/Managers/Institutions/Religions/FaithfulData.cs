using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class FaithfulData : BannerKingsData
    {
        [SaveableField(1)]
        private float piety;

        [SaveableField(2)]
        private Divinity boon;

        private Hero curse;

        public FaithfulData(float piety)
        {
            this.piety = piety;
            boon = null;
            curse = null;
        }

        public void AddPiety(float piety) => this.piety += piety;

        public void AddBoon(Divinity boon) => this.boon = boon;

        public float Piety => piety;
        internal override void Update(PopulationData data)
        {
        }
    }
}
