using BannerKings.Behaviours;
using BannerKings.UI.Court;
using BannerKings.UI.Kingdoms;
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
        private bool courtSelected, courtEnabled, demesneSelected, demesneEnabled;
        private CourtVM courtVM;
        private KingdomDemesneVM demesneVM;
        private readonly KingdomManagementVM kingdomManagement;

        public KingdomManagementMixin(KingdomManagementVM vm) : base(vm)
        {
            kingdomManagement = vm;
            courtVM = new CourtVM(true);
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(vm.Kingdom);
            DemesneEnabled = title != null;

            if (DemesneEnabled)
            {
                demesneVM = new KingdomDemesneVM(title, vm.Kingdom);
            }

            var capital = Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(vm.Kingdom);
            CourtEnabled = capital != null;
        }


        [DataSourceProperty] public string DemesneText => new TextObject("{=6QMDGRSt}Demesne").ToString();

        [DataSourceProperty] public string CourtText => new TextObject("{=2QGyA46m}Court").ToString();

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

        public override void OnRefresh()
        {
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
            if (council != null)
            {
                if (council.Peerage != null && !council.Peerage.CanStartElection)
                {
                    var policy = kingdomManagement.Policy;
                    var diplomacy = kingdomManagement.Diplomacy;
                    var clans = kingdomManagement.Clan;
                    var fiefs = kingdomManagement.Settlement;

                    var text = new TextObject("{=!}The Peerage of {CLAN} does not allow starting elections.")
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
            if (kingdomManagement.Clan.Show || kingdomManagement.Settlement.Show || kingdomManagement.Policy.Show ||
                kingdomManagement.Army.Show || kingdomManagement.Diplomacy.Show)
            {
                Court.IsSelected = false;
                CourtSelected = false;
                DemesneSelected = false;

                if (Demesne != null)
                {
                    Demesne.IsSelected = false;
                }
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
            
            Court.IsSelected = true;
            CourtSelected = true;
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
            }
        }
    }
}