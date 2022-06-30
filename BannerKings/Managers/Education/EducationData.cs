using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Populations;
using System.Collections.Generic;
using BannerKings.Managers.Education.Lifestyles;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Education
{
    public class EducationData : BannerKingsData
    {
        private Dictionary<BookType, float> books;
        private Dictionary<Language, float> languages;
        private Lifestyle lifestyle;
        private BookType currentBook;
        private Language currentLanguage;
        private Hero languageInstructor;

        public EducationData(Dictionary<Language, float> languages, Lifestyle lifestyle = null)
        {
            this.languages = languages;
            books = new Dictionary<BookType, float>();
            this.lifestyle = lifestyle;
            currentBook = null;
            currentLanguage = null;
            languageInstructor = null;
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

        internal override void Update(PopulationData data)
        {
            if (languageInstructor != null && (languageInstructor.IsDead || languageInstructor.IsDisabled))
                languageInstructor = null;
        }
    }
}
