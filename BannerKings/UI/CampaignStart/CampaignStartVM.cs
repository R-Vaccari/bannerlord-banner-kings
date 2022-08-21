using BannerKings.Behaviours;
using BannerKings.Managers.CampaignStart;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.UI.CampaignStart
{
    public class CampaignStartVM : BannerKingsViewModel
    {
        private MBBindingList<StartOptionVM> options;
        private new StartOptionVM selected;

        public CampaignStartVM() : base(null, false)
        {
            options = new MBBindingList<StartOptionVM>();
            RefreshValues();
        }

        [DataSourceProperty] public string ConfirmText => GameTexts.FindText("str_accept").ToString();

        [DataSourceProperty]
        public StartOptionVM Selected
        {
            get => selected;
            set
            {
                if (value != selected)
                {
                    selected = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StartOptionVM> Options
        {
            get => options;
            set
            {
                if (value != options)
                {
                    options = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Options.Clear();

            foreach (var option in DefaultStartOptions.Instance.All)
            {
                Options.Add(new StartOptionVM(option, OnSelectOption));
            }
        }

        public void ExecuteFinish()
        {
            Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>().SetStartOption(Selected.Option);
            ExecuteClose();
        }

        public void OnSelectOption(StartOptionVM option)
        {
            if (Selected != null)
            {
                Selected.IsSelected = false;
            }

            Selected = option;
            Selected.IsSelected = true;
        }
    }
}