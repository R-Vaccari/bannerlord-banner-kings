using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using System.Linq;

namespace BannerKings.Models
{
    class FeudalWorkshopModel : DefaultWorkshopModel
    {
        
        public override float GetPolicyEffectToProduction(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                EconomyModel model = (EconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(IEconomyModel));
                return model.CalculateProductionEfficiency(town.Settlement).ResultNumber;
            } else return base.GetPolicyEffectToProduction(town); 
           
        }
    }
}
