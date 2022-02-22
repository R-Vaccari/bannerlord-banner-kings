using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using BannerKings.UI.InnerPanels;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Panels
{
    public class VillageProjectVM : BannerKingsViewModel
    {
        private SettlementProjectSelectionVM projects;

        public VillageProjectVM(PopulationData data) : base(data, true)
        {
            projects = new SettlementProjectSelectionVM(data.Settlement, new Action(this.OnProjectSelectionDone));
			GameTexts.SetVariable("VILLAGE_NAME", data.Settlement.Name);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
			projects.RefreshValues();
        }

		private void OnProjectSelectionDone()
		{
			VillageData villageData = base.data.VillageData;
			if (villageData != null)
            {
				villageData.BuildingsInProgress.Clear();
				List<Building> localDevelopmentList = this.Projects.LocalDevelopmentList;
				Building building = this.Projects.CurrentDailyDefault.Building;
				if (localDevelopmentList != null)
					foreach (VillageBuilding building2 in localDevelopmentList)
						if (!building2.BuildingType.IsDefaultProject)
							villageData.BuildingsInProgress.Enqueue(building2);
				
			}
			
		}

		[DataSourceProperty]
		public string Title => new TextObject("{=!}Projects at {VILLAGE_NAME}").ToString();

        [DataSourceProperty]
		public SettlementProjectSelectionVM Projects
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
