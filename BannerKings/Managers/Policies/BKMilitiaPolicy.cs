using System;
using System.Collections.Generic;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    internal class BKMilitiaPolicy : BannerKingsPolicy
    {
        public enum MilitiaPolicy
        {
            Balanced,
            Melee,
            Ranged
        }

        public BKMilitiaPolicy(MilitiaPolicy policy, Settlement settlement) : base(settlement, (int) policy)
        {
            Policy = policy;
        }

        [SaveableProperty(3)] public MilitiaPolicy Policy { get; private set; }

        public override string GetIdentifier()
        {
            return "militia";
        }

        public override string GetHint(int value)
        {
            return value switch
            {
                (int) MilitiaPolicy.Melee => "Focus three fourths of the militia as melee troops.",
                (int) MilitiaPolicy.Ranged => "Focus three fourths of the militia as ranged troops.",
                _ => "Split militia equally between ranged and melee troops."
            };
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                Policy = (MilitiaPolicy) vm.Value;
                Selected = vm.Value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
            }
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return MilitiaPolicy.Balanced;
            yield return MilitiaPolicy.Melee;
            yield return MilitiaPolicy.Ranged;
        }
    }
}