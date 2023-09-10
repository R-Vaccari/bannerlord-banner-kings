using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels
{
    public interface ICultureModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data);
        public ExplainedNumber CalculateCultureWeight(Settlement settlement, CultureDataClass data, float baseWeight = 0f);
        public ExplainedNumber GetConversionCost(Hero notable, Hero converter);
        public ExplainedNumber CalculateAcceptanceGain(CultureDataClass data);
    }
}
