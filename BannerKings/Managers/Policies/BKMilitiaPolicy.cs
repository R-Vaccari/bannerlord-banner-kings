using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    class BKMilitiaPolicy : BannerKingsPolicy
    {
        public override string GetIdentifier() => "militia";

        [SaveableProperty(3)]
        public MilitiaPolicy Policy { get; private set; }
        public BKMilitiaPolicy(MilitiaPolicy policy, Settlement settlement) : base(settlement, (int)policy)
        {
            Policy = policy;
        }
        public override string GetHint(int value)
        {
            if (value == (int)MilitiaPolicy.Melee)
                return "Focus three fourths of the militia as melee troops.";
            if (value == (int)MilitiaPolicy.Ranged)
                return "Focus three fourths of the militia as ranged troops.";
            return "Split militia equally between ranged and melee troops.";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                Policy = (MilitiaPolicy)vm.value;
                Selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
            }
        }

        public enum MilitiaPolicy
        {
            Balanced,
            Melee,
            Ranged
        }
        public override IEnumerable<Enum> GetPolicies()
        {
            yield return MilitiaPolicy.Balanced;
            yield return MilitiaPolicy.Melee;
            yield return MilitiaPolicy.Ranged;
        }
    }
}
