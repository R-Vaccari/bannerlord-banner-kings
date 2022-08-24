using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Management
{
    namespace BannerKings.UI.Panels
    {
        public class PopulationVM : BannerKingsViewModel
        {
            private bool isDemesneSelected;
            private bool isEconomySelected;
            private bool isMilitarySelected;
            private bool isOverviewSelected;
            private bool isReligionSelected;
            private DemesneVM demesneVM;
            private EconomyVM economyVM;
            private MilitaryVM militaryVM;
            private OverviewVM overviewVM;
            private ReligionVM religionVM;
            private readonly Settlement settlement;

            public PopulationVM(PopulationData data) : base(data, true)
            {
                settlement = data.Settlement;
                overviewVM = new OverviewVM(data, settlement, true);
                economyVM = new EconomyVM(data, settlement, false);
                demesneVM = new DemesneVM(data, BannerKingsConfig.Instance.TitleManager.GetTitle(settlement), false);
                militaryVM = new MilitaryVM(data, settlement, false);
                religionVM = new ReligionVM(data, settlement, false);
                IsOverviewSelected = true;
            }

            [DataSourceProperty]
            public string OverViewText = new TextObject("{=!}Overview").ToString();

            [DataSourceProperty]
            public string EconomyText = new TextObject("{=!}Economy").ToString();

            [DataSourceProperty]
            public string MilitaryText = new TextObject("{=!}Military").ToString();

            [DataSourceProperty]
            public string DemesneText = new TextObject("{=!}Demesne").ToString();

            [DataSourceProperty]
            public string ReligionText = new TextObject("{=!}Religion").ToString();

            [DataSourceProperty]
            public OverviewVM OverView
            {
                get => overviewVM;
                set
                {
                    if (value != overviewVM)
                    {
                        overviewVM = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public EconomyVM Economy
            {
                get => economyVM;
                set
                {
                    if (value != economyVM)
                    {
                        economyVM = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public DemesneVM Demesne
            {
                get => demesneVM;
                set
                {
                    if (value != demesneVM)
                    {
                        demesneVM = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public MilitaryVM Military
            {
                get => militaryVM;
                set
                {
                    if (value != militaryVM)
                    {
                        militaryVM = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public ReligionVM Religion
            {
                get => religionVM;
                set
                {
                    if (value != religionVM)
                    {
                        religionVM = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsOverviewSelected
            {
                get => isOverviewSelected;
                set
                {
                    if (value != isOverviewSelected)
                    {
                        isOverviewSelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsDemesneSelected
            {
                get => isDemesneSelected;
                set
                {
                    if (value != isDemesneSelected)
                    {
                        isDemesneSelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsEconomySelected
            {
                get => isEconomySelected;
                set
                {
                    if (value != isEconomySelected)
                    {
                        isEconomySelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsMilitarySelected
            {
                get => isMilitarySelected;
                set
                {
                    if (value != isMilitarySelected)
                    {
                        isMilitarySelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsReligionSelected
            {
                get => isReligionSelected;
                set
                {
                    if (value != isReligionSelected)
                    {
                        isReligionSelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
            }

            public void SetSelectedCategory(int index)
            {
                OverView.IsSelected = false;
                Economy.IsSelected = false;
                Demesne.IsSelected = false;
                IsOverviewSelected = false;
                IsEconomySelected = false;
                IsDemesneSelected = false;
                IsMilitarySelected = false;
                Military.IsSelected = false;
                IsReligionSelected = false;
                Religion.IsSelected = false;
                switch (index)
                {
                    case 0:
                        OverView.IsSelected = true;
                        IsOverviewSelected = true;
                        break;
                    case 1:
                        Economy.IsSelected = true;
                        IsEconomySelected = true;
                        break;
                    case 2:
                        Demesne.IsSelected = true;
                        IsDemesneSelected = true;
                        break;
                    case 3:
                        Military.IsSelected = true;
                        IsMilitarySelected = true;
                        break;
                    case 4:
                        IsReligionSelected = true;
                        Religion.IsSelected = true;
                        break;
                }

                RefreshValues();
            }

            public new void ExecuteClose()
            {
                InformationManager.DisplayMessage(new InformationMessage($"Policies updated for {settlement.Name}"));
                militaryVM.OnFinalize();
                economyVM.OnFinalize();
                UIManager.Instance.CloseUI();
            }
        }
    }
}