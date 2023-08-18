using BannerKings.Managers.Innovations;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Titles
{
    public class CultureTabVM : BannerKingsViewModel
    {
        private MBBindingList<InnovationElementVM> innovations;
        private BannerKingsSelectorVM<KingdomSelectorItem> selector;

        public CultureTabVM() : base(null, false)
        {
            Innovations = new MBBindingList<InnovationElementVM>();
            Selector = new BannerKingsSelectorVM<KingdomSelectorItem>(true, 0, null);

            int selected = 0;
            int index = 0;
            foreach (Kingdom k in Kingdom.All)
            {
                var kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(k);
                if (kingdomTitle == null)
                {
                    continue;
                }

                Selector.AddItem(new KingdomSelectorItem(k));
                if (k == Hero.MainHero.CurrentSettlement.MapFaction)
                {
                    selected = index;
                }

                index++;
            }

            Selector.SelectedIndex = selected;
            Selector.SetOnChangeAction(OnChange);
        }

        private void OnChange(SelectorVM<KingdomSelectorItem> obj)
        {
            if (obj.SelectedItem != null)
            {
                //title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(obj.SelectedItem.Kingdom);
                RefreshValues();
            }
        }

        public CultureObject Culture => Settlement.CurrentSettlement.Culture;

        public int Population { get; set; } = 0;

        public override void RefreshValues()
        {
            base.RefreshValues();

            foreach (var i in DefaultInnovations.Instance.All)
            {
                if (i.Requirement == null)
                {
                    Innovations.Add(new InnovationElementVM(i));
                }
            }
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<KingdomSelectorItem> Selector
        {
            get => selector;
            set
            {
                if (value != selector)
                {
                    selector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InnovationElementVM> Innovations
        {
            get => innovations;
            set
            {
                if (value != innovations)
                {
                    innovations = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}