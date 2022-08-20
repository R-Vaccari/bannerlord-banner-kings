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

        [SaveableProperty(1)] private Dictionary<Hero, EducationData> Educations { get; }

        public EducationData InitHeroEducation(Hero hero)
        {
            if (Educations.ContainsKey(hero))
            {
                return null;
            }

            var languages = new Dictionary<Language, float>();
            var native = DefaultLanguages.Instance.All.FirstOrDefault(x => x.Culture == hero.Culture);
            if (native == null)
            {
                native = DefaultLanguages.Instance.Calradian;
            }

            languages.Add(native, 1f);

            Lifestyle mastery = null;
            var masteryProgress = 0f;

            if (hero.IsNotable)
            {
                if (!languages.ContainsKey(DefaultLanguages.Instance.Calradian) && MBRandom.RandomFloat <= 0.15f)
                {
                    languages.Add(DefaultLanguages.Instance.Calradian, MBRandom.RandomFloatRanged(0.5f, 1f));
                }
            }

            if (hero.Occupation == Occupation.Lord || hero.Occupation == Occupation.Wanderer)
            {
            }

            var data = new EducationData(hero, languages);
            Educations.Add(hero, data);
            return data;
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
                native = DefaultLanguages.Instance.Calradian;
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
            if (hero == null || hero.Clan == null || hero.Occupation != Occupation.Lord)
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
            return list.GetReadOnlyList();
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

            return hero.GetPerkValue(BKPerks.Instance.ScholarshipLiterate) && BannerKingsConfig.Instance.EducationModel
                .CalculateBookReadingRate(book, hero).ResultNumber >= 0.2f;
        }

        public void RemoveHero(Hero hero)
        {
            if (Educations.ContainsKey(hero))
            {
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

            return list.GetReadOnlyList();
        }

        public MBReadOnlyList<Lifestyle> GetViableLifestyles(Hero hero)
        {
            var list = new List<Lifestyle>();
            foreach (var lf in DefaultLifestyles.Instance.All)
            {
                if (lf.Culture == null || lf.Culture == hero.Culture)
                {
                    list.Add(lf);
                }
            }

            return list.GetReadOnlyList();
        }

        public MBReadOnlyList<BookType> GetAvailableBooks(MobileParty party)
        {
            var list = new List<BookType>();
            if (party == null)
            {
                goto RETURN;
            }

            foreach (var element in party.ItemRoster)
            {
                if (element.EquipmentElement.Item != null && element.EquipmentElement.Item.StringId.Contains("book"))
                {
                    var type = DefaultBookTypes.Instance.All.FirstOrDefault(x => x.Item == element.EquipmentElement.Item);
                    if (type != null && !list.Contains(type))
                    {
                        list.Add(type);
                    }
                }
            }

            if (party.CurrentSettlement != null && party.LeaderHero != null &&
                party.CurrentSettlement.OwnerClan == party.LeaderHero.Clan)
            {
                foreach (var element in party.CurrentSettlement.Stash)
                {
                    if (element.EquipmentElement.Item != null && element.EquipmentElement.Item.StringId.Contains("book"))
                    {
                        var type = DefaultBookTypes.Instance.All.FirstOrDefault(
                            x => x.Item == element.EquipmentElement.Item);
                        if (type != null && !list.Contains(type))
                        {
                            list.Add(type);
                        }
                    }
                }
            }

            RETURN:
            return list.GetReadOnlyList();
        }
    }
}