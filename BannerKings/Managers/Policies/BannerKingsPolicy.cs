using System;
using System.Collections.Generic;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    public abstract class BannerKingsPolicy
    {
        public BannerKingsPolicy(Settlement settlement, int selected)
        {
            Settlement = settlement;
            Selected = selected;
        }

        [SaveableProperty(1)] public Settlement Settlement { get; }

        [SaveableProperty(2)] public int Selected { get; protected set; }

        public abstract string GetHint(int value);
        public abstract string GetIdentifier();
        public abstract void OnChange(SelectorVM<BKItemVM> obj);
        public abstract IEnumerable<Enum> GetPolicies();
    }
}