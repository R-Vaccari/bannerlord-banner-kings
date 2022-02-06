
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.PolicyManager;
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
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                double nobles = 0;
                if (!BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, PolicyType.EXEMPTION)) nobles = data.GetTypeCount(PopType.Nobles);
                double craftsmen = data.GetTypeCount(PopType.Nobles);
                double serfs = data.GetTypeCount(PopType.Nobles);
                double slaves = data.GetTypeCount(PopType.Slaves);
                baseResult.Add((float)(nobles * NOBLE_OUTPUT + craftsmen * CRAFTSMEN_OUTPUT + serfs * SERF_OUTPUT + slaves * SLAVE_OUTPUT), new TextObject("Population output"));

                TaxType taxType = BannerKingsConfig.Instance.PolicyManager.GetSettlementTax(town.Settlement);
                if (taxType == TaxType.Low)
                    baseResult.AddFactor(-0.15f, new TextObject("Tax policy"));
                else if (taxType == TaxType.High)
                    baseResult.AddFactor(0.15f, new TextObject("Tax policy"));

                float admCost = new AdministrativeModel().CalculateAdministrativeCost(town.Settlement);
                baseResult.AddFactor(admCost * -1f, new TextObject("Administrative costs"));

                if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, PolicyType.SELF_INVEST))
                    if (baseResult.ResultNumber > 0)
                        baseResult.Add(baseResult.ResultNumber * -1f, new TextObject("Self-investment policy"));
            }

            return baseResult;
        }

        public override int CalculateVillageTaxFromIncome(Village village, int marketIncome)
        {
            double baseResult = marketIncome * 0.7;
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                TaxType taxType = BannerKingsConfig.Instance.PolicyManager.GetSettlementTax(village.Settlement);
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
                    float admCost = new AdministrativeModel().CalculateAdministrativeCost(village.Settlement);
                    baseResult *= 1f - admCost;

                    if (village.Settlement != null && BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(village.Settlement, PolicyType.SELF_INVEST))
                        if (baseResult > 0)
                            baseResult -= baseResult * -1f;
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
                if (BannerKingsConfig.Instance.PolicyManager.GetSettlementTariff(town.Settlement) == TariffType.Exemption)
                    return 0f;
            
            return base.GetTownTaxRatio(town);
        }

        public override float GetVillageTaxRatio() => base.GetVillageTaxRatio();
        
    }
}
