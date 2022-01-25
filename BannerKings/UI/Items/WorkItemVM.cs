using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.UI.Items
{
    public class WorkItemVM : SelectorItemVM
    {
        public WorkforcePolicy policy { get; private set; }
        public WorkItemVM(WorkforcePolicy policy, bool isAvailable) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString().Replace("_", " ");
            base.CanBeSelected = isAvailable;
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(BannerKingsConfig.Instance.PolicyManager.GetWorkHint(policy)));
        }
    }
}
