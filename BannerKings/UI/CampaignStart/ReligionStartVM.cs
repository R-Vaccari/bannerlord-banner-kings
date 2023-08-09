using BannerKings.Managers.Institutions.Religions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.UI.CampaignStart
{
    public class ReligionStartVM : BannerKingsViewModel
    {
        private MBBindingList<ReligionStartOptionVM> options;
        private new ReligionStartOptionVM selected;
        private bool canFinish;

        public ReligionStartVM() : base(null, false)
        {
            options = new MBBindingList<ReligionStartOptionVM>();
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
        public ReligionStartOptionVM Selected
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
        public MBBindingList<ReligionStartOptionVM> Options
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

            foreach (var option in DefaultReligions.Instance.All)
            {
                Options.Add(new ReligionStartOptionVM(option, OnSelectOption));
            }
        }

        public void ExecuteFinish()
        {
            BannerKingsConfig.Instance.ReligionsManager.AddToReligion(Hero.MainHero,
                Selected.Religion);
            ExecuteClose();
        }

        public void OnSelectOption(ReligionStartOptionVM option)
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