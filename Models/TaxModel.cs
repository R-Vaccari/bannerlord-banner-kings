
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static Populations.PolicyManager;
using static Populations.Population;

namespace Populations.Models
{
    class TaxModel : DefaultSettlementTaxModel
    {

        public override ExplainedNumber CalculateTownTax(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateTownTax(town, includeDescriptions);

            if (IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = GetPopData(town.Settlement);
                double nobles = 0;
                if (!IsPolicyEnacted(town.Settlement, PolicyType.EXEMPTION)) nobles = data.GetTypeCount(PopType.Nobles);
                double craftsmen = data.GetTypeCount(PopType.Nobles);
                double serfs = data.GetTypeCount(PopType.Nobles);
                baseResult.Add((float)(nobles * 1f + craftsmen * 0.2f + serfs * 0.05f), new TextObject("Population output"));
            }

            TaxType taxType = GetSettlementTax(town.Settlement);
            if (taxType == TaxType.LOW)
                baseResult.AddFactor(-0.15f, new TextObject("Tax policy"));
            else if (taxType == TaxType.HIGH)
                baseResult.AddFactor(0.15f, new TextObject("Tax policy"));

            if (IsPolicyEnacted(town.Settlement, PolicyType.SELF_INVEST))
                if (baseResult.ResultNumber > 0)
                    baseResult.Add(baseResult.ResultNumber * -1f, new TextObject("Self-investment policy"));

            if (IsPolicyEnacted(town.Settlement, PolicyType.CONSCRIPTION))
            {

            }

                return baseResult;
        }

        public override int CalculateVillageTaxFromIncome(Village village, int marketIncome)
        {
            double baseResult;
            TaxType taxType = GetSettlementTax(village.Settlement);
            if (taxType == TaxType.STANDARD)
                baseResult = marketIncome * 0.07f;
            else if (taxType == TaxType.HIGH)
                baseResult = marketIncome * 0.1f;
            else baseResult = marketIncome * 0.4f;

            if (village.Settlement != null && IsPolicyEnacted(village.Settlement, PolicyType.SELF_INVEST))
                if (baseResult > 0)
                    baseResult *= 0f;

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
