using BannerKings.Managers.Populations;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Management
{
    public class ReligionVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> notablesList;
        private MBBindingList<InformationElement> cultureInfo;
        private MBBindingList<InformationElement> religionsList;
        private readonly Settlement settlement;
        private MBBindingList<InformationElement> statsInfo;
       

        public ReligionVM(PopulationData data, Settlement _settlement, bool _isSelected) : base(data, _isSelected)
        {
            notablesList = new MBBindingList<InformationElement>();
            religionsList = new MBBindingList<InformationElement>();
            cultureInfo = new MBBindingList<InformationElement>();
            statsInfo = new MBBindingList<InformationElement>();
            settlement = _settlement;
            RefreshValues();
        }

        [DataSourceProperty]
        public string FaithText => new TextObject("{=OKw2P9m1}Faith").ToString();


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
        public MBBindingList<InformationElement> NotablesList
        {
            get => notablesList;
            set
            {
                if (value != notablesList)
                {
                    notablesList = value;
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

            NotablesList.Clear();
            ReligionList.Clear();
            ReligionInfo.Clear();
            StatsInfo.Clear();

            var totalFaithsWeight = 0f;
            
            foreach (var pair in data.ReligionData.Religions)
            {
                totalFaithsWeight += BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(pair.Key, settlement)
                    .ResultNumber;

                ReligionList.Add(new InformationElement(
                    pair.Key.Faith.GetFaithName().ToString(), 
                    FormatValue(pair.Value),
                    new TextObject("{=tJePaqqg}{RELIGION}\nPresence: {PRESENCE}")
                    .SetTextVariable("RELIGION", pair.Key.Faith.GetFaithDescription())
                    .SetTextVariable("PRESENCE", FormatValue(pair.Value))
                    .ToString()));
            }

            foreach (var notable in settlement.Notables)
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager
                                           .GetHeroReligion(notable);

                if (rel == null)
                {
                    continue;
                }

                var factor = BannerKingsConfig.Instance.ReligionModel.GetNotableFactor(notable, settlement);
                var result = FormatValue(factor / totalFaithsWeight);
                NotablesList.Add(new InformationElement(
                    notable.Name.ToString(),
                    result,
                    new TextObject("{=GxcNDrXt}{HERO} holds sway over {PERCENTAGE} of the population. Changing their faith would strengthen the new faith's grip in the settlement.")
                    .SetTextVariable("HERO", notable.Name)
                    .SetTextVariable("PERCENTAGE", result)
                    .ToString()));
            }


            var tension = BannerKingsConfig.Instance.ReligionModel.CalculateTensionTarget(data.ReligionData);
            StatsInfo.Add(new InformationElement(new TextObject("{=QiyEsZ4L}Religious Tension:").ToString(),
                FormatValue(tension.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=cz0WGSC1}Tensions between the different faiths in this settlement. The less homogenous the population's faith is, the more tensions there are. Tensions are also affected by the dominant religion's view on the other faiths. Tolerated faiths do not incur extra tensions. Untolerated faiths do, and hostile faiths incur a lot of tension. Religious tensions will significantly affect your settlement's loyalty and performance."))
                    .SetTextVariable("EXPLANATIONS", tension.GetExplanations())
                    .ToString()));


            var dominant = data.ReligionData.DominantReligion;
            ReligionInfo.Add(new InformationElement(new TextObject("{=ZcGwd8sq}Dominant Faith:").ToString(),
                dominant.Faith.GetFaithName().ToString(),
                new TextObject("{=8ootTEcK}The most assimilated culture in this settlement, and considered the legal culture.").ToString()));


            var presence = BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(dominant, settlement);
            ReligionInfo.Add(new InformationElement(new TextObject("{=gTzbdsBY}Faith Presence:").ToString(),
                FormatValue(presence.ResultNumber / totalFaithsWeight),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=STxuNCBU}The faith's presence in the settlement. Presence describes how much of the population adheres to the faith. Presence is affected by various factors, such as the faith's fervor, and whether it accepts the culture's settlement or not."))
                    .SetTextVariable("EXPLANATIONS", presence.GetExplanations())
                    .ToString()));


            var fervor = BannerKingsConfig.Instance.ReligionModel.CalculateFervor(dominant);
            ReligionInfo.Add(new InformationElement(new TextObject("{=PUwmzUZy}Fervor:").ToString(),
                FormatValue(fervor.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=ukxmkmQR}The faith's fervor. Faiths with more fervor naturally grow to a bigger share of the population. Fervor is based on doctrines, settlements and clans that follow the faith."))
                    .SetTextVariable("EXPLANATIONS", fervor.GetExplanations())
                    .ToString()));
        }
    }
}