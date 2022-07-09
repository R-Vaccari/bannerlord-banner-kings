using BannerKings.Populations;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings
{
    namespace UI.Panels
    {
        public class PopulationVM : BannerKingsViewModel
        {
            private OverviewVM overviewVM;
            private EconomyVM economyVM;
            private DemesneVM demesneVM;
            private MilitaryVM militaryVM;
            private Settlement settlement;
            private bool _isOverviewSelected;
            private bool _isEconomySelected;
            private bool _isDemesneSelected;
            private bool _isMilitarySelected;

            public PopulationVM(PopulationData data) : base(data, true)
            {
                settlement = data.Settlement;
                overviewVM = new OverviewVM(data, settlement, true);
                economyVM = new EconomyVM(data, settlement, false);
                demesneVM = new DemesneVM(data, BannerKingsConfig.Instance.TitleManager.GetTitle(settlement), false);
                militaryVM = new MilitaryVM(data, settlement, false);
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
                if (index == 0)
                {
                    OverView.IsSelected = true;
                    IsOverviewSelected = true;
                }
                else if (index == 1)
                {
                    Economy.IsSelected = true;
                    IsEconomySelected = true;
                }   
                else if (index == 2)
                {
                    Demesne.IsSelected = true;
                    IsDemesneSelected = true;
                }
                else if (index == 3)
                {
                    Military.IsSelected = true;
                    IsMilitarySelected = true;
                }

                RefreshValues();
            }

            [DataSourceProperty]
            public OverviewVM OverView
            {
                get => this.overviewVM;     
                set
                {
                    if (value != this.overviewVM)
                    {
                        this.overviewVM = value;
                        base.OnPropertyChangedWithValue(value, "OverView");
                    }
                }
            }

            [DataSourceProperty]
            public EconomyVM Economy
            {
                get => this.economyVM;
                set
                {
                    if (value != this.economyVM)
                    {
                        this.economyVM = value;
                        base.OnPropertyChangedWithValue(value, "Economy");
                    }
                }
            }

            [DataSourceProperty]
            public DemesneVM Demesne
            {
                get => this.demesneVM;
                set
                {
                    if (value != this.demesneVM)
                    {
                        this.demesneVM = value;
                        base.OnPropertyChangedWithValue(value, "Demesne");
                    }
                }
            }

            [DataSourceProperty]
            public MilitaryVM Military
            {
                get => this.militaryVM;
                set
                {
                    if (value != this.militaryVM)
                    {
                        this.militaryVM = value;
                        base.OnPropertyChangedWithValue(value, "Military");
                    }
                }
            }

            [DataSourceProperty]
            public bool IsOverviewSelected
            {
                get => this._isOverviewSelected;
                set
                {
                    if (value != this._isOverviewSelected)
                    {
                        this._isOverviewSelected = value;
                        base.OnPropertyChangedWithValue(value, "IsOverviewSelected");
                    }
                }
            }

            [DataSourceProperty]
            public bool IsDemesneSelected
            {
                get => this._isDemesneSelected;
                set
                {
                    if (value != this._isDemesneSelected)
                    {
                        this._isDemesneSelected = value;
                        base.OnPropertyChangedWithValue(value, "IsDemesneSelected");
                    }
                }
            }

            [DataSourceProperty]
            public bool IsEconomySelected
            {
                get => this._isEconomySelected;
                set
                {
                    if (value != this._isEconomySelected)
                    {
                        this._isEconomySelected = value;
                        base.OnPropertyChangedWithValue(value, "IsEconomySelected");
                    }
                }
            }

            [DataSourceProperty]
            public bool IsMilitarySelected
            {
                get => this._isMilitarySelected;
                set
                {
                    if (value != this._isMilitarySelected)
                    {
                        this._isMilitarySelected = value;
                        base.OnPropertyChangedWithValue(value, "IsMilitarySelected");
                    }
                }
            }

            public void ExecuteClose()
            {
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Policies updated for {0}", settlement.Name.ToString())));
                this.militaryVM.OnFinalize();
                this.economyVM.OnFinalize();
                UIManager.Instance.CloseUI();
            }
        }
    } 
}
