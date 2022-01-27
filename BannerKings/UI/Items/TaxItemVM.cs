using TaleWorlds.Core.ViewModelCollection;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.UI.Items
{
    public class TaxItemVM : SelectorItemVM
    {
        public TaxType policy { get; private set; }
        public TaxItemVM(TaxType policy, bool isAvailable, string hint) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString();
            base.CanBeSelected = isAvailable;
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(hint));
        }
    }
}
