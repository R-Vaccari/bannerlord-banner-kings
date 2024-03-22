using BannerKings.Managers.Titles;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace BannerKings.UI.Titles
{
    public class TitleSelectorItem : SelectorItemVM
    {
        public FeudalTitle Title { get; private set; }
        public TitleSelectorItem(FeudalTitle title) : base("")
        {
            Title = title;
            StringItem = title.FullName.ToString();
            CanBeSelected = true;
            Hint = new HintViewModel();
        }
    }
}
