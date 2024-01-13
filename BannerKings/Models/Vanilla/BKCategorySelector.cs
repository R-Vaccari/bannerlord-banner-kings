using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKCategorySelector : DefaultItemCategorySelector
    {
        public override ItemCategory GetItemCategoryForItem(ItemObject itemObject)
        {
            ItemCategory result =  base.GetItemCategoryForItem(itemObject);

            if (result == DefaultItemCategories.Horse)
            {
                if (itemObject.Tier == ItemObject.ItemTiers.Tier6)
                {
                    result = DefaultItemCategories.NobleHorse;
                }
                if (itemObject.Tier == ItemObject.ItemTiers.Tier5)
                {
                    result = DefaultItemCategories.WarHorse;
                }
            }

            if (result == DefaultItemCategories.HorseEquipment)
            {
                if (itemObject.Tier == ItemObject.ItemTiers.Tier6)
                {
                    result = DefaultItemCategories.HorseEquipment5;
                }
                else if(itemObject.Tier == ItemObject.ItemTiers.Tier5)
                {
                    result = DefaultItemCategories.HorseEquipment5;
                }
                else if(itemObject.Tier == ItemObject.ItemTiers.Tier4)
                {
                    result = DefaultItemCategories.HorseEquipment4;
                }
                else if(itemObject.Tier == ItemObject.ItemTiers.Tier3)
                {
                    result = DefaultItemCategories.HorseEquipment3;
                } 
                else if (itemObject.Tier != ItemObject.ItemTiers.Tier2)
                {
                    result = DefaultItemCategories.HorseEquipment;
                }
                else result = DefaultItemCategories.HorseEquipment2;
            }

            return result;
        }
    }
}
