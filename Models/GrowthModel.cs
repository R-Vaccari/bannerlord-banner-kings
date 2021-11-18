
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Models
{
    public class GrowthModel : GameModel
    {
        private static readonly float POP_GROWTH_FACTOR = 0.001f;
        private static readonly float SLAVE_GROWTH_FACTOR = 0.0005f;

        public void CalculatePopulationGrowth(Settlement settlement)
        {
            PopulationData data = GetPopData(settlement);
            int growthFactor = GetDataGrowthFactor(data);
            data.UpdatePopulation(settlement, growthFactor, PopType.None);
        }

        public void CalculateHearthGrowth(Village village, ref ExplainedNumber baseResult)
        {
            PopulationData data = GetPopData(village.Settlement);
            int growthFactor = GetDataGrowthFactor(data);
            float hearths = MBRandom.RandomFloatRanged(growthFactor / 3, growthFactor / 6);
            data.UpdatePopulation(village.Settlement, (int)MBRandom.RandomFloatRanged(hearths * 3f, hearths * 6f), PopType.None);
            baseResult.Add(hearths, null);
        }

        public int GetDataGrowthFactor(PopulationData data)
        {
            int growthFactor = 0;
            data.Classes.ForEach(popClass =>
            {
                if (popClass.type != PopType.Slaves)
                    growthFactor += (int)(popClass.count * POP_GROWTH_FACTOR);
                else growthFactor -= (int)(popClass.count * SLAVE_GROWTH_FACTOR);
            });
            return growthFactor;
        }
    }
}
