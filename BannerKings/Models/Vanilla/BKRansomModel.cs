using BannerKings.Managers.Policies;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using static BannerKings.Managers.Policies.BKCriminalPolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKRansomModel : DefaultRansomValueCalculationModel
    {


        public override int PrisonerRansomValue(CharacterObject prisoner, Hero sellerHero = null)
        {
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
            }
            
            return base.PrisonerRansomValue(prisoner, sellerHero);
        }
    }
}
