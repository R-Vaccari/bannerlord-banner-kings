using BannerKings.Populations;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models
{
    public interface ICultureModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data);

    }
}
