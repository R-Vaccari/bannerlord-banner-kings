using TaleWorlds.CampaignSystem;

namespace BannerKings.Utils.Extensions
{
    internal static class HeroExtensions
    {
        internal static bool IsPlayer(this Hero hero)
        {
            return hero.StringId == Hero.MainHero.StringId;
        }
    }
}