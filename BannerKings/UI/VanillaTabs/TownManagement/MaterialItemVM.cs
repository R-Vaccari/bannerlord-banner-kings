using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.VanillaTabs.TownManagement
{
    public class MaterialItemVM : ViewModel
    {
        private BasicTooltipViewModel hint;
        private int resourceChange, resourceAmount;
        private ImageIdentifierVM visual;

        public MaterialItemVM(ItemObject material, Settlement settlement, int demand)
        {
            Material = material;
            Visual = new ImageIdentifierVM(material);
            int stash = 0;
            int market = 0;
            foreach (ItemRosterElement element in settlement.Stash)
            {
                if (element.EquipmentElement.Item == material)
                {
                    ResourceAmount += element.Amount;
                    stash += element.Amount;
                }
            }

            foreach (ItemRosterElement element in settlement.ItemRoster)
            {
                if (element.EquipmentElement.Item == material)
                {
                    ResourceAmount += element.Amount;
                    market += element.Amount;
                }
            }
            MarketAmount = market;
            StashAmount = stash;
            ResourceChangeAmount = demand;
            ResourceHint = new BasicTooltipViewModel(() => UIHelper.GetTownMaterialTooltip(this));
        }

        public ItemObject Material { get; }
        public int StashAmount { get; }
        public int MarketAmount { get; }

        [DataSourceProperty]
        public BasicTooltipViewModel ResourceHint
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