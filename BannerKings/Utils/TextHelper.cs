using BannerKings.Managers.Cultures;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Utils
{
    public static class TextHelper
    {
        public static uint COLOR_LIGHT_BLUE = 3468224;
        public static uint COLOR_LIGHT_RED = 13582400;
        public static uint COLOR_LIGHT_YELLOW = 16246615;

        public static string INFLUENCE_ICON = "<img src=\"General\\Icons\\Influence@2x\" extend=\"7\">";
        public static string PIETY_ICON = "<img src=\"SPGeneral\\MapOverlay\\Settlement\\icon_morale_big\" extend=\"7\">";
        public static string MORALE_ICON = "<img src=\"General\\Icons\\Morale@2x\" extend=\"7\">";
        public static string FOOD_ICON = "<img src=\"General\\Icons\\Food@2x\" extend=\"7\">";
        public static string PARTY_ICON = "<img src=\"General\\Icons\\Party@2x\" extend=\"7\">";
        public static string SPEED_ICON = "<img src=\"General\\Icons\\Speed@2x\" extend=\"7\">"; 

        public static string GetTraitName(TraitObject trait, int level) =>
            GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), (level + MathF.Abs(trait.MinValue)).ToString()).ToString();

        public static TextObject GetConsumptionSatisfactionText(ConsumptionType type)
        {
            if (type == ConsumptionType.Luxury)
            {
                return new TextObject("{=FU9JZQuP}Luxury Goods");
            }

            if (type == ConsumptionType.Industrial)
            {
                return new TextObject("{=GsJWKW0o}Industrial Goods");
            }

            if (type == ConsumptionType.Food)
            {
                return new TextObject("{=SuHW6rM5}Food Goods");
            }

            return new TextObject("{=HoU7ZObZ}General Goods");
        }

        public static TextObject GetTitlePrefix(TitleType type, CultureObject culture = null) =>
            DefaultTitleNames.Instance.GetTitleName(culture, type).Description;

        public static TextObject GetTitleHonorary(TitleType type, bool female, CultureObject culture = null)
        {
            var name = DefaultTitleNames.Instance.GetTitleName(culture, type);
            return female ? name.Female : name.Name;
        } 

        public static TextObject GetKnightTitle(CultureObject culture, bool female, bool plural)
        {
            var name = DefaultTitleNames.Instance.GetKnightName(culture);
            return plural ? name.Description : (female ? name.Female : name.Name);
        }

        public static TextObject GetPrinceTitles(bool female, CultureObject culture = null)
        {
            var name = DefaultTitleNames.Instance.GetPrinceName(culture);
            return female ? name.Female : name.Name;
        }
    }
}
