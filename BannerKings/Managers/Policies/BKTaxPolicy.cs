using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    class BKTaxPolicy : BannerKingsPolicy
    {
        public override string GetIdentifier() => "tax";

        [SaveableProperty(3)]
        public TaxType Policy { get; private set; }
        public BKTaxPolicy(TaxType policy, Settlement settlement) : base(settlement, (int)policy)
        {
            Policy = policy;
        }

        public override string GetHint(int value)
        {
            if (value == (int)TaxType.High)
            {
                if (!Settlement.IsVillage) return "Yield more tax from the population, at the cost of decreased loyalty.";
                return "Yield more tax from the population, but reduce growth.";
            }

            if (value == (int)TaxType.Low)
            {
                if (!Settlement.IsVillage) return "Reduce tax burden on the population, diminishing your profit but increasing their support towards you.";
                return "Reduce tax burden on the population, encouraging new settlers.";
            }

            if (value == (int)TaxType.Exemption)
                return "Fully exempt notables from taxes, improving their attitude towards you";
            return "Standard tax of the land, with no particular repercussions";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                Policy = (TaxType)vm.value;
                Selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
            }
        }
        public enum TaxType
        {
            Standard,
            High,
            Low,
            Exemption
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return TaxType.Standard;
            yield return TaxType.High;
            yield return TaxType.Low;
            if (Settlement.IsVillage) yield return TaxType.Exemption;
        }
    }
}
