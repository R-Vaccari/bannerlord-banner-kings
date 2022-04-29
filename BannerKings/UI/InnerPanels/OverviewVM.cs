﻿using BannerKings.Managers.Decisions;
using BannerKings.Models;
using BannerKings.Models.Populations;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI
{
    public class OverviewVM : BannerKingsViewModel
    {
        private MBBindingList<PopulationInfoVM> classesList;
        private MBBindingList<CultureElementVM> culturesList;
        private MBBindingList<InformationElement> cultureInfo;
        private MBBindingList<InformationElement> statsInfo;
        private DecisionElement foreignerToogle;
        private Settlement settlement;
        private bool _isSelected;
        private PopulationData data;

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

        public override void RefreshValues()
        {
            base.RefreshValues();
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            this.data = data;
            PopList.Clear();
            CultureList.Clear();
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
                    "The overall stability of this settlement, affected by security, loyalty, assimilation and whether you are legally entitled to the settlement. Stability is the basis of economic prosperity."));
                StatsInfo.Add(new InformationElement("Total Population:", data.TotalPop.ToString(),
                    "Number of people present in this settlement and surrounding regions."));
                StatsInfo.Add(new InformationElement("Population Growth:", new BKGrowthModel().CalculateEffect(settlement, data).ResultNumber.ToString(), 
                    "The population growth of your settlement on a daily basis, distributed among the classes."));
                StatsInfo.Add(new InformationElement("Foreigner Ratio:", FormatValue(new BKForeignerModel().CalculateEffect(settlement).ResultNumber),
                    "Merchant and freemen foreigners that refuse to be assimilated, but have a living in this settlement."));

                CultureInfo.Add(new InformationElement("Dominant Culture:", data.CultureData.DominantCulture.Name.ToString(),
                    "The most assimilated culture in this settlement, and considered the legal culture."));
                CultureInfo.Add(new InformationElement("Cultural Acceptance:", FormatValue(data.CultureData.GetAcceptance(Hero.MainHero.Culture)),
                    "How accepted your culture is towards the general populace. A culture first needs to be accepted to be assimilated into."));
                CultureInfo.Add(new InformationElement("Cultural Assimilation:", FormatValue(data.CultureData.GetAssimilation(Hero.MainHero.Culture)),
                    "Percentage of the population that shares culture with you. Assimilating foreign settlements requires a competent governor that shares your culture."));

                List<BannerKingsDecision> decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
                foreach (BannerKingsDecision decision in decisions)
                {
                    DecisionElement vm = new DecisionElement()
                    .SetAsBooleanOption(decision.GetName(), decision.Enabled, delegate (bool value)
                    {
                        decision.OnChange(value);
                        RefreshValues();

                    }, new TextObject(decision.GetHint()));
                    switch (decision.GetIdentifier())
                    {
                        case "decision_foreigner_ban":
                            foreignerToogle = vm;
                            break;
                    }
                }
            } 
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
    }
}
