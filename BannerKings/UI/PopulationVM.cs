using BannerKings.Populations;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings
{
    namespace UI
    {
        public class PopulationVM : ViewModel
        {
            private OverviewVM overviewVM;
            private EconomyVM economyVM;
            private DemesneVM demesneVM;
            private MilitaryVM militaryVM;
            private Settlement _settlement;
            private bool _isOverviewSelected;
            private bool _isEconomySelected;
            private bool _isDemesneSelected;
            private bool _isMilitarySelected;

            public PopulationVM(Settlement _settlement)
            {
                this._settlement = _settlement;
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(_settlement);
                overviewVM = new OverviewVM(_settlement, true);
                economyVM = new EconomyVM(data, _settlement, false);
                demesneVM = new DemesneVM(BannerKingsConfig.Instance.TitleManager.GetTitle(_settlement), false);
                militaryVM = new MilitaryVM(data, _settlement, false);
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
            }

            public void SetSelectedCategory(int index)
            {
                this.OverView.IsSelected = false;
                this.Economy.IsSelected = false;
                this.Demesne.IsSelected = false;
                this.IsOverviewSelected = false;
                this.IsEconomySelected = false;
                this.IsDemesneSelected = false;
                this.IsMilitarySelected = false;
                this.Military.IsSelected = false;
                if (index == 0)
                {
                    this.OverView.IsSelected = true;
                    this.IsOverviewSelected = true;
                }
                else if (index == 1)
                {
                    this.Economy.IsSelected = true;
                    this.IsEconomySelected = true;
                }   
                else if (index == 2)
                {
                    this.Demesne.IsSelected = true;
                    this.IsDemesneSelected = true;
                }
                else if (index == 3)
                {
                    this.Military.IsSelected = true;
                    this.IsMilitarySelected = true;
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
                    .Format("Policies updated for {0}", _settlement.Name.ToString())));
                this.militaryVM.OnFinalize();
                this.economyVM.OnFinalize();
                UIManager.instance.CloseUI();
            }
        }
    } 
}
