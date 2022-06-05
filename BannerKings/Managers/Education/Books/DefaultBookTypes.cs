using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Items;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class DefaultBookTypes : DefaultTypeInitializer<DefaultBookTypes>
    {
        private BookType bookHeartsDesire, bookSiege, bookStrategikon,
            bookLeadership, bookTrade, bookDictionary, bookMounted;
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
    }
}
