using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;

namespace Populations.Models
{
    class PriceFactorModel : DefaultTradeItemPriceFactorModel
    {

        public override int GetPrice(EquipmentElement itemRosterElement, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStoreValue, float supply, float demand)
        {
            float baseResult = base.GetPrice(itemRosterElement, clientParty, merchant, isSelling, inStoreValue, supply, demand);
            if (merchant != null && merchant.IsSettlement && merchant.Settlement != null && merchant.Settlement.Town != null &&
                PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(merchant.Settlement))
            {
                float commission = new TaxModel().GetTownTaxRatio(merchant.Settlement.Town);
                baseResult *= 1f + commission;
            }
                
            return (int)baseResult;
        }
    }
}
