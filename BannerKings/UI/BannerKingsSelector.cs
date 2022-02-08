using BannerKings.Managers.Policies;
using BannerKings.UI.Items;
using System;
using TaleWorlds.Core.ViewModelCollection;

namespace BannerKings.UI
{
    public class BannerKingsSelector<SelectorItemVM> : SelectorVM<BKItemVM>
    {

        public BannerKingsSelector(BannerKingsPolicy policy, int index, Action<SelectorVM<BKItemVM>> action) : base(index, action)
        {
            foreach (Enum enumValue in policy.GetPolicies())
            {
                BKItemVM item = new BKItemVM(enumValue, true, policy.GetHint());
                this.AddItem(item);
            }

            this.SelectedIndex = policy.selected;
            this.SetOnChangeAction(action);
        }
    }
}
