using BannerKings.Models.BKModels;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using System.Linq;
using TaleWorlds.Core;

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
                    toAdd.Add(pair.Key, ClaimType.Fabricated);

            foreach (KeyValuePair<Hero, ClaimType> pair in toAdd)
                this.title.AddClaim(pair.Key, pair.Value);

            Hero owner = data.Settlement.Owner;
            if (owner != Hero.MainHero && owner != title.deJure)
            {
                float random = MBRandom.RandomFloatRanged(0f, 1f);
                if (random >= 0.2f) return;
                
                BKTitleModel model = BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel;
                TitleAction claimAction = model.GetAction(ActionType.Claim, this.title, owner);
                if (claimAction.Possible)
                {
                    if (owner.GetTraitLevel(DefaultTraits.Honor) > 1)
                    {
                        Kingdom kingdom = owner.Clan.Kingdom;
                        if (kingdom == null || kingdom != this.title.deJure.Clan.Kingdom)
                            claimAction.TakeAction(null);
                    } else claimAction.TakeAction(null);
                }
                else
                {
                    TitleAction usurpAction = model.GetAction(ActionType.Usurp, this.title, owner);
                    if (usurpAction.Possible)
                        usurpAction.TakeAction(null);
                }
            }
        }
    }
}
