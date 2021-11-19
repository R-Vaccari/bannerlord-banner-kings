using TaleWorlds.Core.ViewModelCollection;
using static Populations.PolicyManager;

namespace Populations.UI.Items
{
    public class MilitiaItemVM : SelectorItemVM
    {
        public MilitiaPolicy policy { get; private set; }
        public MilitiaItemVM(MilitiaPolicy policy, bool isAvailable) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString();
            base.CanBeSelected = isAvailable;
        }
    }
}
