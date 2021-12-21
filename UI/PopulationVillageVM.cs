using Populations.Components;
using Populations.Models;
using Populations.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
        public class PopulationVillageVM : ViewModel
        {
            private MBBindingList<PopulationInfoVM> _popInfo;
            private PopulationOptionVM _popAccelerateToogle;
            private PopulationOptionVM _selfInvestToogle;
            private PopulationOptionVM _subsidizeMilitiaToogle;
            private PopulationOptionVM _raiseMilitiaButton;
            private SelectorVM<MilitiaItemVM> _militiaSelector; 
            private SelectorVM<TaxItemVM> _taxSelector;
            private SelectorVM<WorkItemVM> _workSelector;
            private Settlement settlement;

            public PopulationVillageVM(Settlement settlement)
            {
                this.settlement = settlement;
                _popInfo = new MBBindingList<PopulationInfoVM>();
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
                PopInfo.Clear();
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
                if (data != null && data.Classes != null)
                {
                    data.Classes.ForEach(popClass => PopInfo.Add(new PopulationInfoVM(
                        Helpers.Helpers.GetClassName(popClass.type, settlement.Culture).ToString(), popClass.count, 
                        Helpers.Helpers.GetClassHint(popClass.type, settlement.Culture))
                        ));

                    List<PolicyElement> elements = PopulationConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
                    foreach (PolicyElement policy in elements)
                    {
                        PopulationOptionVM vm = new PopulationOptionVM()
                        .SetAsBooleanOption(policy.description, policy.isChecked, delegate (bool value)
                        {
                            PopulationConfig.Instance.PolicyManager.UpdatePolicy(settlement, policy.type, value);
                            this.RefreshValues();
                           
                        }, new TextObject(policy.hint));
                        switch (policy.type)
                        {
                            case PolicyType.POP_GROWTH:
                                AccelerateToogle = vm;
                                break;
                            case PolicyType.SELF_INVEST:
                                InvestToogle = vm;
                                break;
                            case PolicyType.SUBSIDIZE_MILITIA:
                                SubsidizeToogle = vm;
                                break;
                        }
                    }


                    RaiseMilitiaButton = new PopulationOptionVM().SetAsButtonOption("Raise militia", delegate
                    {
                        int serfs = data.GetTypeCount(PopType.Serfs);
                        MobileParty party = settlement.MilitiaPartyComponent.MobileParty;
                        Hero lord = settlement.Owner;
                        if (serfs >= party.MemberRoster.TotalManCount)
                        {
                            int cost = InfluenceCost;
                            if (cost > -1 && lord.Clan.Influence >= cost)
                            {
                                if (party.CurrentSettlement != null && party.CurrentSettlement == settlement)
                                {
                                    MilitiaComponent.CreateMilitiaEscort("raisedmilitia_", settlement, settlement, "Raised Militia from {0}", Hero.MainHero.PartyBelongedTo, party);
                                    if (lord == Hero.MainHero)
                                        InformationManager.DisplayMessage(new InformationMessage(string.Format("{0} men raised as militia at {1}!", party.MemberRoster.TotalManCount, settlement.Name)));
                                }
                            }
                            else if (lord == Hero.MainHero)
                                InformationManager.DisplayMessage(new InformationMessage(string.Format("Not enough influence to raise militia at {0}", settlement.Name)));
                        } else if (lord == Hero.MainHero)
                            InformationManager.DisplayMessage(new InformationMessage(string.Format("Not enough available men to raise militia at {0}", settlement.Name)));

                    }, new TextObject("Raise the current militia of this village."));

                    int militiaIndex = 0;
                    MilitiaPolicy militiaPolicy = PopulationConfig.Instance.PolicyManager.GetMilitiaPolicy(settlement);
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
                    TaxType taxPolicy = PopulationConfig.Instance.PolicyManager.GetSettlementTax(settlement);
                    if (taxPolicy == TaxType.High)
                        taxIndex = 1;
                    else if (taxPolicy == TaxType.Low)
                        taxIndex = 2;
                    else if (taxPolicy == TaxType.Exemption)
                        taxIndex = 3;
                    TaxSelector = new SelectorVM<TaxItemVM>(0, new Action<SelectorVM<TaxItemVM>>(this.OnTaxChange));
                    TaxSelector.SetOnChangeAction(null);
                    foreach (TaxType policy in _taxPolicies)
                    {
                        TaxItemVM item = new TaxItemVM(policy, true, PopulationConfig.Instance.PolicyManager.GetTaxHint(policy, settlement.IsVillage));
                        TaxSelector.AddItem(item);
                    }
                    TaxSelector.SetOnChangeAction(OnTaxChange);
                    TaxSelector.SelectedIndex = taxIndex;


                    int workIndex = 0;
                    WorkforcePolicy workPolicy = PopulationConfig.Instance.PolicyManager.GetSettlementWork(settlement);
                    if (workPolicy == WorkforcePolicy.Land_Expansion)
                        workIndex = 1;
                    else if (workPolicy == WorkforcePolicy.Martial_Law)
                        workIndex = 2;
                    else if (workPolicy == WorkforcePolicy.Construction)
                        workIndex = 3;
                    WorkSelector = new SelectorVM<WorkItemVM>(0, new Action<SelectorVM<WorkItemVM>>(this.OnWorkChange));
                    WorkSelector.SetOnChangeAction(null);
                    foreach (WorkforcePolicy policy in _workPolicies)
                    {
                        WorkItemVM item = new WorkItemVM(policy, true);
                        WorkSelector.AddItem(item);
                    }
                    WorkSelector.SetOnChangeAction(OnWorkChange);
                    WorkSelector.SelectedIndex = workIndex;
                }
            }

            private void OnMilitiaChange(SelectorVM<MilitiaItemVM> obj)
            {
                if (obj.SelectedItem != null)
                {
                    MilitiaItemVM selectedItem = obj.SelectedItem;
                    PopulationConfig.Instance.PolicyManager.UpdateMilitiaPolicy(settlement, selectedItem.policy);
                }
            }

            private void OnTaxChange(SelectorVM<TaxItemVM> obj)
            {
                if (obj.SelectedItem != null)
                {
                    TaxItemVM selectedItem = obj.SelectedItem;
                    PopulationConfig.Instance.PolicyManager.UpdateTaxPolicy(settlement, selectedItem.policy);
                }
            }

            private void OnWorkChange(SelectorVM<WorkItemVM> obj)
            {
                if (obj.SelectedItem != null)
                {
                    WorkItemVM selectedItem = obj.SelectedItem;
                    PopulationConfig.Instance.PolicyManager.UpdateWorkPolicy(settlement, selectedItem.policy);
                }
            }


            private IEnumerable<WorkforcePolicy> _workPolicies
            {
                get
                {
                    yield return WorkforcePolicy.None;
                    yield return WorkforcePolicy.Land_Expansion;
                    yield break;
                }
            }

            private IEnumerable<TaxType> _taxPolicies
            {
                get
                {
                    yield return TaxType.Standard;
                    yield return TaxType.High;
                    yield return TaxType.Low;
                    yield return TaxType.Exemption;
                    yield break;
                }
            }

            private IEnumerable<MilitiaPolicy> _militiaPolicies
            {
                get
                {
                    yield return MilitiaPolicy.Balanced;
                    yield return MilitiaPolicy.Melee;
                    yield return MilitiaPolicy.Ranged;
                    yield break;
                }
            }

            public int InfluenceCost
            {
                get
                {
                    MobileParty party = settlement.MilitiaPartyComponent.MobileParty;
                    Hero lord = settlement.Owner;
                    if (party != null && lord != null && lord.PartyBelongedTo != null)
                        return new InfluenceModel().GetMilitiaInfluenceCost(party, settlement, lord);
                    else return -1;
                }
            }

            [DataSourceProperty]
            public string InfluenceCostText
            {
                get => string.Format("Cost: {0} influence", InfluenceCost);
            }

            [DataSourceProperty]
            public string PopGrowth
            {
                get
                {
                    int growth = new GrowthModel().GetPopulationGrowth(settlement, false);
                    return growth.ToString() + " (Daily)";
                }
            }


            [DataSourceProperty]
            public string Assimilation
            {
                get
                {
                    float result = new CultureModel().GetAssimilationChange(settlement);
                    return (result * 100f).ToString() + '%';
                }
            }
            

            [DataSourceProperty]
            public SelectorVM<WorkItemVM> WorkSelector
            {
                get
                {
                    return this._workSelector;
                }
                set
                {
                    if (value != this._workSelector)
                    {
                        this._workSelector = value;
                        base.OnPropertyChangedWithValue(value, "WorkSelector");
                    }
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
            public string AdministrativeCost
            {
                get
                {
                    float cost = new AdministrativeModel().CalculateAdministrativeCost(settlement);
                    return (cost * 100f).ToString() + '%' ;
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
            public PopulationOptionVM RaiseMilitiaButton
            {
                get => _raiseMilitiaButton;
                set
                {
                    if (value != _raiseMilitiaButton)
                    {
                        _raiseMilitiaButton = value;
                        base.OnPropertyChangedWithValue(value, "RaiseMilitiaButton");
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

            public void ExecuteClose()
            {
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Policies updated for {0}", settlement.Name.ToString())));
                UIManager.instance.CloseUI();
            }
        }
    } 
}
