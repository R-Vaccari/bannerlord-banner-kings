
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Models
{
    public class GrowthModel : GameModel
    {
        private static readonly float POP_GROWTH_FACTOR = 0.005f;
        private static readonly float SLAVE_GROWTH_FACTOR = 0.0005f;

        public void CalculatePopulationGrowth(Settlement settlement)
        {
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
            int growthFactor = GetPopulationGrowth(settlement, true);
            data.UpdatePopulation(settlement, growthFactor, PopType.None);
        }

        public int GetPopulationGrowth(Settlement settlement, bool showMessage)
        {
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
            bool boost = PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.POP_GROWTH);
            int growthFactor = GetDataGrowthFactor(settlement, data, boost, showMessage);
            return growthFactor;
        }

        public void CalculateHearthGrowth(Village village, ref ExplainedNumber baseResult)
        {
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(village.Settlement);
            bool boost = PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(village.Settlement, PolicyManager.PolicyType.POP_GROWTH);
            int growthFactor = GetDataGrowthFactor(village.Settlement, data, boost, false);
            float hearths = MBRandom.RandomFloatRanged(growthFactor / 3, growthFactor / 6);
            data.UpdatePopulation(village.Settlement, (int)MBRandom.RandomFloatRanged(hearths * 3f, hearths * 6f), PopType.None);
            baseResult.Add(hearths, null);
        }

        private int GetDataGrowthFactor(Settlement settlement, PopulationData data, bool boost, bool showMessage)
        {

            int growthFactor = 5;
            if (settlement.IsVillage || !settlement.IsStarving)
            {
                data.Classes.ForEach(popClass =>
                {
                    if (popClass.type != PopType.Slaves)
                        growthFactor += (int)(popClass.count * POP_GROWTH_FACTOR * (boost ? 1.1f : 1f));
                    else growthFactor -= (int)(popClass.count * SLAVE_GROWTH_FACTOR);
                });
            } else if (settlement.IsStarving)
            {
                growthFactor = -5;
                growthFactor += (int)((float)data.TotalPop * -0.007f);
                if (showMessage && settlement.OwnerClan.Leader == Hero.MainHero)
                    InformationManager.DisplayMessage(new InformationMessage(string.Format("Population is starving at {0}!", settlement.Name.ToString())));
            }
            
            return growthFactor;
        }
    }
}
