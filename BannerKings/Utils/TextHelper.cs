using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Utils
{
    public static class TextHelper
    {

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
                    result = isFemale ? new TextObject("{=S3kc2bhW}Knyaginya"): new TextObject("{=!}Knyaz"); 
                }
                else if (culture.StringId == "battania") 
                {
                    result = isFemale ? new TextObject("{=!}Bana-Phrionnsa") : new TextObject("{=!}Prionnsa");
                }
            }

            if (result == null)
            {
                result = isFemale ? new TextObject("{=!}Princess") : new TextObject("{=!}Prince");
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
