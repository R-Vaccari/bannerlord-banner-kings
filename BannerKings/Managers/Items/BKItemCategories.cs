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
        public ItemCategory Honey { get; private set; }
        public ItemCategory Gold { get; private set; }
        public ItemCategory Limestone { get; private set; }
        public ItemCategory Marble { get; private set; }
        public ItemCategory Gems { get; private set; }
        public ItemCategory Mead { get; private set; }
        public ItemCategory Garum { get; private set; }
        public override IEnumerable<ItemCategory> All => throw new NotImplementedException();

        public override void Initialize()
        {
            Book = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("book"));
            Book.InitializeObject(false, 0, 0);

            Apple = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("apple"));
            Apple.InitializeObject(true, 20, 0, ItemCategory.Property.BonusToFoodStores);

            Orange = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("orange"));
            Orange.InitializeObject(true, 20, 0, ItemCategory.Property.BonusToFoodStores);

            Bread = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("bread"));
            Bread.InitializeObject(true, 140, 5, ItemCategory.Property.BonusToFoodStores);

            Pie = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("pie"));
            Pie.InitializeObject(true, 30, 40, ItemCategory.Property.BonusToFoodStores);

            Carrot = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("carrot"));
            Carrot.InitializeObject(true, 20, 0, ItemCategory.Property.BonusToFoodStores);

            Honey = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("honey"));
            Honey.InitializeObject(true, 5, 10, ItemCategory.Property.BonusToFoodStores);

            Mead = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("mead"));
            Mead.InitializeObject(true, 10, 5, ItemCategory.Property.BonusToFoodStores);

            Garum = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("garum"));
            Garum.InitializeObject(true, 10, 5, ItemCategory.Property.BonusToFoodStores);

            Gold = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("gold"));
            Gold.InitializeObject(true, 0, 0, ItemCategory.Property.BonusToTax);

            Limestone = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("limestone"));
            Limestone.InitializeObject(true, 0, 0, ItemCategory.Property.BonusToProduction);

            Marble = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("marble"));
            Marble.InitializeObject(true, 0, 0, ItemCategory.Property.BonusToProsperity);

            Gems = Game.Current.ObjectManager.RegisterPresumedObject(new ItemCategory("gems"));
            Gems.InitializeObject(true, 10, 50, ItemCategory.Property.None);
        }
    }
}