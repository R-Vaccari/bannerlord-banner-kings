using BannerKings.Populations;
using TaleWorlds.Library;

namespace BannerKings.UI
{
    public class BannerKingsViewModel : ViewModel
    {

        protected PopulationData data;
        protected bool selected;

        public BannerKingsViewModel(PopulationData data, bool selected)
        {
            this.data = data;
            this.selected = selected;
        }

        protected string FormatValue(float value) => (value * 100f).ToString("0.00") + '%';

        [DataSourceProperty]
        public bool IsSelected
        {
            get => this.selected;
            set
            {
                if (value != this.selected)
                {
                    this.selected = value;
                    if (value) this.RefreshValues();
                    base.OnPropertyChangedWithValue(value, "IsSelected");
                }
            }
        }
    }
}
