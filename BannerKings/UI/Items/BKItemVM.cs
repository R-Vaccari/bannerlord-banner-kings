﻿using System;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    public class BKItemVM : SelectorItemVM
    {
        public int value { get; private set; }
        public BKItemVM(Enum policy, bool isAvailable, string hint) : base("")
        {
            value = (int)(object)policy;
            StringItem = policy.ToString().Replace("_", " ");
            CanBeSelected = isAvailable;
            Hint = new HintViewModel(new TextObject(hint));
        }
    }
}
