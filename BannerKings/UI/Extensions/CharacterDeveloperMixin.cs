using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class CharacterDeveloperMixin : BaseViewModelMixin<CharacterDeveloperVM>
    {
		private BasicTooltipViewModel pietyHint;
		private bool visible;
		private string educationText;
		public CharacterDeveloperMixin(CharacterDeveloperVM vm) : base(vm)
        {
			EducationVisible = false;
		}

        public override void OnRefresh()
        {
			EducationText = new TextObject("{=!}Education").ToString();
			//if (rel == null) return;
		}

		public void OpenEducation()
        {
			EducationVisible = true;
			OnRefresh();
        }

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
