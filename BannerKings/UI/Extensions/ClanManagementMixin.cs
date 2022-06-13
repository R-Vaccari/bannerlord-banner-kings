using BannerKings.UI.Education;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class ClanManagementMixin : BaseViewModelMixin<ClanManagementVM>
    {
		//private BasicTooltipViewModel pietyHint;
		private ClanManagementVM characterDeveloper;
		private EducationVM educationVM;
		private bool visible;
		private string educationText;
		public ClanManagementMixin(ClanManagementVM vm) : base(vm)
        {
			EducationVisible = false;
			characterDeveloper = vm;
		}

        public override void OnRefresh()
        {

		}

		[DataSourceProperty]
		public string CourtText => new TextObject("{=!}Court").ToString();

		[DataSourceMethod]
		public void SelectCourt()
        {

        }



		[DataSourceProperty]
		public EducationVM Education
		{
			get => educationVM;
			set
			{
				if (value != educationVM)
				{
					educationVM = value;
					ViewModel!.OnPropertyChangedWithValue(value, "Education");
				}
			}
		}

		[DataSourceProperty]
		public bool EducationVisible
        {
			get => visible;
			set
			{
				if (value != visible)
				{
					visible = value;
					ViewModel!.OnPropertyChangedWithValue(value, "EducationVisible");
				}
			}
		}


	}
}
