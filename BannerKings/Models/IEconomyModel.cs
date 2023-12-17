using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models
{
    public interface IEconomyModel : IBannerKingsModel
    {
        public new ExplainedNumber CalculateEffect(Settlement settlement);
        public ExplainedNumber CalculateProductionQuality(Settlement settlement);
    }
}