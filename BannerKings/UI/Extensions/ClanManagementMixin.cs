using BannerKings.UI.Court;
using BannerKings.UI.Education;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("SetSelectedCategory")]
    internal class ClanManagementMixin : BaseViewModelMixin<ClanManagementVM>
    {
		//private BasicTooltipViewModel pietyHint;
		private ClanManagementVM clanManagement;
		private ClanCourtVM courtVM;
		private bool courtSelected;

		public ClanManagementMixin(ClanManagementVM vm) : base(vm)
        {
			clanManagement = vm;
			courtVM = new ClanCourtVM();
		}

        public override void OnRefresh()
        {
			courtVM.RefreshValues();
			if (clanManagement.IsMembersSelected || clanManagement.IsPartiesSelected || clanManagement.IsFiefsSelected || clanManagement.IsIncomeSelected)
            {
				Court.IsSelected = false;
				CourtSelected = false;
			}
		}

		[DataSourceProperty]
		public string CourtText => new TextObject("{=!}Court").ToString();

		[DataSourceMethod]
		public void SelectCourt()
        {
			clanManagement.ClanMembers.IsSelected = false;
			clanManagement.ClanParties.IsSelected = false;
			clanManagement.ClanFiefs.IsSelected = false;
			clanManagement.ClanIncome.IsSelected = false;

			clanManagement.IsMembersSelected = false;
			clanManagement.IsPartiesSelected = false;
			clanManagement.IsFiefsSelected = false;
			clanManagement.IsIncomeSelected = false;
			Court.IsSelected = true;
			CourtSelected = true;
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
		public ClanCourtVM Court
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
