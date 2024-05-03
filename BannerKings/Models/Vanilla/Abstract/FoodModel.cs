using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class FoodModel : DefaultSettlementFoodModel
    {
        public abstract float NobleFood { get; }
        public abstract float CraftsmanFood { get; }
        public abstract float TenantFood { get; }
        public abstract float SerfFood { get; }
        public abstract float SlaveFood { get; }

        public abstract ExplainedNumber GetPopulationFoodConsumption(PopulationData data);
        public abstract ExplainedNumber GetPopulationFoodProduction(PopulationData data, Town town);
    }
}
