using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    class BKDraftPolicy : BannerKingsPolicy
    {
        public override string GetIdentifier() => "draft";

        [SaveableProperty(3)]
        public DraftPolicy Policy { get; private set; }
        public BKDraftPolicy(DraftPolicy policy, Settlement settlement) : base(settlement, (int)policy)
        {
            this.Policy = policy;
        }

        public override string GetHint(int value)
        {
            if (value == (int)DraftPolicy.Conscription)
                return "Extend conscription of the populace, replenishing recruitment slots faster. Increases adm. costs.";
            else if (value == (int)DraftPolicy.Demobilization)
                return "Slow down conscription of new recruits. Slight boost to population growth.";
            return "Standard drafting policy, no particular effect.";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (DraftPolicy)vm.value;
                base.Selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
            }
        }

        public enum DraftPolicy
        {
            Standard,
            Conscription,
            Demobilization
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return DraftPolicy.Standard;
            yield return DraftPolicy.Conscription;
            yield return DraftPolicy.Demobilization;
            yield break;
        }
    }
}
