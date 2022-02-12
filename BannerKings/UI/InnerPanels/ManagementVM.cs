using BannerKings.Managers.Decisions;
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
            HashSet<BannerKingsDecision> decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(_settlement);
            foreach (BannerKingsDecision decision in decisions)
            {
                PopulationOptionVM vm = new PopulationOptionVM()
                .SetAsBooleanOption(decision.GetName(), decision.Enabled, delegate (bool value)
                {
                    decision.OnChange(value);
                    this.RefreshValues();

                }, new TextObject(decision.GetHint()));
                switch (decision.GetIdentifier())
                {
                    case "":
                        SlaveToogle = vm;
                        break;
                }
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
    }
}
