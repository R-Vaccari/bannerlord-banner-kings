using BannerKings.Managers.Titles;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Actions
{
    public static class KingdomActions
    {
        public static void SetRulerWithTitle(Hero ruler, Kingdom kingdom)
        {
            Clan claimantClan = ruler.Clan;
            if (claimantClan.Leader != ruler) claimantClan.SetLeader(ruler);

            ChangeRulingClanAction.Apply(kingdom, claimantClan);
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (title != null)
            {
                var deJure = title.deJure;
                if (deJure != ruler)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(deJure, ruler, title);
                }
            }
        }

        public static TextObject GetKingdomName(Clan rulers)
        {
            TextObject result = rulers.Name;

            FeudalTitle highestTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(rulers.Leader);
            if (highestTitle != null) result = highestTitle.FullName;
            else
            {
                Settlement settlement = rulers.Settlements.FirstOrDefault();
                if (settlement != null)
                {
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                    if (title != null) result = title.FullName;

                }
            }

            return result;
        }
    }
}
