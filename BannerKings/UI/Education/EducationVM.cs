using System;
using System.Collections.Generic;
using System.Text;
using BannerKings.Behaviours;
using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.UI.Items.UI;
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
        private MBBindingList<InformationElement> booksReadInfo,
            knownLanguagesInfo,
            currentBookInfo,
            currentLanguageInfo,
            currentLifestyleInfo,
            lifestyleProgressInfo;

        private bool canAddFocus;
        private new EducationData data;
        private readonly CharacterDeveloperVM developerVM;
        private readonly Hero hero;
        private MBBindingList<PerkVM> perks;
        private InformationElement bookSellers;
        private string lifestyleName, lifestylePassive;

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

        [DataSourceProperty] public bool ChangeBookPossible => hero.PartyBelongedTo != null;

        [DataSourceProperty] public string EducationText => new TextObject("{=0KjrcEam}Education of {HERO}")
                .SetTextVariable("HERO", hero.Name).ToString();
        [DataSourceProperty] public string LanguagesText => new TextObject("{=KBsVXEtH}Languages").ToString();

        [DataSourceProperty] public string KnownLanguagesText => new TextObject("{=Tv2gkkv4}Known Languages").ToString();

        [DataSourceProperty] public string CurrentLanguageText => new TextObject("{=GY7WPdvk}Current Language").ToString();

        [DataSourceProperty] public string ChooseLanguageText => new TextObject("{=bkoJgoDo}Choose Language").ToString();

        [DataSourceProperty] public string BooksText => new TextObject("{=NvnA9WyM}Books").ToString();

        [DataSourceProperty] public string CurrentBookText => new TextObject("{=1BrHXD0A}Current Book").ToString();

        [DataSourceProperty] public string ChooseBookText => new TextObject("{=Y4G7PxXr}Choose Book").ToString();

        [DataSourceProperty] public string BooksReadText => new TextObject("{=NvnA9WyM}Books Read").ToString();

        [DataSourceProperty] public string LifestyleText => new TextObject("{=tYO5xwVe}Lifestyle").ToString();

        [DataSourceProperty] public string ChooseLifestyleText => new TextObject("{=sOT08u5v}Choose Lifestyle").ToString();

        [DataSourceProperty] public string InvestFocusText => new TextObject("{=kweOwoNY}Invest Focus").ToString();

        [DataSourceProperty]
        public string LifestyleNameText
        {
            get => lifestyleName;
            set
            {
                if (value != lifestyleName)
                {
                    lifestyleName = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string LifestylePassiveText
        {
            get => lifestylePassive;
            set
            {
                if (value != lifestylePassive)
                {
                    lifestylePassive = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public InformationElement BookSellers
        {
            get => bookSellers;
            set
            {
                if (value != bookSellers)
                {
                    bookSellers = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PerkVM> Perks
        {
            get => perks;
            set
            {
                if (value != perks)
                {
                    perks = value;
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
                }
            }
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


            StringBuilder sb = new StringBuilder();
            foreach (Hero seller in Campaign.Current.GetCampaignBehavior<BKEducationBehavior>().GetAllBookSellers())
            {
                if (seller.IsAlive && seller.CurrentSettlement != null)
                {
                    sb.AppendLine(seller.Name.ToString() + ": " + seller.CurrentSettlement.Name.ToString());
                }
            }

            BookSellers = new InformationElement(new TextObject("{=rLnPvsJk}Book Sellers").ToString(), string.Empty, sb.ToString());


            if (data.Books.Count == 0)
            {
                BooksReadInfo.Add(new InformationElement(new TextObject("{=uovmgGZa}No books read yet").ToString(), string.Empty,
                    new TextObject("{=NvnA9WyM}Books need to be in your inventory or the current settlement's Stash in order to be readable. To start reading you need to be literate and either have a dictionary or have a minimum understanding of it's language.")
                        .ToString()));
            }
            else
            {
                foreach (var pair in data.Books)
                {
                    BooksReadInfo.Add(new InformationElement(pair.Key.Item.Name.ToString(), FormatValue(pair.Value),
                        pair.Key.Description.ToString()));
                }
            }

            if (data.CurrentBook == null)
            {
                CurrentBookInfo.Add(new InformationElement(new TextObject("{=aBaKuACo}Not currently reading any book").ToString(),
                    string.Empty,
                    new TextObject("{=NvnA9WyM}Books need to be in your inventory or the current settlement's Stash in order to be readable. To start reading you need to be literate and either have a dictionary or have a minimum understanding of it's language.")
                        .ToString()));
            }
            else
            {
                CurrentBookInfo.Add(new InformationElement(new TextObject("{=o1Z28eXv}Name:").ToString(),
                    data.CurrentBook.Item.Name.ToString(),
                    data.CurrentBook.Description.ToString()));

                CurrentBookInfo.Add(new InformationElement(new TextObject("{=4gCw08Kk}Progress:").ToString(),
                    FormatValue(data.CurrentBookProgress),
                    new TextObject("{=z3cC9CTj}How close you are to finishing the book.").ToString()));

                CurrentBookInfo.Add(new InformationElement(new TextObject("{=ANyvNDou}Reading rate:").ToString(),
                    FormatValue(data.CurrentBookReadingRate.ResultNumber),
                    data.CurrentBookReadingRate.GetExplanations()));

                CurrentBookInfo.Add(new InformationElement(new TextObject("{=kjkoLD9d}Language:").ToString(),
                    data.CurrentBook.Language.Name.ToString(),
                    data.CurrentBook.Language.Description.ToString()));
            }

            if (data.CurrentLanguage == null)
            {
                CurrentLanguageInfo.Add(new InformationElement(
                    new TextObject("{=mGnKu6GO}Not currently learning any language").ToString(), string.Empty,
                    new TextObject("{=KBsVXEtH}Languages may be taught by your courtiers that have a good fluency, so long they understand it more than you. Languages can be actively studied on the settlement the courtier is located at.")
                        .ToString()));
            }
            else
            {
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=o1Z28eXv}Name:").ToString(),
                    data.CurrentLanguage.Name.ToString(),
                    data.CurrentLanguage.Description.ToString()));

                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=1zU0h9p7}Fluency:").ToString(),
                    FormatValue(data.CurrentLanguageFluency),
                    new TextObject("{=DxbGVKZ1}How close you are to speaking the language effortlessly.").ToString()));

                var result = data.CurrentLanguageLearningRate;
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=2SLtW912}Learning Rate:").ToString(),
                    FormatValue(result.ResultNumber),
                    result.GetExplanations()));

                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=xPA1FYkB}Instructor:").ToString(),
                    data.LanguageInstructor.Name.ToString(),
                    data.LanguageInstructor.Name.ToString()));

                var settlementString = data.LanguageInstructor.CurrentSettlement != null
                    ? data.LanguageInstructor.CurrentSettlement.Name.ToString()
                    : new TextObject("{=vW3YtyNm}None (in a mobile party)").ToString();
                CurrentLanguageInfo.Add(new InformationElement(new TextObject("{=cqF7eA22}Instructor Location:").ToString(),
                    settlementString,
                    new TextObject("{=sFCqysF2}Active learning can be done at the instructor's location.").ToString()));
            }

            var languageLimit = BannerKingsConfig.Instance.EducationModel.CalculateLanguageLimit(hero);
            KnownLanguagesInfo.Add(new InformationElement(new TextObject("{=KBsVXEtH}Languages limit:").ToString(),
                languageLimit.ResultNumber.ToString(),
                languageLimit.GetExplanations()));
            foreach (var pair in data.Languages)
            {
                KnownLanguagesInfo.Add(new InformationElement(pair.Key.Name.ToString(),
                    UIHelper.GetLanguageFluencyText(pair.Value).ToString(),
                    pair.Key.Description.ToString()));
            }

            if (data.Lifestyle == null)
            {
                LifestyleProgressInfo.Add(new InformationElement(
                    new TextObject("{=MaV9QBJE}No lifestyle currently adopted").ToString(), string.Empty,
                    new TextObject("{=KBsVXEtH}Languages may be taught by your courtiers that have a good fluency, so long they understand it more than you. Languages can be actively studied on the settlement the courtier is located at.")
                        .ToString()));

                LifestyleNameText = null;
                LifestylePassiveText = null;
            }
            else
            {
                CanAddFocus = data.Lifestyle.CanInvestFocus(hero) && hero.HeroDeveloper.UnspentFocusPoints > 0;
                LifestyleNameText = data.Lifestyle.Name.ToString();
                LifestylePassiveText = data.Lifestyle.PassiveEffects.ToString();

                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=WJK9V5ND}Necessary skill:").ToString(),
                    data.Lifestyle.NecessarySkillForFocus.ToString(),
                    new TextObject("{=RBZRv3np}Necessary skill amount to unlock next stage. This is the total amount of both skills combined. You may have the total amount in a single skill, or half of it in each of the lifestyle's skills.")
                        .ToString()));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=yavEq3mq}Invested focus:").ToString(),
                    data.Lifestyle.InvestedFocus.ToString(),
                    new TextObject("{=J6vdpLhZ}The amount of focus points you have invested. Each focus correlates to one perk gained.").ToString()));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=4gCw08Kk}Progress:").ToString(),
                    FormatValue(data.LifestyleProgress),
                    new TextObject("{=78jQbY8E}Current progress in this stage. Once progress hits 100% and you have the necessary skill threshold, you can invest your next focus point in exchange for the next lifestyle perk.")
                        .ToString()));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=ER70o2UR}First skill:").ToString(),
                    data.Lifestyle.FirstSkill.Name.ToString(),
                    data.Lifestyle.FirstSkill.Description.ToString()));
                LifestyleProgressInfo.Add(new InformationElement(new TextObject("{=NSsPm2UN}Second skill:").ToString(),
                    data.Lifestyle.SecondSkill.Name.ToString(),
                    data.Lifestyle.SecondSkill.Description.ToString()));

                var perks = data.Lifestyle.Perks;
                var gainedPerks = data.Perks;
                for (var i = 0; i < perks.Count; i++)
                {
                    var perk = perks[i];
                    Perks.Add(new PerkVM(perk,
                        i <= data.Lifestyle.InvestedFocus,
                        PerkVM.PerkAlternativeType.NoAlternative,
                        null, null,
                        _ => gainedPerks.Contains(perk),
                        _ => data.Lifestyle.InvestedFocus == i - 1));
                }
            }
        }

        private void SelectNewLanguage()
        {
            if (hero.PartyBelongedTo == null)
            {
                return;
            }

            var results = BannerKingsConfig.Instance.EducationManager.GetAvailableLanguagesToLearn(hero);
            var elements = new List<InquiryElement>
            {
                new(new ValueTuple<Language, Hero>(),
                    new TextObject("{=5n3dJTGc}None").ToString(),
                    null)
            };

            foreach (var tuple in results)
            {
                if (tuple.Item1 != data.CurrentLanguage && tuple.Item2 != data.LanguageInstructor)
                {
                    var hero = tuple.Item2;
                    var settlementString = hero.CurrentSettlement != null
                        ? hero.CurrentSettlement.Name.ToString()
                        : new TextObject("{=vW3YtyNm}None (in a mobile party)").ToString();
                    elements.Add(new InquiryElement(tuple,
                        tuple.Item1.Name + " - " + hero.Name,
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject)),
                        hero.IsFriend(Hero.MainHero) || hero.Clan == Clan.PlayerClan, settlementString));
                }
            }


            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=bkoJgoDo}Choose Language").ToString(),
                new TextObject("{=gdCXV7n3}Select a language you would like to learn. Learning a language requires an instructor from your court, and different people have different teaching skills. A courtier must have a good opinion of you in order to be available. Learning languages is easier when they are intelligible with your native language.")
                    .ToString(),
                elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate(List<InquiryElement> x)
                {
                    var result = (ValueTuple<Language, Hero>) x[0].Identifier;
                    BannerKingsConfig.Instance.EducationManager.SetCurrentLanguage(hero, result.Item1, result.Item2);
                    if (result.Item1 != null)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=DVNd6xA5}{HERO} is now learning {LANGUAGE} from {INSTRUCTOR}.")
                                .SetTextVariable("HERO", hero.Name)
                                .SetTextVariable("LANGUAGE", result.Item1.Name)
                                .SetTextVariable("INSTRUCTOR", result.Item2.Name)
                                .ToString()));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=Fd2YMXEF}{HERO} stopped learning any languages.")
                                .SetTextVariable("HERO", hero.Name)
                                .ToString()));
                    }

                    RefreshValues();
                }, null));
        }

        private void SelectNewBook()
        {
            if (hero.PartyBelongedTo == null)
            {
                return;
            }

            var books = BannerKingsConfig.Instance.EducationManager.GetAvailableBooks(hero.PartyBelongedTo);
            var elements = new List<InquiryElement>
            {
                new(null,
                    new TextObject("{=5n3dJTGc}None").ToString(),
                    null)
            };

            foreach (var book in books)
            {
                if (book != data.CurrentBook && book.Use != BookUse.Dictionary)
                {
                    elements.Add(new InquiryElement(book,
                        book.Item.Name + " - " + book.Language.Name,
                        null,
                        BannerKingsConfig.Instance.EducationManager.CanRead(book, hero),
                        book.Description.ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=Y4G7PxXr}Choose Book").ToString(),
                new TextObject("{=pXUyfe6v}Select what book you would like to read. Options may be disabled due to language barrier, or lack of Literate perk.")
                    .ToString(), elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate(List<InquiryElement> x)
                {
                    var book = (BookType) x[0].Identifier;
                    BannerKingsConfig.Instance.EducationManager.SetCurrentBook(hero, book);
                    if (book != null)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=LeTL076P}{HERO} is now reading {BOOK}.")
                                .SetTextVariable("HERO", hero.Name)
                                .SetTextVariable("BOOK", book.Item.Name)
                                .ToString()));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=pzLRq0QJ}{HERO} stopped reading any books.")
                                .SetTextVariable("HERO", hero.Name)
                                .ToString()));
                    }

                    RefreshValues();
                }, null));
        }

        private void SelectLifestyle()
        {
            var lfs = BannerKingsConfig.Instance.EducationManager.GetViableLifestyles(hero);
            var elements = new List<InquiryElement>
            {
                new(null,
                    new TextObject("{=5n3dJTGc}None").ToString(),
                    null)
            };

            foreach (var lf in lfs)
            {
                if (lf != data.Lifestyle)
                {
                    elements.Add(new InquiryElement(lf,
                        new TextObject("{LIFESTYLE} ({SKILL1} / {SKILL2})")
                            .SetTextVariable("LIFESTYLE", lf.Name)
                            .SetTextVariable("SKILL1", lf.FirstSkill.Name)
                            .SetTextVariable("SKILL2", lf.SecondSkill.Name).ToString(),
                        null,
                        lf.CanLearn(hero),
                        lf.Description.ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=sOT08u5v}Choose Lifestyle").ToString(),
                new TextObject("{=8vF2rt3F}Select a lifestyle you would like to adopt. Picking a lifestyle will undo the progress of the lifestyle you are currently learning, if any. Each lifestyle is based on 2 skills, and you need at least 15 profficiency in each skill to adopt it.")
                    .ToString(), elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate(List<InquiryElement> x)
                {
                    var lf = (Lifestyle) x[0].Identifier;
                    BannerKingsConfig.Instance.EducationManager.SetCurrentLifestyle(hero, lf);
                    if (lf != null)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=Z7t7oyXi}{HERO} has adopted the {LIFESTYLE} lifestyle.")
                                .SetTextVariable("HERO", hero.Name)
                                .SetTextVariable("LIFESTYLE", lf.Name)
                                .ToString()));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=YbTbe0FH}{HERO} is not following any lifestyle.")
                                .SetTextVariable("HERO", hero.Name)
                                .ToString()));
                    }

                    RefreshValues();
                }, null));
        }

        private void InvestFocus()
        {
            if (hero.HeroDeveloper.UnspentFocusPoints <= 0)
            {
                return;
            }

            data.Lifestyle.InvestFocus(data, hero);
            developerVM.CurrentCharacter.RefreshValues();
            developerVM.RefreshValues();
        }
    }
}