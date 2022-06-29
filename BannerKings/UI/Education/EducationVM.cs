using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Models.BKModels;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Education
{
    public class EducationVM : BannerKingsViewModel
    {
        private Hero hero;
        private MBBindingList<InformationElement> booksReadInfo, knownLanguagesInfo, currentBookInfo,
            currentLanguageInfo, currentLifestyleInfo;
        private EducationData data;

        public EducationVM(Hero hero) : base(null, false)
        {
            this.hero = hero;
            data = null;
            booksReadInfo = new MBBindingList<InformationElement>();
            knownLanguagesInfo = new MBBindingList<InformationElement>();
            currentBookInfo = new MBBindingList<InformationElement>();
            currentLanguageInfo = new MBBindingList<InformationElement>();
            currentLifestyleInfo = new MBBindingList<InformationElement>();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            BooksReadInfo.Clear();
            KnownLanguagesInfo.Clear();
            CurrentBookInfo.Clear();
            CurrentLanguageInfo.Clear();
            data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
            BKEducationModel model = BannerKingsConfig.Instance.EducationModel;
            if (data.Books.Count == 0)
                BooksReadInfo.Add(new InformationElement(new TextObject("{=!}No books read yet").ToString(), string.Empty,
                    new TextObject("{=!}Books need to be in your inventory or the current settlement's Stash in order to be readable. To start reading you need to be literate and either have a dictionary or have a minimum understanding of it's language.").ToString()));
            else foreach (KeyValuePair<BookType, float> pair in data.Books)
                BooksReadInfo.Add(new InformationElement(pair.Key.Item.Name.ToString(), FormatValue(pair.Value),
                    pair.Key.Description.ToString()));

            if (data.CurrentBook == null)
                CurrentBookInfo.Add(new InformationElement(new TextObject("{=!}Not currently reading any book").ToString(), string.Empty,
                    new TextObject("{=!}Books need to be in your inventory or the current settlement's Stash in order to be readable. To start reading you need to be literate and either have a dictionary or have a minimum understanding of it's language.").ToString()));
            else
            {
                CurrentBookInfo.Add(new InformationElement(new TextObject("{=!}Name:").ToString(),
                    data.CurrentBook.Item.Name.ToString(),
                    data.CurrentBook.Description.ToString()));

                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Progress:").ToString(),
                    FormatValue(data.CurrentBookProgress),
                    new TextObject("{=!}How close you are to finishing the book.").ToString()));

                CurrentBookInfo.Add(new InformationElement(new TextObject("{=!}Language:").ToString(), 
                    data.CurrentBook.Language.Name.ToString(),
                    data.CurrentBook.Language.Description.ToString()));

            }

            if (data.CurrentLanguage == null)
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Not currently learning any language").ToString(), string.Empty,
                    new TextObject("{=!}Languages may be taught by your courtiers that have a good fluency, so long they understand it more than you. Languages can be actively studied on the settlement the courtier is located at.").ToString()));
            else
            {
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Name:").ToString(),
                    data.CurrentLanguage.Name.ToString(),
                    data.CurrentLanguage.Description.ToString()));

                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Fluency:").ToString(),
                    FormatValue(data.CurrentLanguageFluency),
                    new TextObject("{=!}How close you are to speaking the language effortlessly.").ToString()));

                ExplainedNumber result = model.CalculateLanguageLearningRate(hero, data.LanguageInstructor, data.CurrentLanguage);
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Learning Rate:").ToString(),
                    FormatValue(result.ResultNumber),
                    result.GetExplanations()));

                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Instructor:").ToString(), 
                    data.LanguageInstructor.Name.ToString(),
                    data.LanguageInstructor.Name.ToString()));

                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Instructor Location:").ToString(),
                    data.LanguageInstructor.CurrentSettlement.Name.ToString(),
                    new TextObject("{=!}Active learning can be done at the instructor's location.").ToString()));
            }

            foreach (KeyValuePair<Language, float> pair in data.Languages)
                KnownLanguagesInfo.Add(new InformationElement(pair.Key.Name.ToString(), UIHelper.GetLanguageFluencyText(pair.Value).ToString(),
                    pair.Key.Description.ToString()));
        }
        
        private void SelectNewLanguage()
        {
            if (hero.PartyBelongedTo == null) return;
            MBReadOnlyList<ValueTuple<Language, Hero>> results = BannerKingsConfig.Instance.EducationManager.GetAvailableLanguagesToLearn(hero);
            List<InquiryElement> elements = new List<InquiryElement>();
            elements.Add(new InquiryElement(new ValueTuple<Language, Hero>(),
                       new TextObject("{=!}None").ToString(),
                       null));

            foreach (ValueTuple<Language, Hero> tuple in results)
                if (tuple.Item1 != data.CurrentLanguage && tuple.Item2 != data.LanguageInstructor)
                    elements.Add(new InquiryElement(tuple,
                        tuple.Item1.Name.ToString() + " - " + tuple.Item2.Name.ToString(),
                        null));

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=!}Choose Book").ToString(),
                new TextObject("{=!}Select what book you would like to read. Options may be disabled due to language barrier, or lack of Literate perk.").ToString(), elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate (List<InquiryElement> x)
                {
                    ValueTuple<Language, Hero> result = (ValueTuple<Language, Hero>)x[0].Identifier;
                    BannerKingsConfig.Instance.EducationManager.SetCurrentLanguage(hero, result.Item1, result.Item2);
                    if (result.Item1 != null) InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} is now learning {BOOK} from {INSTRUCTOR}.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("BOOK", result.Item1.Name)
                        .SetTextVariable("INSTRUCTOR", result.Item2.Name)
                        .ToString()));
                    else InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} stopped reading any books.")
                        .SetTextVariable("HERO", hero.Name)
                        .ToString()));
                    RefreshValues();
                }, null));
        }

        private void SelectNewBook()
        {
            if (hero.PartyBelongedTo == null) return;
            MBReadOnlyList<BookType> books = BannerKingsConfig.Instance.EducationManager.GetAvailableBooks(hero.PartyBelongedTo);
            List<InquiryElement> elements = new List<InquiryElement>();
            elements.Add(new InquiryElement(null,
                       new TextObject("{=!}None").ToString(),
                       null));

            foreach (BookType book in books)
                if (book != data.CurrentBook && book.Use != BookUse.Dictionary)
                    elements.Add(new InquiryElement(book, 
                        book.Item.Name.ToString() + " - " + book.Language.Name, 
                        null,
                        BannerKingsConfig.Instance.EducationManager.CanRead(book.Language, hero), 
                        book.Description.ToString()));

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=!}Choose Book").ToString(),
                new TextObject("{=!}Select what book you would like to read. Options may be disabled due to language barrier, or lack of Literate perk.").ToString(), elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty, 
                delegate (List<InquiryElement> x)
                {
                    BookType book = (BookType)x[0].Identifier;
                    BannerKingsConfig.Instance.EducationManager.SetCurrentBook(hero, book);
                    if (book != null) InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} is now reading {BOOK}.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("BOOK", book.Item.Name)
                        .ToString()));
                    else InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} stopped reading any books.")
                        .SetTextVariable("HERO", hero.Name)
                        .ToString()));
                    RefreshValues();
                }, null));
        }

        private void SelectLifestyle()
        {

        }

        [DataSourceProperty]
        public bool ChangeBookPossible => hero.PartyBelongedTo != null && BannerKingsConfig.Instance.EducationManager.GetAvailableBooks(hero.PartyBelongedTo).Count > 0;

        [DataSourceProperty]
        public string LanguagesText => new TextObject("{=!}Languages").ToString();

        [DataSourceProperty]
        public string KnownLanguagesText => new TextObject("{=!}Known Languages").ToString();

        [DataSourceProperty]
        public string CurrentLanguageText => new TextObject("{=!}Current Language").ToString();

        [DataSourceProperty]
        public string ChooseLanguageText => new TextObject("{=!}Choose Language").ToString();

        [DataSourceProperty]
        public string BooksText => new TextObject("{=!}Books").ToString();

        [DataSourceProperty]
        public string CurrentBookText => new TextObject("{=!}Current Book").ToString();

        [DataSourceProperty]
        public string ChooseBookText => new TextObject("{=!}Choose Book").ToString();

        [DataSourceProperty]
        public string BooksReadText => new TextObject("{=!}Books Read").ToString();

        [DataSourceProperty]
        public string LifestyleText => new TextObject("{=!}Lifestyle").ToString();

        [DataSourceProperty]
        public string ChooseLifestyleText => new TextObject("{=!}Choose Lifestyle").ToString();

        [DataSourceProperty]
        public string InvestFocusText => new TextObject("{=!}Invest Focus").ToString();

        [DataSourceProperty]
        public MBBindingList<InformationElement> KnownLanguagesInfo
        {
            get => knownLanguagesInfo;
            set
            {
                if (value != knownLanguagesInfo)
                {
                    knownLanguagesInfo = value;
                    OnPropertyChangedWithValue(value, "KnownLanguagesInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> CurrentLanguageInfo
        {
            get => currentLanguageInfo;
            set
            {
                if (value != currentLanguageInfo)
                {
                    currentLanguageInfo = value;
                    OnPropertyChangedWithValue(value, "CurrentLanguageInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> CurrentBookInfo
        {
            get => currentBookInfo;
            set
            {
                if (value != currentBookInfo)
                {
                    currentBookInfo = value;
                    OnPropertyChangedWithValue(value, "CurrentBookInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> CurrentLifestyleInfo
        {
            get => currentLifestyleInfo;
            set
            {
                if (value != currentLifestyleInfo)
                {
                    currentLifestyleInfo = value;
                    OnPropertyChangedWithValue(value, "CurrentLifestyleInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> BooksReadInfo
        {
            get => booksReadInfo;
            set
            {
                if (value != booksReadInfo)
                {
                    booksReadInfo = value;
                    OnPropertyChangedWithValue(value, "BooksReadInfo");
                }
            }
        }
    }
}
