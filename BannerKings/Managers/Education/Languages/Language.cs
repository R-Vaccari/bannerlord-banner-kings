using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Languages
{
    public class Language : BannerKingsObject
    {
        private CultureObject culture;
        private Dictionary<Language, float> inteligible;
        public Language(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, CultureObject culture, Dictionary<Language, float> inteligible)
        {
            Initialize(name, description);
            this.culture = culture;
            this.inteligible = inteligible;
        }

        public CultureObject Culture => culture;

        public MBReadOnlyDictionary<Language, float> Inteligible => inteligible.GetReadOnlyDictionary();
    }
}
