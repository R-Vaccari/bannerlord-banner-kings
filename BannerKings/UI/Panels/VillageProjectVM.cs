using BannerKings.Populations;
using BannerKings.UI.InnerPanels;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Panels
{
    public class VillageProjectVM : BannerKingsViewModel
    {
        private VillageProjectsVM projects;
        public VillageProjectVM(PopulationData data) : base(data, true)
        {
            projects = new VillageProjectsVM(data);
			GameTexts.SetVariable("VILLAGE_NAME", data.Settlement.Name);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
			projects.RefreshValues();
        }

		[DataSourceProperty]
		public string Title => new TextObject("{=!}Projects at {VILLAGE_NAME}").ToString();

        [DataSourceProperty]
		public VillageProjectsVM Projects
		{
			get => this.projects;

			set
			{
				if (value != this.projects)
				{
					this.projects = value;
					base.OnPropertyChangedWithValue(value, "Projects");
				}
			}
		}

		public void ExecuteClose()
		{
			UIManager.Instance.CloseUI();
		}
	}
}
