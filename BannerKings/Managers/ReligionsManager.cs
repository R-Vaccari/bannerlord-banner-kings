using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers
{
    public class ReligionsManager
    {
        private Dictionary<Religion, List<Hero>> Religions { get; set; }
        private Dictionary<CultureObject, Religion> Cultures { get; set; }

        public ReligionsManager()
        {
            this.Religions = new Dictionary<Religion, List<Hero>>();
            this.Cultures = new Dictionary<CultureObject, Religion>();
            InitializeReligions();
        }

        public void InitializeReligions()
        {
            CultureObject aserai = BannerKings.Helpers.Helpers.GetCulture("aserai");
            CultureObject khuzait = BannerKings.Helpers.Helpers.GetCulture("khuzait");
            CultureObject imperial = BannerKings.Helpers.Helpers.GetCulture("imperial");

            Religion aseraiReligion = new Religion(Settlement.All.First(x => x.StringId == "town_A1"), DefaultFaiths.Instance.AseraCode, new DescentralizedLeadership(),
                new List<CultureObject>() { aserai, khuzait, imperial });

            this.Religions.Add(aseraiReligion, new List<Hero>());
            this.Cultures.Add(aserai, aseraiReligion);
        }

        public void InitializePresets()
        {
            foreach (KeyValuePair<CultureObject, Religion> pair in this.Cultures)
            {
                List<CharacterObject> presets = CharacterObject.All.ToList().FindAll(x => x.Occupation == Occupation.Preacher
                && x.Culture == pair.Key);
                foreach (CharacterObject preset in presets)
                {
                    int number = int.Parse(preset.StringId[preset.StringId.Length - 1].ToString());
                    pair.Value.Faith.AddPreset(number, preset);
                }
            }
        }

        public Religion GetIdealReligion(CultureObject culture)
        {
            if (this.Cultures.ContainsKey(culture))
                return this.Cultures[culture];

            return null;
        }
    }
}
