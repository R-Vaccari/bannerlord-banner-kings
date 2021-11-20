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

            string hint;
            if (policy == WorkforcePolicy.Construction)
                hint = "Serfs aid in construction for a gold cost, and food production suffers a penalty if land usage is below 100%";
            else if (policy == WorkforcePolicy.Land_Expansion)
                hint = "Divert slaves and serf workforces to expand the arable land, reducing their outputs while extending usable land";
            else if (policy == WorkforcePolicy.Martial_Law)
                hint = "Put the militia on active duty, increasing security but costing a food upkeep";
            else hint = "No particular policy is implemented";
            base.Hint = new HintViewModel(new TaleWorlds.Localization.TextObject(hint));
        }
    }
}
