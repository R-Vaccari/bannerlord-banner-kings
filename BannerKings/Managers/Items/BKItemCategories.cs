using System;
using TaleWorlds.Core;

namespace BannerKings.Managers.Items
{
    public class BKItemCategories : DefaultTypeInitializer<BKItemCategories>
    {
        private ItemCategory book;

        public ItemCategory Book => book;
        public override void Initialize()
        {
            book = Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory("book"));
            book.InitializeObject(false, 0, 0, ItemCategory.Property.None, null, 0f, false, true);
        }
    }
}
