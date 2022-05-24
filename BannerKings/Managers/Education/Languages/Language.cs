using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Languages
{
    public class Language
    {
        private TextObject name;
        private TextObject description;
        private CultureObject culture;
        private Dictionary<Language, float> inteligible;
        public Language(TextObject name, TextObject description, CultureObject culture)
        {
            this.name = name;
            this.description = description;
            this.culture = culture;
        }

        public void Initialize(Dictionary<Language, float> inteligible)
        {
            this.inteligible = inteligible;
        }

        public TextObject Name => name;
        public TextObject Description => description;
        public CultureObject Culture => culture;
    }
}
