using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using Bannerlord.UIExtenderEx.Attributes;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Kingdoms
{
    public class KingdomDemesneVM : BannerKingsViewModel
    {
        private MBBindingList<DemesneLawVM> laws;
        private MBBindingList<HeirVM> heirs;
        private HeirVM mainHeir;
        private string successionDescription;

        public KingdomDemesneVM(FeudalTitle title, Kingdom kingdom) : base(null, false)
        {
            Title = title;
            Kingdom = kingdom;
            laws = new MBBindingList<DemesneLawVM>();
            Heirs = new MBBindingList<HeirVM>();
        }

        public FeudalTitle Title { get; private set; }
        public Kingdom Kingdom { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Laws.Clear();
            Heirs.Clear();

            if (Title != null)
            {
                SuccessionDescription = Title.Contract.Succession.Description.ToString();

                bool isKing = Kingdom.Leader == Hero.MainHero && Title.deJure == Hero.MainHero;
                foreach (var law in Title.Contract.DemesneLaws)
                {
                    Laws.Add(new DemesneLawVM(DefaultDemesneLaws.Instance.GetLawsByType(law.LawType)
                        .Where(x => x.IsAdequateForKingdom(Kingdom)).ToList(),
                        law,
                        isKing,
                        OnChange));
                }

                var candidates = BannerKingsConfig.Instance.TitleModel.GetSuccessionCandidates(Kingdom.Leader, Title);
                var explanations = new Dictionary<Hero, ExplainedNumber>();

                foreach (Hero hero in candidates)
                {
                    var explanation = BannerKingsConfig.Instance.TitleModel.GetSuccessionHeirScore(Kingdom.Leader, hero, Title, true);
                    explanations.Add(hero, explanation);
                }

                var sorted = (from x in explanations
                              orderby x.Value.ResultNumber descending
                              select x).Take(6);

                for (int i = 0; i < sorted.Count(); i++)
                {
                    var hero = sorted.ElementAt(i).Key;
                    var exp = sorted.ElementAt(i).Value;
                    if (i == 0)
                    {
                        MainHeir = new HeirVM(hero, exp);
                    }
                    else
                    {
                        Heirs.Add(new HeirVM(hero, exp));
                    }
                }
            }
        }

        private void OnChange(SelectorVM<BKDemesneLawItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                var resultLaw = DefaultDemesneLaws.Instance.All.FirstOrDefault(x => x.Equals(vm.DemesneLaw));
                if (resultLaw != null && !resultLaw.Equals(vm.DemesneLaw))
                {
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=yAPnOQQQ}Enact Law").ToString(),
                        new TextObject("{=RSWao3jU}Enact the {LAW} law thoughtout the demesne of {TITLE}. The law will be enacted for every title in the hierarchy.\n\nCost: {INFLUENCE} {INFLUENCE_ICON}")
                        .SetTextVariable("LAW", resultLaw.Name)
                        .SetTextVariable("TITLE", Title.FullName)
                        .SetTextVariable("INFLUENCE", resultLaw.InfluenceCost)
                        .SetTextVariable("INFLUENCE_ICON", GameTexts.FindText("str_html_influence_icon"))
                        .ToString(),
                        Hero.MainHero.Clan.Influence >= resultLaw.InfluenceCost,
                        true,
                        GameTexts.FindText("str_selection_widget_accept").ToString(),
                        GameTexts.FindText("str_selection_widget_cancel").ToString(),
                        () =>
                        {
                            Title.EnactLaw(resultLaw, Hero.MainHero);
                            RefreshValues();
                        },
                        () => RefreshValues()));
                }
            }
        }

        [DataSourceMethod]
        private void ChangeGovernment()
        {
            List<InquiryElement> aspects = new List<InquiryElement>(6);
            foreach (var government in DefaultGovernments.Instance.All)
            {
                aspects.Add(new InquiryElement(
                    government,
                    government.Name.ToString(),
                    null,
                    !government.Equals(Title.Contract.Government) && government.IsKingdomAdequate(Kingdom),
                    government.Description.ToString()));
            }

            ShowOptions(new TextObject("{=!}Governments"),
                new TextObject("{=!}"),
                aspects);
        }

        [DataSourceMethod]
        private void ChangeSuccession()
        {
            List<InquiryElement> aspects = new List<InquiryElement>(6);
            foreach (var succession in DefaultSuccessions.Instance.All)
            {
                aspects.Add(new InquiryElement(
                    succession,
                    succession.Name.ToString(),
                    null,
                    !succession.Equals(Title.Contract.Succession) && succession.IsKingdomAdequate(Kingdom),
                    succession.Description.ToString()));
            }

            ShowOptions(new TextObject("{=!}Successions"),
                new TextObject("{=!}"),
                aspects);
        }

        [DataSourceMethod]
        private void ChangeInheritance()
        {
            List<InquiryElement> aspects = new List<InquiryElement>(6);
            foreach (var inheritance in DefaultInheritances.Instance.All)
            {
                aspects.Add(new InquiryElement(
                    inheritance,
                    inheritance.Name.ToString(),
                    null,
                    !inheritance.Equals(Title.Contract.Inheritance),
                    inheritance.Description.ToString()));
            }

            ShowOptions(new TextObject("{=!}Inheritances"),
                new TextObject("{=!}"),
                aspects);
        }

        [DataSourceMethod]
        private void ChangeGender()
        {
            List<InquiryElement> aspects = new List<InquiryElement>(6);
            foreach (var genderLaw in DefaultGenderLaws.Instance.All)
            {
                aspects.Add(new InquiryElement(
                    genderLaw,
                    genderLaw.Name.ToString(),
                    null,
                    !genderLaw.Equals(Title.Contract.GenderLaw),
                    genderLaw.Description.ToString()));
            }

            ShowOptions(new TextObject("{=!}Gender Laws"),
                new TextObject("{=!}"),
                aspects);
        }

        private void ShowOptions(TextObject title, TextObject description, List<InquiryElement> aspects)
        {
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                title.ToString(),
                description.ToString(),
                aspects,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_selection_widget_cancel").ToString(),
                (List<InquiryElement> list) =>
                {
                    ContractAspect aspect = list.First().Identifier as ContractAspect;
                    FeudalContract contract;
                    if (aspect is Government) contract = new FeudalContract(null,
                        null,
                        aspect as Government,
                        Title.Contract.Succession,
                        Title.Contract.Inheritance,
                        Title.Contract.GenderLaw);
                    else if (aspect is Succession) contract = new FeudalContract(null,
                        null,
                        Title.Contract.Government,
                        aspect as Succession,
                        Title.Contract.Inheritance,
                        Title.Contract.GenderLaw);
                    else if (aspect is Inheritance) contract = new FeudalContract(null,
                        null,
                        Title.Contract.Government,
                        Title.Contract.Succession,
                        aspect as Inheritance,
                        Title.Contract.GenderLaw);
                    else contract = new FeudalContract(null,
                        null,
                        Title.Contract.Government,
                        Title.Contract.Succession,
                        Title.Contract.Inheritance,
                        aspect as GenderLaw);

                    Kingdom.AddDecision(new BKContractChangeDecision(Title, contract, Clan.PlayerClan));
                },
                null,
                Utils.Helpers.GetKingdomDecisionSound()));
        }

        [DataSourceProperty]
        public string GovernmentText => new TextObject("{=!}Government").ToString();

        [DataSourceProperty]
        public string InheritanceText => new TextObject("{=!}Inheritance").ToString();

        [DataSourceProperty]
        public string GenderLawText => new TextObject("{=!}Gender Law").ToString();

        [DataSourceProperty]
        public string StructureText => new TextObject("{=!}Contract Structure").ToString();

        [DataSourceProperty]
        public string GovernmentName => Title?.Contract.Government.Name.ToString();

        [DataSourceProperty]
        public string SuccessionName => Title?.Contract.Succession.Name.ToString();

        [DataSourceProperty]
        public string InheritanceName => Title?.Contract.Inheritance.Name.ToString();

        [DataSourceProperty]
        public string GenderLawName => Title?.Contract.GenderLaw.Name.ToString();

        [DataSourceProperty]
        public HintViewModel GovernmentHint => new HintViewModel(Title?.Contract.Government.Description);

        [DataSourceProperty]
        public HintViewModel SuccessionHint => new HintViewModel(Title?.Contract.Succession.Description);

        [DataSourceProperty]
        public HintViewModel InheritanceHint => new HintViewModel(Title?.Contract.Inheritance.Description);

        [DataSourceProperty]
        public HintViewModel GenderLawHint => new HintViewModel(Title?.Contract.GenderLaw.Description);

        [DataSourceProperty]
        public string HeirText => new TextObject("{=vArnerHC}Heir").ToString();

        [DataSourceProperty]
        public string SuccessionText => new TextObject("{=rTUgik07}Succession").ToString();

        [DataSourceProperty]
        public string LawsText => new TextObject("{=fE6RYz1k}Laws").ToString();

        [DataSourceProperty]
        public string LawsDescriptionText => new TextObject("{=MbSsFJNY}Demesne Laws may be changed a year after they are issued. Changes are made by the sovereign or through voting by the Peers.").ToString();


        [DataSourceProperty]
        public string SuccessionDescription
        {
            get => successionDescription;
            set
            {
                if (value != successionDescription)
                {
                    successionDescription = value;
                    OnPropertyChangedWithValue(value, "SuccessionDescription");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DemesneLawVM> Laws
        {
            get => laws;
            set
            {
                if (value != laws)
                {
                    laws = value;
                    OnPropertyChangedWithValue(value, "Laws");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeirVM> Heirs
        {
            get => heirs;
            set
            {
                if (value != heirs)
                {
                    heirs = value;
                    OnPropertyChangedWithValue(value, "Heirs");
                }
            }
        }

        [DataSourceProperty]
        public HeirVM MainHeir
        {
            get => mainHeir;
            set
            {
                if (value != mainHeir)
                {
                    mainHeir = value;
                    OnPropertyChangedWithValue(value, "MainHeir");
                }
            }
        }
    }
}
