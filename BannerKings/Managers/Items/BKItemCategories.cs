using TaleWorlds.Core;

namespace BannerKings.Managers.Items
{
    public class BKItemCategories : DefaultTypeInitializer<BKItemCategories>
    {
        private ItemCategory book, apple, orange, bread, pie, carrot;

        public ItemCategory Book => book;
        public ItemCategory Apple => apple;
        public ItemCategory Orange => orange;
        public ItemCategory Bread => bread;
        public ItemCategory Pie => pie;
        public ItemCategory Carrot => carrot;
        public override void Initialize()
        {
            book = Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory("book"));
            book.InitializeObject(false, 0, 0, ItemCategory.Property.None, null, 0f, false, true);

            apple = Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory("apple"));
            apple.InitializeObject(true, 10, 0, ItemCategory.Property.BonusToFoodStores, null, 0f, false, true);

            orange = Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory("orange"));
            orange.InitializeObject(true, 10, 0, ItemCategory.Property.BonusToFoodStores, null, 0f, false, true);

            bread = Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory("bread"));
            bread.InitializeObject(true, 50, 5, ItemCategory.Property.BonusToFoodStores, null, 0f, false, true);

            pie = Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory("pie"));
            pie.InitializeObject(true, 20, 10, ItemCategory.Property.BonusToFoodStores, null, 0f, false, true);

            carrot = Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory("carrot"));
            carrot.InitializeObject(true, 10, 0, ItemCategory.Property.BonusToFoodStores, null, 0f, false, true);
        }
    }
}