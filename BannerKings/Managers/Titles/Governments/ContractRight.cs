using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class ContractRight : ContractAspect
    {
        private Action<ContractRight, Hero, Hero> execute;
        private Func<ContractRight, Hero, Hero, bool> canFulfill;

        public ContractRight(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject effect,
            bool isPassive,
            int influence,
            AspectTypes type,
            Action<ContractRight, Hero, Hero> execute = null,
            Func<ContractRight, Hero, Hero, bool> canFulfill = null)
        {
            Initialize(name, description);
            AspectType = type;
            IsPassive = isPassive;
            EffectText = effect;
            Influence = influence;
            this.execute = execute;
            this.canFulfill = canFulfill;
        }

        public override void PostInitialize()
        {
            ContractRight c = DefaultContractAspects.Instance.GetById(this) as ContractRight;
            Initialize(c.name, c.description, c.EffectText, c.IsPassive,
                c.Influence, c.AspectType, c.execute, c.canFulfill);
        }

        public bool IsPassive { get; private set; }
        public int Influence { get; private set; }
        public TextObject EffectText { get; private set; }

        public void Execute(Hero suzerain, Hero vassal)
        {
            if (execute != null)
            {
                execute(this, suzerain, vassal);
                if (suzerain == Hero.MainHero || vassal == Hero.MainHero)
                {
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{VASSAL} has petitioned {SUZERAIN} to fulfil the right of {RIGHT}.")
                        .SetTextVariable("RIGHT", Name)
                        .SetTextVariable("VASSAL", vassal.Name)
                        .SetTextVariable("SUZERAIN", suzerain.Name)
                        .ToString(),
                        Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                }
            }
        }

        public bool CanFulfill(Hero suzerain, Hero vassal)
        {
            if (canFulfill != null) return canFulfill(this, suzerain, vassal);
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is ContractRight)
            {
                return (obj as ContractRight).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}
