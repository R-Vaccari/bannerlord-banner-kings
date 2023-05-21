using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Kingdoms
{
    public class ClanDemesneVM : BannerKingsViewModel
    {
        private MBBindingList<HeroVM> vassals;
        private MBBindingList<HeirVM> heirs;
        private HeirVM mainHeir;
        private HeroVM suzerain;
        private bool hasSuzerain;
        private string suzerainReasonText;

        public ClanDemesneVM(FeudalTitle title, Clan clan) : base(null, false)
        {
            Title = title;
            Clan = clan;
            vassals = new MBBindingList<HeroVM>();
            Heirs = new MBBindingList<HeirVM>();
        }

        public FeudalTitle Title { get; private set; }
        public Clan Clan { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Vassals.Clear();
            Heirs.Clear();

            var candidates = BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(Clan.Leader);
            var explanations = new Dictionary<Hero, ExplainedNumber>();
            foreach (Hero hero in candidates)
            {
                var explanation = BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(Clan.Leader, hero,
                    Title != null ? Title.Contract : null, true);
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

            HasSuzerain = false;
            var suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(Clan.Leader);
            if (suzerain != null)
            {
                HasSuzerain = true;
                Suzerain = new HeroVM(suzerain.deJure);
            }

            if (Title != null)
            {
                SuzerainReasonText = new TextObject("{=iQ7M8YNb}{SUZERAIN} is your de jure suzerain as the {TITLE} is your highest title.")
                        .SetTextVariable("SUZERAIN", Suzerain.NameText)
                        .SetTextVariable("TITLE", Title.FullName)
                        .ToString();
            }
            else
            {
                if (HasSuzerain)
                {
                    SuzerainReasonText = new TextObject("{=QrXhX0kF}{SUZERAIN} is your default suzerain as a vassal without titles.")
                        .SetTextVariable("SUZERAIN", Suzerain.NameText)
                        .ToString();
                }
            }

            var vassals = BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(Clan);
            foreach (var vassal in vassals)
            {
                Vassals.Add(new HeroVM(vassal));
            }
        }

        [DataSourceProperty]
        public string HeirText => new TextObject("{=vArnerHC}Heir").ToString();

        [DataSourceProperty]
        public string InheritanceText => new TextObject("{=aELuNrRC}Inheritance").ToString();

        [DataSourceProperty]
        public string SuzerainText => new TextObject("{=WpXfTefm}Suzerain").ToString();

        [DataSourceProperty]
        public bool HasSuzerain
        {
            get => hasSuzerain;
            set
            {
                if (value != hasSuzerain)
                {
                    hasSuzerain = value;
                    OnPropertyChangedWithValue(value, "HasSuzerain");
                }
            }
        }

        [DataSourceProperty]
        public string SuzerainReasonText
        {
            get => suzerainReasonText;
            set
            {
                if (value != suzerainReasonText)
                {
                    suzerainReasonText = value;
                    OnPropertyChangedWithValue(value, "HasSuzerain");
                }
            }
        }

        [DataSourceProperty]
        public HeroVM Suzerain
        {
            get => suzerain;
            set
            {
                if (value != suzerain)
                {
                    suzerain = value;
                    OnPropertyChangedWithValue(value, "HasSuzerain");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeroVM> Vassals
        {
            get => vassals;
            set
            {
                if (value != vassals)
                {
                    vassals = value;
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
