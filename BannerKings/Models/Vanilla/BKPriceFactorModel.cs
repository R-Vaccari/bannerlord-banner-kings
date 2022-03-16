using BannerKings.Managers.Policies;
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
                if (itemRosterElement.Item.IsFood)
                    if (settlement.Town.FoodChange < 0)
                        baseResult *= 1.3f;



            }
                
            return (int)baseResult;
        }
    }
}
