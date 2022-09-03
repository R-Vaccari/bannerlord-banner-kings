using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKVolunteerAccessModel : DefaultVolunteerModel
    {

        public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
        {
            var result = base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);
            if (sellerHero != null && buyerHero != null && sellerHero.IsPreacher)
            {
                var clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(sellerHero);
                if (clergyman != null)
                {
                    var clergyReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(sellerHero); 
                    var heroReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(buyerHero);
                    if (heroReligion != clergyReligion || buyerHero.GetPerkValue(BKPerks.Instance.TheologyFaithful))
                    {
                        return 0;
                    }
                }
            }

            // NEED LOGIC TO LIMIT SELLER TO KHUZAIT
            if (sellerHero != null && buyerHero != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(buyerHero);

                if (data.HasPerk(BKPerks.Instance.KheshigHonorGuard))
                {
                    return result + 1 > 6 ? 6 : result + 1;
                }
            }

                return result;
        }
    }
}
