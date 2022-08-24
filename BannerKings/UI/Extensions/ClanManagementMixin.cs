using BannerKings.UI.Court;
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
        private readonly ClanManagementVM clanManagement;
        private bool courtSelected, financesVisible;
        private CourtVM courtVM;

        public ClanManagementMixin(ClanManagementVM vm) : base(vm)
        {
            clanManagement = vm;
            courtVM = new CourtVM(false);
            FinancesVisible = true;
        }

        [DataSourceProperty] public string CourtText => new TextObject("{=fN9SroDMB}Court").ToString();

        [DataSourceProperty]
        public bool FinancesVisible
        {
            get => financesVisible;
            set
            {
                if (value != financesVisible)
                {
                    financesVisible = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
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
                    FinancesVisible = !value;
                    ViewModel!.OnPropertyChangedWithValue(value);
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
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            courtVM.RefreshValues();
            if (clanManagement.IsMembersSelected || clanManagement.IsPartiesSelected || clanManagement.IsFiefsSelected ||
                clanManagement.IsIncomeSelected)
            {
                Court.IsSelected = false;
                CourtSelected = false;
            }
        }

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
    }
}