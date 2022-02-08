using System;
using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.UI.Items
{
    public class BKItemVM : SelectorItemVM
    {
        public int value { get; private set; }
        public BKItemVM(Enum policy, bool isAvailable, string hint) : base("")
        {
            this.value = (int)(object)policy;
            base.StringItem = policy.ToString().Replace("_", " ");
            base.CanBeSelected = isAvailable;
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(hint));
        }
    }
}
