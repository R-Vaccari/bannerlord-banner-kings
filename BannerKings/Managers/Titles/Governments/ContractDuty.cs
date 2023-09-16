using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Titles.Governments.ContractAspect;

namespace BannerKings.Managers.Titles.Governments
{
    public class ContractDuty : ContractAspect
    {
        private Action<ContractDuty, Hero, Hero> execute;
        private Func<Hero, Hero, int> calculateDuty;
        private Func<ContractDuty, Hero, Hero, bool> canFulfill;

        public ContractDuty(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, 
            TextObject description, 
            TextObject popupText, 
            int seasonsDelay, 
            int relationsLoss,
            AspectTypes type,
            Action<ContractDuty, Hero, Hero> execute, 
            Func<ContractDuty, Hero, Hero, bool> canFulfill,
            Func<Hero, Hero, int> calculateDuty)
        {
            Initialize(name, description);
            SeasonsDelay = seasonsDelay;
            this.execute = execute;
            PopupText = popupText;
            RelationsLoss = relationsLoss;
            AspectType = type;
            this.calculateDuty = calculateDuty;
            this.canFulfill = canFulfill;
        }

        public override void PostInitialize()
        {

        }

        public void ExecuteDuty(Hero suzerain, Hero vassal)
        {
            if (vassal == Hero.MainHero)
            {
                InformationManager.ShowInquiry(new InquiryData(Name.ToString(),
                    PopupText.ToString(),
                    CanFulfill(suzerain, vassal),
                    true,
                    new TextObject().ToString(),
                    new TextObject().ToString(),
                    () => Execute(suzerain, vassal),
                    () => FailDuty(suzerain, vassal)
                    ));
            }
            else
            {
                if (CanFulfill(suzerain, vassal)) Execute(suzerain, vassal);
                else FailDuty(suzerain, vassal);
            }
        }

        private void FailDuty(Hero suzerain, Hero vassal, TextObject reason = null)
        {
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(vassal, suzerain, -RelationsLoss);
            if (suzerain == Hero.MainHero)
            {
                if (reason == null) reason = new TextObject("{=!}{HERO} is not able to fulfill the {DUTY} duty.");
                InformationManager.DisplayMessage(new InformationMessage(
                    reason.ToString(),
                    Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)
                    ));
            }
        }

        public AspectTypes AspectType { get; private set; }
        public TextObject PopupText { get; private set; }
        public int RelationsLoss { get; private set; }
        public int SeasonsDelay { get; private set; }
        private void Execute(Hero suzerain, Hero vassal) => execute(this, suzerain, vassal);
        public bool CanFulfill(Hero suzerain, Hero vassal) => canFulfill(this, suzerain, vassal);
        public int CalculateDuty(Hero suzerain, Hero vassal) => calculateDuty(suzerain, vassal);
    }
}
