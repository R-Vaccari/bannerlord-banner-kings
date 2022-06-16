using BannerKings.UI.Court;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("SetSelectedCategory")]
    internal class KingdomManagementMixin : BaseViewModelMixin<KingdomManagementVM>
    {
		//private BasicTooltipViewModel pietyHint;
		private KingdomManagementVM kingdomManagement;
		private CourtVM courtVM;
		private bool courtSelected, demesneSelected;

		public KingdomManagementMixin(KingdomManagementVM vm) : base(vm)
        {
			kingdomManagement = vm;
			courtVM = new CourtVM(true);
		}

        public override void OnRefresh()
        {
			courtVM.RefreshValues();
			if (kingdomManagement.Clan.Show || kingdomManagement.Settlement.Show || kingdomManagement.Policy.Show || kingdomManagement.Army.Show || kingdomManagement.Diplomacy.Show)
            {
				Court.IsSelected = false;
				CourtSelected = false;
				DemesneSelected = false;
			}
		}

		[DataSourceProperty]
		public string DemesneText => new TextObject("{=!}Demesne").ToString();

		[DataSourceProperty]
		public string CourtText => new TextObject("{=!}Court").ToString();

		[DataSourceMethod]
		public void SelectCourt()
        {
			kingdomManagement.Clan.Show = false;
			kingdomManagement.Settlement.Show = false;
			kingdomManagement.Policy.Show = false;
			kingdomManagement.Army.Show = false;
			kingdomManagement.Diplomacy.Show = false;

			DemesneSelected = false;
			Court.IsSelected = true;
			CourtSelected = true;
		}

		[DataSourceMethod]
		public void SelectDemesne()
		{
			kingdomManagement.Clan.Show = false;
			kingdomManagement.Settlement.Show = false;
			kingdomManagement.Policy.Show = false;
			kingdomManagement.Army.Show = false;
			kingdomManagement.Diplomacy.Show = false;

			DemesneSelected = true;
			Court.IsSelected = false;
			CourtSelected = false;
		}

		[DataSourceProperty]
		public bool DemesneSelected
		{
			get => demesneSelected;
			set
			{
				if (value != demesneSelected)
				{
					demesneSelected = value;
					ViewModel!.OnPropertyChangedWithValue(value, "DemesneSelected");
				}
			}
		}

		[DataSourceProperty]
		public bool CourtSelected
		{
			get => courtSelected;
			set
			{
				if (value != courtSelected)
				{
					courtSelected = value;
					ViewModel!.OnPropertyChangedWithValue(value, "CourtSelected");
				}
			}
		}

		[DataSourceProperty]
		public CourtVM Court
		{
			get => courtVM;
			set
			{
				if (value != courtVM)
				{
					courtVM = value;
					ViewModel!.OnPropertyChangedWithValue(value, "Court");
				}
			}
		}
	}
}
