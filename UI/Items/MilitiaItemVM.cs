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

            string hint;
            if (policy == MilitiaPolicy.Melee)
                hint = "Focus three fourths of the militia as melee troops";
            else if (policy == MilitiaPolicy.Ranged)
                hint = "Focus three fourths of the militia as ranged troops";
            else hint = "Split militia equally between ranged and melee troops";
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(hint));
        }
    }
}
