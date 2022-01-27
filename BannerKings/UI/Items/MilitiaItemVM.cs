using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.UI.Items
{
    public class MilitiaItemVM : SelectorItemVM
    {
        public MilitiaPolicy policy { get; private set; }
        public MilitiaItemVM(MilitiaPolicy policy, bool isAvailable) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString();
            base.CanBeSelected = isAvailable;
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(BannerKingsConfig.Instance.PolicyManager.GetMilitiaHint(policy)));
        }
    }
}
