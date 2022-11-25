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

        /*private float GetConsumptionFactor(ConsumptionType consumption, float value)
        {
            float diff = value - 1f;
            float factor;
            switch (consumption)
            {
                case ConsumptionType.Luxury:
                    factor = 1.5f;
                    break;
                case ConsumptionType.Industrial:
                    factor = 1.5f;
                    break;
                case ConsumptionType.Food:
                    factor = 1.5f;
                    break;
                default:
                    factor = 1.5f;
                    break;
            }
        }*/

        public override float GetBasePriceFactor(ItemCategory itemCategory, float inStoreValue, float supply, float demand,
            bool isSelling, int transferValue)
        {
            float baseResult = base.GetBasePriceFactor(itemCategory, inStoreValue, supply, demand, isSelling, transferValue);

            if (!itemCategory.IsAnimal)
            {
                baseResult *= 0.9f;
            }

            if (itemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
            {
                return MathF.Clamp(baseResult, 0.3f, 3f);
            }

            if (itemCategory.IsTradeGood)
            {
                return MathF.Clamp(baseResult, 0.3f, 10f);
            }

            return baseResult;
        }
    }
}