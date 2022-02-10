using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;

namespace BannerKings.Managers.Policies
{
    class BKDraftPolicy : BannerKingsPolicy
    {

        DraftPolicy policy;
        public BKDraftPolicy(DraftPolicy policy, Settlement settlement) : base(settlement, (int)policy, "draft")
        {
            this.policy = policy;
        }

        public override string GetHint()
        {
            return "No particular policy is implemented";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.policy = (DraftPolicy)vm.value;
                base.selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, this);
            }
        }

        public enum DraftPolicy
        {
            Standard,
            Enlistment,
            Lax
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return DraftPolicy.Standard;
            yield return DraftPolicy.Enlistment;
            yield return DraftPolicy.Lax;
            yield break;
        }
    }
}
