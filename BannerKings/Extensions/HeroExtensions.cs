using TaleWorlds.CampaignSystem;

namespace BannerKings.Utils.Extensions
{
    public static class HeroExtensions
    {
        public static bool IsPlayer(this Hero hero)
        {
            return hero.StringId == Hero.MainHero.StringId;
        }

        public static bool IsClanLeader(this Hero hero)
        {
            return hero.Clan?.Leader == hero;
        }

        public static bool IsKingdomLeader(this Hero hero)
        {
            return hero.Clan?.Kingdom?.Leader == hero;
        }

        public static bool IsCommonBorn(this Hero hero) => hero.CharacterObject != null && hero.CharacterObject.OriginalCharacter != null
            && hero.CharacterObject.OriginalCharacter.IsTemplate;
    }
}