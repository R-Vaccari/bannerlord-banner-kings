using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Models
{
    public class BKPriceFactorModel : DefaultTradeItemPriceFactorModel
    {
       /* public override int GetPrice(EquipmentElement itemRosterElement, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStoreValue, float supply, float demand)
        {
            float baseResult = base.GetPrice(itemRosterElement, clientParty, merchant, isSelling, inStoreValue, supply, demand);
            if (itemRosterElement.Item.StringId == "mule")
            {
                float penalty = GetTradePenalty(itemRosterElement.Item, clientParty, merchant, isSelling, inStoreValue, supply, demand);
            }
                
            return (int)baseResult;
        } */

        public override float GetBasePriceFactor(ItemCategory itemCategory, float inStoreValue, float supply, float demand, bool isSelling, int transferValue)
        {
            float baseResult = base.GetBasePriceFactor(itemCategory, inStoreValue, supply, demand, isSelling, transferValue);
            
            if (itemCategory.IsTradeGood)
                baseResult = MathF.Clamp(baseResult, 0.4f, 8f);

            if (itemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
                baseResult = MathF.Clamp(baseResult, 0.1f, 4f);

            if (itemCategory.IsAnimal)
                baseResult = MathF.Clamp(baseResult, 0.4f, 4f);

            return baseResult;
        }
    }
}
