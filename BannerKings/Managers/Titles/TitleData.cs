using System.Linq;
using BannerKings.Managers.Populations;
using BannerKings.Models.BKModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles
{
    public class TitleData : BannerKingsData
    {
        public TitleData(FeudalTitle title)
        {
            this.title = title;
        }

        [SaveableProperty(1)] private FeudalTitle title { get; set; }

        public FeudalTitle Title => title;

        internal override void Update(PopulationData data)
        {
            if (title == null)
            {
                title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
            }

            if (title == null)
            {
                return;
            }

            title.CleanClaims();
            title.TickClaims();
            AITick(data.Settlement.Owner);
        }

        private void AITick(Hero owner)
        {
            if (owner == Hero.MainHero || owner == title.deJure)
            {
                return;
            }

            if ((owner.IsFriend(title.deJure) && owner.Clan.Kingdom == title.deJure.Clan.Kingdom)
                || owner.Clan == title.deJure.Clan)
            {
                return;
            }

            var random = MBRandom.RandomFloatRanged(0f, 1f);
            if (random >= 0.2f)
            {
                return;
            }

            var model = BannerKingsConfig.Instance.TitleModel;
            var claimAction = model.GetAction(ActionType.Claim, title, owner);
            if (claimAction.Possible)
            {
                var knight = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(title.deJure).Count == 1 && title.TitleType == TitleType.Lordship;
                if (owner.Clan.Kingdom == null || owner.Clan.Kingdom != title.deJure.Clan.Kingdom || !knight)
                {
                    claimAction.TakeAction(null);
                }
            }
            else
            {
                var usurpAction = model.GetAction(ActionType.Usurp, title, owner);
                if (usurpAction.Possible)
                {
                    usurpAction.TakeAction(null);
                }
            }
        }
    }
}