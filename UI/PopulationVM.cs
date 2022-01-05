using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Populations
{
    namespace UI
    {
        public class PopulationVM : ViewModel
        {
            private OverviewVM overviewVM;
            private ManagementVM managementVM;
            private DemesneVM demesneVM;
            private Settlement _settlement;
            private bool _isOverviewSelected;
            private bool _isManagementSelected;
            private bool _isDemesneSelected;

            public PopulationVM(Settlement _settlement)
            {
                this._settlement = _settlement;
                overviewVM = new OverviewVM(_settlement, true);
                managementVM = new ManagementVM(_settlement, false);
                demesneVM = new DemesneVM(PopulationConfig.Instance.TitleManager.GetTitle(_settlement), false);
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
            }

            public void SetSelectedCategory(int index)
            {
                this.OverView.IsSelected = false;
                this.Management.IsSelected = false;
                this.Demesne.IsSelected = false;
                this.IsOverviewSelected = false;
                this.IsManagementSelected = false;
                this.IsDemesneSelected = false;
                if (index == 0)
                {
                    this.OverView.IsSelected = true;
                    this.IsOverviewSelected = true;
                }
                else if (index == 1)
                {
                    this.Management.IsSelected = true;
                    this.IsManagementSelected = true;
                }   
                else if (index == 2)
                {
                    this.Demesne.IsSelected = true;
                    this.IsDemesneSelected = true;
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
            public ManagementVM Management
            {
                get => this.managementVM;
                set
                {
                    if (value != this.managementVM)
                    {
                        this.managementVM = value;
                        base.OnPropertyChangedWithValue(value, "Management");
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
            public bool IsManagementSelected
            {
                get => this._isManagementSelected;
                set
                {
                    if (value != this._isManagementSelected)
                    {
                        this._isManagementSelected = value;
                        base.OnPropertyChangedWithValue(value, "IsManagementSelected");
                    }
                }
            }

            public void ExecuteClose()
            {
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Policies updated for {0}", _settlement.Name.ToString())));
                UIManager.instance.CloseUI();
            }
        }
    } 
}
