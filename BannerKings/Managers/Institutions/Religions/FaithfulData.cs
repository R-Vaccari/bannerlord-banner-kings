using BannerKings.Populations;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class FaithfulData : BannerKingsData
    {
        [SaveableField(1)]
        private float piety;

        private Divinity boon;

        private Hero curse;

        public FaithfulData(float piety)
        {
            this.piety = piety;
        }

        public void AddPiety(float piety) => this.piety += piety;

        public float Piety => piety;
        internal override void Update(PopulationData data)
        {
            throw new NotImplementedException();
        }
    }
}
