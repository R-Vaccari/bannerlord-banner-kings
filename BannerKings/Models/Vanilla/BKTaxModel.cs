using BannerKings.Managers.Policies;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class BKTaxModel : DefaultSettlementTaxModel
    {
        public static readonly float NOBLE_OUTPUT = 2f;
        public static readonly float CRAFTSMEN_OUTPUT = 0.75f;
        public static readonly float SERF_OUTPUT = 0.2f;
        public static readonly float SLAVE_OUTPUT = 0.3f;

        public override ExplainedNumber CalculateTownTax(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateTownTax(town, includeDescriptions);

            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                bool taxSlaves = BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_slaves_tax");
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                double nobles = data.GetTypeCount(PopType.Nobles);
                double craftsmen = data.GetTypeCount(PopType.Nobles) * (1f - data.EconomicData.Mercantilism.ResultNumber);
                double serfs = data.GetTypeCount(PopType.Nobles);
                double slaves = data.GetTypeCount(PopType.Slaves) * (taxSlaves ? 1f : 1f - data.EconomicData.StateSlaves);
                baseResult.Add((float)(nobles * NOBLE_OUTPUT + craftsmen * CRAFTSMEN_OUTPUT + serfs * SERF_OUTPUT + slaves * SLAVE_OUTPUT), new TextObject("Population output"));

                TaxType taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax")).Policy;
                if (taxType == TaxType.Low)
                    baseResult.AddFactor(-0.15f, new TextObject("Tax policy"));
                else if (taxType == TaxType.High)
                    baseResult.AddFactor(0.15f, new TextObject("Tax policy"));

                float admCost = new BKAdministrativeModel().CalculateEffect(town.Settlement).ResultNumber;
                baseResult.AddFactor(admCost * -1f, new TextObject("Administrative costs"));
            }

            return baseResult;
        }

        public override int CalculateVillageTaxFromIncome(Village village, int marketIncome)
        {
            double baseResult = marketIncome * 0.7;
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                TaxType taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(village.Settlement, "tax")).Policy;
                if (taxType == TaxType.High)
                    baseResult = marketIncome * 9f;
                else if (taxType == TaxType.Low) baseResult = marketIncome * 0.5f;
                else if (taxType == TaxType.Exemption && marketIncome > 0)
                {
                    baseResult = 0;
                    int random = MBRandom.RandomInt(1, 100);
                    if (random <= 33 && village.Settlement.Notables != null)
                        ChangeRelationAction.ApplyPlayerRelation(village.Settlement.Notables.GetRandomElement(), 1);
                }

                if (baseResult > 0)
                {
                    float admCost = new BKAdministrativeModel().CalculateEffect(village.Settlement).ResultNumber;
                    baseResult *= 1f - admCost;
                }  
            }

            return (int)baseResult;
        }

        public override float GetTownCommissionChangeBasedOnSecurity(Town town, float commission)
        {
            return commission;
        }

        public override float GetTownTaxRatio(Town town) {
            if (BannerKingsConfig.Instance.PolicyManager != null)
                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_tariff_exempt"))
                    return 0f;
            
            return base.GetTownTaxRatio(town);
        }

        public override float GetVillageTaxRatio() => base.GetVillageTaxRatio();
        
    }
}
