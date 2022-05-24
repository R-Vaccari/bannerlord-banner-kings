using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Items
{
    public class BKItems : DefaultTypeInitializer<BKItems>
    {
        private ItemObject bookHeartsDesire, bookSiege, bookStrategikon,
            bookLeadership, bookTrade, bookDictionary, bookMounted;
        
        public override void Initialize()
        {
            bookHeartsDesire = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_hearts_desire"));
            InitializeTradeGood(bookHeartsDesire, 
                new TextObject("{=!}Heart's Desire{@Plural}collection of Heart's Desire books{\\@}", null), "lib_book_closed_a", 
                BKItemCategories.Instance.Book, 30000, 1f, ItemObject.ItemTypeEnum.Goods);

            bookSiege = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_siege"));
            InitializeTradeGood(bookSiege,
                new TextObject("{=!}Parangelmata Poliorcetica{@Plural}collection of Parangelmata Poliorcetica books{\\@}", null), "book_f",
                BKItemCategories.Instance.Book, 70000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookStrategikon = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_tactics"));
            InitializeTradeGood(bookStrategikon,
                new TextObject("{=!}Strategikon{@Plural}collection of Strategikon books{\\@}", null), "book_f",
                BKItemCategories.Instance.Book, 70000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookLeadership = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_leadership"));
            InitializeTradeGood(bookLeadership,
                new TextObject("{=!}De Re Militari{@Plural}collection of De Re Militari books{\\@}", null), "book_f",
                BKItemCategories.Instance.Book, 70000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookTrade = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_trade"));
            InitializeTradeGood(bookTrade,
                new TextObject("{=!}A Treatise on the Value of Things{@Plural}collection of A Treatise on the Value of Things books{\\@}", null), "book_f",
                BKItemCategories.Instance.Book, 70000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookMounted = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_mounted"));
            InitializeTradeGood(bookMounted,
                new TextObject("{=!}The Green Knight{@Plural}collection of The Green Knight books{\\@}", null), "book_f",
                BKItemCategories.Instance.Book, 70000, 1.5f, ItemObject.ItemTypeEnum.Goods);

            bookDictionary = Game.Current.ObjectManager.RegisterPresumedObject(new ItemObject("book_dictionary"));
            InitializeTradeGood(bookDictionary,
                new TextObject("{=!}Dictionarium Calradium{@Plural}collection of Dictionarium Calradium books{\\@}", null), "book_f",
                BKItemCategories.Instance.Book, 50000, 2.5f, ItemObject.ItemTypeEnum.Goods);
        }

        static void InitializeTradeGood(ItemObject item, TextObject name, string meshName, ItemCategory category, int value, float weight, ItemObject.ItemTypeEnum itemType, bool isFood = false)
		{
			MethodInfo method = item.GetType().GetMethod("InitializeTradeGood", BindingFlags.Static | BindingFlags.NonPublic);
			method.Invoke(null, new object[] { item, name, meshName, category, value, weight, itemType, isFood });
		}
	}
}
