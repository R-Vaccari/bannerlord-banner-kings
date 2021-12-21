

using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class EconomyModel : DefaultSettlementEconomyModel
    {

        public override float GetDailyDemandForCategory(Town town, ItemCategory category, int extraProsperity)
        {
            float baseResult = base.GetDailyDemandForCategory(town, category, extraProsperity);
            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                int nobles = data.GetTypeCount(PopType.Nobles);
                if (nobles > 10000)
                    baseResult = 1f;
            }

            return baseResult;
        }

        public override float GetEstimatedDemandForCategory(Town town, ItemData itemData, ItemCategory category)
        {
            float baseResult = base.GetEstimatedDemandForCategory(town, itemData, category);
            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                int nobles = data.GetTypeCount(PopType.Nobles);
                if (nobles > 10000)
                    baseResult = 1f;
            }
               
            return baseResult;
        }

        public override float GetDemandChangeFromValue(float purchaseValue)
        {
            float value = base.GetDemandChangeFromValue(purchaseValue);
            return value;
        }

        public override (float, float) GetSupplyDemandForCategory(Town town, ItemCategory category, float dailySupply, float dailyDemand, float oldSupply, float oldDemand)
        {
            ValueTuple<float, float> baseResult = base.GetSupplyDemandForCategory(town, category, dailySupply, dailyDemand, oldSupply, oldDemand);
            return baseResult;
        }

        public override int GetTownGoldChange(Town town)
        {
            return base.GetTownGoldChange(town);
        }
    }
}
