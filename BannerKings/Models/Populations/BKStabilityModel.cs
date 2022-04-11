using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;
using System.Linq;
using BannerKings.Managers.Titles;

namespace BannerKings.Models
{
    class BKStabilityModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float stability = data.Stability;
            if (settlement.Town != null)
            {
                float targetStability = this.CalculateStabilityTarget(settlement).ResultNumber;
                float random1 = 0.01f * MBRandom.RandomFloat;
                float random2 = 0.01f * MBRandom.RandomFloat;
                float change = targetStability > stability ? 0.015f + random1 - random2 : targetStability < stability ? -0.015f - random1 + random2 : 0f;
                result.Add(change, new TextObject());
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                data.Stability = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement.Village.Bound).Stability;
            }

            return result;
        }

        public ExplainedNumber CalculateStabilityTarget(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float stability = data.Stability;
            if (settlement.Town != null)
            {
                Town town = settlement.Town;
                float sec = town.Security * 0.01f;
                float loyalty = town.Loyalty * 0.01f;
                float assimilation = data.CultureData.GetAssimilation(settlement.Owner.Culture);
                float[] satisfactions = data.EconomicData.Satisfactions;

                float averageSatisfaction = 0f;
                foreach (float satisfaction in satisfactions)
                    averageSatisfaction += satisfaction / 4f;

                result.Add(sec / 4f, new TextObject("Security"));
                result.Add(loyalty / 4f, new TextObject("Loyalty"));
                result.Add(assimilation / 4f, new TextObject("Cultural assimilation"));
                result.Add(averageSatisfaction / 4f, new TextObject("Produce satisfactions"));
                result.Add(sec / 4f, new TextObject("Security"));

                float legitimacy = 0f;
                LegitimacyType legitimacyType = (LegitimacyType)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKLegitimacyModel))
                    .CalculateEffect(settlement).ResultNumber;
                if (legitimacyType == LegitimacyType.Lawful)
                    legitimacy = 10f;
                else if (legitimacyType == LegitimacyType.Lawful_Foreigner)
                    legitimacy = 5f;
                else if (legitimacyType == LegitimacyType.Unlawful)
                    legitimacy = -5f;
                else legitimacy = -10f;

                GovernmentType government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Feudal)
                    result.Add(0.05f, new TextObject("{=!}Government"));

                result.Add(legitimacy, new TextObject("Legitimacy"));
            }
            return result;
        }
    }
}
