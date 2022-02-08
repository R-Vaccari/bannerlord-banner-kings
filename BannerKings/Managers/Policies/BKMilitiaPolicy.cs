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

        MilitiaPolicy policy;
        public BKMilitiaPolicy(MilitiaPolicy policy, Settlement settlement) : base(settlement, (int)policy, "militia")
        {
            this.policy = policy;
        }
        public override string GetHint()
        {
            if (policy == MilitiaPolicy.Melee)
                return "";
            else if (policy == MilitiaPolicy.Ranged)
                return "";
            else return "";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.policy = (MilitiaPolicy)vm.value;
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
