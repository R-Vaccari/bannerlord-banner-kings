using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles
{
    public class TitleData : BannerKingsData
    {
        [SaveableProperty(1)]
        private FeudalTitle title { get; set; }

        public TitleData(FeudalTitle title)
        {
            this.title = title;
        }

        public FeudalTitle Title => this.title;
        internal override void Update(PopulationData data)
        {
            this.title.CleanClaims();
            Dictionary<Hero, ClaimType> toAdd = new Dictionary<Hero, ClaimType>();
            foreach (KeyValuePair<Hero,CampaignTime> pair in this.title.OngoingClaims)
                if (pair.Value.RemainingDaysFromNow <= 0)
                    toAdd.Add(pair.Key, this.title.DeFacto == pair.Key ? ClaimType.DeFacto : ClaimType.Fabricated);

            foreach (KeyValuePair<Hero, ClaimType> pair in toAdd)
                this.title.AddClaim(pair.Key, pair.Value);
        }
    }
}
