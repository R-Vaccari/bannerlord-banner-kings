using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Lifestyles;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Education
{
    public class EducationVM : BannerKingsViewModel
    {
        private Hero hero;
        private CharacterDeveloperVM developerVM;
        private MBBindingList<InformationElement> booksReadInfo, knownLanguagesInfo, currentBookInfo,
            currentLanguageInfo, currentLifestyleInfo, lifestyleProgressInfo;
        private MBBindingList<PerkVM> perks;
        private EducationData data;
        private bool canAddFocus = false;

        public EducationVM(Hero hero, CharacterDeveloperVM developerVM) : base(null, false)
        {
            this.hero = hero;
            this.developerVM = developerVM;
            data = null;
            booksReadInfo = new MBBindingList<InformationElement>();
            knownLanguagesInfo = new MBBindingList<InformationElement>();
            currentBookInfo = new MBBindingList<InformationElement>();
            currentLanguageInfo = new MBBindingList<InformationElement>();
            currentLifestyleInfo = new MBBindingList<InformationElement>();
            lifestyleProgressInfo = new MBBindingList<InformationElement>();
            perks = new MBBindingList<PerkVM>();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            BooksReadInfo.Clear();
            KnownLanguagesInfo.Clear();
            CurrentBookInfo.Clear();
            CurrentLanguageInfo.Clear();
            CurrentLifestyleInfo.Clear();
            LifestyleProgressInfo.Clear();
            Perks.Clear();
            data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);

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

                CurrentBookInfo.Add(new InformationElement(new TextObject("{=!}Progress:").ToString(),
                    FormatValue(data.CurrentBookProgress),
                    new TextObject("{=!}How close you are to finishing the book.").ToString()));

                CurrentBookInfo.Add(new InformationElement(new TextObject("{=!}Reading rate:").ToString(),
                    FormatValue(data.CurrentBookReadingRate.ResultNumber),
                    data.CurrentBookReadingRate.GetExplanations()));

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

                ExplainedNumber result = data.CurrentLanguageLearningRate;
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Learning Rate:").ToString(),
                    FormatValue(result.ResultNumber),
                    result.GetExplanations()));

                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Instructor:").ToString(), 
                    data.LanguageInstructor.Name.ToString(),
                    data.LanguageInstructor.Name.ToString()));

                string settlementString = data.LanguageInstructor.CurrentSettlement != null ? data.LanguageInstructor.CurrentSettlement.Name.ToString()
                    : new TextObject("{=!}None (in a mobile party)").ToString();
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=!}Instructor Location:").ToString(),
                    settlementString,
                    new TextObject("{=!}Active learning can be done at the instructor's location.").ToString()));
            }

            ExplainedNumber languageLimit = BannerKingsConfig.Instance.EducationModel.CalculateLanguageLimit(hero);
            KnownLanguagesInfo.Add(new InformationElement(new TextObject("{=!}Languages limit:").ToString(), languageLimit.ResultNumber.ToString(),
                   languageLimit.GetExplanations().ToString()));
            foreach (KeyValuePair<Language, float> pair in data.Languages)
                KnownLanguagesInfo.Add(new InformationElement(pair.Key.Name.ToString(), UIHelper.GetLanguageFluencyText(pair.Value).ToString(),
                    pair.Key.Description.ToString()));

            if (data.Lifestyle == null)
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=!}No lifestyle currently adopted").ToString(), string.Empty,
                    new TextObject("{=!}Languages may be taught by your courtiers that have a good fluency, so long they understand it more than you. Languages can be actively studied on the settlement the courtier is located at.").ToString()));
            else
            {

                CanAddFocus = data.Lifestyle.CanInvestFocus(hero) && hero.HeroDeveloper.UnspentFocusPoints > 0;

                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=!}Lifestyle:").ToString(),
                    data.Lifestyle.Name.ToString(),
                    string.Empty));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=!}Necessary skill:").ToString(),
                    data.Lifestyle.NecessarySkillForFocus.ToString(),
                    new TextObject("{=!}Necessary skill amount in either lifestyle skill to enable next focus investment and perk unlock.")
                    .ToString()));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=!}Invested focus:").ToString(),
                    data.Lifestyle.InvestedFocus.ToString(),
                    string.Empty));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=!}Progress:").ToString(),
                    FormatValue(data.Lifestyle.Progress),
                    new TextObject("{=!}Current progress in this stage. Once progress hits 100% and you have the necessary skill threshold, you can invest your next focus point in exchange for the next lifestyle perk.")
                    .ToString()));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=!}First skill:").ToString(),
                    data.Lifestyle.FirstSkill.Name.ToString(),
                    data.Lifestyle.FirstSkill.Description.ToString()));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=!}Second skill:").ToString(),
                    data.Lifestyle.SecondSkill.Name.ToString(),
                    data.Lifestyle.SecondSkill.Description.ToString()));

                MBReadOnlyList<PerkObject> perks = data.Lifestyle.Perks;
                MBReadOnlyList<PerkObject> gainedPerks = data.Perks;
                for (int i = 0; i < perks.Count; i++)
                {
                    PerkObject perk = perks[i];
                    Perks.Add(new PerkVM(perk, 
                        i <= data.Lifestyle.InvestedFocus, 
                        PerkVM.PerkAlternativeType.NoAlternative, 
                        null, null, 
                        (PerkObject p) => gainedPerks.Contains(perk),
                        (PerkObject p) => data.Lifestyle.InvestedFocus == i - 1));
                }                
            }
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
                {
                    Hero hero = tuple.Item2;
                    string settlementString = hero.CurrentSettlement != null ? hero.CurrentSettlement.Name.ToString() :
                        new TextObject("{=!}None (in a mobile party)").ToString();
                    elements.Add(new InquiryElement(tuple,
                        tuple.Item1.Name.ToString() + " - " + hero.Name.ToString(),
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject)),
                        hero.IsFriend(Hero.MainHero) || hero.Clan == Clan.PlayerClan, settlementString));
                }
                    

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=!}Choose Language").ToString(),
                new TextObject("{=!}Select a language you would like to learn. Learning a language requires an instructor from your court, and different people have different teaching skills. A courtier must have a good opinion of you in order to be available. Learning languages is easier when they are intelligible with your native language.").ToString(), 
                elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate (List<InquiryElement> x)
                {
                    ValueTuple<Language, Hero> result = (ValueTuple<Language, Hero>)x[0].Identifier;
                    BannerKingsConfig.Instance.EducationManager.SetCurrentLanguage(hero, result.Item1, result.Item2);
                    if (result.Item1 != null) InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} is now learning {LANGUAGE} from {INSTRUCTOR}.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("LANGUAGE", result.Item1.Name)
                        .SetTextVariable("INSTRUCTOR", result.Item2.Name)
                        .ToString()));
                    else InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} stopped learning any languages.")
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
                        BannerKingsConfig.Instance.EducationManager.CanRead(book, hero), 
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
            MBReadOnlyList<Lifestyle> lfs = BannerKingsConfig.Instance.EducationManager.GetViableLifestyles(hero);
            List<InquiryElement> elements = new List<InquiryElement>();
            elements.Add(new InquiryElement(null,
                       new TextObject("{=!}None").ToString(),
                       null));

            foreach (Lifestyle lf in lfs)
                if (lf != data.Lifestyle)
                    elements.Add(new InquiryElement(lf,
                        new TextObject("{LIFESTYLE} ({SKILL1} / {SKILL2})")
                        .SetTextVariable("LIFESTYLE", lf.Name)
                        .SetTextVariable("SKILL1", lf.FirstSkill.Name)
                        .SetTextVariable("SKILL2", lf.SecondSkill.Name).ToString(),
                        null,
                        lf.CanLearn(hero),
                        lf.Description.ToString()));

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=!}Choose Lifestyle").ToString(),
               new TextObject("{=!}Select a lifestyle you would like to adopt. Picking a lifestyle will undo the progress of the lifestyle you are currently learning, if any. Each lifestyle is based on 2 skills, and you need at least 150 profficiency in each skill to adopt it.").ToString(), elements, true, 1,
               GameTexts.FindText("str_done").ToString(), string.Empty,
               delegate (List<InquiryElement> x)
               {
                   Lifestyle lf = (Lifestyle)x[0].Identifier;
                   BannerKingsConfig.Instance.EducationManager.SetCurrentLifestyle(hero, lf);
                   if (lf != null) InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} has adopted the {LIFESTYLE} lifestyle.")
                       .SetTextVariable("HERO", hero.Name)
                       .SetTextVariable("LIFESTYLE", lf.Name)
                       .ToString()));
                   else InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} is not following any lifestyle.")
                       .SetTextVariable("HERO", hero.Name)
                       .ToString()));
                   RefreshValues();
               }, null));
        }

        private void InvestFocus()
        {
            if (hero.HeroDeveloper.UnspentFocusPoints <= 0) return;
            hero.HeroDeveloper.UnspentFocusPoints -= 1;
            PerkObject perk = data.Lifestyle.InvestFocus(hero);
            data.AddPerk(perk);

            developerVM.CurrentCharacter.RefreshValues();
            developerVM.RefreshValues();
        }

        [DataSourceProperty]
        public bool ChangeBookPossible => hero.PartyBelongedTo != null;

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
        public MBBindingList<PerkVM> Perks
        {
            get => perks;
            set
            {
                if (value != perks)
                {
                    perks = value;
                    OnPropertyChangedWithValue(value, "Perks");
                }
            }
        }

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
        public MBBindingList<InformationElement> LifestyleProgressInfo
        {
            get => lifestyleProgressInfo;
            set
            {
                if (value != lifestyleProgressInfo)
                {
                    lifestyleProgressInfo = value;
                    OnPropertyChangedWithValue(value, "LifestyleProgressInfo");
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

        [DataSourceProperty]
        public bool CanAddFocus
        {
            get => canAddFocus;
            set
            {
                if (value != canAddFocus)
                {
                    canAddFocus = value;
                    OnPropertyChangedWithValue(value, "CanAddFocus");
                }
            }
        }
    }
}
