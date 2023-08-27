using BannerKings.Managers.Innovations.Eras;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace BannerKings.UI.Titles
{
    public class EraSelectorItem : SelectorItemVM
    {
        public Era Era { get; private set; }
        public EraSelectorItem(Era era) : base("")
        {
            Era = era;
            StringItem = Era.Name.ToString();
            CanBeSelected = true;
            Hint = new HintViewModel(Era.Description);
        }
    }
}
