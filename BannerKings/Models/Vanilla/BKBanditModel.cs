using BannerKings.Settings;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKBanditModel : DefaultBanditDensityModel
    {
        public override int NumberOfMaximumLooterParties => BannerKingsSettings.Instance.BanditPartiesLimit;
        public override int NumberOfMaximumBanditPartiesAroundEachHideout => 20;
    }
}
