using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("UpdatePlayerInfo")]
    internal class MapBarMixin : BaseViewModelMixin<MapInfoVM>
    {
        private int piety;
        private string pietyAbbr;
        private BasicTooltipViewModel pietyHint;
        private bool pietyWarning;

        public MapBarMixin(MapInfoVM vm) : base(vm)
        {
            Piety = 0;
        }

        [DataSourceProperty]
        public int Piety
        {
            get => piety;
            set
            {
                if (value != piety)
                {
                    piety = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public string PietyWithAbbrText
        {
            get => pietyAbbr;
            set
            {
                if (value != pietyAbbr)
                {
                    pietyAbbr = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel PietyHint
        {
            get => pietyHint;
            set
            {
                if (value != pietyHint)
                {
                    pietyHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool IsPietyTooltipWarning
        {
            get => pietyWarning;
            set
            {
                if (value != pietyWarning)
                {
                    pietyWarning = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            if (BannerKingsConfig.Instance.ReligionsManager == null)
            {
                return;
            }

            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            Piety = (int) BannerKingsConfig.Instance.ReligionsManager.GetPiety(rel, Hero.MainHero);
            PietyHint = new BasicTooltipViewModel(() => UIHelper.GetPietyTooltip(rel, Hero.MainHero, Piety));
            PietyWithAbbrText = CampaignUIHelper.GetAbbreviatedValueTextFromValue(Piety);
            IsPietyTooltipWarning = Piety < 0f;
            //if (rel == null) return;
        }
    }
}