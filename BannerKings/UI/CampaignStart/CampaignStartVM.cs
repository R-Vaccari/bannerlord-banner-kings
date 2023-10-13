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
        private bool canFinish;

        public CampaignStartVM() : base(null, false)
        {
            options = new MBBindingList<StartOptionVM>();
            CanFinish = false;
            RefreshValues();
        }

        [DataSourceProperty] public string ConfirmText => GameTexts.FindText("str_accept").ToString();

        [DataSourceProperty]
        public bool CanFinish
        {
            get => canFinish;
            set
            {
                if (value != canFinish)
                {
                    canFinish = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

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
            var behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>();
            behavior.SetStartOption(Selected.Option);
            ExecuteClose();
            behavior.OnCharacterCreationOver();
        }

        public void OnSelectOption(StartOptionVM option)
        {
            if (Selected != null)
            {
                Selected.IsSelected = false;
            }

            CanFinish = true;
            Selected = option;
            Selected.IsSelected = true;
        }
    }
}