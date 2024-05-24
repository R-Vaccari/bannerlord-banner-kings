using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Utils.Models;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("OnSetCurrentDiplomacyItem")]
    internal class KingdomDiplomacyMixin : BaseViewModelMixin<KingdomDiplomacyVM>
    {
        private readonly KingdomDiplomacyVM kingdomDiplomacy;
        private string justificationText, warScoreText, warObjectiveText, playerFrontHeader,
            enemyFrontHeader, playerFatigueHeader, enemyFatigueHeader, playerFrontText,
            enemyFrontText, playerFatigueText, enemyFatigueText, tradePactText,
            truceText, allianceText, warSupportText;
        private HintViewModel justificationHint, warScoreHint, warObjectiveHint, frontHint,
            playerFatigueHint, enemyFatigueHint;
        private BasicTooltipViewModel tradePactHint, allianceHint, warSupportHint;
        private bool warExists, peaceExists;
        private War war;


        public KingdomDiplomacyMixin(KingdomDiplomacyVM vm) : base(vm)
        {
            kingdomDiplomacy = vm;
        }

        [DataSourceProperty] public string JustificationHeader => new TextObject("{=Fs2NR9Os}Casus Belli").ToString();
        [DataSourceProperty] public string WarScoreHeader => new TextObject("{=FFzEZAqj}War Score").ToString();
        [DataSourceProperty] public string WarObjectiveHeader => new TextObject("{=HDfqWzbQ}War Objective").ToString();
        [DataSourceProperty] public string TradePactHeader => new TextObject("{=9B3GozDo}Trade Pact").ToString();
        [DataSourceProperty] public string AllianceHeader => new TextObject("{=ueWn5rM4}Alliance").ToString();
        [DataSourceProperty] public string TruceHeader => new TextObject("{=bNsdETJY}Truce").ToString();
        [DataSourceProperty] public string WarSupportHeader => new TextObject("{=KU4x1UyH}War Support").ToString();

        public override void OnRefresh()
        {
            war = null;
            WarExists = false;
            PeaceExists = false;
            base.ViewModel.ProposeActionExplanationText = new TextObject("{=kyB8tkgY}Consider proposing a diplomatic action").ToString();
            if (kingdomDiplomacy.CurrentSelectedDiplomacyItem != null)
            {
                if (kingdomDiplomacy.CurrentSelectedDiplomacyItem is KingdomWarItemVM)
                {
                    war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                        .GetWar(kingdomDiplomacy.CurrentSelectedDiplomacyItem.Faction1, 
                        kingdomDiplomacy.CurrentSelectedDiplomacyItem.Faction2);
                }
            }

            PlayerFatigueHeader = new TextObject("{=0ksQ2otA}Our Fatigue").ToString();
            EnemyFatigueHeader = new TextObject("{=eesjwvUR}Enemy Fatigue").ToString();

            Kingdom currentKingdom = kingdomDiplomacy.CurrentSelectedDiplomacyItem.Faction1 as Kingdom;
            Kingdom targetKingdom = kingdomDiplomacy.CurrentSelectedDiplomacyItem.Faction2 as Kingdom;

            if (war != null)
            {
                WarExists = true;

                JustificationText = war.CasusBelli.Name.ToString();
                JustificationHint = new HintViewModel(war.CasusBelli.Description);

                BKExplainedNumber warScore = war.CalculateWarScore(Hero.MainHero.MapFaction, true);
                WarScoreText = (warScore.ResultNumber * 100f).ToString("0.00") + '%';
                WarScoreHint = new HintViewModel(new TextObject("{=!}" + warScore.GetFormattedPercentage()));

                WarObjectiveText = war.CasusBelli.ObjectiveText != null ? war.CasusBelli.ObjectiveText.ToString() : 
                    war.CasusBelli.Fief.Name.ToString();
                WarObjectiveHint = new HintViewModel(war.CasusBelli.Description);

                PlayerFrontHeader = new TextObject("{=CMoMTMey}{FACTION} Front")
                    .SetTextVariable("FACTION", Hero.MainHero.MapFaction.Name)
                    .ToString();
                Town playerFront = war.GetFront(Hero.MainHero.MapFaction);
                PlayerFrontText = playerFront != null ? playerFront.Name.ToString() : new TextObject("{=5Gphhg0D}No Front").ToString();

                IFaction enemyFaction = war.GetPlayerEnemyFaction();
                EnemyFrontHeader = new TextObject("{=CMoMTMey}{FACTION} Front")
                    .SetTextVariable("FACTION", enemyFaction.Name)
                    .ToString();
                Town enemyFront = war.GetFront(enemyFaction);
                EnemyFrontText = enemyFront != null ? enemyFront.Name.ToString() : new TextObject("{=5Gphhg0D}No Front").ToString();

                BKExplainedNumber fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, Hero.MainHero.MapFaction, true);
                PlayerFatigueText = (fatigue.ResultNumber * 100f).ToString("0.00") + '%';
                PlayerFatigueHint = new HintViewModel(new TextObject("{=!}" + fatigue.GetFormattedPercentage()));

                BKExplainedNumber enemyFatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, enemyFaction, true);
                EnemyFatigueText = (enemyFatigue.ResultNumber * 100f).ToString("0.00") + '%';
                EnemyFatigueHint = new HintViewModel(new TextObject("{=!}" + enemyFatigue.GetFormattedPercentage()));
            }
            else if (kingdomDiplomacy.CurrentSelectedDiplomacyItem is KingdomTruceItemVM)
            {
                PeaceExists = true;
                var bkDiplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(currentKingdom);
                
                if (bkDiplomacy.HasTradePact(targetKingdom))
                {
                    TradePactText = new TextObject("{=GTQCGxvy}In Effect").ToString();
                }
                else
                {
                    TradePactText = new TextObject("{=er8kmimQ}Not Present").ToString();
                }

                if (bkDiplomacy.HasValidTruce(targetKingdom))
                {
                    TruceText = bkDiplomacy.Truces[targetKingdom].ToString();
                }
                else
                {
                    TruceText = new TextObject("None").ToString();
                }

                StanceLink stance = currentKingdom.GetStanceWith(targetKingdom);
                if (stance.IsAllied)
                {
                    AllianceText = new TextObject("{=GTQCGxvy}In Effect").ToString();
                    TruceText = new TextObject("{=d92ORtRp}Implicit (Alliance)").ToString();
                }
                else
                {
                    AllianceText = new TextObject("{=!}None").ToString();
                }

                TradePactHint = new BasicTooltipViewModel(() =>
                {

                });

                AllianceHint = new BasicTooltipViewModel(() => UIHelper.GetAllianceHint(currentKingdom, targetKingdom));
            }

            KingdomElection election = new KingdomElection(new BKDeclareWarDecision(null, currentKingdom.RulingClan, targetKingdom));
            WarSupportText = UIHelper.FormatValue(election.GetLikelihoodForOutcome(0));
            WarSupportHint = new BasicTooltipViewModel(() => UIHelper.GetWarSupportHint(currentKingdom, targetKingdom));
        }

        [DataSourceProperty]
        public string TradePactText
        {
            get => tradePactText;
            set
            {
                if (value != tradePactText)
                {
                    tradePactText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string AllianceText
        {
            get => allianceText;
            set
            {
                if (value != allianceText)
                {
                    allianceText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string TruceText
        {
            get => truceText;
            set
            {
                if (value != truceText)
                {
                    truceText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel WarSupportHint
        {
            get => warSupportHint;
            set
            {
                if (value != warSupportHint)
                {
                    warSupportHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }
        

        [DataSourceProperty]
        public BasicTooltipViewModel TradePactHint
        {
            get => tradePactHint;
            set
            {
                if (value != tradePactHint)
                {
                    tradePactHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel AllianceHint
        {
            get => allianceHint;
            set
            {
                if (value != allianceHint)
                {
                    allianceHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel TruceHint
        {
            get => justificationHint;
            set
            {
                if (value != justificationHint)
                {
                    justificationHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

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
        public string WarSupportText
        {
            get => warSupportText;
            set
            {
                if (value != warSupportText)
                {
                    warSupportText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel JustificationHint
        {
            get => justificationHint;
            set
            {
                if (value != justificationHint)
                {
                    justificationHint = value;
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
        public HintViewModel WarScoreHint
        {
            get => warScoreHint;
            set
            {
                if (value != warScoreHint)
                {
                    warScoreHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string WarObjectiveText
        {
            get => warObjectiveText;
            set
            {
                if (value != warObjectiveText)
                {
                    warObjectiveText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel WarObjectiveHint
        {
            get => warObjectiveHint;
            set
            {
                if (value != warObjectiveHint)
                {
                    warObjectiveHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel FrontHint
        {
            get => frontHint;
            set
            {
                if (value != frontHint)
                {
                    frontHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel EnemyFatigueHint
        {
            get => enemyFatigueHint;
            set
            {
                if (value != enemyFatigueHint)
                {
                    enemyFatigueHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel PlayerFatigueHint
        {
            get => playerFatigueHint;
            set
            {
                if (value != playerFatigueHint)
                {
                    playerFatigueHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool PeaceExists
        {
            get => peaceExists;
            set
            {
                if (value != peaceExists)
                {
                    peaceExists = value;
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

        [DataSourceProperty]
        public string PlayerFrontHeader
        {
            get => playerFrontHeader;
            set
            {
                if (value != playerFrontHeader)
                {
                    playerFrontHeader = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string PlayerFrontText
        {
            get => playerFrontText;
            set
            {
                if (value != playerFrontText)
                {
                    playerFrontText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string PlayerFatigueHeader
        {
            get => playerFatigueHeader;
            set
            {
                if (value != playerFatigueHeader)
                {
                    playerFatigueHeader = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string PlayerFatigueText
        {
            get => playerFatigueText;
            set
            {
                if (value != playerFatigueText)
                {
                    playerFatigueText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EnemyFatigueHeader
        {
            get => enemyFatigueHeader;
            set
            {
                if (value != enemyFatigueHeader)
                {
                    enemyFatigueHeader = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EnemyFatigueText
        {
            get => enemyFatigueText;
            set
            {
                if (value != enemyFatigueText)
                {
                    enemyFatigueText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EnemyFrontHeader
        {
            get => enemyFrontHeader;
            set
            {
                if (value != enemyFrontHeader)
                {
                    enemyFrontHeader = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EnemyFrontText
        {
            get => enemyFrontText;
            set
            {
                if (value != enemyFrontText)
                {
                    enemyFrontText = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}