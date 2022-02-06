using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using System.Linq;

namespace BannerKings.Models
{
    class BKWorkshopModel : DefaultWorkshopModel
    {
        
        public override float GetPolicyEffectToProduction(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(IEconomyModel));
                return model.CalculateProductionEfficiency(town.Settlement).ResultNumber;
            } else return base.GetPolicyEffectToProduction(town); 
           
        }
    }
}
