using BannerKings.Managers.Populations;
using BannerKings.Models.BKModels;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Management
{
    public class OverviewVM : BannerKingsViewModel
    {
        private bool _isSelected;
        private MBBindingList<PopulationInfoVM> classesList;
        private MBBindingList<InformationElement> cultureInfo;
        private MBBindingList<CultureElementVM> culturesList;
        private new PopulationData data;
        private DecisionElement foreignerToogle;
        private readonly Settlement settlement;
        private MBBindingList<InformationElement> statsInfo;

        public OverviewVM(PopulationData data, Settlement _settlement, bool _isSelected) : base(data, true)
        {
            classesList = new MBBindingList<PopulationInfoVM>();
            culturesList = new MBBindingList<CultureElementVM>();
            cultureInfo = new MBBindingList<InformationElement>();
            statsInfo = new MBBindingList<InformationElement>();
            settlement = _settlement;
            this._isSelected = _isSelected;
            RefreshValues();
        }

        [DataSourceProperty]
        public DecisionElement ForeignerToogle
        {
            get => foreignerToogle;
            set
            {
                if (value != foreignerToogle)
                {
                    foreignerToogle = value;
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            this.data = data;
            PopList.Clear();
            CultureList.Clear();
            CultureInfo.Clear();
            StatsInfo.Clear();
            if (data is {Classes: { }})
            {
                data.Classes.ForEach(popClass => PopList
                    .Add(new PopulationInfoVM(Utils.Helpers.GetClassName(popClass.type, settlement.Culture).ToString(),
                        popClass.count,
                        Utils.Helpers.GetClassHint(popClass.type, settlement.Culture))));

                data.CultureData.Cultures.ForEach(culture => CultureList
                    .Add(new CultureElementVM(data, culture)));

                var stability = BannerKingsConfig.Instance.StabilityModel.CalculateStabilityTarget(settlement);
                StatsInfo.Add(new InformationElement("Stability:", FormatValue(data.Stability),
                    new TextObject("{=!}{TEXT}\nTarget: {TARGET}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject(
                                "{=!}The overall stability of this settlement, affected by security, loyalty, assimilation and whether you are legally entitled to the settlement. Stability is the basis of economic prosperity."))
                        .SetTextVariable("EXPLANATIONS", stability.GetExplanations())
                        .SetTextVariable("TARGET", FormatValue(stability.ResultNumber))
                        .ToString()));

                var autonomy =
                    BannerKingsConfig.Instance.StabilityModel.CalculateAutonomyTarget(settlement, data.Stability);
                StatsInfo.Add(new InformationElement("Autonomy:", FormatValue(data.Autonomy),
                    new TextObject("{=!}{TEXT}\nTarget: {TARGET}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject(
                                "{=!}Autonomy is inversely correlated to stability, therefore less stability equals more autonomy. Higher autonomy will reduce tax revenue while increasing loyalty. Matching culture with the settlement and setting a local notable as governor increases autonomy. Higher autonomy will also slow down assimilation"))
                        .SetTextVariable("EXPLANATIONS", autonomy.GetExplanations())
                        .SetTextVariable("TARGET", FormatValue(autonomy.ResultNumber))
                        .ToString()));

                var support = data.NotableSupport;
                StatsInfo.Add(new InformationElement("Notable Support:", FormatValue(support.ResultNumber),
                    new TextObject("{=!}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject(
                                "{=!}Represents how much the local elite supports you. Support of each notable is weighted on their power, meaning that not having the support of a notable that holds most power will result in a small support percentage. Support is gained through better relations with the notables."))
                        .SetTextVariable("EXPLANATIONS", support.GetExplanations())
                        .ToString()));

                StatsInfo.Add(new InformationElement("Total Population:", data.TotalPop.ToString(),
                    "Number of people present in this settlement and surrounding regions."));

                var influence = BannerKingsConfig.Instance.InfluenceModel.CalculateSettlementInfluence(settlement, data);
                StatsInfo.Add(new InformationElement(GameTexts.FindText("str_total_influence").ToString(),
                    new TextObject("{=!}{INFLUENCE}")
                        .SetTextVariable("INFLUENCE", influence.ResultNumber.ToString("0.00"))
                        .ToString(),
                    new TextObject("{=!}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=!}The amount of influence this settlement provides in your realm."))
                        .SetTextVariable("EXPLANATIONS", influence.GetExplanations())
                        .ToString()));

                StatsInfo.Add(new InformationElement("Population Growth:",
                    new BKGrowthModel().CalculateEffect(settlement, data).ResultNumber.ToString(),
                    "The population growth of your settlement on a daily basis, distributed among the classes."));
                StatsInfo.Add(new InformationElement("Foreigner Ratio:",
                    FormatValue(new BKForeignerModel().CalculateEffect(settlement).ResultNumber),
                    "Merchant and freemen foreigners that refuse to be assimilated, but have a living in this settlement."));

                CultureInfo.Add(new InformationElement("Dominant Culture:",
                    data.CultureData.DominantCulture.Name.ToString(),
                    "The most assimilated culture in this settlement, and considered the legal culture."));
                CultureInfo.Add(new InformationElement("Cultural Acceptance:",
                    FormatValue(data.CultureData.GetAcceptance(Hero.MainHero.Culture)),
                    "How accepted your culture is towards the general populace. A culture first needs to be accepted to be assimilated into."));
                CultureInfo.Add(new InformationElement("Cultural Assimilation:",
                    FormatValue(data.CultureData.GetAssimilation(Hero.MainHero.Culture)),
                    "Percentage of the population that shares culture with you. Assimilating foreign settlements requires a competent governor that shares your culture."));

                var decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
                foreach (var decision in decisions)
                {
                    var vm = new DecisionElement()
                        .SetAsBooleanOption(decision.GetName(), decision.Enabled, delegate(bool value)
                        {
                            decision.OnChange(value);
                            RefreshValues();
                        }, new TextObject(decision.GetHint()));
                    foreignerToogle = decision.GetIdentifier() switch
                    {
                        "decision_foreigner_ban" => vm,
                        _ => foreignerToogle
                    };
                }
            }
        }
    }
}