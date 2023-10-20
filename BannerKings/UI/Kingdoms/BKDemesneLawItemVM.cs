using BannerKings.Managers.Titles.Laws;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Localization;

namespace BannerKings.UI.Kingdoms
{
    public class BKDemesneLawItemVM : SelectorItemVM
    {
        public DemesneLaw DemesneLaw { get; private set; }
        public BKDemesneLawItemVM(DemesneLaw law) : base("")
        {
            DemesneLaw = law;
            StringItem = law.Name.ToString();
            CanBeSelected = true;
            Hint = new HintViewModel(new TextObject("{=CS_explain2}{TEXT}\n\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT", law.Description)
                        .SetTextVariable("EXPLANATIONS", law.Effects));
        }
    }
}