using BannerKings.UI.Education;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class CharacterDeveloperMixin : BaseViewModelMixin<CharacterDeveloperVM>
    {
		//private BasicTooltipViewModel pietyHint;
		private CharacterDeveloperVM characterDeveloper;
		private EducationVM educationVM;
		private bool visible;
		private string educationText;
		public CharacterDeveloperMixin(CharacterDeveloperVM vm) : base(vm)
        {
			EducationVisible = false;
			characterDeveloper = vm;
		}

        public override void OnRefresh()
        {
			EducationText = new TextObject("{=!}Education").ToString();
			Education = new EducationVM(characterDeveloper.CurrentCharacter.Hero, characterDeveloper);
			Education.RefreshValues();
		}

		[DataSourceMethod]
		public void OpenEducation()
        {
			EducationVisible = true;
			OnRefresh();
        }

		[DataSourceMethod]
		public void CloseEducation()
		{
			EducationVisible = false;
			OnRefresh();
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

		[DataSourceProperty]
		public string EducationText
		{
			get => educationText;
			set
			{
				if (value != this.educationText)
				{
					this.educationText = value;
					ViewModel!.OnPropertyChangedWithValue(value, "EducationText");
				}
			}
		}
	}
}
