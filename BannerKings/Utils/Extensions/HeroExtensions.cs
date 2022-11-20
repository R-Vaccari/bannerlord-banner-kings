using TaleWorlds.CampaignSystem;

namespace BannerKings.Utils.Extensions
{
    internal static class HeroExtensions
    {
        internal static bool IsPlayer(this Hero hero)
        {
            return hero.StringId == Hero.MainHero.StringId;
        }

        internal static bool IsClanLeader(this Hero hero)
        {
            return hero.Clan?.Leader == hero;
        }

        internal static bool IsKingdomLeader(this Hero hero)
        {
            return hero.Clan?.Kingdom?.Leader == hero;
        }

        internal static bool IsCommonBorn(this Hero hero) => hero.CharacterObject != null && hero.CharacterObject.OriginalCharacter != null
            && hero.CharacterObject.OriginalCharacter.IsTemplate;
    }
}