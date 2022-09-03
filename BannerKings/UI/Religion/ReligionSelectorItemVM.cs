using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace BannerKings.UI.Religion
{
    public class ReligionSelectorItemVM : SelectorItemVM
    {
        public Managers.Institutions.Religions.Religion Religion { get; private set; }
        public ReligionSelectorItemVM(Managers.Institutions.Religions.Religion religion, bool canSelect) : base("")
        {
            Religion = religion;
            CanBeSelected = canSelect;
            StringItem = religion.Faith.GetFaithName().ToString();
            Hint = new HintViewModel(religion.Faith.GetFaithDescription());
        }
    }
}
