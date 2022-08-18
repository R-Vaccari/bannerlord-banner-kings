using BannerKings.Managers.Education;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using static BannerKings.Managers.Policies.BKCriminalPolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKRansomModel : DefaultRansomValueCalculationModel
    {


        public override int PrisonerRansomValue(CharacterObject prisoner, Hero sellerHero = null)
        {
            int result = base.PrisonerRansomValue(prisoner, sellerHero);
            if (sellerHero != null)
            {
                Settlement settlement = sellerHero.CurrentSettlement;
                if (settlement != null && BannerKingsConfig.Instance.PopulationManager != null
                    && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    CriminalPolicy crime = ((BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "criminal")).Policy;
                    if (crime != CriminalPolicy.Enslavement && !prisoner.IsHero)
                        return 0;
                }


                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(sellerHero);
                if (prisoner.IsHero && education.HasPerk(BKPerks.Instance.OutlawKidnapper))
                    result += (int)(result * 0.3f);
            }
            
            return result;
        }
    }
}
