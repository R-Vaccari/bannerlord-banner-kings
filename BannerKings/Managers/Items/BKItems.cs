using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Items
{
    public class BKItems : DefaultTypeInitializer<BKItems, ItemObject>
    {
        public ItemObject BookHeartsDesire { get; private set; }
        public ItemObject BookSiege { get; private set; }
        public ItemObject BookStrategikon { get; private set; }
        public ItemObject BookLeadership { get; private set; }
        public ItemObject BookTrade { get; private set; }
        public ItemObject BookDictionary { get; private set; }
        public ItemObject BookArtHorsemanship { get; private set; }
        public ItemObject BookGreenKnight { get; private set; }
        public ItemObject BookOneHanded { get; private set; }
        public ItemObject BookTwoHanded { get; private set; }
        public ItemObject BookPolearm { get; private set; }
        public ItemObject BookCrossbow { get; private set; }
        public ItemObject BookBow { get; private set; }
        public ItemObject BookThrowing { get; private set; }
        public ItemObject BookMedicine { get; private set; }
        public ItemObject BookLoveCastle { get; private set; }
        public ItemObject BookKaisLayala { get; private set; }
        public ItemObject BookHelgeredKara { get; private set; }
        public ItemObject BookGardenArgument { get; private set; }
        public ItemObject BookIrkBitig { get; private set; }
        public ItemObject BookNakedFingers { get; private set; }
        public ItemObject BookFabulaeAquilae { get; private set; }
        public ItemObject BookWestGemynt { get; private set; }
        public ItemObject BookSkullsEleftheroi { get; private set; }

        public ItemObject Apple { get; private set; }
        public ItemObject Bread { get; private set; }
        public ItemObject Pie { get; private set; }
        public ItemObject Carrot { get; private set; }
        public ItemObject Orange { get; private set; }
        public ItemObject Honey { get; private set; }
        public ItemObject Limestone { get; private set; }
        public ItemObject Marble { get; private set; }
        public ItemObject GoldOre { get; private set; }
        public ItemObject GoldIngot { get; private set; }
        public ItemObject Gems { get; private set; }
        public ItemObject Mead { get; private set; }
        public ItemObject Garum { get; private set; }
        public ItemObject Spice { get; private set; }
        public ItemObject Papyrus { get; private set; }
        public ItemObject Ink { get; private set; }
        public ItemObject WhaleMeat { get; private set; }
        public ItemObject Carpet { get; private set; }
        public ItemObject PurpleDye { get; private set; }

        public override IEnumerable<ItemObject> All
        {
            get
            {
                yield return BookHeartsDesire;
                yield return BookSiege;
                yield return BookStrategikon;
                yield return BookLeadership;
                yield return BookTrade;
                yield return BookDictionary;
                yield return BookGreenKnight;
                yield return BookOneHanded;
                yield return BookTwoHanded;
                yield return BookPolearm;
                yield return BookCrossbow;
                yield return BookBow;
                yield return BookThrowing;
                yield return Apple;
                yield return Honey;
                yield return Limestone;
                yield return Marble;
                yield return GoldOre;
                yield return GoldIngot;
                yield return Gems;
                yield return Mead;
                yield return Garum;
                yield return Orange;
                yield return Papyrus;
                yield return Ink;
                yield return WhaleMeat;
                yield return PurpleDye;
                yield return Spice;
            }
        }

        public override void Initialize()
        {
            PurpleDye = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("PurpleDye"));
            InitializeTradeGood(PurpleDye,
                new TextObject("{=oQAeHzY3}Purple Dye{@Plural}jars of purple dye{\\@}"), "lib_inkwell",
                BKItemCategories.Instance.Dyes, 500, 10f, ItemObject.ItemTypeEnum.Goods, false);

            WhaleMeat = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("WhaleMeat"));
            ItemObject.InitializeTradeGood(WhaleMeat, 
                new TextObject("{=6U2zYiq6}Whale Meat{@Plural}loads of whale meat{\\@}", null), 
                "merchandise_meat", 
                DefaultItemCategories.Meat, 50, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Papyrus = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("Papyrus"));
            InitializeTradeGood(Papyrus,
                new TextObject("{=fowoOOL4}Papyrus{@Plural}rolls of papyrus{\\@}"), "lib_scroll_a",
                BKItemCategories.Instance.Papyrus, 60, 10f, ItemObject.ItemTypeEnum.Goods, false);

            Ink = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("Ink"));
            InitializeTradeGood(Ink,
                new TextObject("{=H11qZfBw}Ink{@Plural}jars of ink{\\@}"), "lib_inkwell",
                BKItemCategories.Instance.Ink, 200, 10f, ItemObject.ItemTypeEnum.Goods, false);

            Spice = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("spice"));
            InitializeTradeGood(Spice,
                new TextObject("{=1jqxlEkT}Spice{@Plural}sacks of spice{\\@}"), "spice_sack",
                BKItemCategories.Instance.Spice, 300, 10f, ItemObject.ItemTypeEnum.Goods, false);

            Apple = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("apple"));
            InitializeTradeGood(Apple,
                new TextObject("{=1MAr0j2J}Apples{@Plural}baskets of apples{\\@}"), "foods_basket_apple",
                BKItemCategories.Instance.Fruit, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Orange = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("orange"));
            InitializeTradeGood(Orange,
                new TextObject("{=Krqwsins}Oranges{@Plural}baskets of oranges{\\@}"), "foods_orange_basket",
                BKItemCategories.Instance.Fruit, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Bread = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("bread"));
            InitializeTradeGood(Bread,
                new TextObject("{=VJfbLXmi}Bread{@Plural}loathes of bread{\\@}"), "merchandise_bread",
                BKItemCategories.Instance.Bread, 20, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Pie = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("pie"));
            InitializeTradeGood(Pie,
                new TextObject("{=Qa0dQMOc}Pie{@Plural}baskets of pies{\\@}"), "kitchen_pie",
                BKItemCategories.Instance.Pie, 30, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Carrot = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("carrot"));
            InitializeTradeGood(Carrot,
                new TextObject("{=C5Xe5MJK}Carrots{@Plural}baskets of carrots{\\@}"), "foods_carrots_basket",
                BKItemCategories.Instance.Fruit, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Honey = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("honey"));
            InitializeTradeGood(Honey,
                new TextObject("{=1Maj0j6J}Honey{@Plural}barrels of honey{\\@}"), "honey_pot",
                BKItemCategories.Instance.Honey, 28, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Limestone = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("limestone"));
            InitializeTradeGood(Limestone,
                new TextObject("{=ywciPeBS}Limestone{@Plural}stacks of limestone bricks{\\@}"), "limestone",
                BKItemCategories.Instance.Limestone, 50, 10f, ItemObject.ItemTypeEnum.Goods);

            Marble = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("marble"));
            InitializeTradeGood(Marble,
                new TextObject("{=D3mU8bc3}Marble{@Plural}stacks of marble bricks{\\@}"), "marblestone",
                BKItemCategories.Instance.Marble, 150, 10f, ItemObject.ItemTypeEnum.Goods);

            GoldOre = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("gold_ore"));
            InitializeTradeGood(GoldOre,
                new TextObject("{=E6sgXO5n}Gold Ore{@Plural}sacks of gold ore{\\@}"), "goldore",
                BKItemCategories.Instance.Gold, 400, 10f, ItemObject.ItemTypeEnum.Goods);

            GoldIngot = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("goldingot"));
            InitializeTradeGood(GoldIngot,
                new TextObject("{=ak0txv88}Gold Ingot{@Plural}stacks of gold ingots{\\@}"), "goldingot",
                BKItemCategories.Instance.Gold, 1000, 0.5f, ItemObject.ItemTypeEnum.Goods);

            Gems = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("pouchofgems"));
            InitializeTradeGood(Gems,
                new TextObject("{=SajCUfsW}Gems{@Plural}pouches of gems{\\@}"), "pouchofgems",
                BKItemCategories.Instance.Gems, 50000, 1f, ItemObject.ItemTypeEnum.Goods);

            Mead = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("mead"));
            InitializeTradeGood(Mead,
                new TextObject("{=KwR84Oew}Mead{@Plural}barrels of mead{\\@}"), "bd_barrel_a",
                 BKItemCategories.Instance.Mead, 120, 10f, ItemObject.ItemTypeEnum.Goods);

            Garum = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("garum"));
            InitializeTradeGood(Garum,
                new TextObject("{=skP17S9C}Garum{@Plural}amphorae of garum{\\@}"), "amphora_slim",
                BKItemCategories.Instance.Garum, 35, 10f, ItemObject.ItemTypeEnum.Goods);


            BookIrkBitig = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_irk_bitig"));
            InitializeTradeGood(BookIrkBitig,
                new TextObject("{=gWqmDXOC}Irk Bitig{@Plural}collection of Irk Bitig books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book,
                1000,
                1.5f,
                ItemObject.ItemTypeEnum.Goods);

            BookNakedFingers = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_naked_fingers"));
            InitializeTradeGood(BookNakedFingers,
                new TextObject("{=EGsKJpNS}Naked Fingers{@Plural}collection of Naked Fingers books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book,
                1000,
                1.5f,
                ItemObject.ItemTypeEnum.Goods);

            BookWestGemynt = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_west_gemynt"));
            InitializeTradeGood(BookWestGemynt,
                new TextObject("{=ia3kgNVe}West Gemynt{@Plural}collection of West Gemynt books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book,
                1000,
                1.5f,
                ItemObject.ItemTypeEnum.Goods);

            BookFabulaeAquilae = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_fabulae_aquilae"));
            InitializeTradeGood(BookFabulaeAquilae,
                new TextObject("{=3BY8K3S5}Fabulae Aquilae{@Plural}collection of Fabulae Aquilae books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book,
                1000,
                1.5f,
                ItemObject.ItemTypeEnum.Goods);

            BookSkullsEleftheroi = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_skulls_eleftheroi"));
            InitializeTradeGood(BookSkullsEleftheroi,
                new TextObject("{=c0sJc9o9}Skulls of the Eleftheroi{@Plural}collection of Skulls of the Eleftheroi books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book,
                1000,
                1.5f,
                ItemObject.ItemTypeEnum.Goods);

            BookLoveCastle = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_love_castle"));
            InitializeTradeGood(BookLoveCastle,
                new TextObject("{=mnCoKY5H}The Storming of the Castle of Love{@Plural}collection of The Storming of the Castle of Love books{\\@}"),
                "lib_book_closed_a",
                BKItemCategories.Instance.Book,
                750,
                1f,
                ItemObject.ItemTypeEnum.Goods);

            BookGardenArgument = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_garden_argument"));
            InitializeTradeGood(BookGardenArgument,
                new TextObject("{=gCQCWHYT}An Argument in the Garden{@Plural}collection of An Argument in the Garden books{\\@}"),
                "lib_book_closed_a",
                BKItemCategories.Instance.Book,
                750,
                1f,
                ItemObject.ItemTypeEnum.Goods);

            BookHelgeredKara = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_helgered_kara"));
            InitializeTradeGood(BookHelgeredKara,
                new TextObject("{=BMtB9P3L}Helgerad and Kara{@Plural}collection of Helgerad and Kara books{\\@}"),
                "lib_book_closed_a",
                BKItemCategories.Instance.Book,
                750,
                1f,
                ItemObject.ItemTypeEnum.Goods);

            BookKaisLayala = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_kais_layala"));
            InitializeTradeGood(BookKaisLayala,
                new TextObject("{=zwsT7Dzj}Kais and Layala{@Plural}collection of Kais and Layala books{\\@}"),
                "lib_book_closed_a",
                BKItemCategories.Instance.Book,
                750,
                1f,
                ItemObject.ItemTypeEnum.Goods);

            BookHeartsDesire = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_hearts_desire"));
            InitializeTradeGood(BookHeartsDesire,
                new TextObject("{=hFV7jxHj}Heart's Desire{@Plural}collection of Heart's Desire books{\\@}"), "lib_book_closed_a",
                BKItemCategories.Instance.Book, 750, 1f, ItemObject.ItemTypeEnum.Goods);

            BookSiege = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_siege"));
            InitializeTradeGood(BookSiege,
                new TextObject("{=JBn6T1dd}Parangelmata Poliorcetica{@Plural}collection of Parangelmata Poliorcetica books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookStrategikon = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_tactics"));
            InitializeTradeGood(BookStrategikon,
                new TextObject("{=jbbauY27}Strategikon{@Plural}collection of Strategikon books{\\@}"), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookLeadership = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_leadership"));
            InitializeTradeGood(BookLeadership,
                new TextObject("{=dOtwMJwJ}De Re Militari{@Plural}collection of De Re Militari books{\\@}"), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookTrade = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_trade"));
            InitializeTradeGood(BookTrade,
                new TextObject("{=9V3FBo9c}A Treatise on the Value of Things{@Plural}collection of A Treatise on the Value of Things books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookArtHorsemanship = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_mounted"));
            InitializeTradeGood(BookArtHorsemanship,
                new TextObject("{=MRRoTZGw}The Art of Horsemanship{@Plural}collection of The Art of Horsemanship books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookGreenKnight = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_green_knight"));
            InitializeTradeGood(BookGreenKnight,
                new TextObject("{=SRUPiSKM}The Green Knight{@Plural}collection of The Green Knight books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 750, 1f, ItemObject.ItemTypeEnum.Goods);

            BookDictionary = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_dictionary"));
            InitializeTradeGood(BookDictionary,
                new TextObject("{=c44evtZJ}Dictionarium Calradium{@Plural}collection of Dictionarium Calradium books{\\@}"),
                "lib_book_closed_c",
                BKItemCategories.Instance.Book, 1000, 2.5f, ItemObject.ItemTypeEnum.Goods);

            BookOneHanded = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_oneHanded"));
            InitializeTradeGood(BookOneHanded,
                new TextObject("{=OYeWEWXA}Royal Armouries Ms. I.33{@Plural}collection of Royal Armouries Ms. I.33{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookTwoHanded = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_twoHanded"));
            InitializeTradeGood(BookTwoHanded,
                new TextObject("{=Pd777Yb5}Rìghfénnid{@Plural}collection of Rìghfénnid{\\@}"), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookCrossbow = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_crossbow"));
            InitializeTradeGood(BookCrossbow,
                new TextObject("{=b9HdDnYL}Origin and Mechanics of the Crossbow{@Plural}collection of Origin and Mechanics of the Crossbow{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookBow = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_bow"));
            InitializeTradeGood(BookBow,
                new TextObject("{=DgTSOSYQ}The History of Calradian Archery{@Plural}collection of The History of Calradian Archery{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookPolearm = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_polearm"));
            InitializeTradeGood(BookPolearm,
                new TextObject("{=5v2oKnX6}Lycaron debate of 1074{@Plural}collection of Lycaron debate of 1074y{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookThrowing = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_throwing"));
            InitializeTradeGood(BookThrowing,
                new TextObject("{=wNr4YQOo}Franceska{@Plural}collection of Franceska{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookMedicine = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_medicine"));
            InitializeTradeGood(BookMedicine,
                new TextObject("{=w7yf8SAj}Aseran Papyrus{@Plural}collection of Aseran Papyrus{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);
        }

        private static void InitializeTradeGood(ItemObject item, TextObject name, string meshName, ItemCategory category,
            int value, float weight, ItemObject.ItemTypeEnum itemType, bool isFood = false)
        {
            ItemObject.InitializeTradeGood(item, name, meshName, category, value, weight, itemType, isFood);
        }
    }
}