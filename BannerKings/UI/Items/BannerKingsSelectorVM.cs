using System;
using System.Collections.Generic;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class BannerKingsSelectorVM<T> : SelectorVM<T> where T : SelectorItemVM
    {
        private bool enabled;
        public BannerKingsSelectorVM(bool enabled, int selectedIndex, Action<SelectorVM<T>> onChange) : base(selectedIndex, onChange)
        {
            Enabled = enabled;
        }

        public BannerKingsSelectorVM(bool enabled, IEnumerable<string> list, int selectedIndex, Action<SelectorVM<T>> onChange) : base(list, selectedIndex, onChange)
        {
            Enabled = enabled;
        }

        [DataSourceProperty]
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}
