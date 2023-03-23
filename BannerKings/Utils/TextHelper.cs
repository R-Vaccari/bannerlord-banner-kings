using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Utils
{
    public static class TextHelper
    {
        public static uint COLOR_LIGHT_BLUE = 3468224;
        public static uint COLOR_LIGHT_RED = 13582400;
        public static uint COLOR_LIGHT_YELLOW = 16246615;
        public static TextObject GetPrinceTitles(GovernmentType government, bool isFemale, CultureObject culture = null)
        {
            TextObject result = null;

            if (culture!= null)
            {
                if (culture.StringId == "empire")
                {
                    result = isFemale ? new TextObject("{=gNVEqLz4}Principissa") : new TextObject("{=ouHkQtyZ}Princeps");
                }
                else if (culture.StringId == "sturgia")
                {
                    result = isFemale ? new TextObject("{=S3kc2bhW}Knyaginya"): new TextObject("{=1XDPfDim}Knyaz"); 
                }
                else if (culture.StringId == "battania") 
                {
                    result = isFemale ? new TextObject("{=RYoxePAG}Bana-Phrionnsa") : new TextObject("{=7z7iEwxU}Prionnsa");
                }
            }

            if (result == null)
            {
                result = isFemale ? new TextObject("{=e7Nhe2YX}Princess") : new TextObject("{=V219eHY6}Prince");
            }

            return result;
        }

        public static TextObject GetName(GovernmentType value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }

        public static TextObject GetName(InheritanceType value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }

        public static TextObject GetName(GenderLaw value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }

        public static TextObject GetName(SuccessionType value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }
    }
}
