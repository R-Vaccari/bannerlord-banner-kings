using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;

namespace BannerKings.Managers.Policies
{
    class BKTaxPolicy : BannerKingsPolicy
    {
        public override string GetIdentifier() => "tax";
        public TaxType Policy { get; private set; }
        public BKTaxPolicy(TaxType policy, Settlement settlement) : base(settlement, (int)policy)
        {
            this.Policy = policy;
        }

        public override string GetHint()
        {
            if (Policy == TaxType.High)
            {
                if (!Settlement.IsVillage) return "Yield more tax from the population, at the cost of decreased loyalty";
                else return "Yield more tax from the population, but reduce growth"; 
            }
            else if (Policy == TaxType.Low)
            {
                if (!Settlement.IsVillage) return "Reduce tax burden on the population, diminishing your profit but increasing their support towards you";
                else return "Reduce tax burden on the population, encouraging new settlers";
            }
            else if (Policy == TaxType.Exemption)
                return "Fully exempt notables from taxes, improving their attitude towards you";
            else return "Standard tax of the land, with no particular repercussions";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (TaxType)vm.value;
                base.Selected = vm.value;
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
            yield break;
        }
    }
}
