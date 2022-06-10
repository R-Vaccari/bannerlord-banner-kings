using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Populations;
using System.Collections.Generic;
using BannerKings.Managers.Education.Training;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Education
{
    public class EducationData : BannerKingsData
    {
        private Dictionary<BookType, float> books;
        private Dictionary<Language, float> languages;
        private Mastery mastery;
        private float masteryProgress;
        private BookType currentBook;
        private Language currentLanguage;
        private Hero languageInstructor;

        public EducationData(Dictionary<Language, float> languages, float masteryProgress = 0f, Mastery mastery = null)
        {
            this.languages = languages;
            books = new Dictionary<BookType, float>();
            this.mastery = mastery;
            this.masteryProgress = masteryProgress;
            currentBook = null;
            currentLanguage = null;
            languageInstructor = null;
        }

        public void SetCurrentBook(BookType book) => currentBook = book;

        public BookType CurrentBook => currentBook;
        public Language CurrentLanguage => currentLanguage;
        public Hero LanguageInstructor => languageInstructor;
        public Mastery Mastery => mastery;
        public MBReadOnlyDictionary<Language, float> Languages => languages.GetReadOnlyDictionary();
        public MBReadOnlyDictionary<BookType, float> Books => books.GetReadOnlyDictionary();

        public bool IsBookRead(BookType type) => books.ContainsKey(type) && books[type] >= 1f;
        public bool HasTraining => masteryProgress >= 1f;

        internal override void Update(PopulationData data)
        {
            if (languageInstructor != null && (languageInstructor.IsDead || languageInstructor.IsDisabled))
                languageInstructor = null;
        }
    }
}
