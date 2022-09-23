using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.TownManagement
{
    public class MaterialItemVM : ViewModel
    {
        private HintViewModel hint;
        private int resourceChange, resourceAmount;
        private ImageIdentifierVM visual;

        public MaterialItemVM(ItemObject material, Settlement settlement)
        {
            Material = material;
            Visual = new ImageIdentifierVM(material);
            foreach (ItemRosterElement element in settlement.ItemRoster)
            {
                if (element.EquipmentElement.Item == material)
                {
                    ResourceAmount += element.Amount;
                }
            }
            ResourceHint = new HintViewModel(material.Name);
        }

        public ItemObject Material { get; }

        [DataSourceProperty]
        public HintViewModel ResourceHint
        {
            get => hint;
            set
            {
                if (value != hint)
                {
                    hint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public int ResourceAmount
        {
            get => resourceAmount;
            set
            {
                if (value != resourceAmount)
                {
                    resourceAmount = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public int ResourceChangeAmount
        {
            get => resourceChange;

            set
            {
                if (value != resourceChange)
                {
                    resourceChange = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Visual
        {
            get => visual;
            set
            {
                if (value != visual)
                {
                    visual = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}