using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Languages
{
    public class Language : BannerKingsObject
    {
        private Dictionary<Language, float> inteligible;

        public Language(string id) : base(id)
        {
        }

        public CultureObject Culture { get; private set; }

        public MBReadOnlyDictionary<Language, float> Inteligible => inteligible.GetReadOnlyDictionary();

        public void Initialize(TextObject name, TextObject description, CultureObject culture,
            Dictionary<Language, float> inteligible)
        {
            Initialize(name, description);
            this.Culture = culture;
            this.inteligible = inteligible;
        }
    }
}