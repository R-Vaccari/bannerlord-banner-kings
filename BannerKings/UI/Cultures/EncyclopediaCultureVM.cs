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

        [DataSourceProperty] public string AssumeCultureHeadText => new TextObject("{=e12LxgBp}Culture Head").ToString();

        public string ChangeFascinationText => new TextObject("{=szdaPNy3}Fascination").ToString();

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

            Information.Add(new StringPairItemVM(new TextObject("{=VRbXbsPE}Population:").ToString(),
                population.ToString()));

            if (language != null)
            {
                Information.Add(new StringPairItemVM(new TextObject("{=kjkoLD9d}Language:").ToString(),
                    language.Name.ToString(), new BasicTooltipViewModel(() => language.Description.ToString())));
            }

            Information.Add(new StringPairItemVM(new TextObject("{=OC34ZRMB}Settlements:").ToString(),
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
                    Information.Add(new StringPairItemVM(new TextObject("{=AoqhWZU9}Cultural Head:").ToString(),
                        innovationData.CulturalHead.Name.ToString()));
                }

                if (innovationData.Fascination != null)
                {
                    Information.Add(new StringPairItemVM(new TextObject("{=eLLt37Yx}Cultural Fascination:").ToString(),
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

                Information.Add(new StringPairItemVM(new TextObject("{=mykO6Ydo}Research (Daily):").ToString(),
                    research.ResultNumber.ToString("0.00"), new BasicTooltipViewModel(() => research.GetExplanations())));

                foreach (var innovation in innovationData.Innovations)
                {
                    Innovations.Add(new TripleStringItemVM(innovation.Name.ToString(),
                        innovation.Effects.ToString(),
                        new TextObject("{=0ABQRW1Z}{CURRENT}/{REQUIRED} ({PERCENTAGE})")
                            .SetTextVariable("CURRENT", innovation.CurrentProgress.ToString("0.00"))
                            .SetTextVariable("REQUIRED", innovation.RequiredProgress)
                            .SetTextVariable("PERCENTAGE",
                                FormatValue(innovation.CurrentProgress / innovation.RequiredProgress))
                            .ToString(),
                        new BasicTooltipViewModel(() => innovation.Description.ToString())));
                }
            }
        }
    }
}