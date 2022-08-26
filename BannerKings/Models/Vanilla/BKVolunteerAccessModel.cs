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
            int result = base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);
            if (sellerHero != null && buyerHero != null && sellerHero.IsPreacher)
            {
                Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(sellerHero);
                if (clergyman != null)
                {
                    Religion clergyReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(sellerHero); 
                    Religion heroReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(buyerHero);
                    if (heroReligion != clergyReligion || buyerHero.GetPerkValue(BKPerks.Instance.TheologyFaithful))
                    {
                        return 0;
                    }
                }
            }

            return result;
        }
    }
}
