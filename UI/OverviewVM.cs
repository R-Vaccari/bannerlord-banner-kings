using Populations.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using static Populations.PopulationManager;

namespace Populations.UI
{
    public class OverviewVM : ViewModel
    {
        private MBBindingList<PopulationInfoVM> _popInfo;
        private MBBindingList<InformationElement> _satisfactionInfo;
        private MBBindingList<InformationElement> _statsInfo;
        private MBBindingList<InformationElement> _foodInfo;
        private MBBindingList<InformationElement> _productionInfo;
        private MBBindingList<InformationElement> _defenseInfo;
        private Settlement _settlement;
        private bool _isSelected;

        public OverviewVM(Settlement _settlement, bool _isSelected)
        {
            _defenseInfo = new MBBindingList<InformationElement>();
            _popInfo = new MBBindingList<PopulationInfoVM>();
            _satisfactionInfo = new MBBindingList<InformationElement>();
            _statsInfo = new MBBindingList<InformationElement>();
            _foodInfo = new MBBindingList<InformationElement>();
            _productionInfo = new MBBindingList<InformationElement>();
            this._settlement = _settlement;
            this._isSelected = _isSelected;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(_settlement);
            PopInfo.Clear();
            SatisfactionInfo.Clear();
            StatsInfo.Clear();
            FoodInfo.Clear();
            ProductionInfo.Clear();
            DefenseInfo.Clear();
            if (data != null && data.Classes != null)
            {
                data.Classes.ForEach(popClass => PopInfo
                .Add(new PopulationInfoVM(Helpers.Helpers.GetClassName(popClass.type, _settlement.Culture).ToString(), popClass.count,
                    Helpers.Helpers.GetClassHint(popClass.type, _settlement.Culture))));

                for (int i = 0; i < 4; i++)
                {
                    float value = data.GetSatisfactions()[i];
                    ConsumptionType type = (ConsumptionType)i;
                    string desc = type.ToString() + " Goods:";
                    SatisfactionInfo.Add(new InformationElement(desc, FormatValue(value), Helpers.Helpers.GetConsumptionHint(type)));
                }

                StatsInfo.Add(new InformationElement("Stability:", FormatValue(data.Stability),
                    "The overall stability of this settlement, affected by security, loyalty, assimilation and whether you are legally entitled to the settlement. Stability is the basis of economic prosperity"));
                StatsInfo.Add(new InformationElement("Population Growth:", new GrowthModel().GetPopulationGrowth(_settlement, true).ToString(), 
                    "The population growth of your settlement on a daily basis, distributed among the classes"));
                StatsInfo.Add(new InformationElement("Administrative Cost:", FormatValue(new AdministrativeModel().CalculateAdministrativeCost(_settlement)),
                    "Costs associated with the settlement administration, including those of active policies and decisions, deducted on tax revenue"));
                StatsInfo.Add(new InformationElement("Cultural Assimilation:", FormatValue(data.Assimilation),
                    "Percentage of the population that shares culture with you. Assimilating foreign settlements requires a competent governor that shares your culture"));

                FoodInfo.Add(new InformationElement("Storage Limit:", _settlement.Town.FoodStocksUpperLimit().ToString(), 
                    "The amount of food this settlement is capable of storing"));
                FoodInfo.Add(new InformationElement("Estimated Holdout:", string.Format("{0} Days", new FoodModel().GetFoodEstimate(_settlement.Town, true, _settlement.Town.FoodStocksUpperLimit())),
                    "How long this settlement will take to start starving in case of a siege"));

                ProductionInfo.Add(new InformationElement("Tariff:", FormatValue(new TaxModel().GetTownTaxRatio(_settlement.Town)),
                    "Percentage of an item's value charged as tax when sold"));
                ProductionInfo.Add(new InformationElement("Merchants' Revenue:", new EconomyModel().GetMerchantIncome(_settlement.Town).ToString(),
                   "Daily revenue of local merchants, based on slave workforce and production efficiency"));
                //ProductionInfo.Add(new InformationElement("Population Cap:", new GrowthModel().CalculateSettlementCap(_settlement).ToString(),
                //    "The maximum capacity of people this settlement can naturally support"));
                ProductionInfo.Add(new InformationElement("Production Efficiency:", FormatValue(new FeudalWorkshopModel().GetPolicyEffectToProduction(_settlement.Town)),
                    "The speed at which workshops produce goods, affected by kingdom policies and craftsmen"));

                DefenseInfo.Add(new InformationElement("Militia Cap:", new MilitiaModel().GetMilitiaLimit(data, _settlement.IsCastle).ToString(),
                    "The maximum number of militiamen this settlement can support, based on it's population"));
                DefenseInfo.Add(new InformationElement("Militia Quality:", FormatValue(new MilitiaModel().CalculateEliteMilitiaSpawnChance(_settlement)),
                        "Chance of militiamen being spawned as veterans instead of recruits"));
            } 
        }

        private string FormatValue(float value) => (value * 100f).ToString("0.00") + '%';

        [DataSourceProperty]
        public bool IsSelected
        {
            get => this._isSelected;
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
        public MBBindingList<InformationElement> DefenseInfo
        {
            get => _defenseInfo;
            set
            {
                if (value != _defenseInfo)
                {
                    _defenseInfo = value;
                    base.OnPropertyChangedWithValue(value, "DefenseInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> ProductionInfo
        {
            get => _productionInfo;
            set
            {
                if (value != _productionInfo)
                {
                    _productionInfo = value;
                    base.OnPropertyChangedWithValue(value, "ProductionInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> FoodInfo
        {
            get => _foodInfo;
            set
            {
                if (value != _foodInfo)
                {
                    _foodInfo = value;
                    base.OnPropertyChangedWithValue(value, "FoodInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> SatisfactionInfo
        {
            get => _satisfactionInfo;
            set
            {
                if (value != _satisfactionInfo)
                {
                    _satisfactionInfo = value;
                    base.OnPropertyChangedWithValue(value, "SatisfactionInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> StatsInfo
        {
            get => _statsInfo;
            set
            {
                if (value != _statsInfo)
                {
                    _statsInfo = value;
                    base.OnPropertyChangedWithValue(value, "StatsInfo");
                }
            }
        }

        [DataSourceProperty]
        public string AdministrativeCost
        {
            get
            {
                float cost = new AdministrativeModel().CalculateAdministrativeCost(_settlement);
                return FormatValue(cost);
            }
        }

        [DataSourceProperty]
        public string PopGrowth
        {
            get
            {
                int growth = new GrowthModel().GetPopulationGrowth(_settlement, false);
                return growth.ToString() + " (Daily)";
            }
        }

        [DataSourceProperty]
        public string Assimilation
        {
            get
            {
                float result = PopulationConfig.Instance.PopulationManager.GetPopData(_settlement).Assimilation;
                return (result * 100f).ToString() + '%';
            }
        }
    }
}
