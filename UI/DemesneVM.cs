using Populations.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static Populations.Managers.TitleManager;

namespace Populations.UI
{
    public class DemesneVM : ViewModel
    {
		private HeroVM _deJure;
		private bool _isSelected;
		private MBBindingList<VassalTitleVM> _vassals;
		private FeudalTitle _title;

		public DemesneVM(FeudalTitle title, bool isSelected)
        {
			this._title = title;
			this._deJure = new HeroVM(title.deJure, false);
			this._isSelected = isSelected;
			this._vassals = new MBBindingList<VassalTitleVM>();
			if (title.vassals != null)
				foreach (FeudalTitle vassal in title.vassals)
					if (vassal.fief != null) _vassals.Add(new VassalTitleVM(vassal));
		}

		public override void RefreshValues()
        {
            base.RefreshValues();
        }

		private void OnContractPress()
        {
			Kingdom kingdom = _title.fief.OwnerClan.Kingdom;
			if (kingdom != null)
				PopulationConfig.Instance.TitleManager.ShowContract(kingdom.Leader, "Close");
			else InformationManager.DisplayMessage(new InformationMessage("Unable to open contract: no kingdom associated with this title."));
		}

		[DataSourceProperty]
		public bool IsSelected
		{
			get =>this._isSelected;
			set
			{
				if (value != this._isSelected)
				{
					this._isSelected = value;
					if (value) this.RefreshValues();
					base.OnPropertyChangedWithValue(value, "IsSelected");
				}
			}
		}


		[DataSourceProperty]
		public HeroVM DeJure
		{
			get => this._deJure;
			set
			{
				if (value != this._deJure)
				{
					this._deJure = value;
					base.OnPropertyChangedWithValue(value, "DeJure");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<VassalTitleVM> Vassals
		{
			get => this._vassals;
			set
			{
				if (value != this._vassals)
				{
					this._vassals = value;
					base.OnPropertyChanged("Vassals");
				}
			}
		}
	}
}
