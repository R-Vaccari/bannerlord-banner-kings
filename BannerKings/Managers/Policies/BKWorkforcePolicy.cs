using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    class BKWorkforcePolicy : BannerKingsPolicy
    {

        [SaveableProperty(3)]
        public WorkforcePolicy Policy { get; private set; }
        public BKWorkforcePolicy(WorkforcePolicy policy, Settlement settlement) : base(settlement, (int)policy)
        {
            Policy = policy;
        }

        public override string GetHint(int value)
        {
            if (value == (int)WorkforcePolicy.Construction)
                return "Put all state slaves to work in construction, doubling their amount (standard amount is 50% of them). These slaves will be deducted from workforce. Increases adm. costs.";
            if (value == (int)WorkforcePolicy.Land_Expansion)
                return "Divert a share of slaves and serfs to work on creating new usable acres. Every day progress will be made into new acres, the choosen type depends on the terrain. These laborers will be deducted from workforce. Increases adm. costs.";
            if (value == (int)WorkforcePolicy.Martial_Law)
                return "Put half the militia on active duty, increasing security. These militiamen will be deducted from serf workforce. Decreases production efficiency. Increases adm. costs.";
            return "No particular policy is implemented. No administrative costs.";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                Policy = (WorkforcePolicy)vm.value;
                Selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
            }
        }
        public enum WorkforcePolicy
        {
            None,
            Land_Expansion,
            Martial_Law,
            Construction
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return WorkforcePolicy.None;
            yield return WorkforcePolicy.Land_Expansion;
            yield return WorkforcePolicy.Martial_Law;
            yield return WorkforcePolicy.Construction;
        }

        public override string GetIdentifier() => "workforce";
    }
}
