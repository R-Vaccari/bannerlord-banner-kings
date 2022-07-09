using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using System.Linq;
using BannerKings.Managers.Titles;

namespace BannerKings.Models
{
    public class BKStabilityModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateAutonomyEffect(Settlement settlement, float stability, float autonomy)
        {
            ExplainedNumber result = new ExplainedNumber();
            result.LimitMin(-0.01f);
            result.LimitMax(0.01f);
            float targetAutonomy = CalculateAutonomyTarget(settlement, stability).ResultNumber;
            float random1 = 0.005f * MBRandom.RandomFloat;
            float random2 = 0.005f * MBRandom.RandomFloat;
            float change = targetAutonomy > autonomy ? 0.005f + random1 - random2 : targetAutonomy < autonomy ? -0.005f - random1 + random2 : 0f;
            result.Add(change, new TextObject());

            return result;
        }
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            result.LimitMin(-0.01f);
            result.LimitMax(0.01f);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float stability = data.Stability;
            if (settlement.Town != null)
            {
                float targetStability = this.CalculateStabilityTarget(settlement).ResultNumber;
                float random1 = 0.005f * MBRandom.RandomFloat;
                float random2 = 0.005f * MBRandom.RandomFloat;
                float change = targetStability > stability ? 0.005f + random1 - random2 : targetStability < stability ? -0.005f - random1 + random2 : 0f;
                result.Add(change, new TextObject());
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                data.Stability = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement.Village.Bound).Stability;
            }

            return result;
        }

        public ExplainedNumber CalculateNotableSupport(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
            float support = 0f;
            float totalPower = 0f;
            foreach (Hero notable in settlement.Notables)
                totalPower += notable.Power;

            foreach (Hero notable in settlement.Notables)
            {
                float powerShare = notable.Power / totalPower;
                float relation = (float)notable.GetRelation(settlement.OwnerClan.Leader) * 0.01f + 0.5f;
                result.Add(relation * powerShare, notable.Name);
            }

            return result;
        }

        public ExplainedNumber CalculateAutonomyTarget(Settlement settlement, float stability)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

            result.Add(1f - stability, new TextObject("{=!}Stability"));
            if (settlement.Town != null && settlement.Town.Governor != null && settlement.Town.Governor.IsNotable)
                result.Add(0.2f, new TextObject("{=!}Notable governor"));

            if (settlement.Culture == settlement.Owner.Culture)
                result.Add(-0.1f, GameTexts.FindText("str_culture"));


            return result;
        }

        public ExplainedNumber CalculateStabilityTarget(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
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

                result.Add(sec / 5f, new TextObject("Security"));
                result.Add(loyalty / 5f, new TextObject("Loyalty"));
                result.Add(assimilation / 5f, new TextObject("Cultural assimilation"));
                result.Add(averageSatisfaction / 5f, new TextObject("Produce satisfactions"));
                result.Add(data.NotableSupport.ResultNumber / 5f, new TextObject("{=!}Notable support"));

                float legitimacy = 0f;
                LegitimacyType legitimacyType = (LegitimacyType)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKLegitimacyModel))
                    .CalculateEffect(settlement).ResultNumber;
                if (legitimacyType == LegitimacyType.Lawful)
                    legitimacy = 0.1f;
                else if (legitimacyType == LegitimacyType.Lawful_Foreigner)
                    legitimacy = 0.05f;
                else if (legitimacyType == LegitimacyType.Unlawful)
                    legitimacy = -0.05f;
                else legitimacy = -0.1f;

                GovernmentType government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Feudal)
                    result.Add(0.05f, new TextObject("{=!}Government"));

                result.Add(legitimacy, new TextObject("Legitimacy"));
            }
            return result;
        }
    }
}
