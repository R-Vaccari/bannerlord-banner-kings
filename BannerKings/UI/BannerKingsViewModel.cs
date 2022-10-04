using System;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.UI.Items;
using TaleWorlds.Core.ViewModelCollection.Selector;
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

        [DataSourceProperty] public bool HasTown => !IsVillage;

        [DataSourceProperty] public bool IsVillage => data != null && data.Settlement.IsVillage;


        [DataSourceProperty]
        public bool IsSelected
        {
            get => selected;
            set
            {
                if (value != selected)
                {
                    selected = value;
                    if (value)
                    {
                        RefreshValues();
                    }

                    OnPropertyChangedWithValue(value);
                }
            }
        }
        protected string FormatFloatWithSymbols(float value)
        {

            string formatted = value.ToString("0.00");
            if (value > 0f)
            {
                return '+' + formatted;
            }
            else if (value < 0f)
            {
                return '-' + formatted;
            }

            return formatted;
        }

        protected string FormatValue(float value)
        {
            return (value * 100f).ToString("0.00") + '%';
        }

        protected string FormatDays(float value)
        {
            return value.ToString("0");
        }

        protected SelectorVM<BKItemVM> GetSelector(BannerKingsPolicy policy, Action<SelectorVM<BKItemVM>> action)
        {
            var selector = new SelectorVM<BKItemVM>(0, action);
            selector.SetOnChangeAction(null);
            var i = 0;
            foreach (var enumValue in policy.GetPolicies())
            {
                var item = new BKItemVM(enumValue, true, policy.GetHint(i));
                selector.AddItem(item);
                i++;
            }


            return selector;
        }

        public void ExecuteClose()
        {
            UIManager.Instance.CloseUI();
        }
    }
}