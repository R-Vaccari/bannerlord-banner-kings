using BannerKings.Managers.Institutions.Religions.Doctrines;
using System.Linq;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKPregnancyModel : DefaultPregnancyModel
    {
        public override float GetDailyChanceOfPregnancyForHero(Hero hero)
        {
            int num = hero.Children.Count + 1;
            float num2 = (float)(4 + 4 * hero.Clan.Tier);
            int num3 = hero.Clan.Lords.Count((Hero x) => x.IsAlive);
            float num4 = (hero != Hero.MainHero && hero.Spouse != Hero.MainHero) ? Math.Min(1f, (2f * num2 - (float)num3) / num2) : 1f;
            float num5 = (1.2f - (hero.Age - 18f) * 0.04f) / (float)(num * num) * 0.12f * num4;

            ExplainedNumber explainedNumber = new ExplainedNumber(num5, false, null);
            if (hero.GetPerkValue(DefaultPerks.Charm.Virile) || hero.Spouse.GetPerkValue(DefaultPerks.Charm.Virile))
            {
                explainedNumber.AddFactor(DefaultPerks.Charm.Virile.PrimaryBonus, DefaultPerks.Charm.Virile.Name);
            }

            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel != null && rel.HasDoctrine(DefaultDoctrines.Instance.Childbirth))
                explainedNumber.AddFactor(1.15f);

            if (hero.Spouse != null)
            {
                var spouseRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero.Spouse);
                if (spouseRel != null && spouseRel.HasDoctrine(DefaultDoctrines.Instance.Childbirth))
                    explainedNumber.AddFactor(1.15f);
            }

            return explainedNumber.ResultNumber;
        }
    }
}
