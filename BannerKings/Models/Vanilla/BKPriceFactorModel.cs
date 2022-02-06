using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.Models
{
    class BKPriceFactorModel : DefaultTradeItemPriceFactorModel
    {
        public override int GetPrice(EquipmentElement itemRosterElement, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStoreValue, float supply, float demand)
        {
            float baseResult = base.GetPrice(itemRosterElement, clientParty, merchant, isSelling, inStoreValue, supply, demand);
            if (clientParty != null && merchant != null && merchant.MobileParty != null && merchant.MobileParty.PartyComponent != null && merchant.MobileParty.PartyComponent.HomeSettlement != null 
                && BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(merchant.MobileParty.PartyComponent.HomeSettlement))
            {
                Settlement settlement = merchant.MobileParty.PartyComponent.HomeSettlement;
                TariffType type = BannerKingsConfig.Instance.PolicyManager.GetSettlementTariff(settlement);
                Town town = settlement.Town;
                if (type != TariffType.Exemption && town != null)
                {
                    float commission = new BKTaxModel().GetTownTaxRatio(town);
                    baseResult *= 1f + commission;
                }  
                if (type == TariffType.Internal_Consumption)
                    if (clientParty != null && merchant.MobileParty == clientParty && isSelling)
                        baseResult *= 0.66f;

                if (itemRosterElement.Item.IsFood)
                    if (settlement.Town.FoodChange < 0)
                        baseResult *= 1.5f;

            }
                
            return (int)baseResult;
        }
    }
}
