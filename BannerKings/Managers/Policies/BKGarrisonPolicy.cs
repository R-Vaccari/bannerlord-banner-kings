using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Policies
{
    class BKGarrisonPolicy : BannerKingsPolicy
    {
        public override string GetIdentifier() => "garrison";

        [SaveableProperty(3)]
        public GarrisonPolicy Policy { get; private set; }
        public BKGarrisonPolicy(GarrisonPolicy policy, Settlement settlement) : base(settlement, (int)policy)
        {
            this.Policy = policy;
        }
        public override string GetHint(int value)
        {
            if (value == (int)GarrisonPolicy.Dischargement)
                return "Discharge a garrison member on a daily basis from duty. Slows down garrison trainning.";
            else if (value == (int)GarrisonPolicy.Enlistment)
                return "Increase the quantity of auto recruited garrison soldiers, as well as provide more trainning. Increases adm. costs.";
            else return "Standard garrison policy, no particular effect.";
        }

        public override void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                BKItemVM vm = obj.GetCurrentItem();
                this.Policy = (GarrisonPolicy)vm.value;
                base.Selected = vm.value;
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(Settlement, this);
            }
        }

        public enum GarrisonPolicy
        {
            Standard,
            Enlistment,
            Dischargement
        }

        public override IEnumerable<Enum> GetPolicies()
        {
            yield return GarrisonPolicy.Standard;
            yield return GarrisonPolicy.Enlistment;
            yield return GarrisonPolicy.Dischargement;
            yield break;
        }
    }
}
