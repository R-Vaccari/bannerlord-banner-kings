using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Actions
{
    public static class BuyGoodsAction
    {
        private static void BuyGoods(ItemRoster stash, Town market, Hero buyer, Dictionary<ItemCategory, int> goods,
            int startingIndex, int finalIndex, int indexChange, int budget = -1, TextObject reason = null)
        {
            ItemRoster sellerInventory = market.Settlement.ItemRoster;
            int totalCost = 0;
            int totalItems = 0;
            foreach (var pair in goods)
            {
                int desired = pair.Value;
                int consumed = 0;
                if (startingIndex > 0) startingIndex = market.Settlement.ItemRoster.Count - 1;

                for (int i = startingIndex; i != finalIndex; i += indexChange)
                {
                    ItemRosterElement element = sellerInventory.ElementAt(i);
                    if (element.EquipmentElement.Item == null) continue;

                    if (pair.Key == element.EquipmentElement.Item.ItemCategory)
                    {
                        if (consumed < desired)
                        {
                            int diff = desired - consumed;
                            int max = MathF.Min(element.Amount, diff);
                            float price = market.GetItemPrice(element.EquipmentElement);
                            if (budget > 0)
                            {
                                int maxCanBuy = (int)((float)budget / price);
                                max = MathF.Min(max, maxCanBuy);
                                if (max > 0)
                                {
                                    budget -= (int)(max * price);
                                }
                            }

                            if (max > 0)
                            {
                                sellerInventory.AddToCounts(element.EquipmentElement, -max);
                                if (stash != null)
                                {
                                    stash.AddToCounts(element.EquipmentElement, max);
                                }

                                totalCost += (int)(max * price);
                                totalItems += max;
                                consumed += max;
                            }
                        }
                    }
                }
            }

            buyer.ChangeHeroGold(-totalCost);
            if (reason != null)
            {
                InformationManager.DisplayMessage(new InformationMessage(reason
                    .SetTextVariable("GOLD", totalCost)
                    .SetTextVariable("ITEMS", totalItems)
                    .ToString(),
                    Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
            }
        }

        public static void BuyBestToWorst(ItemRoster buyerInventory, Town market, Hero buyer, Dictionary<ItemCategory, int> goods,
            int budget = -1, TextObject reason = null)
        {
            if (market == null) return;
            BuyGoods(buyerInventory, market, buyer, goods, market.Settlement.ItemRoster.Count - 1,
                -1, -1, budget, reason);
        }
    }
}
