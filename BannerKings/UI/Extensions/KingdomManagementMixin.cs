using BannerKings.Behaviours.Diplomacy;
using BannerKings.UI.Court;
using BannerKings.UI.VanillaTabs.Kingdoms;
using BannerKings.UI.VanillaTabs.Kingdoms.Groups;
using BannerKings.UI.VanillaTabs.Kingdoms.Mercenary;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class KingdomManagementMixin : BaseViewModelMixin<KingdomManagementVM>
    {
        private readonly KingdomManagementVM kingdomManagement;
        private bool courtSelected, courtEnabled, demesneSelected, demesneEnabled, groupsEnabled,
            groupsSelected, showCareer, careerSelected;
        private CourtVM courtVM;
        private KingdomDemesneVM demesneVM;
        private KingdomGroupsVM groupsVM;
        private MercenaryCareerVM careerVM;

        public KingdomManagementMixin(KingdomManagementVM vm) : base(vm)
        {
            kingdomManagement = vm;
            courtVM = new CourtVM(true);
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(vm.Kingdom);
            DemesneEnabled = title != null;
            demesneVM = new KingdomDemesneVM(title, vm.Kingdom);
            demesneVM.IsSelected = DemesneEnabled;
            //var capital = Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(vm.Kingdom);
            CourtEnabled = true;
            kingdomManagement.RefreshValues();

            var diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(vm.Kingdom);
            Groups = new KingdomGroupsVM(diplomacy);
            GroupsEnabled = diplomacy != null;

            ShowCareer = false;
            Career = new MercenaryCareerVM();
            if (Clan.PlayerClan.IsUnderMercenaryService)
            {
                ShowCareer = true;
            }
        }

        [DataSourceProperty] public string DemesneText => new TextObject("{=6QMDGRSt}Demesne").ToString();
        [DataSourceProperty] public string CourtText => new TextObject("{=2QGyA46m}Court").ToString();
        [DataSourceProperty] public string CareerText => new TextObject("{=WmzEL8hL}Career").ToString();
        [DataSourceProperty] public string GroupsText => new TextObject("{=F4Vv8Lc8}Groups").ToString();
        

        [DataSourceProperty]
        public bool ShowCareer
        {
            get => showCareer;
            set
            {
                if (value != showCareer)
                {
                    showCareer = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CareerSelected
        {
            get => careerSelected;
            set
            {
                if (value != careerSelected)
                {
                    careerSelected = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MercenaryCareerVM Career
        {
            get => careerVM;
            set
            {
                if (value != careerVM)
                {
                    careerVM = value;
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
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool GroupsEnabled
        {
            get => groupsEnabled;
            set
            {
                if (value != groupsEnabled)
                {
                    groupsEnabled = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool GroupsSelected
        {
            get => groupsSelected;
            set
            {
                if (value != groupsSelected)
                {
                    groupsSelected = value;
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
        public KingdomDemesneVM Demesne
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

        [DataSourceProperty]
        public KingdomGroupsVM Groups
        {
            get => groupsVM;
            set
            {
                if (value != groupsVM)
                {
                    groupsVM = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
            if (council != null)
            {
                if (council.Peerage == null || (council.Peerage != null && !council.Peerage.CanStartElection))
                {
                    var policy = kingdomManagement.Policy;
                    var diplomacy = kingdomManagement.Diplomacy;
                    var clans = kingdomManagement.Clan;
                    var fiefs = kingdomManagement.Settlement;

                    var text = new TextObject("{=RDDOdoeR}The Peerage of {CLAN} does not allow starting elections.")
                        .SetTextVariable("CLAN", Clan.PlayerClan.Name);

                    if (policy.CanProposeOrDisavowPolicy)
                    {
                        policy.DoneHint.HintText = text;
                        policy.CanProposeOrDisavowPolicy = false;
                    }
                   
                    if (diplomacy.IsActionEnabled)
                    {
                        diplomacy.ActionHint.HintText = text;
                        diplomacy.IsActionEnabled = false;
                    }
                    
                    if (clans.CanExpelCurrentClan)
                    {
                        clans.ExpelHint.HintText = text;
                        clans.CanExpelCurrentClan = false;
                    }
                    
                    if (fiefs.CanAnnexCurrentSettlement)
                    {
                        fiefs.AnnexHint.HintText = text;
                        fiefs.CanAnnexCurrentSettlement = false;
                    }
                }
            }

            Court.RefreshValues();
            Demesne?.RefreshValues();
            Groups.RefreshValues();
            Career?.RefreshValues();
            if (kingdomManagement.Clan.Show || kingdomManagement.Settlement.Show || kingdomManagement.Policy.Show ||
                kingdomManagement.Army.Show || kingdomManagement.Diplomacy.Show)
            {
                Court.IsSelected = false;
                CourtSelected = false;
                DemesneSelected = false;
                CareerSelected = false;

                if (Demesne != null)
                {
                    Demesne.IsSelected = false;
                }

                if (Career != null)
                {
                    Career.IsSelected = false;
                }

                Groups.IsSelected = false;
                GroupsSelected = false;
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
            if (Demesne != null)
            {
                Demesne.IsSelected = false;
            }

            if (Career != null)
            {
                Career.IsSelected = false;
                CareerSelected = false;
            }

            Court.IsSelected = true;
            CourtSelected = true;

            GroupsSelected = false;
            Groups.IsSelected = false;

            kingdomManagement.RefreshValues();
        }

        [DataSourceMethod]
        public void SelectDemesne()
        {
            if (Demesne != null)
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
                if (Career != null)
                {
                    Career.IsSelected = false;
                    CareerSelected = false;
                }

                GroupsSelected = false;
                Groups.IsSelected = false;
            }

            kingdomManagement.RefreshValues();
        }

        [DataSourceMethod]
        public void SelectGroups()
        {
            if (Demesne != null)
            {
                kingdomManagement.Clan.Show = false;
                kingdomManagement.Settlement.Show = false;
                kingdomManagement.Policy.Show = false;
                kingdomManagement.Army.Show = false;
                kingdomManagement.Diplomacy.Show = false;

                DemesneSelected = false;
                Demesne.IsSelected = false;
                Court.IsSelected = false;
                CourtSelected = false;

                GroupsSelected = true;
                Groups.IsSelected = true;

                if (Career != null)
                {
                    Career.IsSelected = false;
                    CareerSelected = false;
                }
            }

            kingdomManagement.RefreshValues();
        }

        

        [DataSourceMethod]
        public void SelectCareer()
        {
            kingdomManagement.Clan.Show = false;
            kingdomManagement.Settlement.Show = false;
            kingdomManagement.Policy.Show = false;
            kingdomManagement.Army.Show = false;
            kingdomManagement.Diplomacy.Show = false;

            DemesneSelected = false;
            if (Demesne != null)
            {
                Demesne.IsSelected = false;
            }

            Court.IsSelected = false;
            CourtSelected = false;

            kingdomManagement.RefreshValues();

            CareerSelected = true;
            Career.IsSelected = true;
        }
    }
}