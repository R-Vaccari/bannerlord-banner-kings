using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using System.Linq;
using BannerKings.Populations;
using BannerKings.Managers.Policies;

namespace BannerKings.Models
{
    class BKWorkshopModel : DefaultWorkshopModel
    {
        
        public override float GetPolicyEffectToProduction(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateProductionEfficiency(town.Settlement).ResultNumber;
            } else return base.GetPolicyEffectToProduction(town); 
           
        }

        public ExplainedNumber CalculateWorkshopTax(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            result.LimitMin(0f);
            result.LimitMax(0.5f);

            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            BKTaxPolicy tax = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            if (tax.Policy == BKTaxPolicy.TaxType.High) result.Add(0.3f);
            else if (tax.Policy == BKTaxPolicy.TaxType.High) result.Add(0.2f);
            else result.Add(0.1f);

            result.AddFactor(data.EconomicData.Mercantilism.ResultNumber * -0.5f);

            return result;
        }
    }
}
