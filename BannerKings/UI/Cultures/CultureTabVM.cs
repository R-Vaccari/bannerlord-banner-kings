using BannerKings.Managers.Education;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Innovations.Eras;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using BannerKings.UI.Titles;
using Bannerlord.UIExtenderEx.Attributes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Cultures
{
    public class CultureTabVM : BannerKingsViewModel
    {
        private MBBindingList<InnovationElementVM> innovations;
        private MBBindingList<InformationElement> cultureInfo;
        private BannerKingsSelectorVM<EraSelectorItem> selector;
        private InnovationData innovationData;
        private ImageIdentifierVM banner;

        private Era Era { get; set; }

        public CultureTabVM() : base(null, false)
        {
            Innovations = new MBBindingList<InnovationElementVM>();
            CultureInfo = new MBBindingList<InformationElement>();
            Selector = new BannerKingsSelectorVM<EraSelectorItem>(true, 0, null);
            innovationData = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(Culture);
            int selected = 0;
            int index = 0;

            Managers.Institutions.Religions.Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetIdealReligion(Culture);
            if (rel != null)
            {
                Banner = new ImageIdentifierVM(rel.Faith.GetBanner());
            }

            foreach (Era era in DefaultEras.Instance.All)
            {
                Selector.AddItem(new EraSelectorItem(era));
                if (era.Equals(innovationData.Era))
                {
                    Era = era;
                    selected = index;
                }

                index++;
            }

            Selector.SelectedIndex = selected;
            Selector.SetOnChangeAction(OnChange);
        }

        private void OnChange(SelectorVM<EraSelectorItem> obj)
        {
            if (obj.SelectedItem != null)
            {
                Era = obj.SelectedItem.Era;
                RefreshValues();
            }
        }

        public CultureObject Culture => Settlement.CurrentSettlement.Culture;

        public override void RefreshValues()
        {
            base.RefreshValues();
            CultureInfo.Clear();
            Innovations.Clear();

            if (innovationData != null)
            {
                float requiredResearch = 0f;
                foreach (var i in innovationData.GetEraInnovations(Era))
                {
                    if (i.Requirement == null)
                    {
                        Innovations.Add(new InnovationElementVM(i, innovationData, Era));  
                    }

                    requiredResearch += i.RequiredProgress - i.CurrentProgress;
                }


                if (innovationData.CulturalHead != null)
                {
                    CultureInfo.Add(new InformationElement(new TextObject("{=AoqhWZU9}Cultural Head:").ToString(),
                        innovationData.CulturalHead.Name.ToString(),
                        new TextObject("{=!}The Cultural Head is the most proeminent lord or lady within a culture. They are able to determine a culture's Fascination, which is an innovation that develops faster. They are also more influential as a family. Usually this position is occupied by a ruler.").ToString()));
                }

                if (innovationData.Fascination != null)
                {
                    CultureInfo.Add(new InformationElement(new TextObject("{=eLLt37Yx}Cultural Fascination:").ToString(),
                        innovationData.Fascination.Name.ToString(),
                        new TextObject("{=!}A Cultural Fascination is an innovation that takes precedence over others. This innovation will receive more research points, developing faster while slowing down other innovations. The Cultural Head is responsible for deciding the fascination.\n{NAME}:\n{DESCRIPTION}")
                        .SetTextVariable("DESCRIPTION", innovationData.Fascination.Description)
                        .SetTextVariable("NAME", innovationData.Fascination.Name)
                        .ToString()));
                }

                var research = BannerKingsConfig.Instance.InnovationsModel.CalculateCultureResearch(Culture, true);

                float days = requiredResearch / research.ResultNumber;
                CultureInfo.Add(new InformationElement(new TextObject("{=!}Years Remaining:").ToString(),
                    new TextObject("{=!}{COUNT} years")
                    .SetTextVariable("COUNT", (days / (float)CampaignTime.DaysInYear).ToString("0.0"))
                    .ToString(),
                    new TextObject("{=!}How many years remanining until all innovations for this Era are done, approximately.").ToString()));

                CultureInfo.Add(new InformationElement(new TextObject("{=mykO6Ydo}Research (Daily):").ToString(),
                    research.ResultNumber.ToString("0.00"), 
                    research.GetExplanations()));
            }

            var settlements = 0;
            var population = 0;
            foreach (var settlement in Settlement.All)
            {
                if (settlement.Culture != Culture)
                {
                    continue;
                }

                settlements++;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null)
                {
                    population += (int)(data.TotalPop * data.CultureData.GetAssimilation(Culture));
                }
            }

            CultureInfo.Add(new InformationElement(new TextObject("{=VRbXbsPE}Population:").ToString(),
                population.ToString(),
                new TextObject("{=!}The approximate number of people that follow this culture, across the continent.").ToString()));

            var language = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(Culture);
            if (language != null)
            {
                CultureInfo.Add(new InformationElement(new TextObject("{=kjkoLD9d}Language:").ToString(),
                    language.Name.ToString(), 
                    language.Description.ToString()));
            }

            CultureInfo.Add(new InformationElement(new TextObject("{=J6oPqQmt}Settlements:").ToString(),
                settlements.ToString(),
                new TextObject("{=!}The number of settlements that predominantly follow this culture, across all realms.").ToString()));
        }

        [DataSourceProperty] public bool FascinationEnabled => innovationData.CulturalHead == Clan.PlayerClan;
        [DataSourceProperty] public string FascinationText => new TextObject("{=!}Change Fascination").ToString();
        [DataSourceProperty] public string ResearchText => new TextObject("{=!}Research").ToString();
        [DataSourceProperty] public HintViewModel ResearchHint => new HintViewModel(
            new TextObject("{=!}A Research project is a personal endeavour towards progressing an innovation. You can research an innovation regardless of being Cultural Head or not. Researching will increase your Scholarship as well as an additional skill, depending on the innovation's type (Buildings, Military, Civic, etc).{newline}{newline}To make progress in your research, visit a friendly fief, select Banner Kings -> Take an Action -> Research."));

        [DataSourceMethod]
        private void AssumeCultureHead()
        {
            if (innovationData != null)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=e12LxgBp}Culture Head").ToString(),
                    new TextObject("{=1WXxCssM}Assume the position of culture head.").ToString(), true, true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    () => innovationData.AssumeCulturalHead(Clan.PlayerClan),
                null
                ));
            }
        }

        [DataSourceMethod]
        private void DoResearch()
        {
            EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
            var elements = new List<InquiryElement>();
            foreach (var innovation in innovationData.Innovations)
            {
                elements.Add(new InquiryElement(innovation,
                    innovation.Name.ToString(),
                    null,
                    innovationData.CanResearch(innovation),
                    innovation.Description.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Choose Research").ToString(),
                new TextObject("{=!}Choose a research proejct. Your current research progress is {PROGRESS} research points, based on your Scholarship, Intelligence and perks. An innovation is researchable if it has no previous requirements or its direct requirement is already fully researched.{newline}{newline}To make progress in your research, visit a friendly fief, select Banner Kings -> Take an Action -> Research.")
                .SetTextVariable("PROGRESS", education.ResearchProgress)    
                .ToString(),
                elements, true, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate (List<InquiryElement> x)
                {
                    var innov = (Innovation)x[0].Identifier;
                    if (innov == null)
                    {
                        return;
                    }

                    education.SetResearch(innov);
                    RefreshValues();
                }, null));
        }

        [DataSourceMethod]
        private void ChangeFascination()
        {
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
                    new TextObject("{=QkJA3Qjb}Choose Fascination").ToString(),
                    new TextObject("{=!}The cultural fascination is an innovation that progresses faster than others. As Cultural Head, you can determine the fascination. Changing fascinations incurs no type of cost.")
                        .ToString(),
                    elements, true, 1,
                    GameTexts.FindText("str_done").ToString(), string.Empty,
                    delegate (List<InquiryElement> x)
                    {
                        var innov = (Innovation)x[0].Identifier;
                        if (innov == null)
                        {
                            return;
                        }

                        innovationData.ChangeFascination(innov);
                        RefreshValues();
                    }, null));
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Banner
        {
            get => banner;
            set
            {
                if (value != banner)
                {
                    banner = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<EraSelectorItem> Selector
        {
            get => selector;
            set
            {
                if (value != selector)
                {
                    selector = value;
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
        public MBBindingList<InnovationElementVM> Innovations
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
    }
}