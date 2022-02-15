using BannerKings.Models;
using BannerKings.Models.Populations;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI
{
    public class OverviewVM : ViewModel
    {
        private MBBindingList<PopulationInfoVM> classesList;
        private MBBindingList<CultureElementVM> culturesList;
        private MBBindingList<InformationElement> cultureInfo;
        private MBBindingList<InformationElement> statsInfo;
        private Settlement settlement;
        private bool _isSelected;
        private PopulationData data;

        public OverviewVM(Settlement _settlement, bool _isSelected)
        {
            classesList = new MBBindingList<PopulationInfoVM>();
            culturesList = new MBBindingList<CultureElementVM>();
            cultureInfo = new MBBindingList<InformationElement>();
            statsInfo = new MBBindingList<InformationElement>();
            this.settlement = _settlement;
            this._isSelected = _isSelected;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            this.data = data;
            PopList.Clear();
            CultureInfo.Clear();
            StatsInfo.Clear();
            if (data != null && data.Classes != null)
            {
                data.Classes.ForEach(popClass => PopList
                    .Add(new PopulationInfoVM(Helpers.Helpers.GetClassName(popClass.type, settlement.Culture).ToString(), popClass.count,
                        Helpers.Helpers.GetClassHint(popClass.type, settlement.Culture))));

                data.CultureData.Cultures.ForEach(culture => CultureList
                    .Add(new CultureElementVM(data, culture)));

                StatsInfo.Add(new InformationElement("Stability:", FormatValue(data.Stability),
                    "The overall stability of this settlement, affected by security, loyalty, assimilation and whether you are legally entitled to the settlement. Stability is the basis of economic prosperity"));
                StatsInfo.Add(new InformationElement("Total Population:", data.TotalPop.ToString(),
                    "Number of people present in this settlement and surrounding regions"));
                StatsInfo.Add(new InformationElement("Population Growth:", new BKGrowthModel().CalculateEffect(settlement, data).ResultNumber.ToString(), 
                    "The population growth of your settlement on a daily basis, distributed among the classes"));

                CultureInfo.Add(new InformationElement("Foreigner Ratio:", FormatValue(new BKForeignerModel().CalculateEffect(settlement).ResultNumber),
                    "Foreigners of all kinds are naturally attracted to economically booming towns. This represents a share of the population that is impervious to assimilation"));
                CultureInfo.Add(new InformationElement("Dominant Culture:", data.CultureData.DominantCulture.Name.ToString(),
                    "The most assimilated culture in this settlement"));
                CultureInfo.Add(new InformationElement("Cultural Acceptance:", FormatValue(data.CultureData.GetAssimilation(Hero.MainHero.Culture)),
                    "How accepted your culture is towards the general populace. A culture first needs to be accepted to be assimilated into"));
                CultureInfo.Add(new InformationElement("Cultural Assimilation:", FormatValue(data.CultureData.GetAssimilation(Hero.MainHero.Culture)),
                    "Percentage of the population that shares culture with you. Assimilating foreign settlements requires a competent governor that shares your culture"));

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
        public MBBindingList<CultureElementVM> CultureList
        {
            get => culturesList;
            set
            {
                if (value != culturesList)
                {
                    culturesList = value;
                    base.OnPropertyChangedWithValue(value, "CultureList");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PopulationInfoVM> PopList
        {
            get => classesList;
            set
            {
                if (value != classesList)
                {
                    classesList = value;
                    base.OnPropertyChangedWithValue(value, "PopList");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> CultureInfo
        {
            get => cultureInfo;
            set
            {
                if (value != cultureInfo)
                {
                    cultureInfo = value;
                    base.OnPropertyChangedWithValue(value, "CultureInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> StatsInfo
        {
            get => statsInfo;
            set
            {
                if (value != statsInfo)
                {
                    statsInfo = value;
                    base.OnPropertyChangedWithValue(value, "StatsInfo");
                }
            }
        }

        [DataSourceProperty]
        public string AdministrativeCost
        {
            get
            {
                float cost = new AdministrativeModel().CalculateAdministrativeCost(settlement);
                return FormatValue(cost);
            }
        }

        [DataSourceProperty]
        public string PopGrowth
        {
            get
            {
                int growth = (int)new BKGrowthModel().CalculateEffect(settlement, data).ResultNumber;
                return growth.ToString() + " (Daily)";
            }
        }

        [DataSourceProperty]
        public string Assimilation
        {
            get
            {
                float result = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).CultureData.GetAssimilation(Hero.MainHero.Culture);
                return (result * 100f).ToString() + '%';
            }
        }
    }
}
