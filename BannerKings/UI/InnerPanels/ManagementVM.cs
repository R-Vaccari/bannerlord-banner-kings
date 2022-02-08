using BannerKings.Models;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PolicyManager;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI
{
    public class ManagementVM : ViewModel
    {
        private PopulationOptionVM _slaveToogle;
        private PopulationOptionVM _popAccelerateToogle;
        private PopulationOptionVM _selfInvestToogle;
        private PopulationOptionVM _conscriptionToogle;
        private PopulationOptionVM _nobleExemptionToogle;
        private PopulationOptionVM _subsidizeMilitiaToogle;
        private SelectorVM<MilitiaItemVM> _militiaSelector;
        private SelectorVM<TaxItemVM> _taxSelector;
        private SelectorVM<WorkItemVM> _workSelector;
        private SelectorVM<TariffItemVM> _tariffSelector;
        private SelectorVM<CrimeItemVM> _crimeSelector;
        
        private Settlement _settlement;
        private bool _isSelected;

        public ManagementVM(Settlement _settlement, bool _isSelected)
        {
            this._settlement = _settlement;
            this._isSelected = _isSelected;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            List<PolicyElement> elements = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(_settlement);
            foreach (PolicyElement policy in elements)
            {
                PopulationOptionVM vm = new PopulationOptionVM()
                .SetAsBooleanOption(policy.description, policy.isChecked, delegate (bool value)
                {
                    BannerKingsConfig.Instance.PolicyManager.UpdatePolicy(_settlement, policy.type, value);
                    this.RefreshValues();

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
            MilitiaPolicy militiaPolicy = BannerKingsConfig.Instance.PolicyManager.GetMilitiaPolicy(_settlement);
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
            TaxType taxPolicy = BannerKingsConfig.Instance.PolicyManager.GetSettlementTax(_settlement);
            if (taxPolicy == TaxType.High)
                taxIndex = 1;
            else if (taxPolicy == TaxType.Low)
                taxIndex = 2;
            TaxSelector = new SelectorVM<TaxItemVM>(0, new Action<SelectorVM<TaxItemVM>>(this.OnTaxChange));
            TaxSelector.SetOnChangeAction(null);
            foreach (TaxType policy in _taxPolicies)
            {
                TaxItemVM item = new TaxItemVM(policy, true, BannerKingsConfig.Instance.PolicyManager.GetTaxHint(policy, _settlement.IsVillage));
                TaxSelector.AddItem(item);
            }
            TaxSelector.SetOnChangeAction(OnTaxChange);
            TaxSelector.SelectedIndex = taxIndex;


            int workIndex = 0;
            WorkforcePolicy workPolicy = BannerKingsConfig.Instance.PolicyManager.GetSettlementWork(_settlement);
            if (workPolicy == WorkforcePolicy.Martial_Law)
                workIndex = 1;
            else if (workPolicy == WorkforcePolicy.Construction)
                workIndex = 2;
            WorkSelector = new SelectorVM<WorkItemVM>(0, new Action<SelectorVM<WorkItemVM>>(this.OnWorkChange));
            WorkSelector.SetOnChangeAction(null);
            foreach (WorkforcePolicy policy in _workPolicies)
            {
                WorkItemVM item = new WorkItemVM(policy, true);
                WorkSelector.AddItem(item);
            }
            WorkSelector.SetOnChangeAction(OnWorkChange);
            WorkSelector.SelectedIndex = workIndex;

            int tariffIndex = 0;
            TariffType tariff = BannerKingsConfig.Instance.PolicyManager.GetSettlementTariff(_settlement);
            if (tariff == TariffType.Internal_Consumption)
                tariffIndex = 1;
            else if (tariff == TariffType.Exemption)
                tariffIndex = 2;
            TariffSelector = new SelectorVM<TariffItemVM>(0, new Action<SelectorVM<TariffItemVM>>(this.OnTariffChange));
            TariffSelector.SetOnChangeAction(null);
            foreach (TariffType policy in _tariffPolicies)
            {
                TariffItemVM item = new TariffItemVM(policy, true);
                TariffSelector.AddItem(item);
            }
            TariffSelector.SetOnChangeAction(OnTariffChange);
            TariffSelector.SelectedIndex = tariffIndex;

            int crimeIndex = 0;
            CriminalPolicy criminalPolicy = BannerKingsConfig.Instance.PolicyManager.GetCriminalPolicy(_settlement);
            if (criminalPolicy == CriminalPolicy.Execution)
                crimeIndex = 1;
            else if (criminalPolicy == CriminalPolicy.Forgiveness)
                crimeIndex = 2;
            CrimeSelector = new SelectorVM<CrimeItemVM>(0, new Action<SelectorVM<CrimeItemVM>>(this.OnCrimeChange));
            CrimeSelector.SetOnChangeAction(null);
            foreach (CriminalPolicy policy in _crimePolicies)
            {
                CrimeItemVM item = new CrimeItemVM(policy, true);
                CrimeSelector.AddItem(item);
            }
            CrimeSelector.SetOnChangeAction(OnCrimeChange);
            CrimeSelector.SelectedIndex = crimeIndex;       
        }

        private string FormatValue(float value) => (value * 100f).ToString("0.00") + '%';

        private void OnTariffChange(SelectorVM<TariffItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                TariffItemVM selectedItem = obj.SelectedItem;
                BannerKingsConfig.Instance.PolicyManager.UpdateTariffPolicy(_settlement, selectedItem.policy);
            }
        }

        private void OnCrimeChange(SelectorVM<CrimeItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                CrimeItemVM selectedItem = obj.SelectedItem;
                BannerKingsConfig.Instance.PolicyManager.UpdateCriminalPolicy(_settlement, selectedItem.policy);
            }
        }

        private void OnMilitiaChange(SelectorVM<MilitiaItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                MilitiaItemVM selectedItem = obj.SelectedItem;
                BannerKingsConfig.Instance.PolicyManager.UpdateMilitiaPolicy(_settlement, selectedItem.policy);
            }
        }

        private void OnTaxChange(SelectorVM<TaxItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                TaxItemVM selectedItem = obj.SelectedItem;
                BannerKingsConfig.Instance.PolicyManager.UpdateTaxPolicy(_settlement, selectedItem.policy);
            }
        }

        private void OnWorkChange(SelectorVM<WorkItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                WorkItemVM selectedItem = obj.SelectedItem;
                BannerKingsConfig.Instance.PolicyManager.UpdateWorkPolicy(_settlement, selectedItem.policy);
            }
        }

        [DataSourceProperty]
        public string ManumissionButtonName
        {
            get => "Grant Manumission";
        }

        [DataSourceProperty]
        public HintViewModel ManumissionButtonHint
        {
            get => new HintViewModel(new TextObject("Concede forgiveness to slaves for their crimes and debts, making them serfs instead. The amount of freed slaves is relative to the current slave population"));
        }

        [DataSourceProperty]
        public string AidButtonName
        {
            get => "Require Aid";
        }

        [DataSourceProperty]
        public HintViewModel AidButtonHint
        {
            get => new HintViewModel(new TextObject("Require financial aid from local merchants and notables. The amount is relative to how much wealth the merchants currently have, as well as the influence cost and relations impact with local notables"));
        }

        public int InfluenceCost
        {
            get
            {
                MobileParty party = _settlement.MilitiaPartyComponent.MobileParty;
                Hero lord = _settlement.OwnerClan.Leader;
                if (party != null && lord != null && lord.PartyBelongedTo != null)
                    return new BKInfluenceModel().GetMilitiaInfluenceCost(party, _settlement, lord);
                else return -1;
            }
        }

        [DataSourceProperty]
        public string InfluenceCostText
        {
            get => string.Format("Cost: {0} influence", InfluenceCost);
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    if (value) this.RefreshValues();
                    base.OnPropertyChangedWithValue(value, "IsSelected");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<TariffItemVM> TariffSelector
        {
            get
            {
                return this._tariffSelector;
            }
            set
            {
                if (value != this._tariffSelector)
                {
                    this._tariffSelector = value;
                    base.OnPropertyChangedWithValue(value, "TariffSelector");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<CrimeItemVM> CrimeSelector
        {
            get
            {
                return this._crimeSelector;
            }
            set
            {
                if (value != this._crimeSelector)
                {
                    this._crimeSelector = value;
                    base.OnPropertyChangedWithValue(value, "CrimeSelector");
                }
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

        private IEnumerable<WorkforcePolicy> _workPolicies
        {
            get
            {
                yield return WorkforcePolicy.None;
                yield return WorkforcePolicy.Martial_Law;
                yield return WorkforcePolicy.Construction;
                yield break;
            }
        }

        private IEnumerable<CriminalPolicy> _crimePolicies
        {
            get
            {
                yield return CriminalPolicy.Enslavement;
                yield return CriminalPolicy.Execution;
                yield return CriminalPolicy.Forgiveness;
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
                yield break;
            }
        }

        private IEnumerable<TariffType> _tariffPolicies
        {
            get
            {
                yield return TariffType.Standard;
                yield return TariffType.Internal_Consumption;
                yield return TariffType.Exemption;
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
    }
}
