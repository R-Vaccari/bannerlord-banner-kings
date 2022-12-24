using TaleWorlds.Core;

namespace BannerKings.Extensions
{
    public static class ItemExtensions
    {
        public static bool IsMineral(this ItemObject item)
        {
            return item.StringId is "clay" or "iron" or "salt" or "silver" or "goldore";
        }

        public static bool CanBeConsumedAsFood(this ItemObject item)
        {
            return item.ItemCategory.Properties == ItemCategory.Property.BonusToFoodStores || 
                (item.HasHorseComponent && item.HorseComponent.IsLiveStock);
        }

        public static int GetItemFoodValue(this ItemObject item)
        {
            int result = 0;
            if (item.ItemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
            {
                result = 1;
            }

            if (item.HasHorseComponent && item.HorseComponent.IsLiveStock)
            {
                result = item.HorseComponent.MeatCount;
            }

            return result;
        }
    }
}
