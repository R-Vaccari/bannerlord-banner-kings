using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Lifestyles;
using TaleWorlds.CampaignSystem;
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
                    result *= 1.2f;
                }
            }

            return result;
        }

        public override float GetBasePriceFactor(ItemCategory itemCategory, float inStoreValue, float supply, float demand,
            bool isSelling, int transferValue)
        {
            var baseResult = base.GetBasePriceFactor(itemCategory, inStoreValue, supply, demand, isSelling, transferValue);

            if (itemCategory.IsTradeGood)
            {
                baseResult = MathF.Clamp(baseResult, 0.4f, 8f);
            }

            if (itemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
            {
                baseResult = MathF.Clamp(baseResult, 0.1f, 4f);
            }

            if (itemCategory.IsAnimal)
            {
                baseResult = MathF.Clamp(baseResult, 0.4f, 4f);
            }

            return baseResult;
        }
    }
}