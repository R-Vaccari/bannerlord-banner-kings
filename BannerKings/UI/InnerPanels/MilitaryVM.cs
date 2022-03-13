using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Models;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private DecisionElement conscriptionToogle, subsidizeToogle, rationToogle;
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
                    "The maximum number of militiamen this settlement can support, based on it's population."));
            DefenseInfo.Add(new InformationElement("Militia Quality:", FormatValue(new BKMilitiaModel().CalculateEliteMilitiaSpawnChance(settlement)),
                    "Chance of militiamen being spawned as veterans instead of recruits."));

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

            HashSet<BannerKingsDecision> decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
            if (base.HasTown)
            {
                SiegeInfo.Add(new InformationElement("Storage Limit:", settlement.Town.FoodStocksUpperLimit().ToString(),
                    "The amount of food this settlement is capable of storing."));
                SiegeInfo.Add(new InformationElement("Estimated Holdout:", string.Format("{0} Days", base.data.MilitaryData.Holdout),
                    "How long this settlement will take to start starving in case of a siege."));

                StringBuilder sb = new StringBuilder();
                sb.Append(base.data.MilitaryData.Catapultae);
                sb.Append(", ");
                sb.Append(base.data.MilitaryData.Catapultae);
                sb.Append(", ");
                sb.Append(base.data.MilitaryData.Trebuchets);
                sb.Append(" (Ballis., Catap., Treb.)");
                SiegeInfo.Add(new InformationElement("Engines:", sb.ToString(),
                    "Pre-built siege engines to defend the walls, in case of siege."));

                garrisonItem = (BKGarrisonPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "garrison");
                GarrisonSelector = base.GetSelector(garrisonItem, new Action<SelectorVM<BKItemVM>>(this.garrisonItem.OnChange));
                GarrisonSelector.SelectedIndex = garrisonItem.Selected;
                GarrisonSelector.SetOnChangeAction(this.garrisonItem.OnChange);

                

                BannerKingsDecision rationDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");

                rationToogle = new DecisionElement()
                .SetAsBooleanOption(rationDecision.GetName(), rationDecision.Enabled, delegate (bool value)
                {
                    rationDecision.OnChange(value);
                    this.RefreshValues();

                }, new TextObject(rationDecision.GetHint()));
            }

            militiaItem = (BKMilitiaPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "militia");
            MilitiaSelector = base.GetSelector(militiaItem, new Action<SelectorVM<BKItemVM>>(this.militiaItem.OnChange));
            MilitiaSelector.SelectedIndex = militiaItem.Selected;
            MilitiaSelector.SetOnChangeAction(this.militiaItem.OnChange);

            draftItem = (BKDraftPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "draft");
            DraftSelector = base.GetSelector(draftItem, new Action<SelectorVM<BKItemVM>>(this.draftItem.OnChange));
            DraftSelector.SelectedIndex = draftItem.Selected;
            DraftSelector.SetOnChangeAction(this.draftItem.OnChange);

            BannerKingsDecision conscriptionDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_militia_encourage");
            BannerKingsDecision subsidizeDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_militia_subsidize");  

            conscriptionToogle = new DecisionElement()
                .SetAsBooleanOption(conscriptionDecision.GetName(), conscriptionDecision.Enabled, delegate (bool value)
                {
                    conscriptionDecision.OnChange(value);
                    this.RefreshValues();

                }, new TextObject(conscriptionDecision.GetHint()));

            subsidizeToogle = new DecisionElement()
                .SetAsBooleanOption(subsidizeDecision.GetName(), subsidizeDecision.Enabled, delegate (bool value)
                {
                    subsidizeDecision.OnChange(value);
                    this.RefreshValues();

                }, new TextObject(subsidizeDecision.GetHint()));

            

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
        public DecisionElement SubsidizeToogle
        {
            get => subsidizeToogle;
            set
            {
                if (value != subsidizeToogle)
                {
                    subsidizeToogle = value;
                    base.OnPropertyChangedWithValue(value, "SubsidizeToogle");
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement RationToogle
        {
            get => rationToogle;
            set
            {
                if (value != rationToogle)
                {
                    rationToogle = value;
                    base.OnPropertyChangedWithValue(value, "RationToogle");
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement ConscriptionToogle
        {
            get => conscriptionToogle;
            set
            {
                if (value != conscriptionToogle)
                {
                    conscriptionToogle = value;
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
