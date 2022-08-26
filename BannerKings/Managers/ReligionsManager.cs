using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Leaderships;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class ReligionsManager
    {
        public ReligionsManager()
        {
            Religions = new Dictionary<Religion, Dictionary<Hero, FaithfulData>>();
            InitializeReligions();
        }

        [SaveableProperty(1)] private Dictionary<Religion, Dictionary<Hero, FaithfulData>> Religions { get; set; }

        private Dictionary<Hero, Religion> HeroesCache { get; set; }

        public void InitializeReligions()
        {
            var aserai = Utils.Helpers.GetCulture("aserai");
            var khuzait = Utils.Helpers.GetCulture("khuzait");
            var imperial = Utils.Helpers.GetCulture("empire");
            var battania = Utils.Helpers.GetCulture("battania");
            var vlandia = Utils.Helpers.GetCulture("vlandia");

            var aseraiReligion = new Religion(null,
                DefaultFaiths.Instance.AseraCode, new KinshipLeadership(),
                new List<CultureObject> {aserai, khuzait, imperial},
                new List<string> {"literalism", "legalism", "heathen_tax"});

            var battaniaReligion = new Religion(null,
                DefaultFaiths.Instance.AmraOllahm, new AutonomousLeadership(),
                new List<CultureObject> {battania},
                new List<string> {"druidism", "animism"});

            var darusosianReligion = new Religion(Settlement.All.First(x => x.StringId == "town_ES4"),
                DefaultFaiths.Instance.Darusosian, new HierocraticLeadership(),
                new List<CultureObject> {imperial},
                new List<string> {"legalism"});

            var vlandiaReligion = new Religion(null,
                DefaultFaiths.Instance.Canticles, new HierocraticLeadership(),
                new List<CultureObject> {vlandia},
                new List<string> {"sacrifice", "literalism", "childbirth"});

            var religions = new List<Religion>
            {
                aseraiReligion,
                battaniaReligion,
                darusosianReligion,
                vlandiaReligion
            };

            foreach (var religion in religions.Where(rel => !Religions.ContainsKey(rel)))
            {
                Religions.Add(religion, new Dictionary<Hero, FaithfulData>());
                InitializeFaithfulHeroes(religion);
            }

            RefreshCaches();
        }

        public void InitializeFaithfulHeroes(Religion rel)
        {
            foreach (var hero in Hero.AllAliveHeroes)
            {
                if (hero == Hero.MainHero || hero.IsDisabled || hero.IsChild || hero.Culture != rel.MainCulture)
                {
                    continue;
                }

                var id = rel.Faith.GetId();
                if (id == "darusosian")
                {
                    Kingdom kingdom = null;
                    if (hero.Clan != null)
                    {
                        kingdom = hero.Clan.Kingdom;
                    }
                    else if (hero.IsNotable && hero.CurrentSettlement is {OwnerClan: { }})
                    {
                        kingdom = hero.CurrentSettlement.OwnerClan.Kingdom;
                    }
                    else if (hero.IsWanderer && hero.BornSettlement is {OwnerClan: { }})
                    {
                        kingdom = hero.BornSettlement.OwnerClan.Kingdom;
                    }

                    if (kingdom == null || (id == "darusosian" && kingdom.StringId != "empire_s"))
                    {
                        continue;
                    }
                }

                Religions[rel].Add(hero, new FaithfulData(100f));
            }
        }

        public void PostInitialize()
        {
            foreach (var pair in Religions.ToList())
            {
                var rel = pair.Key;
                var faith = DefaultFaiths.Instance.GetById(rel.Faith.GetId());
                var presets = CharacterObject.All.ToList().FindAll(x => x.Occupation == Occupation.Preacher && x.IsTemplate && x.StringId.Contains("bannerkings") && x.StringId.Contains(faith.GetId()));
                foreach (var preset in presets)
                {
                    var number = int.Parse(preset.StringId[preset.StringId.Length - 1].ToString());
                    faith.AddPreset(number, preset);
                }

                rel.PostInitialize(faith);

                foreach(var keyPair in pair.Value)
                {
                    keyPair.Value.PostInitialize();
                }
            }

            RefreshCaches();
        }

        public void RefreshCaches()
        {
            HeroesCache ??= new Dictionary<Hero, Religion>();

            foreach (var pair in Religions)
            {
                var heroes = pair.Value.Keys.ToList();
                foreach (var hero in heroes)
                {
                    if (!HeroesCache.ContainsKey(hero))
                    {
                        HeroesCache.Add(hero, pair.Key);
                        continue;
                    }

                    if (!HeroesCache.ContainsValue(pair.Key))
                    {
                        HeroesCache[hero] = pair.Key;
                    }
                }
            }
        }

        public void ShowPopup()
        {
            if (GetHeroReligion(Hero.MainHero) != null)
            {
                return;
            }

            var elements = Religions.Keys.ToList()
                .Select(religion => new InquiryElement(religion, new TextObject("{=Eu97WkgX}{RELIGION} - {PIETY} piety").SetTextVariable("RELIGION", religion.Faith.GetFaithName())
                    .SetTextVariable("PIETY", GetPiety(religion))
                    .ToString(), null, true, religion.Faith.GetFaithDescription().ToString()))
                .ToList();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=VrzR1ZzZ}Your faith").ToString(),
                new TextObject("{=ASWWGeQ3}You look up to the skies and realize there must be something more. You feel there must be a higher purpose for yourself, and people expect you to defend a certain faith. Upholding your cultural forefathers' faith would be considered most pious. Similarly, following a faith that accepts your culture would be pious, however not as much as your true ancestry. Alternatively, having a completely different faith is possible, though a less walked path. What is your faith?")
                    .ToString(),
                elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty, delegate(List<InquiryElement> element)
                {
                    var religion = (Religion) element[0].Identifier;
                    Religions[religion].Add(Hero.MainHero, new FaithfulData(GetPiety(religion)));
                }, null));
        }

        private int GetPiety(Religion religion)
        {
            var piety = 0;
            if (religion.MainCulture == Hero.MainHero.Culture)
            {
                piety = 100;
            }
            else if (religion.FavoredCultures.Contains(Hero.MainHero.Culture))
            {
                piety = 50;
            }

            return piety;
        }

        public FaithfulData GetFaithfulData(Hero hero)
        {
            var rel = GetHeroReligion(hero);
            return rel != null ? Religions[rel][hero] : null;
        }


        public List<Religion> GetReligions()
        {
            return Religions.Keys.ToList();
        }

        public void AddBlessing(Divinity divinity, Hero hero, Religion religion, bool notify = false)
        {
            if (!Religions[religion].ContainsKey(hero))
            {
                return;
            }

            Religions[religion][hero].AddBlessing(divinity, hero);
            if (notify)
            {
                MBInformationManager.AddQuickInformation(religion.Faith.GetBlessingQuickInformation()
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("DIVINITY", divinity.Name),
                    0, hero.CharacterObject, "event:/ui/notification/relation");
            }
        }

        public Religion GetReligionById(string id)
        {
            return Religions.FirstOrDefault(pair => pair.Key.Faith.GetId() == id).Key;
        }

        public Religion GetHeroReligion(Hero hero)
        {
            if (HeroesCache != null && HeroesCache.ContainsKey(hero))
            {
                return HeroesCache[hero];
            }
            return Religions.FirstOrDefault(pair => pair.Value.ContainsKey(hero)).Key;
        }

        public List<Hero> GetFaithfulHeroes(Religion religion)
        {
            var heroes = new List<Hero>();
            if (!Religions.ContainsKey(religion))
            {
                return heroes;
            }

            heroes.AddRange(Religions[religion].Keys.ToList());

            return heroes;
        }

        public Religion GetIdealReligion(CultureObject culture)
        {
            return Religions.Keys.ToList().FirstOrDefault(rel => rel.MainCulture == culture);
        }

        public bool IsReligionMember(Hero hero, Religion religion)
        {
            if (!Religions.ContainsKey(religion))
            {
                return false;
            }

            return Religions[religion].ContainsKey(hero);
        }

        public void RemoveHero(Hero hero)
        {
            var rel = GetHeroReligion(hero);
            if (rel != null)
            {
                Religions[rel].Remove(hero);
            }
        }

        public void AddPiety(Religion rel, Hero hero, float piety, bool notifyPlayer = false)
        {
            if (rel == null || hero == null)
            {
                return;
            }

            if (Religions[rel].ContainsKey(hero))
            {
                Religions[rel][hero].AddPiety(piety);
            }

            if (notifyPlayer && hero == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=purf5QPz}{HERO} has recieved {PIETY} piety.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("PIETY", piety));
            }
        }

        public void AddPiety(Hero hero, float piety, bool notifyPlayer = false)
        {
            if (hero == null)
            {
                return;
            }

            var rel = GetHeroReligion(hero);
            if (rel == null)
            {
                return;
            }

            if (Religions[rel].ContainsKey(hero))
            {
                Religions[rel][hero].AddPiety(piety);
            }

            if (notifyPlayer && hero == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=purf5QPz}{HERO} has recieved {PIETY} piety.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("PIETY", (int) piety));
            }
        }

        public float GetPiety(Religion rel, Hero hero)
        {
            if (rel == null || hero == null)
            {
                return 0f;
            }

            var piety = 0f;
            if (Religions[rel].ContainsKey(hero))
            {
                piety = Religions[rel][hero].Piety;
            }

            return MBMath.ClampFloat(piety, -1000f, 1000f);
        }

        public bool IsPreacher(Hero hero)
        {
            return Religions.Keys.ToList().SelectMany(rel => rel.Clergy.Values.ToList()).Any(clergy => clergy.Hero == hero);
        }

        public Clergyman GetClergymanFromHeroHero(Hero hero)
        {
            return Religions.Keys.ToList().SelectMany(rel => rel.Clergy.Values.ToList()).FirstOrDefault(clergy => clergy.Hero == hero);
        }

        public Religion GetClergymanReligion(Clergyman clergyman)
        {
            return (from rel in Religions.Keys.ToList() from clergy in rel.Clergy.Values.ToList() where clergy == clergyman select rel).FirstOrDefault();
        }
    }
}