using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Populations;
using System.Collections.Generic;
using BannerKings.Managers.Education.Lifestyles;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;
using BannerKings.Models.BKModels;
using BannerKings.Managers.Skills;

namespace BannerKings.Managers.Education
{
    public class EducationData : BannerKingsData
    {
        private Hero hero;
        private Dictionary<BookType, float> books;
        private Dictionary<Language, float> languages;
        private Lifestyle lifestyle;
        private BookType currentBook;
        private Language currentLanguage;
        private Hero languageInstructor;

        private const float LANGUAGE_RATE = 1f / (CampaignTime.DaysInYear * 4f);
        private const float BOOK_RATE = 1f / (CampaignTime.DaysInYear * 2f);

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
            hero.AddSkillXp(BKSkills.Instance.Scholarship, MBMath.ClampInt((int)result, 1, 10));
            
        }

        public void GainBookReading(BookType book, float rate)
        {
            float result = BOOK_RATE * rate;
            books[book] += result;
            hero.AddSkillXp(BKSkills.Instance.Scholarship, MBMath.ClampInt((int)result, 1, 10));
        }

        internal override void Update(PopulationData data)
        {
            BKEducationModel model = BannerKingsConfig.Instance.EducationModel;
            if (languageInstructor != null && (languageInstructor.IsDead || languageInstructor.IsDisabled))
                languageInstructor = null;

            if (CurrentLanguage != null && LanguageInstructor != null) GainLanguageFluency(CurrentLanguage, CurrentLanguageLearningRate.ResultNumber);
            if (CurrentBook != null) GainBookReading(CurrentBook, CurrentBookReadingRate.ResultNumber);
            
        }
    }
}
