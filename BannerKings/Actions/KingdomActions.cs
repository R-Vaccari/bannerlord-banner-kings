using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

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
    }
}
