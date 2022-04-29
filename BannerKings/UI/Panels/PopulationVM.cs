﻿using BannerKings.Populations;
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
            public bool IsOverviewSelected
            {
                get => _isOverviewSelected;
                set
                {
                    if (value != _isOverviewSelected)
                    {
                        _isOverviewSelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsDemesneSelected
            {
                get => _isDemesneSelected;
                set
                {
                    if (value != _isDemesneSelected)
                    {
                        _isDemesneSelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsEconomySelected
            {
                get => _isEconomySelected;
                set
                {
                    if (value != _isEconomySelected)
                    {
                        _isEconomySelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public bool IsMilitarySelected
            {
                get => _isMilitarySelected;
                set
                {
                    if (value != _isMilitarySelected)
                    {
                        _isMilitarySelected = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            public void ExecuteClose()
            {
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Policies updated for {0}", settlement.Name)));
                militaryVM.OnFinalize();
                economyVM.OnFinalize();
                UIManager.Instance.CloseUI();
            }
        }
    } 
}
