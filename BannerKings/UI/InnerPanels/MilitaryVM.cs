using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Models;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI
{
    public class MilitaryVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> defenseInfo;
        private MBBindingList<InformationElement> manpowerInfo;
        private MBBindingList<InformationElement> siegeInfo;
        private SelectorVM<BKItemVM> militiaSelector, garrisonSelector, draftSelector;
        private PopulationOptionVM _conscriptionToogle, _subsidizeMilitiaToogle, draftingToogle;
        private BKGarrisonPolicy garrisonItem;
        private BKMilitiaPolicy militiaItem;
        private BKDraftPolicy draftItem;
        private Settlement settlement;

        public MilitaryVM(PopulationData data, Settlement _settlement, bool selected) : base(data, selected)
        {
            defenseInfo = new MBBindingList<InformationElement>();
            manpowerInfo = new MBBindingList<InformationElement>();
            siegeInfo = new MBBindingList<InformationElement>();
            this.settlement = _settlement;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            DefenseInfo.Clear();
            ManpowerInfo.Clear();
            SiegeInfo.Clear();
            DefenseInfo.Add(new InformationElement("Militia Cap:", new BKMilitiaModel().GetMilitiaLimit(data, settlement).ToString(),
                "The maximum number of militiamen this settlement can support, based on it's population"));
            DefenseInfo.Add(new InformationElement("Militia Quality:", FormatValue(new BKMilitiaModel().CalculateEliteMilitiaSpawnChance(settlement)),
                    "Chance of militiamen being spawned as veterans instead of recruits"));

            ManpowerInfo.Add(new InformationElement("Manpower:", base.data.MilitaryData.Manpower.ToString(),
                    "Manpower"));
            ManpowerInfo.Add(new InformationElement("Noble Manpower:", base.data.MilitaryData.NobleManpower.ToString(),
                   "Manpower"));
            ManpowerInfo.Add(new InformationElement("Peasant Manpower:", base.data.MilitaryData.PeasantManpower.ToString(),
                   "Manpower"));
            ManpowerInfo.Add(new InformationElement("Militarism:", base.FormatValue(base.data.MilitaryData.Militarism.ResultNumber),
                   "Manpower"));
            ManpowerInfo.Add(new InformationElement("Draft Efficiency:", base.FormatValue(base.data.MilitaryData.DraftEfficiency.ResultNumber),
                   "Manpower"));

            SiegeInfo.Add(new InformationElement("Storage Limit:", settlement.Town.FoodStocksUpperLimit().ToString(),
                    "The amount of food this settlement is capable of storing"));
            SiegeInfo.Add(new InformationElement("Estimated Holdout:", string.Format("{0} Days", base.data.MilitaryData.Holdout),
                "How long this settlement will take to start starving in case of a siege"));

            StringBuilder sb = new StringBuilder();
            sb.Append(base.data.MilitaryData.Catapultae);
            sb.Append(" ,");
            sb.Append(base.data.MilitaryData.Catapultae);
            sb.Append(" ,");
            sb.Append(base.data.MilitaryData.Trebuchets);
            sb.Append(" (Ballis., Catap., Treb.)");
            SiegeInfo.Add(new InformationElement("Engines:",  sb.ToString(),
                "How long this settlement will take to start starving in case of a siege"));

            militiaItem = (BKMilitiaPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "militia");
            MilitiaSelector = base.GetSelector(militiaItem, new Action<SelectorVM<BKItemVM>>(this.militiaItem.OnChange));
            MilitiaSelector.SelectedIndex = militiaItem.Selected;

            garrisonItem = (BKGarrisonPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "garrison"); 
            GarrisonSelector = base.GetSelector(garrisonItem, new Action<SelectorVM<BKItemVM>>(this.garrisonItem.OnChange));
            GarrisonSelector.SelectedIndex = garrisonItem.Selected;

            draftItem = (BKDraftPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "draft");
            DraftSelector = base.GetSelector(draftItem, new Action<SelectorVM<BKItemVM>>(this.draftItem.OnChange));
            DraftSelector.SelectedIndex = draftItem.Selected;

            HashSet<BannerKingsDecision> decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
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
                    case "decision_militia_encourage":
                        _conscriptionToogle = vm;
                        break;
                    case "decision_militia_subsidize":
                        _subsidizeMilitiaToogle = vm;
                        break;
                    case "decision_drafting_encourage":
                        draftingToogle = vm;
                        break;
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> DraftSelector
        {
            get
            {
                return this.draftSelector;
            }
            set
            {
                if (value != this.draftSelector)
                {
                    this.draftSelector = value;
                    base.OnPropertyChangedWithValue(value, "DraftSelector");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> GarrisonSelector
        {
            get
            {
                return this.garrisonSelector;
            }
            set
            {
                if (value != this.garrisonSelector)
                {
                    this.garrisonSelector = value;
                    base.OnPropertyChangedWithValue(value, "GarrisonSelector");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> MilitiaSelector
        {
            get
            {
                return this.militiaSelector;
            }
            set
            {
                if (value != this.militiaSelector)
                {
                    this.militiaSelector = value;
                    base.OnPropertyChangedWithValue(value, "MilitiaSelector");
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

        [DataSourceProperty]
        public PopulationOptionVM DraftingToogle
        {
            get => draftingToogle;
            set
            {
                if (value != draftingToogle)
                {
                    draftingToogle = value;
                    base.OnPropertyChangedWithValue(value, "DraftingToogle");
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
        public MBBindingList<InformationElement> DefenseInfo
        {
            get => defenseInfo;
            set
            {
                if (value != defenseInfo)
                {
                    defenseInfo = value;
                    base.OnPropertyChangedWithValue(value, "DefenseInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> ManpowerInfo
        {
            get => manpowerInfo;
            set
            {
                if (value != manpowerInfo)
                {
                    manpowerInfo = value;
                    base.OnPropertyChangedWithValue(value, "ManpowerInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> SiegeInfo
        {
            get => siegeInfo;
            set
            {
                if (value != siegeInfo)
                {
                    siegeInfo = value;
                    base.OnPropertyChangedWithValue(value, "SiegeInfo");
                }
            }
        }
    }
}
