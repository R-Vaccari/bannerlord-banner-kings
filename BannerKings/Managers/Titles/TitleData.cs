using System.Collections.Generic;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
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

            if (title.deJure != null && title.deJure.IsDead)
            {
                var list = new List<FeudalTitle>() { Title };
                InheritanceHelper.ApplyInheritanceAllTitles(list, title.deJure);
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

            var model = BannerKingsConfig.Instance.TitleModel;
            var claimAction = model.GetAction(ActionType.Claim, title, owner);
            if (claimAction.Possible && claimAction.IsWilling)
            {
                claimAction.TakeAction(null);
            }
            else
            {
                var usurpAction = model.GetAction(ActionType.Usurp, title, owner);
                if (usurpAction.Possible && usurpAction.IsWilling)
                {
                    usurpAction.TakeAction(null);
                }
            }
        }
    }
}