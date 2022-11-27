using System;
using System.Collections.Generic;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    public class BKTaxPolicy : BannerKingsPolicy
    {
        public enum TaxType
        {
            Standard,
            High,
            Low,
            Exemption
        }

        public BKTaxPolicy(TaxType policy, Settlement settlement) : base(settlement, (int) policy)
        {
            Policy = policy;
        }

        [SaveableProperty(3)] public TaxType Policy { get; private set; }

        public override string GetIdentifier()
        {
            return "tax";
        }

        public override string GetHint(int value)
        {
            return value switch
            {
                (int) TaxType.High when !Settlement.IsVillage =>
                    "Yield more tax from the population, at the cost of decreased loyalty.",
                (int) TaxType.High => "Yield more tax from the population, but reduce growth.",
                (int) TaxType.Low when !Settlement.IsVillage =>
                    "Reduce tax burden on the population, diminishing your profit but increasing their support towards you.",
                (int) TaxType.Low => "Reduce tax burden on the population, encouraging new settlers.",
                (int) TaxType.Exemption => "Fully exempt notables from taxes, improving their attitude towards you",
                _ => "Standard tax of the land, with no particular repercussions"
            };
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                Policy = (TaxType) vm.Value;
                Selected = vm.Value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
            }
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return TaxType.Standard;
            yield return TaxType.High;
            yield return TaxType.Low;
            if (Settlement.IsVillage)
            {
                yield return TaxType.Exemption;
            }
        }
    }
}