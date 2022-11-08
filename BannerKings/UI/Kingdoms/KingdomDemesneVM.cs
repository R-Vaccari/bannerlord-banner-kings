using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using BannerKings.UI.Items;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
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

            bool isKing = Kingdom.Leader == Hero.MainHero && Title.deJure == Hero.MainHero;
            foreach (var law in Title.contract.DemesneLaws)
            {
                Laws.Add(new DemesneLawVM(DefaultDemesneLaws.Instance.GetLawsByType(law.LawType),
                    law,
                    isKing,
                    OnChange));
            }

            var candidates = BannerKingsConfig.Instance.TitleModel.GetSuccessionCandidates(Kingdom.Leader, Title.contract);
            var explanations = new Dictionary<Hero, ExplainedNumber>();
            var maxScore = 0f;

            foreach (Hero hero in candidates)
            {
                var explanation = BannerKingsConfig.Instance.TitleModel.GetSuccessionHeirScore(Kingdom.Leader, hero, Title.contract, true);
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

        private void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                var policyIndex = vm.Value;
                var lawType = (DemesneLawTypes)vm.Reference;

                var resultLaw = DefaultDemesneLaws.Instance.All.FirstOrDefault(x => x.LawType == lawType && x.Index == policyIndex);
                var currentLaw = Title.contract.GetLawByType(lawType);
                if (resultLaw != null && !resultLaw.Equals(currentLaw))
                {
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Enact Law").ToString(),
                        new TextObject("{=!}Enact the {LAW} law thoughtout the demesne of {TITLE}. The law will be enacted for every title in the hierarchy.\n\nCost: {INFLUENCE} {INFLUENCE_ICON}")
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

        [DataSourceProperty]
        public string HeirText => new TextObject("{=!}Heir").ToString();

        [DataSourceProperty]
        public string SuccessionText => new TextObject("{=!}Succession").ToString();

        [DataSourceProperty]
        public string LawsText => new TextObject("{=!}Laws").ToString();

        [DataSourceProperty]
        public string LawsDescriptionText => new TextObject("{=!}Demesne Laws may be changed a year after they are issued. Changes are made by the sovereign or through votation of the Peers.").ToString();

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
