using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    class BKCriminalPolicy : BannerKingsPolicy
    {
        [SaveableProperty(3)]
        public CriminalPolicy Policy { get; private set; }
        public BKCriminalPolicy(CriminalPolicy policy, Settlement settlement) : base(settlement, (int)policy)
        {
            this.Policy = policy;
        }
        public override string GetHint(int value)
        {
            if (value == (int)CriminalPolicy.Enslavement)
                return "Prisoners sold in the settlement will be enslaved and join the population. No particular repercussions.";
            else if (value == (int)CriminalPolicy.Execution)
                return "Prisoners will suffer the death penalty. No ransom is paid (to non-lord prisoners), but the populace supports this action - if they share your culture. If not, the opposite applies.";
            else return "Forgive prisoners of war. No ransom is paid (to non-lord prisoners), and soldiers rejoin the population as serfs in a settlement of their culture. The populace supports this, if they do not share your culture. The opposite applies.";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (CriminalPolicy)vm.value;
                base.Selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
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

        public override string GetIdentifier() => "criminal";
    }
}
