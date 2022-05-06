using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Leaderships;
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
            CultureObject aserai = Utils.Helpers.GetCulture("aserai");
            CultureObject khuzait = Utils.Helpers.GetCulture("khuzait");
            CultureObject imperial = Utils.Helpers.GetCulture("imperial");
            CultureObject battania = Utils.Helpers.GetCulture("battania");

            Religion aseraiReligion = new Religion(Settlement.All.First(x => x.StringId == "town_A1"), 
                DefaultFaiths.Instance.AseraCode, new KinshipLeadership(),
                new List<CultureObject> { aserai, khuzait, imperial },
                new List<string>());

            Religion battaniaReligion = new Religion(null,
                DefaultFaiths.Instance.AmraOllahm, new AutonomousLeadership(),
                new List<CultureObject> { battania },
                new List<string>() { "druidism", "animism" });

            Religions.Add(aseraiReligion, new List<Hero>());
            Religions.Add(battaniaReligion, new List<Hero>());
            InitializeFaithfulHeroes(aseraiReligion, aserai);
            InitializeFaithfulHeroes(aseraiReligion, aserai);
        }

        public void InitializeFaithfulHeroes(Religion rel, CultureObject culture)
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
                if (!hero.IsDisabled && (hero.IsNoble || hero.IsNotable || hero.IsWanderer) && hero.Culture == culture
                    && !hero.IsChild)
                    Religions[rel].Add(hero);
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

        public List<Religion> GetReligions()
        {
            List<Religion> religions = new List<Religion>();
            foreach (Religion rel in Religions.Keys)
                religions.Add(rel);

            return religions;
        }

        public Religion GetHeroReligion(Hero hero) => Religions.FirstOrDefault(pair => pair.Value.Contains(hero)).Key;

        public List<Hero> GetFaithfulHeroes(Religion religion)
        {
            List<Hero> heroes = new List<Hero>();
            if (Religions.ContainsKey(religion))
                foreach (Hero hero in Religions[religion])
                    heroes.Add(hero);

            return heroes;
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
            if (this.Religions.ContainsKey(religion))
                if (this.Religions[religion].Contains(hero))
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
            foreach (Religion rel in this.Religions.Keys.ToList())
                foreach (Clergyman clergy in rel.Clergy.Values.ToList())
                    if (clergy.Hero == hero)
                        return clergy;

            return null;
        }

        public Religion GetClergymanReligion(Clergyman clergyman)
        {
            foreach (Religion rel in this.Religions.Keys.ToList())
                foreach (Clergyman clergy in rel.Clergy.Values.ToList())
                    if (clergy == clergyman)
                        return rel;
            return null;
        }
    }
}
