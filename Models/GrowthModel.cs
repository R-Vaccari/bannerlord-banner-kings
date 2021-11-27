
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Models
{
    public class GrowthModel : GameModel
    {
        private static readonly float POP_GROWTH_FACTOR = 0.005f;
        private static readonly float SLAVE_GROWTH_FACTOR = 0.0005f;

        public int CalculatePopulationGrowth(Settlement settlement)
        {
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
            bool boost = PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.POP_GROWTH);
            int growthFactor = GetDataGrowthFactor(data, boost);
            data.UpdatePopulation(settlement, growthFactor, PopType.None);
            return growthFactor;
        }

        public void CalculateHearthGrowth(Village village, ref ExplainedNumber baseResult)
        {
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(village.Settlement);
            bool boost = PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(village.Settlement, PolicyManager.PolicyType.POP_GROWTH);
            int growthFactor = GetDataGrowthFactor(data, boost);
            float hearths = MBRandom.RandomFloatRanged(growthFactor / 3, growthFactor / 6);
            data.UpdatePopulation(village.Settlement, (int)MBRandom.RandomFloatRanged(hearths * 3f, hearths * 6f), PopType.None);
            baseResult.Add(hearths, null);
        }

        public int GetDataGrowthFactor(PopulationData data, bool boost)
        {
            
            int growthFactor = 5;
            data.Classes.ForEach(popClass =>
            {
                if (popClass.type != PopType.Slaves)
                    growthFactor += (int)(popClass.count * POP_GROWTH_FACTOR * (boost ? 1.1f : 1f));
                else growthFactor -= (int)(popClass.count * SLAVE_GROWTH_FACTOR);
            });
            return growthFactor;
        }
    }
}
