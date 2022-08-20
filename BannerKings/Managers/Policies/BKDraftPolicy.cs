using System;
using System.Collections.Generic;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies;

internal class BKDraftPolicy : BannerKingsPolicy
{
    public enum DraftPolicy
    {
        Standard,
        Conscription,
        Demobilization
    }

    public BKDraftPolicy(DraftPolicy policy, Settlement settlement) : base(settlement, (int) policy)
    {
        Policy = policy;
    }

    [SaveableProperty(3)] public DraftPolicy Policy { get; private set; }

    public override string GetIdentifier()
    {
        return "draft";
    }

    public override string GetHint(int value)
    {
        if (value == (int) DraftPolicy.Conscription)
        {
            return "Extend conscription of the populace, replenishing recruitment slots faster. Increases adm. costs.";
        }

        if (value == (int) DraftPolicy.Demobilization)
        {
            return "Slow down conscription of new recruits. Slight boost to population growth.";
        }

        return "Standard drafting policy, no particular effect.";
    }

    public override void OnChange(SelectorVM<BKItemVM> obj)
    {
        if (obj.SelectedItem != null)
        {
            var vm = obj.GetCurrentItem();
            Policy = (DraftPolicy) vm.value;
            Selected = vm.value;
            BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
        }
    }

    public override IEnumerable<Enum> GetPolicies()
    {
        yield return DraftPolicy.Standard;
        yield return DraftPolicy.Conscription;
        yield return DraftPolicy.Demobilization;
    }
}