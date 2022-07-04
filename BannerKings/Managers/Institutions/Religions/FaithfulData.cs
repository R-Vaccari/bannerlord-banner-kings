using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Populations;
using System.Collections.Generic;
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

        [SaveableField(3)]
        private Dictionary<RiteType, CampaignTime> performedRites;

        private Hero curse;

        public FaithfulData(float piety)
        {
            this.piety = piety;
            boon = null;
            curse = null;
            performedRites = new Dictionary<RiteType, CampaignTime>();
        }

        public void AddPiety(float piety) => this.piety += piety;

        public void AddBoon(Divinity boon) => this.boon = boon;

        public bool HasTimePassedForRite(RiteType type, float years)
        {
            if (performedRites.ContainsKey(type)) return performedRites[type].ElapsedYearsUntilNow >= years;
            else return true;
        }

        public void AddPerformedRite(RiteType type)
        {
            if (performedRites.ContainsKey(type)) performedRites[type] = CampaignTime.Now;
            else performedRites.Add(type, CampaignTime.Now);
        }

        public float Piety => piety;
        internal override void Update(PopulationData data)
        {
        }
    }
}
