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

        public List<CultureObject> Cultures { get; private set; }

        public MBReadOnlyDictionary<Language, float> Inteligible => inteligible.GetReadOnlyDictionary();

        public void Initialize(TextObject name, TextObject description, List<CultureObject> cultures, Dictionary<Language, float> inteligible)
        {
            Initialize(name, description);
            Cultures = cultures;
            this.inteligible = inteligible;
        }
    }
}