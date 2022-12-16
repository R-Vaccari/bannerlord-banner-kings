using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyFoodBuyingModel : DefaultPartyFoodBuyingModel
    {
        public override void FindItemToBuy(MobileParty mobileParty, Settlement settlement, out ItemRosterElement itemElement, out float itemElementsPrice)
        {
            itemElement = ItemRosterElement.Invalid;
            itemElementsPrice = 0f;
            float num = 0f;
            SettlementComponent settlementComponent = settlement.SettlementComponent;
            int num2 = -1;
            for (int i = 0; i < settlement.ItemRoster.Count; i++)
            {
                ItemRosterElement elementCopyAtIndex = settlement.ItemRoster.GetElementCopyAtIndex(i);
                if (elementCopyAtIndex.Amount > 0)
                {
                    bool flag = elementCopyAtIndex.EquipmentElement.Item.HasHorseComponent && 
                        elementCopyAtIndex.EquipmentElement.Item.HorseComponent.IsLiveStock &&
                        elementCopyAtIndex.EquipmentElement.Item.HorseComponent.MeatCount > 0 &&
                        elementCopyAtIndex.EquipmentElement.Item.HorseComponent.HideCount > 0;
                    if (elementCopyAtIndex.EquipmentElement.Item.IsFood || flag)
                    {
                        int itemPrice = settlementComponent.GetItemPrice(elementCopyAtIndex.EquipmentElement, mobileParty, false);
                        int itemValue = elementCopyAtIndex.EquipmentElement.ItemValue;
                        if ((itemPrice < 120 || flag) && mobileParty.LeaderHero.Gold >= itemPrice)
                        {
                            float obj = flag ? ((120f - (float)(itemPrice / elementCopyAtIndex.EquipmentElement.Item.HorseComponent.MeatCount)) * 0.0083f) : ((float)(120 - itemPrice) * 0.0083f);
                            float num3 = flag ? ((100f - (float)(itemValue / elementCopyAtIndex.EquipmentElement.Item.HorseComponent.MeatCount)) * 0.01f) : ((float)(100 - itemValue) * 0.01f);
                            float obj2 = obj;
                            float num4 = obj2 * obj2 * num3 * num3;
                            if (num4 > 0f)
                            {
                                if (MBRandom.RandomFloat * (num + num4) >= num)
                                {
                                    num2 = i;
                                    itemElementsPrice = (float)itemPrice;
                                }
                                num += num4;
                            }
                        }
                    }
                }
            }
            if (num2 != -1)
            {
                itemElement = settlement.ItemRoster.GetElementCopyAtIndex(num2);
            }
        }
    }
}
