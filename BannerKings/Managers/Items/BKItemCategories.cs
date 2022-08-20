using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace BannerKings.Managers.Items
{
    public class BKItemCategories : DefaultTypeInitializer<BKItemCategories, ItemCategory>
    {
        public ItemCategory Book { get; private set; }

        public ItemCategory Apple { get; private set; }

        public ItemCategory Orange { get; private set; }

        public ItemCategory Bread { get; private set; }

        public ItemCategory Pie { get; private set; }

        public ItemCategory Carrot { get; private set; }

        public override IEnumerable<ItemCategory> All => throw new NotImplementedException();

        public override void Initialize()
        {
            Book = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("book"));
            Book.InitializeObject();

            Apple = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("apple"));
            Apple.InitializeObject(true, 10, 0, ItemCategory.Property.BonusToFoodStores);

            Orange = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("orange"));
            Orange.InitializeObject(true, 10, 0, ItemCategory.Property.BonusToFoodStores);

            Bread = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("bread"));
            Bread.InitializeObject(true, 50, 5, ItemCategory.Property.BonusToFoodStores);

            Pie = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("pie"));
            Pie.InitializeObject(true, 20, 10, ItemCategory.Property.BonusToFoodStores);

            Carrot = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("carrot"));
            Carrot.InitializeObject(true, 10, 0, ItemCategory.Property.BonusToFoodStores);
        }
    }
}