using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.Managers.Policies
{
    class GarrisonPolicyItem : BannerKingsPolicy
    {

        GarrisonPolicy policy;
        public GarrisonPolicyItem(GarrisonPolicy policy, Settlement settlement) : base(settlement, (int)policy)
        {
            this.policy = policy;
        }
        public override string GetHint()
        {
            if (policy == GarrisonPolicy.Enlist_Locals)
                return "";
            else if (policy == GarrisonPolicy.Enlist_Locals)
                return "";
            else return "";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.policy = (GarrisonPolicy)vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateGarrisonPolicy(settlement, this.policy);
            }
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return GarrisonPolicy.Standard;
            yield return GarrisonPolicy.Enlist_Locals;
            yield return GarrisonPolicy.Enlist_Mercenaries;
            yield break;
        }
    }
}
