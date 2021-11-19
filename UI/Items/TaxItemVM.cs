using TaleWorlds.Core.ViewModelCollection;
using static Populations.PolicyManager;

namespace Populations.UI.Items
{
    public class TaxItemVM : SelectorItemVM
    {
        public TaxType policy { get; private set; }
        public TaxItemVM(TaxType policy, bool isAvailable) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString();
            base.CanBeSelected = isAvailable;
        }
    }
}
