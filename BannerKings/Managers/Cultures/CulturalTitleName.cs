using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Cultures
{
    public class CulturalTitleName : BannerKingsObject
    {
        private CulturalTitleName(string stringId) : base(stringId)
        {
        }

        public CultureObject Culture { get; private set; }
        public TextObject Female { get; private set; }
        public TitleType TitleType { get; private set; }

        public TextObject GetName(bool female) => female ? Female : Name;
        public bool IsKnightsName { get; private set; }
        public bool IsPrinceName { get; private set; }

        public static CulturalTitleName CreateEmpire(string id, CultureObject culture, TextObject name, TextObject female, TextObject description)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                description);
            result.TitleType = TitleType.Empire;
            return result;
        }

        public static CulturalTitleName CreateKingdom(string id, CultureObject culture, TextObject name, TextObject female, TextObject description)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                description);
            result.TitleType = TitleType.Kingdom;
            return result;
        }

        public static CulturalTitleName CreateDuchy(string id, CultureObject culture, TextObject name, TextObject female, TextObject description)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                description);
            result.TitleType = TitleType.Dukedom;
            return result;
        }

        public static CulturalTitleName CreateCounty(string id, CultureObject culture, TextObject name, TextObject female, TextObject description)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                description);
            result.TitleType = TitleType.County;
            return result;
        }

        public static CulturalTitleName CreateBarony(string id, CultureObject culture, TextObject name, TextObject female, TextObject description)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                description);
            result.TitleType = TitleType.Barony;
            return result;
        }

        public static CulturalTitleName CreateLordship(string id, CultureObject culture, TextObject name, TextObject female, TextObject description)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                description);
            result.TitleType = TitleType.Lordship;
            return result;
        }

        public static CulturalTitleName CreateKnight(string id, CultureObject culture, TextObject name, TextObject female, TextObject plural)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                plural);
            result.IsKnightsName = true;
            return result;
        }

        public static CulturalTitleName CreatePrince(string id, CultureObject culture, TextObject name, TextObject female, TextObject plural)
        {
            CulturalTitleName result = InitTitle(id,
                culture,
                name,
                female,
                plural);
            result.IsPrinceName = true;
            return result;
        }

        private static CulturalTitleName InitTitle(string id, CultureObject culture, TextObject name, TextObject female, TextObject description)
        {
            CulturalTitleName culturalName = new CulturalTitleName(id);
            culturalName.Culture = culture;
            culturalName.Initialize(name, description);
            culturalName.Female = female;
            return culturalName;
        }
    }
}
