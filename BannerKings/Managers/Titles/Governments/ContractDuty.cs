using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Titles.Governments
{
    public class ContractDuty : ContractAspect
    {
        private Action<Hero, Hero> execute;
        public ContractDuty(string stringId) : base(stringId)
        {
        }

        public override void PostInitialize()
        {

        }

        public int YearlyLimit { get; private set; }
        public void Execute(Hero suzerain, Hero vassal) => execute(suzerain, vassal);
    }
}
