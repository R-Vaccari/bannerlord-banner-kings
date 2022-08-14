﻿using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Leaderships;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class ReligionsManager
    {
        [SaveableProperty(1)]
        private Dictionary<Religion, Dictionary<Hero, FaithfulData>> Religions { get; set; }

        public ReligionsManager()
        {
            Religions = new Dictionary<Religion, Dictionary<Hero, FaithfulData>>();
            InitializeReligions();
        }

        public void InitializeReligions()
        {
            CultureObject aserai = Utils.Helpers.GetCulture("aserai");
            CultureObject khuzait = Utils.Helpers.GetCulture("khuzait");
            CultureObject imperial = Utils.Helpers.GetCulture("empire");
            CultureObject battania = Utils.Helpers.GetCulture("battania");
            CultureObject vlandia = Utils.Helpers.GetCulture("vlandia");
            List<Religion> rels = new List<Religion>();
            Religion aseraiReligion = new Religion(null, 
                DefaultFaiths.Instance.AseraCode, new KinshipLeadership(),
                new List<CultureObject> { aserai, khuzait, imperial },
                new List<string>() { "literalism" , "legalism", "heathen_tax" });

            Religion battaniaReligion = new Religion(null,
                DefaultFaiths.Instance.AmraOllahm, new AutonomousLeadership(),
                new List<CultureObject> { battania },
                new List<string>() { "druidism", "animism" });

            Religion darusosianReligion = new Religion(Settlement.All.First(x => x.StringId == "town_ES4"),
               DefaultFaiths.Instance.Darusosian, new HierocraticLeadership(),
               new List<CultureObject> { imperial },
               new List<string>() { "legalism" });

            Religion vlandiaReligion = new Religion(null,
               DefaultFaiths.Instance.Canticles, new HierocraticLeadership(),
               new List<CultureObject> { vlandia },
               new List<string>() { "sacrifice", "literalism", "childbirth" });
            rels.Add(aseraiReligion);
            rels.Add(battaniaReligion);
            rels.Add(darusosianReligion);
            rels.Add(vlandiaReligion);

            foreach (Religion rel in rels)
                if (!Religions.ContainsKey(rel))
                {
                    Religions.Add(rel, new Dictionary<Hero, FaithfulData>());
                    InitializeFaithfulHeroes(rel);
                }
        }

        public void InitializeFaithfulHeroes(Religion rel)
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero == Hero.MainHero || hero.IsDisabled || hero.IsChild || hero.Culture != rel.MainCulture) continue;

                string id = rel.Faith.GetId();
                if (id == "darusosian")
                {
                    Kingdom kingdom = null;
                    if (hero.Clan != null)
                        kingdom = hero.Clan.Kingdom;
                    else if (hero.IsNotable && hero.CurrentSettlement != null && hero.CurrentSettlement.OwnerClan != null)
                        kingdom = hero.CurrentSettlement.OwnerClan.Kingdom;
                    else if (hero.IsWanderer && hero.BornSettlement != null && hero.BornSettlement.OwnerClan != null)
                        kingdom = hero.BornSettlement.OwnerClan.Kingdom;

                    if (kingdom == null || (id == "darusosian" && kingdom.StringId != "empire_s"))
                        continue;
                }
                    
                Religions[rel].Add(hero, new FaithfulData(100f));
            }
        }

        public void ShowPopup()
        {
            if (GetHeroReligion(Hero.MainHero) != null) return;

            List<InquiryElement> elements = new List<InquiryElement>();
            foreach (Religion religion in Religions.Keys.ToList())
                elements.Add(new InquiryElement(religion,
                    new TextObject("{=!}{RELIGION} - {PIETY} piety")
                    .SetTextVariable("RELIGION", religion.Faith.GetFaithName())
                    .SetTextVariable("PIETY", GetPiety(religion))
                    .ToString(),
                    null, true, religion.Faith.GetFaithDescription().ToString()));

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=!}Your faith").ToString(),
                new TextObject("{=!}You look up to the skies and realize there must be something more. You feel there must be a higher purpose for yourself, and people expect you to defend a certain faith. Upholding your cultural forefathers' faith would be considered most pious. Similarly, following a faith that accepts your culture would be pious, however not as much as your true ancestry. Alternatively, having a completely different faith is possible, though a less walked path. What is your faith?").ToString(),
                elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty, delegate (List<InquiryElement> element)
                {
                    Religion religion = (Religion)element[0].Identifier;
                    Religions[religion].Add(Hero.MainHero, new FaithfulData(GetPiety(religion)));
                }, null));
        }

        private int GetPiety(Religion religion)
        {
            int piety = 0;
            if (religion.MainCulture == Hero.MainHero.Culture)
                piety = 100;
            else if (religion.FavoredCultures.Contains(Hero.MainHero.Culture))
                piety = 50;
            return piety;
        }

        public FaithfulData GetFaithfulData(Hero hero)
        {
            Religion rel = GetHeroReligion(hero);
            if (rel != null) return Religions[rel][hero];
            else return null;
        }

        public void PostInitialize()
        {
            foreach (Religion rel in Religions.Keys.ToList())
            {
                Faith faith = DefaultFaiths.Instance.GetById(rel.Faith.GetId());
                List<CharacterObject> presets = CharacterObject.All.ToList().FindAll(x => x.Occupation == Occupation.Preacher
                && x.IsTemplate && x.StringId.Contains("bannerkings") && x.StringId.Contains(faith.GetId()));
                foreach (CharacterObject preset in presets)
                {
                    int number = int.Parse(preset.StringId[preset.StringId.Length - 1].ToString());
                    faith.AddPreset(number, preset);
                }
                rel.PostInitialize(faith);
            }
        }

        public List<Religion> GetReligions()
        {
            List<Religion> religions = new List<Religion>();
            foreach (Religion rel in Religions.Keys)
                religions.Add(rel);

            return religions;
        }

        public void AddBlessing(Divinity divinity, Hero hero, Religion religion, bool notify = false)
        {
            if (Religions[religion].ContainsKey(hero))
            {
                Religions[religion][hero].AddBlessing(divinity);
                if (notify) InformationManager.AddQuickInformation(religion.Faith.GetBlessingQuickInformation()
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("DIVINITY", divinity.Name),
                    0, hero.CharacterObject, "event:/ui/notification/relation");
            } 
        }

        public Religion GetHeroReligion(Hero hero) => Religions.FirstOrDefault(pair => pair.Value.ContainsKey(hero)).Key;

        public List<Hero> GetFaithfulHeroes(Religion religion)
        {
            List<Hero> heroes = new List<Hero>();
            if (Religions.ContainsKey(religion))
                foreach (Hero hero in Religions[religion].Keys.ToList())
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
            if (Religions.ContainsKey(religion))
                if (Religions[religion].ContainsKey(hero))
                    return true;
            return false;
        }

        public void RemoveHero(Hero hero)
        {
            Religion rel = GetHeroReligion(hero);
            if (rel != null) Religions[rel].Remove(hero);
        }

        public void AddPiety(Religion rel, Hero hero, float piety, bool notifyPlayer = false)
        {
            if (rel == null || hero == null) return;
            if (Religions[rel].ContainsKey(hero))
                Religions[rel][hero].AddPiety(piety);

            if (notifyPlayer && hero == Hero.MainHero)
                InformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has recieved {PIETY} piety.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("PIETY", piety));
        }

        public void AddPiety(Hero hero, float piety, bool notifyPlayer = false)
        {
            if (hero == null) return;

            Religion rel = GetHeroReligion(hero);
            if (rel == null) return;

            if (Religions[rel].ContainsKey(hero))
                Religions[rel][hero].AddPiety(piety);

            if (notifyPlayer && hero == Hero.MainHero)
                InformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has recieved {PIETY} piety.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("PIETY", (int)piety));
        }

        public float GetPiety(Religion rel, Hero hero)
        {
            if (rel == null || hero == null) return 0f;
            float piety = 0f;
            if (Religions[rel].ContainsKey(hero))
                piety = Religions[rel][hero].Piety;

            return MBMath.ClampFloat(piety, -1000f, 1000f);
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
