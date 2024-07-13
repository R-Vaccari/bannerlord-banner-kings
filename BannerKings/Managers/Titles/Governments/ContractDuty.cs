using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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
            TextObject resultsText,
            int seasonsDelay, 
            int relationsLoss,
            AspectTypes type,
            Action<ContractDuty, Hero, Hero> execute, 
            Func<ContractDuty, Hero, Hero, bool> canFulfill,
            Func<Hero, Hero, int> calculateDuty,
            Func<Kingdom, bool> isAdequateForKingdom)
        {
            Initialize(name, description);
            SeasonsDelay = seasonsDelay;
            this.execute = execute;
            PopupText = popupText;
            ResultsText = resultsText;
            RelationsLoss = relationsLoss;
            AspectType = type;
            this.calculateDuty = calculateDuty;
            this.canFulfill = canFulfill;
            this.isAdequateForKingdom = isAdequateForKingdom;
        }

        public override void PostInitialize()
        {
            ContractDuty duty = DefaultContractAspects.Instance.GetById(this) as ContractDuty;
            Initialize(duty.name, duty.description, duty.PopupText, duty.ResultsText,
                duty.SeasonsDelay, duty.RelationsLoss, duty.AspectType, duty.execute, duty.canFulfill,
                duty.calculateDuty, duty.isAdequateForKingdom);
        }

        public void ExecuteDuty(Hero suzerain, Hero vassal)
        {
            if (vassal == Hero.MainHero)
            {
                InformationManager.ShowInquiry(new InquiryData(Name.ToString(),
                    PopupText.SetTextVariable("SUZERAIN", suzerain.Name)
                    .SetTextVariable("RESULT", CalculateDuty(suzerain, vassal))
                    .ToString(),
                    CanFulfill(suzerain, vassal),
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_reject").ToString(),
                    () => FinishDuty(suzerain, vassal),
                    () => FailDuty(suzerain, vassal)
                    ));
            }
            else
            {
                if (CanFulfill(suzerain, vassal)) FinishDuty(suzerain, vassal);
                else FailDuty(suzerain, vassal);
            }
        }

        private void FinishDuty(Hero suzerain, Hero vassal)
        {
            Execute(suzerain, vassal);
            SetTitleDuty(vassal);
        }

        private void SetTitleDuty(Hero vassal)
        {
            foreach (FeudalTitle title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(vassal))
            {
                title.FulfillDuty(this);
            }
        }

        private void FailDuty(Hero suzerain, Hero vassal, TextObject reason = null)
        {
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(vassal, suzerain, -RelationsLoss);
            if (suzerain == Hero.MainHero)
            {
                if (reason == null) reason = new TextObject("{=rUaAFgRr}{HERO} is not able to fulfill the {DUTY} duty.");
                InformationManager.DisplayMessage(new InformationMessage(
                    reason.ToString(),
                    Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)
                    ));
            }
        }

        private TextObject ResultsText { get;  set; }
        public TextObject GetResults(Hero suzerain, Hero vassal) => ResultsText
            .SetTextVariable("RESULTS", CalculateDuty(suzerain, vassal))
            .SetTextVariable("VASSAL", vassal.Name);

        public TextObject PopupText { get; private set; }   
        public int RelationsLoss { get; private set; }
        public int SeasonsDelay { get; private set; }
        private void Execute(Hero suzerain, Hero vassal) => execute(this, suzerain, vassal);
        public bool CanFulfill(Hero suzerain, Hero vassal) => canFulfill(this, suzerain, vassal);
        public int CalculateDuty(Hero suzerain, Hero vassal) => calculateDuty(suzerain, vassal);

        public override bool Equals(object obj)
        {
            if (obj is ContractDuty)
            {
                return (obj as ContractDuty).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}
