using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Populations;
using System.Collections.Generic;
using BannerKings.Managers.Education.Lifestyles;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;
using BannerKings.Managers.Skills;
using TaleWorlds.SaveSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education
{
    public class EducationData : BannerKingsData
    {
        [SaveableField(1)]
        private Hero hero;

        [SaveableField(2)]
        private Dictionary<BookType, float> books;

        [SaveableField(3)]
        private Dictionary<Language, float> languages;

        [SaveableField(4)]
        private Lifestyle lifestyle;

        [SaveableField(5)]
        private BookType currentBook;

        [SaveableField(6)]
        private Language currentLanguage;

        [SaveableField(7)]
        private Hero languageInstructor;

        private const float LANGUAGE_RATE = 1f / (CampaignTime.DaysInYear * 5f);
        private const float BOOK_RATE = 1f / (CampaignTime.DaysInYear * 3f);

        public EducationData(Hero hero, Dictionary<Language, float> languages, Lifestyle lifestyle = null)
        {
            this.hero = hero;
            this.languages = languages;
            books = new Dictionary<BookType, float>();
            this.lifestyle = lifestyle;
            currentBook = null;
            currentLanguage = null;
            languageInstructor = null;
        }

        public void PostInitialize()
        {
            Lifestyle lf = DefaultLifestyles.Instance.GetById(lifestyle);
            if (lf != null) lifestyle.Initialize(lf.Name, lf.Description, lf.FirstSkill, lf.SecondSkill, new List<PerkObject>(lf.Perks), lf.Culture);

            foreach(KeyValuePair<Language, float> pair in languages)
            {
                Language language = pair.Key;
                Language l2 = DefaultLanguages.Instance.GetById(language);
                language.Initialize(l2.Name, l2.Description, l2.Culture, DefaultLanguages.Instance.GetIntelligibles(l2));
            }

            foreach (KeyValuePair<BookType, float> pair in books)
            {
                BookType book = pair.Key;
                BookType b = DefaultBookTypes.Instance.GetById(book);
                book.Initialize(b.Item, b.Description, b.Language, b.Use, b.Skill);
            }

            Language l = DefaultLanguages.Instance.GetById(currentLanguage);
            if (l != null) currentLanguage.Initialize(l.Name, l.Description, l.Culture, DefaultLanguages.Instance.GetIntelligibles(l));
        }

        public void SetCurrentBook(BookType book)
        {
            if (book != null && !books.ContainsKey(book)) books.Add(book, 0f);
            currentBook = book;
        }

        public void SetCurrentLanguage(Language language, Hero instructor)
        {
            if (language != null && !languages.ContainsKey(language)) languages.Add(language, 0f);
            currentLanguage = language;
            languageInstructor = instructor;
        }

        public void SetCurrentLifestyle(Lifestyle lifestyle)
        {
            this.lifestyle = lifestyle;
        }
        public BookType CurrentBook => currentBook;
        public float CurrentBookProgress
        {
            get
            {
                float progress = 0f;
                if (currentBook != null && books.ContainsKey(currentBook)) progress = books[currentBook];
                return progress;
            }
        }

        public Language CurrentLanguage => currentLanguage;
        public float CurrentLanguageFluency
        {
            get
            {
                float progress = 0f;
                if (currentLanguage != null && languages.ContainsKey(currentLanguage)) progress = languages[currentLanguage];
                return progress;
            }
        }

        public Hero LanguageInstructor => languageInstructor;
        public Lifestyle Lifestyle => lifestyle;
        public MBReadOnlyDictionary<Language, float> Languages => languages.GetReadOnlyDictionary();
        public MBReadOnlyDictionary<BookType, float> Books => books.GetReadOnlyDictionary();
        public bool IsBookRead(BookType type) => books.ContainsKey(type) && books[type] >= 1f;

        public ExplainedNumber CurrentLanguageLearningRate => BannerKingsConfig.Instance.EducationModel.CalculateLanguageLearningRate(hero, LanguageInstructor, CurrentLanguage);
        public ExplainedNumber CurrentBookReadingRate => BannerKingsConfig.Instance.EducationModel.CalculateBookReadingRate(currentBook, hero);
        public float GetLanguageFluency(Language language)
        {
            if (languages.ContainsKey(language)) return languages[language];
            else return 0f;
        }

        public void GainLanguageFluency(Language language, float rate)
        {
            float result = LANGUAGE_RATE * rate;
            languages[language] += result;
            if (languages[language] >= 1f)
            {
                currentLanguage = null;
                languageInstructor = null;
                if (hero.Clan == Clan.PlayerClan) InformationManager.DisplayMessage(new InformationMessage(new TextObject("{HERO} has finished learning the {LANGUAGE} language.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("LANGUAGE", language.Name)
                    .ToString()));
            }

            hero.AddSkillXp(BKSkills.Instance.Scholarship, MBMath.ClampInt((int)result, 1, 10)); 
        }

        public void GainBookReading(BookType book, float rate)
        {
            float result = BOOK_RATE * rate;
            books[book] += result;
            if (books[book] >= 1f)
            {
                currentBook = null;
                if (hero.Clan == Clan.PlayerClan) InformationManager.DisplayMessage(new InformationMessage(new TextObject("{HERO} has finished reading {BOOK}.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("BOOK", book.Name)
                    .ToString()));
            }
            
            hero.AddSkillXp(BKSkills.Instance.Scholarship, MBMath.ClampInt((int)result, 1, 10));
        }

        internal override void Update(PopulationData data)
        {
            if (languageInstructor != null && (languageInstructor.IsDead || languageInstructor.IsDisabled))
                languageInstructor = null;

            if (hero.IsDead || hero.IsPrisoner) return;

            if (CurrentLanguage != null && LanguageInstructor != null) GainLanguageFluency(CurrentLanguage, CurrentLanguageLearningRate.ResultNumber);
            if (CurrentBook != null)
            {
                float rate = CurrentBookReadingRate.ResultNumber;
                if (rate == 0f) currentBook = null;
                else GainBookReading(CurrentBook, CurrentBookReadingRate.ResultNumber);
            }

            if (Lifestyle != null)
            {
                float progress = 1f / (CampaignTime.DaysInYear * 2f);
                Lifestyle.AddProgress(progress);
            }
        }
    }
}
