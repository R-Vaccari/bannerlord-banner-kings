using BannerKings.Utils;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyBuyingFoodModel : DefaultPartyFoodBuyingModel
    {
        public override float MinimumDaysFoodToLastWhileBuyingFoodFromTown => 50f;
        public override float MinimumDaysFoodToLastWhileBuyingFoodFromVillage => 30f;

        public override void FindItemToBuy(MobileParty mobileParty, Settlement settlement, out ItemRosterElement itemElement, out float itemElementsPrice)
        {
            var elementResult = ItemRosterElement.Invalid;
            var priceResult = 0f;
            ExceptionUtils.TryCatch(() =>
            {
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
                            int gold;
                            if (mobileParty.LeaderHero != null)
                            {
                                gold = mobileParty.LeaderHero.Gold;
                            }
                            else
                            {
                                gold = mobileParty.PartyTradeGold;
                            }

                            if ((itemPrice < 120 || flag) && gold >= itemPrice)
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
                                        priceResult = (float)itemPrice;
                                    }
                                    num += num4;
                                }
                            }
                        }
                    }
                }
                if (num2 != -1)
                {
                    elementResult = settlement.ItemRoster.GetElementCopyAtIndex(num2);
                }
            },
            GetType().Name,
            false);

            itemElement = elementResult;
            itemElementsPrice = priceResult;
        }
    }
}
