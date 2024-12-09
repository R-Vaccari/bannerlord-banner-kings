using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using static BannerKings.Managers.Policies.BKCriminalPolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKRansomModel : DefaultRansomValueCalculationModel
    {
        public override int PrisonerRansomValue(CharacterObject prisoner, Hero sellerHero = null)
        {
            var result = base.PrisonerRansomValue(prisoner, sellerHero);
            if (sellerHero != null)
            {
                var settlement = sellerHero.CurrentSettlement;
                if (settlement != null)
                {
                    if (settlement.Town == null)
                        return result;

                    if (!prisoner.IsHero)
                    {
                        var crime = ((BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "criminal"))
                                                .Policy;
                        if (crime == CriminalPolicy.Enslavement)
                            result = (int)BannerKingsConfig.Instance.GrowthModel.CalculateSlavePrice(settlement).ResultNumber;
                    }
                }

                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(sellerHero);
                if (prisoner.IsHero && education.HasPerk(BKPerks.Instance.OutlawKidnapper))
                {
                    result += (int) (result * 0.3f);
                }
            }

            if (prisoner.IsHero)
            {
                if (prisoner.HeroObject.CompanionOf != null)
                {
                    result = (int)(result * 0.3f);
                }
            }

            return result;
        }
    }
}