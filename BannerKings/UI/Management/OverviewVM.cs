using BannerKings.Managers.Populations;
using BannerKings.Models.BKModels;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using System.Linq;
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
            
            var data = this.data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);

            PopList.Clear();
            CultureList.Clear();
            CultureInfo.Clear();
            StatsInfo.Clear();

            if (data is not {Classes: { }})
            {
                return;
            }

            data.Classes.ForEach(popClass => PopList
                .Add(new PopulationInfoVM(Utils.Helpers.GetClassName(popClass.type, settlement.Culture).ToString(),
                    popClass.count,
                    Utils.Helpers.GetClassHint(popClass.type, settlement.Culture))));

            data.CultureData.Cultures.ForEach(culture => CultureList
                .Add(new CultureElementVM(data, culture)));


            var totalCultureWeight = 0f;
            foreach (var culture in data.CultureData.Cultures)
            {
                totalCultureWeight += BannerKingsConfig.Instance.CultureAssimilationModel.CalculateCultureWeight(settlement, culture)
                    .ResultNumber;
            }


            var stability = BannerKingsConfig.Instance.StabilityModel.CalculateStabilityTarget(settlement);
            StatsInfo.Add(new InformationElement("Stability:", $"{data.Stability:P}",
                new TextObject("{=Uw3xBMKd}{TEXT}\nTarget: {TARGET}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject(
                            "{=MKfkuKiS}The overall stability of this settlement, affected by security, loyalty, assimilation and whether you are legally entitled to the settlement. Stability is the basis of economic prosperity."))
                    .SetTextVariable("EXPLANATIONS", stability.GetExplanations())
                    .SetTextVariable("TARGET", FormatValue(stability.ResultNumber))
                    .ToString()));

            var autonomy = BannerKingsConfig.Instance.StabilityModel.CalculateAutonomyTarget(settlement, data.Stability);
            StatsInfo.Add(new InformationElement("Autonomy:", $"{data.Autonomy:P}",
                new TextObject("{=Uw3xBMKd}{TEXT}\nTarget: {TARGET}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject(
                            "{=xMsWoSnL}Autonomy is inversely correlated to stability, therefore less stability equals more autonomy. Higher autonomy will reduce tax revenue while increasing loyalty. Matching culture with the settlement and setting a local notable as governor increases autonomy. Higher autonomy will also slow down assimilation"))
                    .SetTextVariable("EXPLANATIONS", autonomy.GetExplanations())
                    .SetTextVariable("TARGET", FormatValue(autonomy.ResultNumber))
                    .ToString()));

            var support = data.NotableSupport;
            StatsInfo.Add(new InformationElement("Notable Support:", $"{support.ResultNumber:P}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject(
                            "{=mVTYGkNP}Represents how much the local elite supports you. Support of each notable is weighted on their power, meaning that not having the support of a notable that holds most power will result in a small support percentage. Support is gained through better relations with the notables."))
                    .SetTextVariable("EXPLANATIONS", support.GetExplanations())
                    .ToString()));

            StatsInfo.Add(new InformationElement("Total Population:", $"{data.TotalPop:n0}",
                "Number of people present in this settlement and surrounding regions."));

            var influence = BannerKingsConfig.Instance.InfluenceModel.CalculateSettlementInfluence(settlement, data);
            StatsInfo.Add(new InformationElement(GameTexts.FindText("str_total_influence").ToString(),
                new TextObject("{=YrCRA6CA}{INFLUENCE}")
                    .SetTextVariable("INFLUENCE", influence.ResultNumber.ToString("0.00"))
                    .ToString(),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=8mSDgwhX}The amount of influence this settlement provides in your realm."))
                    .SetTextVariable("EXPLANATIONS", influence.GetExplanations())
                    .ToString()));

            StatsInfo.Add(new InformationElement("Population Growth:",
                $"{new BKGrowthModel().CalculateEffect(settlement, data).ResultNumber:P}",
                new TextObject("{=!}The population growth of your settlement on a daily basis, distributed among the classes.")
                .ToString()));
            StatsInfo.Add(new InformationElement("Foreigner Ratio:",
                FormatValue(new BKForeignerModel().CalculateEffect(settlement).ResultNumber),
                new TextObject("{=!}Merchant and freemen foreigners that refuse to be assimilated, but have a living in this settlement.")
                .ToString()));


            CultureInfo.Add(new InformationElement("Dominant Culture:",
                data.CultureData.DominantCulture.Name.ToString(),
                new TextObject("{=!}The most assimilated culture in this settlement, and considered the legal culture.")
                .ToString()));

            var heroCulture = data.CultureData.Cultures.FirstOrDefault(x => x.Culture == Hero.MainHero.Culture);
            if (heroCulture != null)
            {
                var presence = BannerKingsConfig.Instance.CultureAssimilationModel.CalculateCultureWeight(settlement, heroCulture);
                CultureInfo.Add(new InformationElement("Cultural Presence:",
                    FormatValue(presence.ResultNumber / totalCultureWeight),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=!}How present your culture is. Presence is affected by notables and governor following your culture, as well as other factors. This is the percentage that you culture's assimilation will gravitate towards in the settlement."))
                    .SetTextVariable("EXPLANATIONS", presence.GetExplanations())
                    .ToString()));
            }
          

            CultureInfo.Add(new InformationElement(new TextObject("{=!}Cultural Acceptance:").ToString(),
                $"{data.CultureData.GetAcceptance(Hero.MainHero.Culture):P}",
                new TextObject("{=!}How accepted your culture is towards the general populace. A culture first needs to be accepted to be assimilated into. Acceptance is gained through a stable reign and competent governor in place.")
                .ToString()));
            CultureInfo.Add(new InformationElement(new TextObject("{=!}Cultural Assimilation:").ToString(),
                $"{data.CultureData.GetAssimilation(Hero.MainHero.Culture):P}",
                new TextObject("{=!}Percentage of the population that shares culture with you. Assimilation will drift towards the target set by the cultural presence.")
                .ToString()));

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