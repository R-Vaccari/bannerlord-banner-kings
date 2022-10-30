using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace BannerKings.UI.Kingdoms
{
    public class KingdomDemesneVM : BannerKingsViewModel
    {
        private MBBindingList<DemesneLawVM> laws;

        public KingdomDemesneVM(FeudalTitle title, Kingdom kingdom) : base(null, false)
        {
            Title = title;
            Kingdom = kingdom;
            laws = new MBBindingList<DemesneLawVM>();
        }

        public FeudalTitle Title { get; private set; }
        public Kingdom Kingdom { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Laws.Clear();
            foreach (var law in Title.contract.DemesneLaws)
            {
                Laws.Add(new DemesneLawVM(DefaultDemesneLaws.Instance.GetLawsByType(law.LawType),
                    law,
                    OnChange));
            }
        }

        private void OnChange(SelectorVM<BKItemVM> obj)
        {

        }

        [DataSourceProperty]
        public MBBindingList<DemesneLawVM> Laws
        {
            get => laws;
            set
            {
                if (value != laws)
                {
                    laws = value;
                    OnPropertyChangedWithValue(value, "Laws");
                }
            }
        }
    }
}
