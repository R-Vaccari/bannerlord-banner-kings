using BannerKings.UI.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.UI.Crafting
{
    public class ArmorCraftingVM : ViewModel
    {
        private MBBindingList<ArmorItemVM> armors;
        private ArmorCraftingSortController sortController;
        private ArmorItemVM currentItem;
        private CraftingMixin mixin;

        public ArmorCraftingVM(CraftingMixin mixin)
        {
            this.mixin = mixin;
            armors = new MBBindingList<ArmorItemVM>();
            SortController = new ArmorCraftingSortController();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Armors.Clear();

            foreach (ItemObject item in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
            {
                if (!item.HasArmorComponent) continue;
                Armors.Add(new ArmorItemVM(this, item, GetItemType(item)));
            }

            SortController.SetListToControl(Armors);
            CurrentItem = Armors[0];
        }

        public ItemType GetItemType(ItemObject item)
        {
            if (item.HasArmorComponent)
            {
                if (item.ItemType == ItemObject.ItemTypeEnum.HorseHarness) return ItemType.Barding;
                else return ItemType.BodyArmor;
            }

            return ItemType.Ammo;
        }

        public Hero Hero => mixin.Hero;

        

        [DataSourceProperty]
        public ArmorCraftingSortController SortController
        {
            get => sortController;
            set
            {
                if (value != sortController)
                {
                    sortController = value;
                    OnPropertyChangedWithValue(value, "SortController");
                }
            }
        }

        [DataSourceProperty]
        public ArmorItemVM CurrentItem
        {
            get => currentItem;
            set
            {
                if (value != currentItem)
                {
                    currentItem = value;
                    OnPropertyChangedWithValue(value, "CurrentItem");
                    mixin.UpdateMaterials();
                    mixin.UpdateMainAction();
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ArmorItemVM> Armors
        {
            get => armors;
            set
            {
                if (value != armors)
                {
                    armors = value;
                    OnPropertyChangedWithValue(value, "Armors");
                }
            }
        }

        public enum ItemType
        {
            BodyArmor,
            Barding,
            Shield,
            Ammo
        }
    }
}
