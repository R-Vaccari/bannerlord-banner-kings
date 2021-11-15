using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static Populations.PolicyManager;
using static Populations.Population;

namespace Populations
{
    namespace UI
    {
        public class PopulationVM : ViewModel
        {
            private MBBindingList<PopulationInfoVM> _popInfo;
            private PopulationOptionVM _slaveToogle;
            private PopulationOptionVM _popAccelerateToogle;
            private PopulationOptionVM _selfInvestToogle;
            private PopulationOptionVM _conscriptionToogle;
            private PopulationOptionVM _nobleExemptionToogle;
            private Settlement settlement;

            public PopulationVM(Settlement settlement)
            {
                this.settlement = settlement;
                _popInfo = new MBBindingList<PopulationInfoVM>();
                _slaveToogle = new PopulationOptionVM();
                _popAccelerateToogle = new PopulationOptionVM();
                _selfInvestToogle = new PopulationOptionVM();
                _conscriptionToogle = new PopulationOptionVM();
                _nobleExemptionToogle = new PopulationOptionVM();
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
                PopInfo.Clear();
                PopulationData data = Population.GetPopData(Settlement.CurrentSettlement);
                if (data != null && data.Classes != null)
                {
                    data.Classes.ForEach(popClass => PopInfo.Add(new PopulationInfoVM(
                        popClass.type.ToString(), popClass.count)
                        ));

                    List<PolicyElement> elements = PolicyManager.GetSettlementPolicies(settlement);
                    foreach (PolicyElement policy in elements)
                    {
                        PopulationOptionVM vm = new PopulationOptionVM()
                        .SetAsBooleanOption(policy.description, policy.isChecked, delegate (bool value)
                        {
                            PolicyManager.UpdatePolicy(settlement, policy.type, value);
                            InformationManager.DisplayMessage(new InformationMessage(
                                String.Format("Policies update for {0}", settlement.Name.ToString()))
                            );
                        }, new TextObject(policy.hint));
                        switch (policy.type)
                        {
                            case PolicyType.EXPORT_SLAVES:
                                SlaveToogle = vm;
                                break;
                            case PolicyType.POP_GROWTH:
                                AccelerateToogle = vm;
                                break;
                            case PolicyType.SELF_INVEST:
                                InvestToogle = vm;
                                break;
                            case PolicyType.CONSCRIPTION:
                                ConscriptionToogle = vm;
                                break;
                            case PolicyType.EXEMPTION:
                                ExemptionToogle = vm;
                                break;
                        }
                    }   
                }
            }

            [DataSourceProperty]
            public MBBindingList<PopulationInfoVM> PopInfo
            {
                get => _popInfo;
                set
                {
                    if (value != _popInfo)
                    {
                        _popInfo = value;
                        base.OnPropertyChangedWithValue(value, "PopInfo");
                    }
                }
            }

            [DataSourceProperty]
            public PopulationOptionVM SlaveToogle
            {
                get => _slaveToogle;
                set
                {
                    if (value != _slaveToogle)
                    {
                        _slaveToogle = value;
                        base.OnPropertyChangedWithValue(value, "SlaveToogle");
                    }
                }
            }

            [DataSourceProperty]
            public PopulationOptionVM AccelerateToogle
            {
                get => _popAccelerateToogle;
                set
                {
                    if (value != _popAccelerateToogle)
                    {
                        _popAccelerateToogle = value;
                        base.OnPropertyChangedWithValue(value, "AccelerateToogle");
                    }
                }
            }

            [DataSourceProperty]
            public PopulationOptionVM InvestToogle
            {
                get => _selfInvestToogle;
                set
                {
                    if (value != _selfInvestToogle)
                    {
                        _selfInvestToogle = value;
                        base.OnPropertyChangedWithValue(value, "InvestToogle");
                    }
                }
            }

            [DataSourceProperty]
            public PopulationOptionVM ConscriptionToogle
            {
                get => _conscriptionToogle;
                set
                {
                    if (value != _conscriptionToogle)
                    {
                        _conscriptionToogle = value;
                        base.OnPropertyChangedWithValue(value, "ConscriptionToogle");
                    }
                }
            }

            [DataSourceProperty]
            public PopulationOptionVM ExemptionToogle
            {
                get => _nobleExemptionToogle;
                set
                {
                    if (value != _nobleExemptionToogle)
                    {
                        _nobleExemptionToogle = value;
                        base.OnPropertyChangedWithValue(value, "ExemptionToogle");
                    }
                }
            }

            public void ExecuteClose() => UIManager.instance.CloseUI();  
        }
    } 
}
