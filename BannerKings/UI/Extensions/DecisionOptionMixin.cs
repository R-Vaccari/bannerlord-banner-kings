using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class DecisionOptionMixin : BaseViewModelMixin<DecisionOptionVM>
    {
        private BasicTooltipViewModel hint;
        public DecisionOptionMixin(DecisionOptionVM vm) : base(vm)
        {
        }

        [DataSourceProperty] public string DemesneText => new TextObject("{=6QMDGRSt}Demesne").ToString();  

        [DataSourceProperty]
        public BasicTooltipViewModel Hint
        {
            get => hint;
            set
            {
                if (value != hint)
                {
                    hint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            Hint = new BasicTooltipViewModel(() => UIHelper.GetDecisionOptionTooltip(ViewModel.Decision,
                ViewModel.Option));
        }
    }
}