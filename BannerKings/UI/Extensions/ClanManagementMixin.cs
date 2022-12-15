using BannerKings.UI.Court;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
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
        private bool courtSelected, courtEnabled, demesneSelected, demesneEnabled, financesVisible;
        private CourtVM courtVM;

        public ClanManagementMixin(ClanManagementVM vm) : base(vm)
        {
            clanManagement = vm;
            courtVM = new CourtVM(false);
            FinancesVisible = true;

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
            CourtEnabled = false;
            if (council != null && council.Peerage != null)
            {
                CourtEnabled = council.Peerage.CanHaveCouncil;
            }
        }

        [DataSourceProperty] public string CourtText => new TextObject("{=2QGyA46m}Court").ToString();
        [DataSourceProperty] public string DemesneText => new TextObject("{=6QMDGRSt}Demesne").ToString();

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
        public bool DemesneEnabled
        {
            get => demesneEnabled;
            set
            {
                if (value != demesneEnabled)
                {
                    demesneEnabled = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
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
                    FinancesVisible = !value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CourtEnabled
        {
            get => courtEnabled;
            set
            {
                if (value != courtEnabled)
                {
                    courtEnabled = value;
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

            DemesneSelected = false;

            Court.IsSelected = true;
            CourtSelected = true;
        }

        [DataSourceMethod]
        public void SelectDemesne()
        {
            clanManagement.ClanMembers.IsSelected = false;
            clanManagement.ClanParties.IsSelected = false;
            clanManagement.ClanFiefs.IsSelected = false;
            clanManagement.ClanIncome.IsSelected = false;

            clanManagement.IsMembersSelected = false;
            clanManagement.IsPartiesSelected = false;
            clanManagement.IsFiefsSelected = false;
            clanManagement.IsIncomeSelected = false;
            Court.IsSelected = false;
            CourtSelected = false;

            DemesneSelected = true;
        }
    }
}