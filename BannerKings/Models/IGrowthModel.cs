using BannerKings.Populations;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models
{
    public interface IGrowthModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement, PopulationData data);

    }
}
