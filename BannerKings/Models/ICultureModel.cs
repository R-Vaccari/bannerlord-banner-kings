using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models
{
    public interface ICultureModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data);
    }
}