using BannerKings.Models.BKModels;
using BannerKings.Populations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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

        public FeudalTitle Title => title;
        internal override void Update(PopulationData data)
        {
            if (title == null) title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
            if (title == null) return;
            title.CleanClaims();
            title.TickClaims();
            AITick(data.Settlement.Owner);
        }

        private void AITick(Hero owner)
        {
            if (owner == Hero.MainHero || owner == title.deJure) return;
            
            if ((owner.IsFriend(title.deJure) && owner.Clan.Kingdom == title.deJure.Clan.Kingdom)
                || owner.Clan == title.deJure.Clan) return;

            float random = MBRandom.RandomFloatRanged(0f, 1f);
            if (random >= 0.2f) return;

            BKTitleModel model = BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel;
            TitleAction claimAction = model.GetAction(ActionType.Claim, title, owner);
            if (claimAction.Possible)
            {
                bool knight = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(title.deJure).Count() == 1 && title.type == TitleType.Lordship;
                if ((owner.Clan.Kingdom == null || owner.Clan.Kingdom != title.deJure.Clan.Kingdom) || !knight)
                    claimAction.TakeAction(null);
            }
            else
            {
                TitleAction usurpAction = model.GetAction(ActionType.Usurp, title, owner);
                if (usurpAction.Possible)
                    usurpAction.TakeAction(null);
            } 
        }
    }
}
