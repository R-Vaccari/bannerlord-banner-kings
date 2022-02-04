using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Models
{
    class StabilityModel : GameModel
    {

        public void CalculateStabilityChange(Settlement settlement)
        {
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
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
                float random1 = 0.01f * MBRandom.RandomFloat;
                float random2 = 0.01f * MBRandom.RandomFloat;
                float change = targetStability > stability ? 0.015f + random1 - random2: targetStability < stability ? -0.015f - random1 + random2 : 0f;
                data.Stability += change;
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                data.Stability = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement.Village.Bound).Stability;
            }
        }
    }
}
