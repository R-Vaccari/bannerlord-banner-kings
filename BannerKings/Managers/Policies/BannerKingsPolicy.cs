using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;

namespace BannerKings.Managers.Policies
{
    public abstract class BannerKingsPolicy
    {
        public Settlement settlement;
        public int selected;
        public BannerKingsPolicy(Settlement settlement, int selected)
        {
            this.settlement = settlement;
            this.selected = selected;
        }
        public abstract string GetHint();

        public abstract void OnChange(SelectorVM<BKItemVM> obj);

        public abstract IEnumerable<Enum> GetPolicies();
    }
}
