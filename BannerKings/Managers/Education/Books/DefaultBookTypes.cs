using System.Collections.Generic;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Items;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class DefaultBookTypes : DefaultTypeInitializer<DefaultBookTypes, BookType>
    {
        public BookType HeatsDesire { get; private set; }

        public BookType Siege { get; private set; }

        public BookType Strategikon { get; private set; }

        public BookType Trade { get; private set; }

        public BookType Dictionary { get; private set; }

        public BookType Mounted { get; private set; }

        public BookType Leadership { get; private set; }

        public BookType OneHanded { get; private set; }

        public BookType TwoHanded { get; private set; }

        public BookType Polearm { get; private set; }

        public BookType Crossbow { get; private set; }

        public BookType Bow { get; private set; }

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

        public override void Initialize()
        {
            HeatsDesire = new BookType("book_heartsDesire");
            HeatsDesire.Initialize(BKItems.Instance.BookHeartsDesire, new TextObject(), DefaultLanguages.Instance.Vlandic,
                BookUse.Skillbook, DefaultSkills.Charm);

            Siege = new BookType("book_siege");
            Siege.Initialize(BKItems.Instance.BookSiege, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Engineering);

            Strategikon = new BookType("book_tactics");
            Strategikon.Initialize(BKItems.Instance.BookStrategikon, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Tactics);

            Trade = new BookType("book_trade");
            Trade.Initialize(BKItems.Instance.BookTrade, new TextObject(), DefaultLanguages.Instance.Aseran,
                BookUse.Focusbook, DefaultSkills.Trade);

            Dictionary = new BookType("book_dictionary");
            Dictionary.Initialize(BKItems.Instance.BookDictionary, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Dictionary);

            Mounted = new BookType("book_riding");
            Mounted.Initialize(BKItems.Instance.BookMounted, new TextObject(), DefaultLanguages.Instance.Vlandic,
                BookUse.Focusbook, DefaultSkills.Riding);

            Leadership = new BookType("book_leadership");
            Leadership.Initialize(BKItems.Instance.BookLeadership, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Leadership);

            OneHanded = new BookType("book_oneHanded");
            OneHanded.Initialize(BKItems.Instance.BookOneHanded, new TextObject(), DefaultLanguages.Instance.Vlandic,
                BookUse.Focusbook, DefaultSkills.OneHanded);

            TwoHanded = new BookType("book_twoHanded");
            TwoHanded.Initialize(BKItems.Instance.BookTwoHanded, new TextObject(), DefaultLanguages.Instance.Battanian,
                BookUse.Focusbook, DefaultSkills.TwoHanded);

            Crossbow = new BookType("book_crossbow");
            Crossbow.Initialize(BKItems.Instance.BookCrossbow, new TextObject(), DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Crossbow);

            Bow = new BookType("book_bow");
            Bow.Initialize(BKItems.Instance.BookBow,
                new TextObject(
                    "\"The History of Calradian Archery\" is a monography of most popular weapon around the continent: bows and arrows. Written by renowned archer, Rovin of Erithrys, the book describes the variety of bows and arrows used across the continent, along with the description of culture favorite variants. Each Calradian culture archery traditions and practical ways to use those in warfare is described in separate chapters. The author also introduces most of bow constructions invented in Calradia, from simplest mountain bows to rather expensive and complicated bows used by the nobility."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Bow);

            Polearm = new BookType("book_polearm");
            Polearm.Initialize(BKItems.Instance.BookPolearm,
                new TextObject(
                    "\"Lycaron debate of 1074\" is a record of events that ocured during a tournament of Lycaron in local inn. A group of Khan's Guards, entourage of young Khuzait warrior Monchug (who came to Lycaron to take part in the tournament) has gotten into discussion with Vlandian voulgiers serving in local garrison about whos swingeable polearm armament is better. Soon the present Cataphracts of the same garrison joined the discussion, with intention to prove that long, thrust lances are superior. The debate resulted in 12 deaths, 23 injured and maimed and, as declared unanimously by witnesses, was won by Gareth, a local townfolk, who was passing by the inn and entered carrying his trusty rake, to see what all the ruckus is about."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook, DefaultSkills.Bow);
        }
    }
}