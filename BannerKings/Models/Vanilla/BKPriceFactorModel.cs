using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Items;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Models.Vanilla
{
    public class BKPriceFactorModel : DefaultTradeItemPriceFactorModel
    {
        public override float GetTradePenalty(ItemObject item, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStore, float supply, float demand)
        {
            var result = base.GetTradePenalty(item, clientParty, merchant, isSelling, inStore, supply, demand);

            if (clientParty != null && clientParty.LeaderHero != null)
            {
                var leader = clientParty.LeaderHero;
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Gladiator))
                {
                    result *= 0.8f;
                }
            }

            if (clientParty != null && clientParty.IsCaravan)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(clientParty.Owner);
                if (education.HasPerk(BKPerks.Instance.CaravaneerOutsideConnections))
                {
                    result *= 0.95f;
                }
            }

            Settlement settlement = merchant?.Settlement;
            if (settlement != null && settlement.IsCastle) result *= 3f;

            if (item.HasWeaponComponent || item.HasArmorComponent || item.HasSaddleComponent) result *= 5f;

            return result;
        }

        public override int GetPrice(EquipmentElement itemRosterElement, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStoreValue, float supply, float demand)
        {
            if (merchant != null && merchant.Settlement != null)
            {
                Settlement settlement = merchant.Settlement;
                if (settlement.Town != null && settlement.Town.CurrentBuilding != null)
                {
                    ItemCategory category = itemRosterElement.Item.GetItemCategory();
                    if (category == DefaultItemCategories.Wood || category == DefaultItemCategories.Clay || category == BKItemCategories.Instance.Limestone ||
                        category == DefaultItemCategories.Iron || category == BKItemCategories.Instance.Marble)
                    {
                        foreach (var requirement in BannerKingsConfig.Instance.ConstructionModel.GetMaterialRequirements(settlement.Town.CurrentBuilding))
                        {
                            if (requirement.Item1.ItemCategory == category)
                            {
                                demand += requirement.Item2;
                            }
                        }
                    }
                }

                if (settlement.IsVillage)
                {
                    inStoreValue += merchant.ItemRoster.GetItemNumber(itemRosterElement.Item) * 20f;
                }

                if (settlement.IsCastle)
                {
                    inStoreValue += merchant.ItemRoster.GetItemNumber(itemRosterElement.Item) * 10f;
                }
            }

            int price = base.GetPrice(itemRosterElement, clientParty, merchant, isSelling, inStoreValue, supply, demand);
            ItemObject item = itemRosterElement.Item;
            if (item.HasHorseComponent)
            {
                if (item.HorseComponent.MeatCount > 0)
                {
                    ItemObject meat = DefaultItems.Meat;
                    price += (int)(meat.Value * 0.5f * GetPriceFactor(meat, clientParty, merchant, inStoreValue, supply, demand, isSelling));
                }

                if (item.HorseComponent.HideCount > 0)
                {
                    ItemObject hides = DefaultItems.Hides;
                    price += (int)(hides.Value * 0.5f * GetPriceFactor(hides, clientParty, merchant, inStoreValue, supply, demand, isSelling));
                }
            }

            return price;
        }

        private float GetPriceFactor(ItemObject item, MobileParty tradingParty, PartyBase merchant, float inStoreValue, float supply, float demand, bool isSelling)
        {
            float basePriceFactor = GetBasePriceFactor(item.GetItemCategory(), inStoreValue, supply, demand, isSelling, item.Value);
            float tradePenalty = GetTradePenalty(item, tradingParty, merchant, isSelling, inStoreValue, supply, demand);
            if (!isSelling)
            {
                return basePriceFactor * (1f + tradePenalty);
            }

            return basePriceFactor * 1f / (1f + tradePenalty);
        }

        public override float GetBasePriceFactor(ItemCategory itemCategory, float inStoreValue, float supply, float demand,
            bool isSelling, int transferValue)
        {
            if (isSelling)
            {
                inStoreValue += (float)transferValue;
            }

            float value = MathF.Pow(demand / (0.1f * supply + inStoreValue * 0.04f + 2f), itemCategory.IsAnimal ? 0.3f : 0.6f);
            if (itemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
            {
                return MathF.Clamp(value, 0.3f, 3f);
            }

            if (itemCategory.IsTradeGood)
            {
                return MathF.Clamp(value, 0.3f, 10f);
            }

            return MathF.Clamp(value, 0.7f, 1.3f);
        }
    }
}