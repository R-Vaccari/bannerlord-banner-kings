using TaleWorlds.Core.ViewModelCollection;
using static Populations.PolicyManager;

namespace Populations.UI.Items
{
    public class CrimeItemVM : SelectorItemVM
    {
        public CriminalPolicy policy { get; private set; }
        public CrimeItemVM(CriminalPolicy policy, bool isAvailable) : base("")
        {
            this.policy = policy;
            base.StringItem = policy.ToString().Replace("_", " ");
            base.CanBeSelected = isAvailable;
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(PopulationConfig.Instance.PolicyManager.GetCrimeHint(policy)));
        }
    }
}
