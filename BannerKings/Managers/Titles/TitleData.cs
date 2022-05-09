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
            if (title == null) title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
            if (this.title == null) return;

            this.title.CleanClaims();
            Dictionary<Hero, ClaimType> toAdd = new Dictionary<Hero, ClaimType>();
            foreach (KeyValuePair<Hero,CampaignTime> pair in this.title.OngoingClaims)
                if (pair.Value.ElapsedYearsUntilNow >= 1f)
                    toAdd.Add(pair.Key, ClaimType.Fabricated);

            foreach (KeyValuePair<Hero, ClaimType> pair in toAdd)
                this.title.AddClaim(pair.Key, pair.Value);

            this.AITick(data.Settlement.Owner);
        }

        private void AITick(Hero owner)
        {
            if (owner == Hero.MainHero || owner == title.deJure) return;
            
            if ((owner.IsFriend(title.deJure) && owner.Clan.Kingdom == title.deJure.Clan.Kingdom)
                || owner.Clan == title.deJure.Clan) return;

            float random = MBRandom.RandomFloatRanged(0f, 1f);
            if (random >= 0.2f) return;

            BKTitleModel model = BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel;
            TitleAction claimAction = model.GetAction(ActionType.Claim, this.title, owner);
            if (claimAction.Possible)
            {
                bool knight = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(title.deJure).Count() == 1 && title.type == TitleType.Lordship;
                if ((owner.Clan.Kingdom == null || owner.Clan.Kingdom != this.title.deJure.Clan.Kingdom) || !knight)
                    claimAction.TakeAction(null);
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
