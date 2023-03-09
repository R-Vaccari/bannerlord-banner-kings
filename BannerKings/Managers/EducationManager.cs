using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class EducationManager
    {
        public EducationManager()
        {
            Educations = new Dictionary<Hero, EducationData>();
        }

        [SaveableProperty(1)] private Dictionary<Hero, EducationData> Educations { get; set; }

        public void CleanEntries()
        {
            var newDic = new Dictionary<Hero, EducationData>();
            foreach (var pair in Educations)
            {
                if (pair.Key != null && pair.Value != null)
                {
                    if (!newDic.ContainsKey(pair.Key))
                    {
                        newDic.Add(pair.Key, pair.Value);
                    }
                }
            }

            Educations.Clear();
            foreach (var pair in newDic)
            {
                if (!Educations.ContainsKey(pair.Key))
                {
                    Educations.Add(pair.Key, pair.Value);
                }
            }
        }

        public EducationData InitHeroEducation(Hero hero, Dictionary<Language, float> startingLanguages = null)
        {
            if (Educations.ContainsKey(hero))
            {
                return null;
            }

            if (startingLanguages != null)
            {
                var startData = new EducationData(hero, startingLanguages);
                Educations.Add(hero, startData);
                return startData;
            }


            var languages = new Dictionary<Language, float>();
            var native = DefaultLanguages.Instance.All.FirstOrDefault(x => x.Culture == hero.Culture) ?? DefaultLanguages.Instance.Sicilian;

            languages.Add(native, 1f);

            if (hero.IsNotable)
            {
                if (!languages.ContainsKey(DefaultLanguages.Instance.Sicilian) && MBRandom.RandomFloat <= 0.15f)
                {
                    languages.Add(DefaultLanguages.Instance.Sicilian, MBRandom.RandomFloatRanged(0.5f, 1f));
                }

                if (hero.Culture.StringId == "sturgia" && MBRandom.RandomFloat < 0.05f)
                {
                    languages.Add(DefaultLanguages.Instance.Vakken, MBRandom.RandomFloatRanged(0.5f, 1f));
                }
            }

            if (hero.Occupation is Occupation.Wanderer && MBRandom.RandomFloat < 0.1f)
            {
                languages.Add(DefaultLanguages.Instance.All.ToList().GetRandomElementWithPredicate(x => x != native),
                    MBRandom.RandomFloatRanged(0.5f, 1f));
            }

            var data = new EducationData(hero, languages);
            Educations.Add(hero, data);

            return data;
        }

        public void CorrectPlayerEducation()
        {
            Educations.Remove(Hero.MainHero);
            var languages = new Dictionary<Language, float>();
            var native = DefaultLanguages.Instance.All.FirstOrDefault(x => x.Culture == Hero.MainHero.Culture) ?? DefaultLanguages.Instance.Sicilian;

            languages.Add(native, 1f);
            var data = new EducationData(Hero.MainHero, languages);
            Educations.Add(Hero.MainHero, data);
        }

        public void PostInitialize()
        {
            foreach (var data in Educations.Values.ToList())
            {
                data.PostInitialize();
            }
        }

        public Language GetNativeLanguage(CultureObject culture)
        {
            var native = DefaultLanguages.Instance.All.FirstOrDefault(x => x.Culture == culture);
            if (native == null)
            {
                native = DefaultLanguages.Instance.Sicilian;
            }

            return native;
        }

        public Language GetNativeLanguage(Hero hero)
        {
            var native = GetNativeLanguage(hero.Culture);
            if (Educations.ContainsKey(hero))
            {
                if (!Educations[hero].Languages.ContainsKey(native))
                {
                    native = Educations[hero].Languages.First().Key;
                }
            }

            return native;
        }

        public EducationData GetHeroEducation(Hero hero)
        {
            EducationData data = null;
            if (Educations.ContainsKey(hero))
            {
                data = Educations[hero];
            }
            else
            {
                data = InitHeroEducation(hero);
            }

            return data;
        }

        public void UpdateHeroData(Hero hero)
        {
            if (Educations.ContainsKey(hero))
            {
                Educations[hero].Update(null);
            }
        }

        public MBReadOnlyList<ValueTuple<Language, Hero>> GetAvailableLanguagesToLearn(Hero hero)
        {
            var list = new List<(Language, Hero)>();
            if (hero?.Clan == null || hero.Occupation != Occupation.Lord)
            {
                goto RETURN;
            }

            var court = BannerKingsConfig.Instance.CourtManager.GetCouncil(hero).GetCourtMembers();
            if (court.Contains(hero))
            {
                court.Remove(hero);
            }

            foreach (var language in DefaultLanguages.Instance.All)
            {
                if (KnowsLanguage(hero, language))
                {
                    continue;
                }

                foreach (var courtMember in court)
                {
                    if (courtMember.IsChild || courtMember.IsPrisoner)
                    {
                        continue;
                    }

                    if (KnowsLanguage(courtMember, language))
                    {
                        list.Add(new ValueTuple<Language, Hero>(language, courtMember));
                    }
                }
            }

            RETURN:
            return new MBReadOnlyList<ValueTuple<Language, Hero>>(list);
        }

        private bool KnowsLanguage(Hero hero, Language language)
        {
            if (!Educations.ContainsKey(hero))
            {
                InitHeroEducation(hero);
            }

            return Educations[hero].Languages.ContainsKey(language) && Educations[hero].Languages[language] == 1f;
        }

        public bool CanRead(BookType book, Hero hero)
        {
            var readBooks = Educations[hero].Books;
            if (readBooks.ContainsKey(book) && readBooks[book] >= 1f)
            {
                return false;
            }

            return hero.GetPerkValue(BKPerks.Instance.ScholarshipLiterate) && BannerKingsConfig.Instance.EducationModel.CalculateBookReadingRate(book, hero).ResultNumber >= 0.2f;
        }

        public void RemoveHero(Hero hero)
        {
            if (Educations.ContainsKey(hero))
            {
                foreach (var education in Educations)
                {
                    
                    if (education.Value != null && education.Value.LanguageInstructor == hero)
                    {
                        education.Value.SetCurrentLanguage(null, null);
                    }
                }
                Educations.Remove(hero);
            }
        }


        public void SetCurrentBook(Hero hero, BookType book)
        {
            if (Educations.ContainsKey(hero))
            {
                Educations[hero].SetCurrentBook(book);
            }
        }

        public void SetCurrentLanguage(Hero hero, Language language, Hero instructor)
        {
            if (Educations.ContainsKey(hero))
            {
                Educations[hero].SetCurrentLanguage(language, instructor);
            }
        }

        public void SetCurrentLifestyle(Hero hero, Lifestyle lf)
        {
            if (Educations.ContainsKey(hero))
            {
                Educations[hero].SetCurrentLifestyle(lf);
            }
        }

        public void SetStartOptionLifestyle(Hero hero, Lifestyle lf)
        {
            if (Educations.ContainsKey(hero))
            {
                Educations[hero].SetCurrentLifestyle(lf);
                Educations[hero].Lifestyle.InvestFocus(Educations[hero], hero, true);
            }
        }

        public MBReadOnlyList<Lifestyle> GetLearnableLifestyles(Hero hero)
        {
            var list = new List<Lifestyle>();
            foreach (var lf in GetViableLifestyles(hero))
            {
                if (lf.CanLearn(hero))
                {
                    list.Add(lf);
                }
            }

            return new MBReadOnlyList<Lifestyle>(list);
        }

        public MBReadOnlyList<Lifestyle> GetViableLifestyles(Hero hero)
        {
            return new MBReadOnlyList<Lifestyle>(DefaultLifestyles.Instance.All.Where(lf => lf.Culture == null || lf.Culture == hero.Culture)
                .ToList());
        }

        public MBReadOnlyList<BookType> GetAvailableBooks(MobileParty party)
        {
            var list = new List<BookType>();
            if (party == null)
            {
                return new MBReadOnlyList<BookType>(list);
            }

            foreach (var element in party.ItemRoster)
            {
                if (element.EquipmentElement.Item == null || !element.EquipmentElement.Item.StringId.Contains("book"))
                {
                    continue;
                }

                var type = DefaultBookTypes.Instance.All.FirstOrDefault(x => x.Item == element.EquipmentElement.Item);
                if (type != null && !list.Contains(type))
                {
                    list.Add(type);
                }
            }

            if (party.CurrentSettlement == null || party.LeaderHero == null || party.CurrentSettlement.OwnerClan != party.LeaderHero.Clan)
            {
                return new MBReadOnlyList<BookType>(list);
            }
            
            foreach (var element in party.CurrentSettlement.Stash)
            {
                if (element.EquipmentElement.Item == null || !element.EquipmentElement.Item.StringId.Contains("book"))
                {
                    continue;
                }

                var type = DefaultBookTypes.Instance.All.FirstOrDefault(x => x.Item == element.EquipmentElement.Item);
                if (type != null && !list.Contains(type))
                {
                    list.Add(type);
                }
            }

            return new MBReadOnlyList<BookType>(list);
        }
    }
}