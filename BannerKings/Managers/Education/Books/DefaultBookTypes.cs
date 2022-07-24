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
            bookLeadership, bookTrade, bookDictionary, bookMounted, bookOneHanded, bookTwoHanded, bookPolearm, bookCrossbow,
            bookBow;

        public BookType HeatsDesire => bookHeartsDesire;
        public BookType Siege => bookSiege;
        public BookType Strategikon => bookStrategikon;
        public BookType Trade => bookTrade;
        public BookType Dictionary => bookDictionary;
        public BookType Mounted => bookMounted;
        public BookType Leadership => bookLeadership;
        public BookType OneHanded => bookOneHanded;
        public BookType TwoHanded => bookTwoHanded;
        public BookType Polearm => bookPolearm;
        public BookType Crossbow => bookCrossbow;
        public BookType Bow => bookBow;

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

            bookOneHanded = new BookType("book_oneHanded");
            bookOneHanded.Initialize(BKItems.Instance.BookOneHanded, new TextObject(), DefaultLanguages.Instance.Vlandic,
                BookUse.Focusbook, DefaultSkills.OneHanded);

            bookTwoHanded = new BookType("book_twoHanded");
            bookTwoHanded.Initialize(BKItems.Instance.BookTwoHanded, new TextObject(), DefaultLanguages.Instance.Battanian,
                BookUse.Focusbook, DefaultSkills.TwoHanded);

            bookCrossbow = new BookType("book_crossbow");
            bookCrossbow.Initialize(BKItems.Instance.BookCrossbow, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Crossbow);

            bookBow = new BookType("book_bow");
            bookBow.Initialize(BKItems.Instance.BookBow, new TextObject("\"The History of Calradian Archery\" is a monography of most popular weapon around the continent: bows and arrows. Written by renowned archer, Rovin of Erithrys, the book describes the variety of bows and arrows used across the continent, along with the description of culture favorite variants. Each Calradian culture archery traditions and practical ways to use those in warfare is described in separate chapters. The author also introduces most of bow constructions invented in Calradia, from simplest mountain bows to rather expensive and complicated bows used by the nobility."), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Bow);

            bookPolearm = new BookType("book_polearm");
            bookPolearm.Initialize(BKItems.Instance.BookPolearm, new TextObject("\"Lycaron debate of 1074\" is a record of events that ocured during a tournament of Lycaron in local inn. A group of Khan's Guards, entourage of young Khuzait warrior Monchug (who came to Lycaron to take part in the tournament) has gotten into discussion with Vlandian voulgiers serving in local garrison about whos swingeable polearm armament is better. Soon the present Cataphracts of the same garrison joined the discussion, with intention to prove that long, thrust lances are superior. The debate resulted in 12 deaths, 23 injured and maimed and, as declared unanimously by witnesses, was won by Gareth, a local townfolk, who was passing by the inn and entered carrying his trusty rake, to see what all the ruckus is about."), 
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Bow);
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
                yield return OneHanded;
                yield return TwoHanded;
                yield return Polearm;
                yield return Crossbow;
                yield return Bow;
            }
        }
    }
}
