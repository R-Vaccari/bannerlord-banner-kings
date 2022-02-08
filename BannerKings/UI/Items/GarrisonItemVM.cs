using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.UI.Items
{
    public class GarrisonItemVM : SelectorItemVM
    {
        public GarrisonPolicy policy { get; private set; }
        public GarrisonItemVM(GarrisonPolicy policy, bool isAvailable) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString();
            base.CanBeSelected = isAvailable;
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(BannerKingsConfig.Instance.PolicyManager.GetGarrisonHint(policy)));
        }
    }
}
