using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Items;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class DefaultBookTypes : DefaultTypeInitializer<DefaultBookTypes, BookType>
    {
        private BookType bookHeartsDesire, bookSiege, bookStrategikon,
            bookLeadership, bookTrade, bookDictionary, bookMounted;

        public BookType HeatsDesire => bookHeartsDesire;
        public BookType Siege => bookSiege;
        public BookType Strategikon => bookStrategikon;
        public BookType Trade => bookTrade;
        public BookType Dictionary => bookDictionary;
        public BookType Mounted => bookMounted;
        public BookType Leadership => bookLeadership;

        public override void Initialize()
        {
            bookHeartsDesire = new BookType("book_heartsDesire");
            bookHeartsDesire.Initialize(BKItems.Instance.BookHeartsDesire, new TextObject(), DefaultLanguages.Instance.Vlandic,
               BookUse.Skillbook, DefaultSkills.Charm);

            bookSiege = new BookType("book_siege");
            bookSiege.Initialize(BKItems.Instance.BookSiege, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Engineering);

            bookStrategikon = new BookType("book_tactics");
            bookStrategikon.Initialize(BKItems.Instance.BookStrategikon, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Tactics);

            bookTrade = new BookType("book_trade");
            bookTrade.Initialize(BKItems.Instance.BookTrade, new TextObject(), DefaultLanguages.Instance.Aseran,
               BookUse.Focusbook, DefaultSkills.Trade);

            bookDictionary = new BookType("book_dictionary");
            bookDictionary.Initialize(BKItems.Instance.BookDictionary, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Dictionary);

            bookMounted = new BookType("book_riding");
            bookMounted.Initialize(BKItems.Instance.BookMounted, new TextObject(), DefaultLanguages.Instance.Vlandic,
                BookUse.Focusbook, DefaultSkills.Riding);

            bookLeadership = new BookType("book_leadership");
            bookLeadership.Initialize(BKItems.Instance.BookLeadership, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Leadership);
        }

        public override IEnumerable<BookType> All
        {
            get
            {
                yield return HeatsDesire;
                yield return Siege;
                yield return Strategikon;
                yield return Trade;
                yield return Dictionary;
                yield return Mounted;
                yield return Leadership;
            }
        }
    }
}
