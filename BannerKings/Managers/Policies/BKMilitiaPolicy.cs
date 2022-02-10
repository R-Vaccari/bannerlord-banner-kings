using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.Managers.Policies
{
    class BKMilitiaPolicy : BannerKingsPolicy
    {

        public MilitiaPolicy Policy { get; private set; }
        public BKMilitiaPolicy(MilitiaPolicy policy, Settlement settlement) : base(settlement, (int)policy, "militia")
        {
            this.Policy = policy;
        }
        public override string GetHint()
        {
            if (Policy == MilitiaPolicy.Melee)
                return "Focus three fourths of the militia as melee troops";
            else if (Policy == MilitiaPolicy.Ranged)
                return "Focus three fourths of the militia as ranged troops";
            else return "Split militia equally between ranged and melee troops";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (MilitiaPolicy)vm.value;
                base.selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, this);
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
            yield break;
        }
    }
}
