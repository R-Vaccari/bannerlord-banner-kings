using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
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

            return result;
        }

        public override int GetPrice(EquipmentElement itemRosterElement, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStoreValue, float supply, float demand)
        {
            int result = base.GetPrice(itemRosterElement, clientParty, merchant, isSelling, inStoreValue, supply, demand);
            if (itemRosterElement.Item.ItemCategory.IsAnimal)
            {
                result += (int)(result * 0.5f);
            }

            return result;
        }

        public override float GetBasePriceFactor(ItemCategory itemCategory, float inStoreValue, float supply, float demand,
            bool isSelling, int transferValue)
        {
            float baseResult = 0f;
            if (isSelling)
            {
                inStoreValue += (float)transferValue;
            }

            float value = demand / (supply + inStoreValue);
            if (itemCategory.IsTradeGood)
            {
                baseResult = MathF.Clamp(value, 0.1f, 10f);
            }

            if (itemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
            {
                return MathF.Clamp(baseResult, 0.1f, 4f);
            }

            if (itemCategory.IsAnimal)
            {
                return MathF.Clamp(baseResult, 0.4f, 4f);
            }

            if (itemCategory.IsTradeGood)
            {
                return MathF.Clamp(baseResult, 0.4f, 8f);
            }

            return MathF.Clamp(value, 0.8f, 1.3f);
        }
    }
}