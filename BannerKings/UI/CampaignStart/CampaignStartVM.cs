using BannerKings.Managers.CampaignStart;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.UI.CampaignStart
{
    public class CampaignStartVM : BannerKingsViewModel
    {
        private MBBindingList<StartOptionVM> options;
        private StartOptionVM selected;

        public CampaignStartVM() : base(null, false)
        {
            options = new MBBindingList<StartOptionVM>();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Options.Clear();

            foreach (StartOption option in DefaultStartOptions.Instance.All)
                Options.Add(new StartOptionVM(option, new Action<StartOptionVM>(OnSelectOption)));
        }

        public void ExecuteFinish()
        {
            Selected.Action?.Invoke();
            ExecuteClose();
        }

        public void OnSelectOption(StartOptionVM option)
        {
            if (Selected != null) Selected.IsSelected = false;

            Selected = option;
            Selected.IsSelected = true;
        }

        [DataSourceProperty]
        public string ConfirmText => GameTexts.FindText("str_accept").ToString();

        [DataSourceProperty]
        public StartOptionVM Selected
        {
            get => selected;
            set
            {
                if (value != selected)
                {
                    selected = value;
                    OnPropertyChangedWithValue(value, "Selected");
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
                    OnPropertyChangedWithValue(value, "Options");
                }
            }
        }
    }
}
