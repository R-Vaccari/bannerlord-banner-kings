using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKPregnancyModel : DefaultPregnancyModel
    {
        public override float GetDailyChanceOfPregnancyForHero(Hero hero)
        {
            float result =  base.GetDailyChanceOfPregnancyForHero(hero);
            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel != null && rel.HasDoctrine(DefaultDoctrines.Instance.Childbirth))
            {
                result *= 1.15f;
            }

            if (hero.Spouse != null)
            {
                var spouseRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero.Spouse);
                if (spouseRel != null && spouseRel.HasDoctrine(DefaultDoctrines.Instance.Childbirth))
                {
                    result *= 1.15f;
                }
            }

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero, DefaultDivinities.Instance.TreeloreMoon))
            {
                result *= 1.25f;
            }

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero, DefaultDivinities.Instance.SheWolf))
            {
                result *= 1.25f;
            }

            if (hero.Spouse != null)
            {
                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero.Spouse, DefaultDivinities.Instance.SheWolf))
                {
                    result *= 1.25f;
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero.Spouse, DefaultDivinities.Instance.TreeloreMoon))
                {
                    result *= 1.25f;
                }
            }

            return result;
        }
    }
}
