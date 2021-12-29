using TaleWorlds.CampaignSystem;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class StabilityModel
    {

        public void CalculateStabilityChange(Settlement settlement)
        {
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
            float stability = data.Stability;
            if (settlement.Town != null)
            {
                Town town = settlement.Town;
                float sec = town.Security * 0.01f;
                float loyalty = town.Loyalty * 0.01f;
                float assimilation = data.Assimilation;
                float[] satisfactions = data.GetSatisfactions();

                float averageSatisfaction = 0f;
                foreach (float satisfaction in satisfactions)
                    averageSatisfaction += satisfaction / 4f;

                float targetStability = (sec + loyalty + assimilation + averageSatisfaction) / 4f;
                float change = targetStability > stability ? 0.015f : targetStability < stability ? -0.015f : 0f;
                data.Stability += change;
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                data.Stability = PopulationConfig.Instance.PopulationManager.GetPopData(settlement.Village.Bound).Stability;
            }
        }
    }
}
