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

            string hint;
            if (policy == TaxType.High)
                hint = "Yield more tax from the population, at the cost of decreased loyalty";
            else if (policy == TaxType.Low)
                hint = "Reduce tax burden on the population, diminishing your profit but increasing their support towards you";
            else hint = "Standard tax of the land, with no particular repercussions";
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(hint));
        }
    }
}
