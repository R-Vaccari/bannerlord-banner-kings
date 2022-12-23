using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Extensions
{
    public static class HeroExtensions
    {
        public static void SetCulturalName(this Hero hero)
        {
            TextObject firstName;
            TextObject fullName;
            NameGenerator.Current.GenerateHeroNameAndHeroFullName(hero, out firstName, out fullName, false);
            hero.SetName(fullName, firstName);
        }
    }
}
