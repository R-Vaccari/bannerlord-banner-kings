
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class TaxModel : DefaultSettlementTaxModel
    {
        public static readonly float NOBLE_OUTPUT = 2f;
        public static readonly float CRAFTSMEN_OUTPUT = 0.75f;
        public static readonly float SERF_OUTPUT = 0.2f;
        public static readonly float SLAVE_OUTPUT = 0.3f;

        public override ExplainedNumber CalculateTownTax(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateTownTax(town, includeDescriptions);

            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                double nobles = 0;
                if (!PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, PolicyType.EXEMPTION)) nobles = data.GetTypeCount(PopType.Nobles);
                double craftsmen = data.GetTypeCount(PopType.Nobles);
                double serfs = data.GetTypeCount(PopType.Nobles);
                double slaves = data.GetTypeCount(PopType.Slaves);
                baseResult.Add((float)(nobles * NOBLE_OUTPUT + craftsmen * CRAFTSMEN_OUTPUT + serfs * SERF_OUTPUT + slaves * SLAVE_OUTPUT), new TextObject("Population output"));

                TaxType taxType = PopulationConfig.Instance.PolicyManager.GetSettlementTax(town.Settlement);
                if (taxType == TaxType.Low)
                    baseResult.AddFactor(-0.15f, new TextObject("Tax policy"));
                else if (taxType == TaxType.High)
                    baseResult.AddFactor(0.15f, new TextObject("Tax policy"));

                float admCost = new AdministrativeModel().CalculateAdministrativeCost(town.Settlement);
                baseResult.AddFactor(admCost * -1f, new TextObject("Administrative costs"));

                if (PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, PolicyType.SELF_INVEST))
                    if (baseResult.ResultNumber > 0)
                        baseResult.Add(baseResult.ResultNumber * -1f, new TextObject("Self-investment policy"));
            }

            return baseResult;
        }

        public override int CalculateVillageTaxFromIncome(Village village, int marketIncome)
        {
            double baseResult = marketIncome * 0.7;
            if (PopulationConfig.Instance.PolicyManager != null)
            {
                TaxType taxType = PopulationConfig.Instance.PolicyManager.GetSettlementTax(village.Settlement);
                 if (taxType == TaxType.High)
                    baseResult = marketIncome * 1f;
                else if (taxType == TaxType.Low) baseResult = marketIncome * 0.4f;

                float admCost = new AdministrativeModel().CalculateAdministrativeCost(village.Settlement);
                baseResult *= 1f - admCost;

                if (village.Settlement != null && PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(village.Settlement, PolicyType.SELF_INVEST))
                    if (baseResult > 0)
                        baseResult -= baseResult * -1f;
            }

            return (int)baseResult;
        }

        public override float GetTownTaxRatio(Town town)
        {
            return base.GetTownTaxRatio(town);
        }

        public override float GetVillageTaxRatio()
        {
            return base.GetVillageTaxRatio();
        }
    }
}
