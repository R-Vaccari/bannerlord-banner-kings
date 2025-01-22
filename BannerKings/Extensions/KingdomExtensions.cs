using BannerKings.Behaviours.Diplomacy;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Extensions
{
    public static class KingdomExtensions
    {
        public static KingdomDiplomacy GetKingdomDiplomacy(this Kingdom kingdom) =>
            Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom);
    }
}
