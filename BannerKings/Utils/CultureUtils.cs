using TaleWorlds.CampaignSystem;

namespace BannerKings.Utils
{
    public static class CultureUtils
    {
        public static bool IsDevseg(CultureObject culture)
        {
            string id = culture.StringId;
            return id == "khuzait" || id == "iltanlar";
        }

        public static bool IsWilunding(CultureObject culture)
        {
            string id = culture.StringId;
            return id == "vlandia" || id == "balion" || id == "swadia" || id == "rhodok";
        }
    }
}
