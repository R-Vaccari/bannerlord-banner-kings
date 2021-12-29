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
            private Settlement _settlement;

            public PopulationVM(Settlement _settlement)
            {
                this._settlement = _settlement;
                overviewVM = new OverviewVM(_settlement, true);
                managementVM = new ManagementVM(_settlement, false);
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
            }

            public void SetSelectedCategory(int index)
            {
                this.OverView.IsSelected = false;
                this.Management.IsSelected = false;
                if (index == 0)
                    this.OverView.IsSelected = true;
                else if (index == 1)
                    this.Management.IsSelected = true;
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

            public void ExecuteClose()
            {
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Policies updated for {0}", _settlement.Name.ToString())));
                UIManager.instance.CloseUI();
            }
        }
    } 
}
