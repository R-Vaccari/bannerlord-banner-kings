using System;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    public class BKItemVM : SelectorItemVM
    {
        public BKItemVM(Enum policy, bool isAvailable, string hint) : base("")
        {
            value = (int) (object) policy;
            StringItem = policy.ToString().Replace("_", " ");
            CanBeSelected = isAvailable;
            Hint = new HintViewModel(new TextObject("{=!}" + hint));
        }

        public int value { get; }
    }
}