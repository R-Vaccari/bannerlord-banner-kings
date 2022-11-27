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

        public ItemObject BookMounted { get; private set; }

        public ItemObject BookOneHanded { get; private set; }

        public ItemObject BookTwoHanded { get; private set; }

        public ItemObject BookPolearm { get; private set; }

        public ItemObject BookCrossbow { get; private set; }

        public ItemObject BookBow { get; private set; }

        public ItemObject BookThrowing { get; private set; }
        public ItemObject BookMedicine { get; private set; }

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
                yield return BookMounted;
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
            }
        }

        public override void Initialize()
        {
            Apple = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("apple"));
            InitializeTradeGood(Apple,
                new TextObject("{=1MAr0j2J}Apples{@Plural}baskets of apples\\@}"), "foods_basket_apple",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Orange = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("orange"));
            InitializeTradeGood(Orange,
                new TextObject("{=Krqwsins}Oranges{@Plural}baskets of oranges\\@}"), "foods_orange_basket",
                BKItemCategories.Instance.Orange, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Bread = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("bread"));
            InitializeTradeGood(Bread,
                new TextObject("{=VJfbLXmi}Bread{@Plural}loathes of bread\\@}"), "merchandise_bread",
                BKItemCategories.Instance.Bread, 20, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Pie = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("pie"));
            InitializeTradeGood(Pie,
                new TextObject("{=Qa0dQMOc}Pie{@Plural}baskets of pies\\@}"), "kitchen_pie",
                BKItemCategories.Instance.Pie, 30, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Carrot = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("carrot"));
            InitializeTradeGood(Carrot,
                new TextObject("{=C5Xe5MJK}Carrots{@Plural}baskets of carrots\\@}"), "foods_carrots_basket",
                BKItemCategories.Instance.Carrot, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

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

            BookMounted = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_mounted"));
            InitializeTradeGood(BookMounted,
                new TextObject("{=SRUPiSKM}The Green Knight{@Plural}collection of The Green Knight books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 1000, 1.5f, ItemObject.ItemTypeEnum.Goods);

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