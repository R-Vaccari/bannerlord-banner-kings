using BannerKings.Managers.Institutions.Religions;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class CharacterDeveloperMixin : BaseViewModelMixin<CharacterDeveloperVM>
    {
		private BasicTooltipViewModel pietyHint;
		private int piety;
		private string educationText;
		public CharacterDeveloperMixin(CharacterDeveloperVM vm) : base(vm)
        {
        }

        public override void OnRefresh()
        {
			EducationText = new TextObject("{=!}Education").ToString();
			//if (rel == null) return;
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
