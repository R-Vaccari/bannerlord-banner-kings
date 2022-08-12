using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.UI.Crafting
{
    public class ArmorCraftingVM : ViewModel
    {

        private MBBindingList<ArmorItemVM> armors;

        public ArmorCraftingVM()
        {
            armors = new MBBindingList<ArmorItemVM>();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Armors.Clear();

            foreach (ItemObject item in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
            {
                if (!item.HasArmorComponent) continue;
                Armors.Add(new ArmorItemVM(item));
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
    }
}
