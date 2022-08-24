using BannerKings.UI.Court;
using BannerKings.UI.Titles;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("SetSelectedCategory")]
    internal class KingdomManagementMixin : BaseViewModelMixin<KingdomManagementVM>
    {
        private bool courtSelected, demesneSelected;
        private CourtVM courtVM;
        private DemesneHierarchyVM demesneVM;
        private readonly KingdomManagementVM kingdomManagement;

        public KingdomManagementMixin(KingdomManagementVM vm) : base(vm)
        {
            kingdomManagement = vm;
            courtVM = new CourtVM(true);
            demesneVM = new DemesneHierarchyVM(BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(vm.Kingdom),
                vm.Kingdom);
        }


        [DataSourceProperty] public string DemesneText => new TextObject("{=Qfbq72zey}Crown Demesne").ToString();

        [DataSourceProperty] public string CourtText => new TextObject("{=4X2A8tVjO}Court").ToString();

        [DataSourceProperty]
        public bool DemesneSelected
        {
            get => demesneSelected;
            set
            {
                if (value != demesneSelected)
                {
                    demesneSelected = value;
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

        [DataSourceProperty]
        public DemesneHierarchyVM Demesne
        {
            get => demesneVM;
            set
            {
                if (value != demesneVM)
                {
                    demesneVM = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            courtVM.RefreshValues();
            if (kingdomManagement.Clan.Show || kingdomManagement.Settlement.Show || kingdomManagement.Policy.Show ||
                kingdomManagement.Army.Show || kingdomManagement.Diplomacy.Show)
            {
                Court.IsSelected = false;
                CourtSelected = false;
                DemesneSelected = false;
                Demesne.IsSelected = false;
            }
        }

        [DataSourceMethod]
        public void SelectCourt()
        {
            kingdomManagement.Clan.Show = false;
            kingdomManagement.Settlement.Show = false;
            kingdomManagement.Policy.Show = false;
            kingdomManagement.Army.Show = false;
            kingdomManagement.Diplomacy.Show = false;

            DemesneSelected = false;
            Demesne.IsSelected = false;
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
            Demesne.IsSelected = true;
            Court.IsSelected = false;
            CourtSelected = false;
        }
    }
}