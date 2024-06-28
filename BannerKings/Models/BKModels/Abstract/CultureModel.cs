using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class CultureModel
    {
        public abstract ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data);
        public abstract ExplainedNumber CalculateCultureWeight(Settlement settlement, CultureDataClass data, float baseWeight = 0f);
        public abstract ExplainedNumber GetConversionCost(Hero notable, Hero converter);
        public abstract ExplainedNumber CalculateAcceptanceGain(CultureDataClass data);
    }
}
