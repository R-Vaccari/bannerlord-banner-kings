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
                totalCultureWeight += BannerKingsConfig.Instance.CultureModel.CalculateCultureWeight(settlement, culture)
                    .ResultNumber;
            }

            if (!settlement.IsVillage)
            {
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
            }
            else
            {
                var hearts = BannerKingsConfig.Instance.ProsperityModel.CalculateHearthChange(settlement.Village, true);
                StatsInfo.Add(new InformationElement(new TextObject("{=eJECyLLw}Hearth Growth:").ToString(),
                    FormatFloatGain(hearts.ResultNumber),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=wcHDxPQJ}The number of homes in this village. Hearths are used to calculated the population capacity. Each hearth on average houses 4 people. Increasing hearths allows for population to keep growing and thus making the village more productive and relevant."))
                        .SetTextVariable("EXPLANATIONS", hearts.GetExplanations())
                        .ToString()));
            }

            StatsInfo.Add(new InformationElement(new TextObject("{=9s1iC4kE}Total Population:").ToString(), 
                $"{data.TotalPop:n0}",
                new TextObject("{=zaCDpfP2}Number of people present in this settlement and surrounding regions.").ToString()));

            var cap = BannerKingsConfig.Instance.GrowthModel.CalculateSettlementCap(settlement, data);
            StatsInfo.Add(new InformationElement(new TextObject("{=OfNn72n9}Population Cap:").ToString(),
                ((int)cap.ResultNumber).ToString(),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=TSwN7iFc}The maximum number of people this settlement is capable of having."))
                    .SetTextVariable("EXPLANATIONS", cap.GetExplanations())
                    .ToString()));

            var growth = BannerKingsConfig.Instance.GrowthModel.CalculateEffect(settlement, data, true);
            StatsInfo.Add(new InformationElement(new TextObject("{=HcFqTg6k}Population Growth:").ToString(),
                FormatFloatGain((int)growth.ResultNumber).ToString(),
                 new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=hANYHGHR}The population growth of your settlement on a daily basis, distributed among the classes."))
                    .SetTextVariable("EXPLANATIONS", growth.GetExplanations())
                    .ToString()));

            var influence = BannerKingsConfig.Instance.InfluenceModel.CalculateSettlementInfluence(settlement, data, true);
            StatsInfo.Add(new InformationElement(GameTexts.FindText("str_total_influence").ToString(),
                FormatFloatGain(influence.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=8mSDgwhX}The amount of influence this settlement provides in your realm."))
                    .SetTextVariable("EXPLANATIONS", influence.GetExplanations())
                    .ToString()));

            /*StatsInfo.Add(new InformationElement(new TextObject("{=NP0RCoMH}Foreigner Ratio:").ToString(),
                FormatValue(new BKForeignerModel().CalculateEffect(settlement).ResultNumber),
                new TextObject("{=E2QuHXGQ}Merchant and freemen foreigners that refuse to be assimilated, but have a living in this settlement.")
                .ToString())); */

            var dominantCulture = data.CultureData.DominantCulture;
            CultureInfo.Add(new InformationElement(new TextObject("{=Vk6Maijr}Dominant Culture:").ToString(),
                dominantCulture.Name.ToString(),
                new TextObject("The most assimilated culture in this settlement, and considered the legal culture.")
                .ToString()));

            var dominantLanguage = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(dominantCulture);
            if (dominantLanguage != null)
            {
                CultureInfo.Add(new InformationElement(new TextObject("{=jnE7pP1M}Dominant Language:").ToString(),
               dominantLanguage.Name.ToString(),
               new TextObject("{=Lf9styae}Language spoken by population of the dominant culture. Speaking their language helps in creating cultural acceptance of your own language, if dominant culture is not your own.")
               .ToString()));
            }
           
            var heroCulture = data.CultureData.Cultures.FirstOrDefault(x => x.Culture == Hero.MainHero.Culture);
            if (heroCulture != null)
            {
                var presence = BannerKingsConfig.Instance.CultureModel.CalculateCultureWeight(settlement, heroCulture);
                CultureInfo.Add(new InformationElement(new TextObject("{=R8mbRNcL}Cultural Presence:").ToString(),
                    FormatValue(presence.ResultNumber / totalCultureWeight),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=36DgL50p}How present your culture is. Presence is affected by notables and governor following your culture, as well as other factors. This is the percentage that you culture's assimilation will gravitate towards in the settlement."))
                    .SetTextVariable("EXPLANATIONS", presence.GetExplanations())
                    .ToString()));


                var acceptanceGain = heroCulture.AcceptanceGain;
                CultureInfo.Add(new InformationElement(new TextObject("{=qKsaiPd0}Acceptance Gain:").ToString(),
                    FormatFloatGainPercentage(acceptanceGain.ResultNumber),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=ADimYYhg}How much acceptance your culture gains in this settlement, per day."))
                    .SetTextVariable("EXPLANATIONS", acceptanceGain.GetExplanations())
                    .ToString()));
            }
          
            CultureInfo.Add(new InformationElement(new TextObject("{=Lb5b7JZX}Cultural Acceptance:").ToString(),
                $"{data.CultureData.GetAcceptance(Hero.MainHero.Culture):P}",
                new TextObject("{=K4DsLUBQ}How accepted your culture is towards the general populace. A culture first needs to be accepted to be assimilated into. Acceptance is gained through a stable reign and competent governor in place.")
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