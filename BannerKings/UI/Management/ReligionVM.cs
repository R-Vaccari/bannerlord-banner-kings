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
    public class ReligionVM : BannerKingsViewModel
    {
        private MBBindingList<PopulationInfoVM> classesList;
        private MBBindingList<InformationElement> cultureInfo;
        private MBBindingList<InformationElement> religionsList;
        private readonly Settlement settlement;
        private MBBindingList<InformationElement> statsInfo;
       

        public ReligionVM(PopulationData data, Settlement _settlement, bool _isSelected) : base(data, _isSelected)
        {
            classesList = new MBBindingList<PopulationInfoVM>();
            religionsList = new MBBindingList<InformationElement>();
            cultureInfo = new MBBindingList<InformationElement>();
            statsInfo = new MBBindingList<InformationElement>();
            settlement = _settlement;
            RefreshValues();
        }

       

        [DataSourceProperty]
        public MBBindingList<InformationElement> ReligionList
        {
            get => religionsList;
            set
            {
                if (value != religionsList)
                {
                    religionsList = value;
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
        public MBBindingList<InformationElement> ReligionInfo
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
            

            if (data.ReligionData?.Religions == null)
            {
                return;
            }

            PopList.Clear();
            ReligionList.Clear();
            ReligionInfo.Clear();
            StatsInfo.Clear();

            float totalFaithsWeight = 0f;
            foreach (var pair in data.ReligionData.Religions)
            {
                totalFaithsWeight += BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(pair.Key, settlement)
                    .ResultNumber;

                ReligionList.Add(new InformationElement(
                    pair.Key.Faith.GetFaithName().ToString(), 
                    FormatValue(pair.Value),
                    new TextObject("{=!}{RELIGION}\nPresence: {PRESENCE}")
                    .SetTextVariable("RELIGION", pair.Key.Faith.GetFaithDescription())
                    .SetTextVariable("PRESENCE", FormatValue(pair.Value))
                    .ToString()));
            }
                


            StatsInfo.Add(new InformationElement("{=QxLXQ67c}Population Growth:",
                new BKGrowthModel().CalculateEffect(settlement, data).ResultNumber.ToString(),
                "{=FiU5oGi9}The population growth of your settlement on a daily basis, distributed among the classes."));

            StatsInfo.Add(new InformationElement("{=J1VG2giN}Foreigner Ratio:",
                FormatValue(new BKForeignerModel().CalculateEffect(settlement).ResultNumber),
                "{=gXeYeB24}Merchant and freemen foreigners that refuse to be assimilated, but have a living in this settlement."));



            Managers.Institutions.Religions.Religion dominant = data.ReligionData.DominantReligion;
            ReligionInfo.Add(new InformationElement(new TextObject("{=!}Dominant Faith:").ToString(),
                dominant.Faith.GetFaithName().ToString(),
                new TextObject("{=8ootTEcK}The most assimilated culture in this settlement, and considered the legal culture.").ToString()));


            ExplainedNumber presence = BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(dominant, settlement);
            ReligionInfo.Add(new InformationElement(new TextObject("{=!}Faith Presence:").ToString(),
                FormatValue(presence.ResultNumber / totalFaithsWeight),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=!}The faith's presence in the settlement. Presence describes how much of the population adheres to the faith. Presence is affected by various factors, such as the faith's fervor, and whether it accepts the culture's settlement or not."))
                    .SetTextVariable("EXPLANATIONS", presence.GetExplanations())
                    .ToString()));


            ExplainedNumber fervor = BannerKingsConfig.Instance.ReligionModel.CalculateFervor(dominant);
            ReligionInfo.Add(new InformationElement(new TextObject("{=!}Fervor:").ToString(),
                FormatValue(fervor.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=!}The faith's fervor. Faiths with more fervor naturally grow to a bigger share of the population. Fervor is based on doctrines, settlements and clans that follow the faith."))
                    .SetTextVariable("EXPLANATIONS", fervor.GetExplanations())
                    .ToString()));
        }
    }
}