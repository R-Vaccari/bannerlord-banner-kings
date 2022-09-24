using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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
            ResourceHint = new HintViewModel(new TextObject("{=!}{MATERIAL}\n{DESCRIPTION}\nStash: {STASH}\nMarket: {MARKET}")
                .SetTextVariable("MATERIAL", material.Name)
                .SetTextVariable("STASH", stash)
                .SetTextVariable("MARKET", market)
                .SetTextVariable("DESCRIPTION", GameTexts.FindText("str_bk_description", material.StringId)));
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