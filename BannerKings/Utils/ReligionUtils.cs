using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Utils
{
    public static class ReligionUtils
    {
        public static FaithStance CalculateStanceBetweenHeroes(Hero hero1, Hero hero2, out TextObject explanation)
        {
            FaithStance result = FaithStance.Untolerated;
            explanation = null;
            Religion heroRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero1);
            if (heroRel != null)
            {
                Religion targetRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero2);
                if (targetRel != null) 
                {
                    result = heroRel.GetStance(targetRel.Faith);
                    explanation = new TextObject("{=xSxKo7Gq}Hostile faith to {FAITH}")
                        .SetTextVariable("FAITH", targetRel.Faith.GetFaithName());
                }
            }

            if (result == FaithStance.Tolerated) explanation = new TextObject("{=!}Same faith or tolerated by {FAITH}")
                        .SetTextVariable("FAITH", heroRel.Faith.GetFaithName());
            
            if (explanation == null) explanation = new TextObject("{=!}Untolerated faith differences");

            return result;
        }
    }
}
