using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
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

        public void CleanEntries()
        {
            var newDic = new Dictionary<Religion, Dictionary<Hero, FaithfulData>>();
            foreach (var pair in Religions)
            {
                if (pair.Key != null && pair.Value != null)
                {
                    var newValueDic = new Dictionary<Hero, FaithfulData>();
                    foreach (var pair2 in pair.Value)
                    {
                        if (pair2.Key != null && pair.Value != null && !newValueDic.ContainsKey(pair2.Key))
                        {
                            newValueDic.Add(pair2.Key, pair2.Value);
                        }
                    }

                    newDic.Add(pair.Key, newValueDic);
                }
            }

            Religions.Clear();
            foreach (var pair in newDic)
            {
                Religions.Add(pair.Key, pair.Value);
            }
        }

        [SaveableProperty(1)] private Dictionary<Religion, Dictionary<Hero, FaithfulData>> Religions { get; set; }

        private Dictionary<Hero, Religion> HeroesCache { get; set; }

        public void InitializeReligions()
        {
            foreach (var religion in DefaultReligions.Instance.All)
            {
                if (!Religions.ContainsKey(religion))
                {
                    Religions.Add(religion, new Dictionary<Hero, FaithfulData>());
                    InitializeFaithfulHeroes(religion);
                }
            }

            RefreshCaches();
        }

        public void InitializeFaithfulHeroes(Religion rel)
        {
            foreach (var hero in Hero.AllAliveHeroes)
            {
                if (hero == Hero.MainHero || hero.IsChild || GetHeroReligion(hero) != null)
                {
                    continue;
                }

                if (rel.Faith.IsHeroNaturalFaith(hero))
                {
                    InitializeHeroFaith(hero, rel);
                }
            }
        }

        public void InitializeHeroFaith(Hero hero, Religion rel = null)
        {
            if (rel == null)
            {
                rel = GetIdealReligion(hero.Culture);
                if (rel == null)
                {
                    return;
                }
            }

            if (Religions[rel].ContainsKey(hero))
            {
                RefreshCaches();
                return;
            }

            Religions[rel].Add(hero, new FaithfulData(100f));
        }

        public void PostInitialize()
        {
            DefaultDivinities.Instance.Initialize();
            DefaultFaithGroups.Instance.Initialize();   
            DefaultFaiths.Instance.Initialize();
            foreach (var pair in Religions.ToList())
            {
                var rel = pair.Key;
                if (rel == null)
                {
                    continue;
                }

                Faith faith;
                if (rel.Faith == null)
                {
                    faith = DefaultFaiths.Instance.GetById(rel.StringId);
                }
                else
                {
                    faith = DefaultFaiths.Instance.GetById(rel.Faith.GetId());
                }
                
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

        public void ExecuteRemoveHero(Hero hero, bool isConversion = false)
        {
            var rel = GetHeroReligion(hero);
            /*if (IsPreacher(hero))
            {
                var clergyman = GetClergymanFromHeroHero(hero);
                if (clergyman != null)
                {
                    rel = GetClergymanReligion(clergyman);
                    rel.RemoveClergyman(clergyman);
                }
            }*/

            if (rel != null)
            {
                if (Religions[rel].ContainsKey(hero))
                {
                    Religions[rel].Remove(hero);
                } 
                if (HeroesCache != null && HeroesCache.ContainsKey(hero))
                {
                    HeroesCache.Remove(hero);
                }
            }
        }

        public void ExecuteAddToReligion(Hero hero, Religion religion)
        {
            if (!Religions[religion].ContainsKey(hero))
            {
                Religions[religion].Add(hero, new FaithfulData(0f));
                if (HeroesCache != null)
                {
                    HeroesCache.Add(hero, religion);
                }
            }
        }

        public void AddClergyman(Religion religion, Hero hero, Settlement settlement)
        {
            religion.AddClergyman(settlement, hero);
        }

        public void AddToReligion(Hero hero, Religion religion)
        {
            Religion rel = GetHeroReligion(hero);
            var conversion = rel != null;

            if (rel != religion)
            {
                ExecuteRemoveHero(hero);
                ExecuteAddToReligion(hero, religion);

                if (conversion)
                {
                    if (hero.Clan != null)
                    {
                        if (hero.Clan == Clan.PlayerClan)
                        {
                            MBInformationManager.AddQuickInformation(new TextObject("{=sjy26XtU}{HERO} has converted to the {FAITH} faith.")
                                    .SetTextVariable("HERO", hero.Name)
                                    .SetTextVariable("FAITH", religion.Faith.GetFaithName()),
                                0, hero.CharacterObject, Utils.Helpers.GetKingdomDecisionSound());
                        }

                        if (hero == hero.Clan.Leader)
                        {
                            hero.Clan.Renown -= 100f;
                        }
                        else
                        {
                            hero.Clan.Renown -= 50f;
                        }
                    }
                    else if (hero.IsNotable)
                    {
                        hero.AddPower(-20f);
                    }
                }
                else if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=sjy26XtU}{HERO} has converted to the {FAITH} faith.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("FAITH", religion.Faith.GetFaithName()),
                        0, hero.CharacterObject, Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public void ShowPopup()
        {
            if (GetHeroReligion(Hero.MainHero) != null)
            {
                return;
            }

            var elements = new List<InquiryElement>();
            foreach (var religion in Religions.Keys)
            {
                elements.Add(new InquiryElement(religion, 
                    new TextObject("{=Eu97WkgX}{RELIGION} - {PIETY} piety").SetTextVariable("RELIGION", religion.Faith.GetFaithName())
                    .SetTextVariable("PIETY", GetStartingPiety(religion)).ToString(), 
                    null, 
                    true, 
                    religion.Faith.GetFaithDescription().ToString()));
            }
                
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=VrzR1ZzZ}Your faith").ToString(),
                new TextObject("{=ASWWGeQ3}You look up to the skies and realize there must be something more. You feel there must be a higher purpose for yourself, and people expect you to defend a certain faith. Upholding your cultural forefathers' faith would be considered most pious. Similarly, following a faith that accepts your culture would be pious, however not as much as your true ancestry. Alternatively, having a completely different faith is possible, though a less walked path. What is your faith?")
                    .ToString(),
                elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty, delegate(List<InquiryElement> element)
                {
                    var religion = (Religion) element[0].Identifier;
                    Religions[religion].Add(Hero.MainHero, new FaithfulData(GetStartingPiety(religion)));
                }, null));
        }

        public bool HasBlessing(Hero hero, Divinity blessing, Religion rel = null)
        {
            if (rel == null)
            {
                rel = GetHeroReligion(hero);
            }
             
            if (rel != null)
            {
                return Religions[rel][hero].Blessing == blessing;
            }

            return false;
        }

        public int GetStartingPiety(Religion religion, Hero hero = null)
        {
            if (hero == null)
            {
                hero = Hero.MainHero;
            }

            var piety = 0;
            if (religion.MainCulture == hero.Culture)
            {
                piety = 100;
            }
            else if (religion.FavoredCultures.Contains(hero.Culture))
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
            AddPiety(religion, hero, -divinity.BlessingCost(hero), notify);
        }

        public Religion GetReligionById(string id)
        {
            return Religions.FirstOrDefault(pair => pair.Key.Faith.GetId() == id).Key;
        }

        public Religion GetHeroReligion(Hero hero)
        {
            if (hero == null)
            {
                return null;
            }

            if (HeroesCache != null && HeroesCache.ContainsKey(hero))
            {
                return HeroesCache[hero];
            }
            return Religions.FirstOrDefault(pair => pair.Value != null && pair.Value.ContainsKey(hero)).Key;
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
            Religion result = null;
            foreach(var religion in Religions.Keys.ToList())
            {
                if (!religion.Faith.Active) continue;

                if (religion.Faith.IsCultureNaturalFaith(culture))
                {
                    result = religion;
                }
            }

            return result;
        }

        public bool IsReligionMember(Hero hero, Religion religion)
        {
            if (!Religions.ContainsKey(religion))
            {
                return false;
            }

            return Religions[religion].ContainsKey(hero);
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
                if (piety < 0)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has lost {PIETY} piety.")
                                                            .SetTextVariable("HERO", hero.Name)
                                                            .SetTextVariable("PIETY", MathF.Abs(piety)));
                }
                else
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=purf5QPz}{HERO} has recieved {PIETY} piety.")
                                        .SetTextVariable("HERO", hero.Name)
                                        .SetTextVariable("PIETY", piety));
                }
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

            AddPiety(rel, hero, piety, notifyPlayer);
        }

        public float GetPiety(Hero hero)
        {
            var rel = GetHeroReligion(hero);
            if (rel == null || hero == null)
            {
                return 0f;
            }

            var piety = 0f;
            if (Religions[rel].ContainsKey(hero))
            {
                piety = Religions[rel][hero].Piety;
            }

            return piety;
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

            return piety;
        }

        public bool IsPreacher(Hero hero)
        {
            if (hero == null)
            {
                return false;
            }

            foreach (var rel in Religions.Keys)
            {
                foreach (var clergy in rel.Clergy.Values)
                {
                    if (clergy != null && clergy.Hero == hero)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Clergyman GetClergymanFromHeroHero(Hero hero)
        {
            foreach (var rel in Religions.Keys)
            {
                foreach (var clergyPair in rel.Clergy)
                {
                    if (clergyPair.Value != null && clergyPair.Value.Hero == hero)
                    {
                        return clergyPair.Value;
                    }
                }
            }

            return null;
        }

        public Religion GetClergymanReligion(Clergyman clergyman)
        {
            return (from rel in Religions.Keys.ToList() from clergy in rel.Clergy.Values.ToList() where clergy == clergyman select rel).FirstOrDefault();
        }
    }
}