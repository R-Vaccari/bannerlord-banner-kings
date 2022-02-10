using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;

namespace BannerKings.Managers.Policies
{
    class BKTariffPolicy : BannerKingsPolicy
    {

        public TariffType Policy { get; private set; }
        public BKTariffPolicy(TariffType policy, Settlement settlement) : base(settlement, (int)policy, "tariff")
        {
            this.Policy = policy;
        }

        public override string GetHint()
        {
            if (Policy == TariffType.Standard)
                return "A tariff is paid to the lord by the settlement when items are sold. This tariff is embedded into prices, meaning tariffs make prices higher overall";
            else if (Policy == TariffType.Internal_Consumption)
                return "The standard tariff is maintained and a discount is offered to internal consumers (workshops and population). This discount is paid for by the merchants, who won't be happy with it";
            else return "No tariff is charged, reducing prices and possibly attracting more caravans";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (TariffType)vm.value;
                base.selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, this);
            }
        }

        public enum TariffType
        {
            Standard,
            Internal_Consumption,
            Exemption
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return TariffType.Standard;
            yield return TariffType.Internal_Consumption;
            yield return TariffType.Exemption;
            yield break;
        }
    }
}
