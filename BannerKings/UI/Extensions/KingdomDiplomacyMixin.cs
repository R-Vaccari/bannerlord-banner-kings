using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class KingdomDiplomacyMixin : BaseViewModelMixin<KingdomDiplomacyVM>
    {
        private readonly KingdomDiplomacyVM kingdomDiplomacy;
        private string justificationText, warScoreText, warFaitgueText;
        private bool warExists;
        private War war;


        public KingdomDiplomacyMixin(KingdomDiplomacyVM vm) : base(vm)
        {
            kingdomDiplomacy = vm;
        }

        [DataSourceProperty] public string JustificationHeader => new TextObject("{=!}Casus Belli").ToString();
        [DataSourceProperty] public string WarScoreHeader => new TextObject("{=!}War Score").ToString();
        [DataSourceProperty] public string WarFatigueHeader => new TextObject("{=!}War Fatigue").ToString();

        [DataSourceProperty]
        public string JustificationText
        {
            get => justificationText;
            set
            {
                if (value != justificationText)
                {
                    justificationText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string WarScoreText
        {
            get => warScoreText;
            set
            {
                if (value != warScoreText)
                {
                    warScoreText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string WarFaitgueText
        {
            get => warFaitgueText;
            set
            {
                if (value != warFaitgueText)
                {
                    warFaitgueText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool WarExists
        {
            get => warExists;
            set
            {
                if (value != warExists)
                {
                    warExists = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            WarExists = false;
            if (kingdomDiplomacy.CurrentSelectedDiplomacyItem != null)
            {
                if (kingdomDiplomacy.CurrentSelectedDiplomacyItem is KingdomWarItemVM)
                {
                    war = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                        .GetWar(kingdomDiplomacy.CurrentSelectedDiplomacyItem.Faction1, 
                        kingdomDiplomacy.CurrentSelectedDiplomacyItem.Faction2);
                }
            }

            if (war != null)
            {
                WarExists = true;

                JustificationText = war.CasusBelli.Name.ToString();
                WarScoreText = (war.CalculateWarScore(Hero.MainHero.MapFaction, true).ResultNumber * 100f)
                    .ToString("0.00") + '%';
            }
            else
            {
                JustificationText = "";
                WarScoreText = "";
            }
        }
    }
}