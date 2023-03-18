using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace BannerKings.UI.Titles
{
    public class KingdomSelectorItem : SelectorItemVM
    {
        public Kingdom Kingdom { get; private set; }
        public KingdomSelectorItem(Kingdom kingdom) : base("")
        {
            Kingdom = kingdom;
            StringItem = Kingdom.Name.ToString();
            CanBeSelected = true;
            Hint = new HintViewModel(Kingdom.EncyclopediaText);
        }
    }
}
