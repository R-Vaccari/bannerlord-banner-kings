using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;

namespace BannerKings.Managers.Policies
{
    class BKWorkforcePolicy : BannerKingsPolicy
    {

        public WorkforcePolicy Policy { get; private set; }
        public BKWorkforcePolicy(WorkforcePolicy policy, Settlement settlement) : base(settlement, (int)policy, "workforce")
        {
            this.Policy = policy;
        }

        public override string GetHint()
        {
            if (Policy == WorkforcePolicy.Construction)
                return "Serfs aid in construction for a gold cost, and food production suffers a penalty";
            else if (Policy == WorkforcePolicy.Land_Expansion)
                return "Divert slaves and serf workforces to expand the arable land, reducing their outputs while extending usable land";
            else if (Policy == WorkforcePolicy.Martial_Law)
                return "Put the militia on active duty, increasing security but costing a food upkeep. Negatively impacts production efficiency";
            else return "No particular policy is implemented";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (WorkforcePolicy)vm.value;
                base.selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, this);
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
            yield break;
        }
    }
}
