using Populations.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

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
            private PopulationOptionVM _subsidizeMilitiaToogle;
            private SelectorVM<MilitiaItemVM> _militiaSelector;
            private SelectorVM<TaxItemVM> _taxSelector;
            private Settlement settlement;

            public PopulationVM(Settlement settlement)
            {
                this.settlement = settlement;
                _popInfo = new MBBindingList<PopulationInfoVM>();
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
                PopInfo.Clear();
                PopulationData data = GetPopData(Settlement.CurrentSettlement);
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
                            case PolicyType.SUBSIDIZE_MILITIA:
                                SubsidizeToogle = vm;
                                break;
                        }
                    }

                    int militiaIndex = 0;
                    MilitiaPolicy militiaPolicy = PolicyManager.GetMilitiaPolicy(settlement);
                    if (militiaPolicy == MilitiaPolicy.Melee)
                        militiaIndex = 1;
                    else if (militiaPolicy == MilitiaPolicy.Ranged)
                        militiaIndex = 2;
                    MilitiaSelector = new SelectorVM<MilitiaItemVM>(0, new Action<SelectorVM<MilitiaItemVM>>(this.OnMilitiaChange));
                    MilitiaSelector.SetOnChangeAction(null);
                    foreach (MilitiaPolicy policy in _militiaPolicies)
                    {
                        MilitiaItemVM item = new MilitiaItemVM(policy, true);
                        MilitiaSelector.AddItem(item);
                    }
                    MilitiaSelector.SetOnChangeAction(OnMilitiaChange);
                    MilitiaSelector.SelectedIndex = militiaIndex;


                    int taxIndex = 0;
                    TaxType taxPolicy = PolicyManager.GetSettlementTax(settlement);
                    if (taxPolicy == TaxType.High)
                        taxIndex = 1;
                    else if (taxPolicy == TaxType.Low)
                        taxIndex = 2;
                    TaxSelector = new SelectorVM<TaxItemVM>(0, new Action<SelectorVM<TaxItemVM>>(this.OnTaxChange));
                    TaxSelector.SetOnChangeAction(null);
                    foreach (TaxType policy in _taxPolicies)
                    {
                        TaxItemVM item = new TaxItemVM(policy, true);
                        TaxSelector.AddItem(item);
                    }
                    TaxSelector.SetOnChangeAction(OnTaxChange);
                    TaxSelector.SelectedIndex = taxIndex;
                }
            }

            private void OnMilitiaChange(SelectorVM<MilitiaItemVM> obj)
            {
                if (obj.SelectedItem != null)
                {
                    MilitiaItemVM selectedItem = obj.SelectedItem;
                    PolicyManager.UpdateMilitiaPolicy(settlement, selectedItem.policy);
                }
            }

            private void OnTaxChange(SelectorVM<TaxItemVM> obj)
            {
                if (obj.SelectedItem != null)
                {
                    TaxItemVM selectedItem = obj.SelectedItem;
                    PolicyManager.UpdateTaxPolicy(settlement, selectedItem.policy);
                }
            }

            private IEnumerable<TaxType> _taxPolicies
            {
                get
                {
                    yield return TaxType.Standard;
                    yield return TaxType.High;
                    yield return TaxType.Low;
                    yield break;
                }
            }

            private IEnumerable<MilitiaPolicy> _militiaPolicies
            {
                get
                {
                    yield return MilitiaPolicy.None;
                    yield return MilitiaPolicy.Melee;
                    yield return MilitiaPolicy.Ranged;
                    yield break;
                }
            }

            [DataSourceProperty]
            public SelectorVM<TaxItemVM> TaxSelector
            {
                get
                {
                    return this._taxSelector;
                }
                set
                {
                    if (value != this._taxSelector)
                    {
                        this._taxSelector = value;
                        base.OnPropertyChangedWithValue(value, "TaxSelector");
                    }
                }
            }

            [DataSourceProperty]
            public SelectorVM<MilitiaItemVM> MilitiaSelector
            {
                get
                {
                    return this._militiaSelector;
                }
                set
                {
                    if (value != this._militiaSelector)
                    {
                        this._militiaSelector = value;
                        base.OnPropertyChangedWithValue(value, "MilitiaSelector");
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

            [DataSourceProperty]
            public PopulationOptionVM SubsidizeToogle
            {
                get => _subsidizeMilitiaToogle;
                set
                {
                    if (value != _subsidizeMilitiaToogle)
                    {
                        _subsidizeMilitiaToogle = value;
                        base.OnPropertyChangedWithValue(value, "SubsidizeToogle");
                    }
                }
            }

            public void ExecuteClose() => UIManager.instance.CloseUI();  
        }
    } 
}
