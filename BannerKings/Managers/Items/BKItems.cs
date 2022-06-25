using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Items
{
    public class BKItems : DefaultTypeInitializer<BKItems>
    {
        private ItemObject apple, orange, bread, pie, carrot, bookHeartsDesire, bookSiege, bookStrategikon,
            bookLeadership, bookTrade, bookDictionary, bookMounted;

        public ItemObject BookHeartsDesire => bookHeartsDesire;
        public ItemObject BookSiege => bookSiege;
        public ItemObject BookStrategikon => bookStrategikon;
        public ItemObject BookLeadership => bookLeadership;
        public ItemObject BookTrade => bookTrade;
        public ItemObject BookDictionary => bookDictionary;
        public ItemObject BookMounted => bookMounted;

        public ItemObject Apple => apple;

        public ItemObject Bread => bread;

        public ItemObject Pie => pie;

        public ItemObject Carrot => carrot;

        public ItemObject Orange => orange;

        public override void Initialize()
        {
            apple = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("apple"));
            InitializeTradeGood(apple,
                new TextObject("{=bk_item_apple}Apples{@Plural}baskets of apples\\@}", null), "foods_basket_apple",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            orange = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("orange"));
            InitializeTradeGood(orange,
                new TextObject("{=bk_item_orange}Oranges{@Plural}baskets of oranges\\@}", null), "foods_orange_basket",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            bread = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("bread"));
            InitializeTradeGood(bread,
                new TextObject("{=bk_item_bread}Bread{@Plural}loathes of bread\\@}", null), "merchandise_bread",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            pie = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("pie"));
            InitializeTradeGood(pie,
                new TextObject("{=bk_item_pie}Pie{@Plural}baskets of pies\\@}", null), "kitchen_pie",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);

            carrot = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("carrot"));
            InitializeTradeGood(carrot,
                new TextObject("{=bk_item_carrot}Carrots{@Plural}baskets of carrots\\@}", null), "foods_carrots_basket",
                BKItemCategories.Instance.Apple, 5, 10f, ItemObject.ItemTypeEnum.Goods, true);


            bookHeartsDesire = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_hearts_desire"));
            InitializeTradeGood(bookHeartsDesire,
                new TextObject("{=!}Heart's Desire{@Plural}collection of Heart's Desire books{\\@}", null), "lib_book_closed_a",
                BKItemCategories.Instance.Book, 300000, 1f, ItemObject.ItemTypeEnum.Goods);

            bookSiege = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_siege"));
            InitializeTradeGood(bookSiege,
                new TextObject("{=!}Parangelmata Poliorcetica{@Plural}collection of Parangelmata Poliorcetica books{\\@}", null), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookStrategikon = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_tactics"));
            InitializeTradeGood(bookStrategikon,
                new TextObject("{=!}Strategikon{@Plural}collection of Strategikon books{\\@}", null), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookLeadership = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_leadership"));
            InitializeTradeGood(bookLeadership,
                new TextObject("{=!}De Re Militari{@Plural}collection of De Re Militari books{\\@}", null), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookTrade = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_trade"));
            InitializeTradeGood(bookTrade,
                new TextObject("{=!}A Treatise on the Value of Things{@Plural}collection of A Treatise on the Value of Things books{\\@}", null), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookMounted = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_mounted"));
            InitializeTradeGood(bookMounted,
                new TextObject("{=!}The Green Knight{@Plural}collection of The Green Knight books{\\@}", null), "lib_book_closed_b",
                BKItemCategories.Instance.Book, 700000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookDictionary = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_dictionary"));
            InitializeTradeGood(bookDictionary,
                new TextObject("{=!}Dictionarium Calradium{@Plural}collection of Dictionarium Calradium books{\\@}", null), "lib_book_closed_c",
                BKItemCategories.Instance.Book, 500000, 2.5f, ItemObject.ItemTypeEnum.Goods);
        }

        static void InitializeTradeGood(ItemObject item, TextObject name, string meshName, ItemCategory category, int value, float weight, ItemObject.ItemTypeEnum itemType, bool isFood = false)
        {
            MethodInfo method = item.GetType().GetMethod("InitializeTradeGood", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, new object[] { item, name, meshName, category, value, weight, itemType, isFood });
        }
    }
}