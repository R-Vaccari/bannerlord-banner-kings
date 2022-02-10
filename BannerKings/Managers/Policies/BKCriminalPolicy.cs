using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.Managers.Policies
{
    class BKCriminalPolicy : BannerKingsPolicy
    {

        public CriminalPolicy Policy { get; private set; }
        public BKCriminalPolicy(CriminalPolicy policy, Settlement settlement) : base(settlement, (int)policy, "criminal")
        {
            this.Policy = policy;
        }
        public override string GetHint()
        {
            if (Policy == CriminalPolicy.Enslavement)
                return "Prisoners sold in the settlement will be enslaved and join the population. No particular repercussions";
            else if (Policy == CriminalPolicy.Execution)
                return "Prisoners will suffer the death penalty. No ransom is paid, but the populace feels at ease knowing there are less threats in their daily lives";
            else return "Forgive criminals and prisoners of war";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (CriminalPolicy)vm.value;
                base.selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, this);
            }
        }

        public enum CriminalPolicy
        {
            Enslavement,
            Execution,
            Forgiveness
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return CriminalPolicy.Enslavement;
            yield return CriminalPolicy.Execution;
            yield return CriminalPolicy.Forgiveness;
            yield break;
        }
    }
}
