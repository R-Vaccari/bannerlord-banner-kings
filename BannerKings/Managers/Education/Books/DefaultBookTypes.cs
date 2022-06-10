using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Items;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class DefaultBookTypes : DefaultTypeInitializer<DefaultBookTypes>
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
            bookHeartsDesire = new BookType(BKItems.Instance.BookHeartsDesire, new TextObject(), DefaultLanguages.Instance.Vlandic,
               BookUse.Skillbook, DefaultSkills.Charm);

            bookSiege = new BookType(BKItems.Instance.BookSiege, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Engineering);

            bookStrategikon = new BookType(BKItems.Instance.BookStrategikon, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Tactics);

            bookTrade = new BookType(BKItems.Instance.BookTrade, new TextObject(), DefaultLanguages.Instance.Aseran,
               BookUse.Focusbook, DefaultSkills.Trade);

            bookDictionary = new BookType(BKItems.Instance.BookDictionary, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Dictionary);

            bookMounted = new BookType(BKItems.Instance.BookMounted, new TextObject(), DefaultLanguages.Instance.Vlandic,
                BookUse.Focusbook, DefaultSkills.Riding);

            bookLeadership = new BookType(BKItems.Instance.BookLeadership, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Leadership);
        }

        public IEnumerable<BookType> All
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
