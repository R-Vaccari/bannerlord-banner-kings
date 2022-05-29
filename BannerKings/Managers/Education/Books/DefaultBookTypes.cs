using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Items;
using TaleWorlds.CampaignSystem;
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
                delegate (Hero hero) 
                { 

                });

            bookSiege = new BookType(BKItems.Instance.BookSiege, new TextObject(), DefaultLanguages.Instance.Calradian,
                delegate (Hero hero)
                {

                });

            bookStrategikon = new BookType(BKItems.Instance.BookStrategikon, new TextObject(), DefaultLanguages.Instance.Calradian,
                delegate (Hero hero)
                {

                });

            bookTrade = new BookType(BKItems.Instance.BookTrade, new TextObject(), DefaultLanguages.Instance.Aseran,
                delegate (Hero hero)
                {

                });

            bookDictionary = new BookType(BKItems.Instance.BookDictionary, new TextObject(), DefaultLanguages.Instance.Calradian,
                delegate (Hero hero)
                {

                });

            bookMounted = new BookType(BKItems.Instance.BookMounted, new TextObject(), DefaultLanguages.Instance.Vlandic,
                delegate (Hero hero)
                {

                });

            bookLeadership = new BookType(BKItems.Instance.BookLeadership, new TextObject(), DefaultLanguages.Instance.Calradian,
                delegate (Hero hero)
                {

                });
        }
    }
}
