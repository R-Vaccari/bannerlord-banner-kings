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

        public ItemObject Apple { get; private set; }

        public ItemObject Bread { get; private set; }

        public ItemObject Pie { get; private set; }

        public ItemObject Carrot { get; private set; }

        public ItemObject Orange { get; private set; }

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
                yield return Apple;
            }
        }

        public override void Initialize()
        {
            Apple = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("apple"));
            InitializeTradeGood(Apple,
                new TextObject("{=bk_item_apple}Apples{@Plural}baskets of apples\\@}"), "foods_basket_apple",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Orange = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("orange"));
            InitializeTradeGood(Orange,
                new TextObject("{=bk_item_orange}Oranges{@Plural}baskets of oranges\\@}"), "foods_orange_basket",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Bread = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("bread"));
            InitializeTradeGood(Bread,
                new TextObject("{=bk_item_bread}Bread{@Plural}loathes of bread\\@}"), "merchandise_bread",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Pie = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("pie"));
            InitializeTradeGood(Pie,
                new TextObject("{=bk_item_pie}Pie{@Plural}baskets of pies\\@}"), "kitchen_pie",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            Carrot = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("carrot"));
            InitializeTradeGood(Carrot,
                new TextObject("{=bk_item_carrot}Carrots{@Plural}baskets of carrots\\@}"), "foods_carrots_basket",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);


            BookHeartsDesire = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_hearts_desire"));
            InitializeTradeGood(BookHeartsDesire,
                new TextObject("{=Gu29w8Qd}Heart's Desire{@Plural}collection of Heart's Desire books{\\@}"), "lib_book_closed_a",
                BKItemCategories.Instance.Book, 300000, 1f, ItemObject.ItemTypeEnum.Goods);

            BookSiege = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_siege"));
            InitializeTradeGood(BookSiege,
                new TextObject("{=2COPRNjY}Parangelmata Poliorcetica{@Plural}collection of Parangelmata Poliorcetica books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookStrategikon = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_tactics"));
            InitializeTradeGood(BookStrategikon,
                new TextObject("{=dK5yqkEx}Strategikon{@Plural}collection of Strategikon books{\\@}"), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookLeadership = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_leadership"));
            InitializeTradeGood(BookLeadership,
                new TextObject("{=8sgK3eGZ}De Re Militari{@Plural}collection of De Re Militari books{\\@}"), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookTrade = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_trade"));
            InitializeTradeGood(BookTrade,
                new TextObject(
                    "{=BJ0eaFHn}A Treatise on the Value of Things{@Plural}collection of A Treatise on the Value of Things books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookMounted = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_mounted"));
            InitializeTradeGood(BookMounted,
                new TextObject("{=1NqPsYG5}The Green Knight{@Plural}collection of The Green Knight books{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookDictionary = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_dictionary"));
            InitializeTradeGood(BookDictionary,
                new TextObject("{=zABnZ6Ff}Dictionarium Calradium{@Plural}collection of Dictionarium Calradium books{\\@}"),
                "lib_book_closed_c",
                BKItemCategories.Instance.Book, 500000, 2.5f, ItemObject.ItemTypeEnum.Goods);

            BookOneHanded = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_oneHanded"));
            InitializeTradeGood(BookOneHanded,
                new TextObject("{=qFADbFjX}Royal Armouries Ms. I.33{@Plural}collection of Royal Armouries Ms. I.33{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookTwoHanded = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_twoHanded"));
            InitializeTradeGood(BookTwoHanded,
                new TextObject("{=6yNJWF7X}Rìghfénnid{@Plural}collection of Rìghfénnid{\\@}"), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookCrossbow = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_crossbow"));
            InitializeTradeGood(BookCrossbow,
                new TextObject(
                    "{=BS7c5zFk}Origin and Mechanics of the Crossbow{@Plural}collection of Origin and Mechanics of the Crossbow{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookBow = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_bow"));
            InitializeTradeGood(BookBow,
                new TextObject(
                    "{=mkV3cdAF}The History of Calradian Archery{@Plural}collection of The History of Calradian Archery{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            BookPolearm = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_polearm"));
            InitializeTradeGood(BookPolearm,
                new TextObject("{=CUyVEELv}Lycaron debate of 1074{@Plural}collection of Lycaron debate of 1074y{\\@}"),
                "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);
        }

        private static void InitializeTradeGood(ItemObject item, TextObject name, string meshName, ItemCategory category,
            int value, float weight, ItemObject.ItemTypeEnum itemType, bool isFood = false)
        {
            ItemObject.InitializeTradeGood(item, name, meshName, category, value, weight, itemType, isFood);
        }
    }
}