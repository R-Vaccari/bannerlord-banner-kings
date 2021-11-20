using TaleWorlds.Core.ViewModelCollection;
using static Populations.PolicyManager;

namespace Populations.UI.Items
{
    public class WorkItemVM : SelectorItemVM
    {
        public WorkforcePolicy policy { get; private set; }
        public WorkItemVM(WorkforcePolicy policy, bool isAvailable) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString().Replace("_", " ");
            base.CanBeSelected = isAvailable;
        }
    }
}
