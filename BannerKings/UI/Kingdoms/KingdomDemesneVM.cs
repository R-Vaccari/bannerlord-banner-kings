using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using BannerKings.UI.Items;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
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

                if (explanation.ResultNumber > maxScore)
                {
                    MainHeir = new HeirVM(hero, explanation);
                }
            }

            var sorted = (from x in explanations
                         orderby x.Value.ResultNumber descending
                         select x).Take(5);
            foreach (var pair in sorted)
            {
                var hero = pair.Key;
                if (hero == MainHeir.Hero)
                {
                    continue;
                }

                Heirs.Add(new HeirVM(hero, explanations[hero]));
            }
        }

        private void OnChange(SelectorVM<BKItemVM> obj)
        {

        }

        [DataSourceProperty]
        public string HeirText => new TextObject("{=!}Heir").ToString();

        [DataSourceProperty]
        public string SuccessionText => new TextObject("{=!}Succession").ToString();

        [DataSourceProperty]
        public string LawsText => new TextObject("{=!}Laws").ToString();

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
