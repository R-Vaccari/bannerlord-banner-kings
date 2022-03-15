using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    public abstract class BannerKingsPolicy
    {
        [SaveableProperty(1)]
        public Settlement Settlement { get; private set; }

        [SaveableProperty(2)]
        public int Selected { get; protected set; }

        public BannerKingsPolicy(Settlement settlement, int selected)
        {
            this.Settlement = settlement;
            this.Selected = selected;
        }
        public abstract string GetHint(int value);
        public abstract string GetIdentifier();
        public abstract void OnChange(SelectorVM<BKItemVM> obj);
        public abstract IEnumerable<Enum> GetPolicies();
    }
}
