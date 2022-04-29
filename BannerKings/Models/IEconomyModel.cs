using TaleWorlds.CampaignSystem;

namespace BannerKings.Models
{
    public interface IEconomyModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement);
        public ExplainedNumber CalculateProductionEfficiency(Settlement settlement);
        public ExplainedNumber CalculateProductionQuality(Settlement settlement);

    }
}
