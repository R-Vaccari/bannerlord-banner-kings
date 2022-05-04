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

        public ReligionsManager()
        {
            Religions = new Dictionary<Religion, List<Hero>>();
            InitializeReligions();
        }

        public void InitializeReligions()
        {
            CultureObject aserai = Utils.Helpers.GetCulture("aserai");
            CultureObject khuzait = Utils.Helpers.GetCulture("khuzait");
            CultureObject imperial = Utils.Helpers.GetCulture("imperial");

            Religion aseraiReligion = new Religion(Settlement.All.First(x => x.StringId == "town_A1"), 
                DefaultFaiths.Instance.AseraCode, new DescentralizedLeadership(),
                new List<CultureObject> { aserai, khuzait, imperial },
                new List<string>());

            Religions.Add(aseraiReligion, new List<Hero>());
        }

        public void InitializePresets()
        {
            foreach (Religion rel in Religions.Keys.ToList())
            {
                string id = rel.Faith.GetId();
                List<CharacterObject> presets = CharacterObject.All.ToList().FindAll(x => x.Occupation == Occupation.Preacher
                && x.Culture == rel.MainCulture && x.IsTemplate && x.StringId.Contains("bannerkings") && x.StringId.Contains(id));
                foreach (CharacterObject preset in presets)
                {
                    int number = int.Parse(preset.StringId[preset.StringId.Length - 1].ToString());
                    rel.Faith.AddPreset(number, preset);
                }
            }
        }

        public Religion GetIdealReligion(CultureObject culture)
        {
            foreach (Religion rel in Religions.Keys.ToList())
                if (rel.MainCulture == culture)
                    return rel;

            return null;
        }

        public bool IsReligionMember(Hero hero, Religion religion)
        {
            if (Religions.ContainsKey(religion))
                if (Religions[religion].Contains(hero))
                    return true;
            return false;
        }

        public bool IsPreacher(Hero hero)
        {
            foreach (Religion rel in Religions.Keys.ToList())
                foreach (Clergyman clergy in rel.Clergy.Values.ToList())
                    if (clergy.Hero == hero)
                        return true;

            return false;
        }

        public Clergyman GetClergymanFromHeroHero(Hero hero)
        {
            foreach (Religion rel in Religions.Keys.ToList())
                foreach (Clergyman clergy in rel.Clergy.Values.ToList())
                    if (clergy.Hero == hero)
                        return clergy;

            return null;
        }

        public Religion GetClergymanReligion(Clergyman clergyman)
        {
            foreach (Religion rel in Religions.Keys.ToList())
                foreach (Clergyman clergy in rel.Clergy.Values.ToList())
                    if (clergy == clergyman)
                        return rel;
            return null;
        }
    }
}
