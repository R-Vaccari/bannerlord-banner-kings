using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Innovations;
using BannerKings.Populations;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Linq;

namespace BannerKings.UI.Cultures
{
    public class EncyclopediaCultureVM : BannerKingsViewModel
    {
        private CultureObject culture;
        private MBBindingList<StringPairItemVM> information, traits;
        private MBBindingList<TripleStringItemVM> innovations;
        private string description;

        public EncyclopediaCultureVM(CultureObject culture) : base(null, false)
        {
            this.culture = culture;
            information = new MBBindingList<StringPairItemVM>();
            traits = new MBBindingList<StringPairItemVM>();
            innovations = new MBBindingList<TripleStringItemVM>();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Information.Clear();
            Traits.Clear();
            Innovations.Clear();

            Description = GameTexts.FindText("str_culture_description", culture.StringId).ToString();

            int settlements = 0;
            int population = 0;
            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.Culture != culture) continue;
                settlements++;
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null) population += data.TotalPop;
            }

            Language language = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(culture);

            Information.Add(new StringPairItemVM(new TextObject("{=!}Population:").ToString(),
                    population.ToString(), null));

            if (language != null) Information.Add(new StringPairItemVM(new TextObject("{=!}Language:").ToString(),
                    language.Name.ToString(), new BasicTooltipViewModel(() => language.Description.ToString())));

            Information.Add(new StringPairItemVM(new TextObject("{=!}Settlements:").ToString(),
                    settlements.ToString(), null));

            foreach (FeatObject trait in culture.GetCulturalFeats())
                Traits.Add(new StringPairItemVM(string.Empty,
                    trait.Description.ToString(), null));

            InnovationData innovationData = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(culture);
            if (innovationData != null)
            {

                if (innovationData.CulturalHead != null) Information.Add(new StringPairItemVM(new TextObject("{=!}Cultural Head:").ToString(),
                    innovationData.CulturalHead.Name.ToString(), null));

                if (innovationData.Fascination != null) Information.Add(new StringPairItemVM(new TextObject("{=!}Cultural Fascination:").ToString(),
                    innovationData.Fascination.Name.ToString(), new BasicTooltipViewModel(() => innovationData.Fascination.Description.ToString())));

                ExplainedNumber research = new ExplainedNumber(0f, true);
                foreach (Settlement settlement in Settlement.All)
                {
                    if (settlement.Culture != culture) continue;
                    research.Add(BannerKingsConfig.Instance.InnovationsModel.CalculateSettlementResearch(settlement).ResultNumber, settlement.Name);
                }

                Information.Add(new StringPairItemVM(new TextObject("{=!}Research (Daily):").ToString(),
                    research.ResultNumber.ToString("0.00"), new BasicTooltipViewModel(() => research.GetExplanations())));

                foreach (Innovation innovation in innovationData.Innovations)
                    Innovations.Add(new TripleStringItemVM(innovation.Name.ToString(),
                        innovation.Effects.ToString(),
                        new TextObject("{=!}{CURRENT}/{REQUIRED} ({PERCENTAGE})")
                        .SetTextVariable("CURRENT", innovation.CurrentProgress.ToString("0.00"))
                        .SetTextVariable("REQUIRED", innovation.RequiredProgress)
                        .SetTextVariable("PERCENTAGE", FormatValue(innovation.CurrentProgress / innovation.RequiredProgress))
                        .ToString(),
                        new BasicTooltipViewModel(() => innovation.Description.ToString())));
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
                    OnPropertyChangedWithValue(value, "Description");
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
                    OnPropertyChangedWithValue(value, "Information");
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
                    OnPropertyChangedWithValue(value, "Innovations");
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
                    OnPropertyChangedWithValue(value, "Traits");
                }
            }
        }
    }
}
