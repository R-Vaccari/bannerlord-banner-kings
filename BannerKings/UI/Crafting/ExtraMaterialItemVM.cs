using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Crafting
{
    public class ExtraMaterialItemVM : ViewModel
    {
        private ItemObject material;
		private ImageIdentifierVM visual;
		private int resourceChange, resourceAmount;
		private HintViewModel hint;

		public ExtraMaterialItemVM(ItemObject material)
        {
            this.material = material;
			Visual = new ImageIdentifierVM(material, "");
			ResourceAmount = PartyBase.MainParty.ItemRoster.GetItemNumber(material);
			ResourceHint = new HintViewModel(material.Name, null);
		}

		public ItemObject Material => material;

		[DataSourceProperty]
		public HintViewModel ResourceHint
		{
			get => hint;
			set
			{
				if (value != hint)
				{
					hint = value;
					OnPropertyChangedWithValue(value, "ResourceHint");
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
					OnPropertyChangedWithValue(value, "ResourceAmount");
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
					OnPropertyChangedWithValue(value, "ResourceChangeAmount");
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
					OnPropertyChangedWithValue(value, "Visual");
				}
			}
		}
	}
}
