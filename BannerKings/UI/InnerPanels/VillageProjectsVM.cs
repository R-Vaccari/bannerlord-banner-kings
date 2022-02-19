using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Library;

namespace BannerKings.UI.InnerPanels
{
	public class VillageProjectsVM : BannerKingsViewModel
    {
		private MBBindingList<SettlementBuildingProjectVM> projects;

		public VillageProjectsVM(PopulationData data) : base(data, true)
        {
			this.projects = new MBBindingList<SettlementBuildingProjectVM>();

		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			VillageData villageData = base.data.VillageData;
			if (villageData != null)
            {
				foreach (VillageBuilding building in villageData.Buildings)
					projects.Add(new SettlementBuildingProjectVM(delegate { },
						delegate { },
						delegate { },
						building
					));
            }
        }

        [DataSourceProperty]
		public MBBindingList<SettlementBuildingProjectVM> AvailableProjects
		{
			get => this.projects;
			
			set
			{
				if (value != this.projects)
				{
					this.projects = value;
					base.OnPropertyChangedWithValue(value, "AvailableProjects");
				}
			}
		}
	}
}
