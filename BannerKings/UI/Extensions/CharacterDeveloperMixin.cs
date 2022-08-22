using BannerKings.UI.Education;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class CharacterDeveloperMixin : BaseViewModelMixin<CharacterDeveloperVM>
    {
        //private BasicTooltipViewModel pietyHint;
        private readonly CharacterDeveloperVM characterDeveloper;
        private string educationText;
        private EducationVM educationVM;
        private bool visible;

        public CharacterDeveloperMixin(CharacterDeveloperVM vm) : base(vm)
        {
            EducationVisible = false;
            characterDeveloper = vm;
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
                    ViewModel!.OnPropertyChangedWithValue(value);
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
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EducationText => new TextObject("{=!}Education").ToString();

        [DataSourceProperty]
        public string DecisionsText => new TextObject("{=!}Decisions").ToString();

        [DataSourceProperty]
        public string FaithText => new TextObject("{=!}Faith").ToString();

        public override void OnRefresh()
        {
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
        public void OpenFaith()
        {
            EducationVisible = true;
            OnRefresh();
        }

        [DataSourceMethod]
        public void OpenDecisions()
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
    }
}