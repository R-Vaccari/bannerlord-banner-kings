using System.Collections.Generic;
using BannerKings.Managers.Innovations;
using BannerKings.UI.Items;
using Bannerlord.UIExtenderEx.Attributes;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Cultures
{
    public class EncyclopediaCultureVM : BannerKingsViewModel
    {
        private bool assumeHead, changeFascination;
        private readonly CultureObject culture;
        private string description;
        private MBBindingList<StringPairItemVM> information, traits;
        private MBBindingList<TripleStringItemVM> innovations;

        public EncyclopediaCultureVM(CultureObject culture) : base(null, false)
        {
            this.culture = culture;
            information = new MBBindingList<StringPairItemVM>();
            traits = new MBBindingList<StringPairItemVM>();
            innovations = new MBBindingList<TripleStringItemVM>();
            assumeHead = false;
            changeFascination = false;
            RefreshValues();
        }

        [DataSourceProperty] public string AssumeCultureHeadText => new TextObject("{=4CGhDHo8B}Culture Head").ToString();

        public string ChangeFascinationText => new TextObject("{=m52mnYOfz}Fascination").ToString();

        [DataSourceProperty]
        public bool ChangeFascinationPossible
        {
            get => changeFascination;
            set
            {
                if (value != changeFascination)
                {
                    changeFascination = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool AssumeHeadPossible
        {
            get => assumeHead;
            set
            {
                if (value != assumeHead)
                {
                    assumeHead = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string Description
        {
            get => description;
            set
            {
                if (value != description)
                {
                    description = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> Information
        {
            get => information;
            set
            {
                if (value != information)
                {
                    information = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<TripleStringItemVM> Innovations
        {
            get => innovations;
            set
            {
                if (value != innovations)
                {
                    innovations = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> Traits
        {
            get => traits;
            set
            {
                if (value != traits)
                {
                    traits = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Information.Clear();
            Traits.Clear();
            Innovations.Clear();

            Description = GameTexts.FindText("str_culture_description", culture.StringId).ToString();

            var settlements = 0;
            var population = 0;
            foreach (var settlement in Settlement.All)
            {
                if (settlement.Culture != culture)
                {
                    continue;
                }

                settlements++;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null)
                {
                    population += data.TotalPop;
                }
            }

            var language = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(culture);

            Information.Add(new StringPairItemVM(new TextObject("{=QiXM0kSWS}Population:").ToString(),
                population.ToString()));

            if (language != null)
            {
                Information.Add(new StringPairItemVM(new TextObject("{=st3ZPv2sY}Language:").ToString(),
                    language.Name.ToString(), new BasicTooltipViewModel(() => language.Description.ToString())));
            }

            Information.Add(new StringPairItemVM(new TextObject("{=FJYqPUPp8}Settlements:").ToString(),
                settlements.ToString()));

            foreach (var trait in culture.GetCulturalFeats())
            {
                Traits.Add(new StringPairItemVM(string.Empty,
                    trait.Description.ToString()));
            }

            var innovationData = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(culture);
            if (innovationData != null)
            {
                if (innovationData.CulturalHead != null)
                {
                    Information.Add(new StringPairItemVM(new TextObject("{=noSiBYiic}Cultural Head:").ToString(),
                        innovationData.CulturalHead.Name.ToString()));
                }

                if (innovationData.Fascination != null)
                {
                    Information.Add(new StringPairItemVM(new TextObject("{=18ZqX4wDz}Cultural Fascination:").ToString(),
                        innovationData.Fascination.Name.ToString(),
                        new BasicTooltipViewModel(() => innovationData.Fascination.Description.ToString())));
                }

                ChangeFascinationPossible = innovationData.CulturalHead == Clan.PlayerClan;
                AssumeHeadPossible = innovationData.CanAssumeCulturalHead(Clan.PlayerClan);

                var research = new ExplainedNumber(0f, true);
                foreach (var settlement in Settlement.All)
                {
                    if (settlement.Culture != culture)
                    {
                        continue;
                    }

                    research.Add(
                        BannerKingsConfig.Instance.InnovationsModel.CalculateSettlementResearch(settlement).ResultNumber,
                        settlement.Name);
                }

                Information.Add(new StringPairItemVM(new TextObject("{=tmWpVmX57}Research (Daily):").ToString(),
                    research.ResultNumber.ToString("0.00"), new BasicTooltipViewModel(() => research.GetExplanations())));

                foreach (var innovation in innovationData.Innovations)
                {
                    Innovations.Add(new TripleStringItemVM(innovation.Name.ToString(),
                        innovation.Effects.ToString(),
                        new TextObject("{=AKVUaaqBO}{CURRENT}/{REQUIRED} ({PERCENTAGE})")
                            .SetTextVariable("CURRENT", innovation.CurrentProgress.ToString("0.00"))
                            .SetTextVariable("REQUIRED", innovation.RequiredProgress)
                            .SetTextVariable("PERCENTAGE",
                                FormatValue(innovation.CurrentProgress / innovation.RequiredProgress))
                            .ToString(),
                        new BasicTooltipViewModel(() => innovation.Description.ToString())));
                }
            }
        }

        [DataSourceMethod]
        private void AssumeCultureHead()
        {
            var innovationData = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(culture);
            if (innovationData != null)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=4CGhDHo8B}Culture Head").ToString(),
                    new TextObject("{=EnnYER8ot}Assume the position of culture head.").ToString(), true, true,
                    GameTexts.FindText("str_confirm").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    () => innovationData.AssumeCulturalHead(Clan.PlayerClan),
                    null
                ));
            }
        }

        [DataSourceMethod]
        private void ChangeFascination()
        {
            var innovationData = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(culture);
            if (innovationData != null)
            {
                var elements = new List<InquiryElement>();
                foreach (var innovation in innovationData.Innovations)
                {
                    elements.Add(new InquiryElement(innovation,
                        innovation.Name.ToString(),
                        null,
                        innovationData.CanChangeFascination(Clan.PlayerClan, innovation),
                        innovation.Description.ToString()));
                }

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=sF8HWH4mP}Choose Fascination").ToString(),
                    new TextObject("{=qSt2FhWhz}The cultural fascination is an innovation that progresses faster than others.")
                        .ToString(),
                    elements, true, 1,
                    GameTexts.FindText("str_done").ToString(), string.Empty,
                    delegate(List<InquiryElement> x)
                    {
                        var innov = (Innovation) x[0].Identifier;
                        if (innov == null)
                        {
                            return;
                        }

                        innovationData.ChangeFascination(innov);
                        RefreshValues();
                    }, null));
            }
        }
    }
}