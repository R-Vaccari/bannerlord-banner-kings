using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Managers
{
    public class EducationManager
    {
        private Dictionary<Hero, EducationData> Educations { get; set; }

        public EducationManager()
        {
            Educations = new Dictionary<Hero, EducationData>();
            InitializeEducations();
        }

        public void InitializeEducations()
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                Dictionary<Language, float> languages = new Dictionary<Language, float>();
                Language native = DefaultLanguages.Instance.All.FirstOrDefault(x => x.Culture == hero.Culture);
                if (native == null) native = DefaultLanguages.Instance.Calradian;
                languages.Add(native, 1f);

                Lifestyle mastery = null;
                float masteryProgress = 0f;

                if (hero.IsNotable)
                {
                    if (!languages.ContainsKey(DefaultLanguages.Instance.Calradian) && MBRandom.RandomFloat <= 0.15f)
                        languages.Add(DefaultLanguages.Instance.Calradian, MBRandom.RandomFloatRanged(0.5f, 1f));
                }

                if (hero.Occupation == Occupation.Lord || hero.Occupation == Occupation.Wanderer)
                {

                }


                Educations.Add(hero, new EducationData(languages));
            }
        }

        public EducationData GetHeroEducation(Hero hero)
        {
            EducationData data = null;
            if (Educations.ContainsKey(hero))
                data = Educations[hero];

            return data;
        }

        public MBReadOnlyList<ValueTuple<Language, Hero>> GetAvailableLanguagesToLearn(Hero hero)
        {
            List<ValueTuple<Language, Hero>> list = new List<(Language, Hero)>();
            if (hero == null || hero.Clan == null || hero.Occupation != Occupation.Lord) goto RETURN;

            List<Hero> court = BannerKingsConfig.Instance.CourtManager.GetCouncil(hero).GetCourtMembers();
            if (court.Contains(hero)) court.Remove(hero);

            foreach(Language language in DefaultLanguages.Instance.All)
            {
                if (KnowsLanguage(hero, language)) continue;
                foreach(Hero courtMember in court)
                {
                    if (courtMember.IsChild || courtMember.IsPrisoner) continue;
                    if (KnowsLanguage(courtMember, language))
                        list.Add(new(language, courtMember));
                }
            }

            RETURN:
            return list.GetReadOnlyList();
        }

        private bool KnowsLanguage(Hero hero, Language language) => Educations[hero].Languages.ContainsKey(language) && Educations[hero].Languages[language] == 1f;

        public bool CanRead(Language language, Hero hero)
        {
            return true;
        }

        public void SetCurrentBook(Hero hero, BookType book)
        {
            if (Educations.ContainsKey(hero))
                Educations[hero].SetCurrentBook(book);
        }

        public void SetCurrentLanguage(Hero hero, Language language, Hero instructor)
        {
            if (Educations.ContainsKey(hero))
                Educations[hero].SetCurrentLanguage(language, instructor);
        }

        public MBReadOnlyList<BookType> GetAvailableBooks(MobileParty party)
        {
            List<BookType> list = new List<BookType>();
            if (party == null) goto RETURN;
            
            foreach(ItemRosterElement element in party.ItemRoster)
                if (element.EquipmentElement.Item != null && element.EquipmentElement.Item.StringId.Contains("book"))
                {
                    BookType type = DefaultBookTypes.Instance.All.FirstOrDefault(x => x.Item == element.EquipmentElement.Item);
                    if (type != null && !list.Contains(type)) list.Add(type);
                }

            if (party.CurrentSettlement != null && party.LeaderHero != null && party.CurrentSettlement.OwnerClan == party.LeaderHero.Clan)
                foreach (ItemRosterElement element in party.CurrentSettlement.Stash)
                    if (element.EquipmentElement.Item != null && element.EquipmentElement.Item.StringId.Contains("book"))
                    {
                        BookType type = DefaultBookTypes.Instance.All.FirstOrDefault(x => x.Item == element.EquipmentElement.Item);
                        if (type != null && !list.Contains(type)) list.Add(type);
                    }

            RETURN:
            return list.GetReadOnlyList();
        }
    }
}
