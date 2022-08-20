using BannerKings.Managers.Populations.Villages;
using BannerKings.Models;
using BannerKings.Populations;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Panels
{
    public class VillageProjectVM : BannerKingsViewModel
    {
        private SettlementProjectSelectionVM projects;
		private MBBindingList<InformationElement> constructionInfo;
		private MBBindingList<InformationElement> productionInfo;
		private VillageData villageData;

		public VillageProjectVM(PopulationData data) : base(data, true)
        {
            projects = new SettlementProjectSelectionVM(data.Settlement, OnProjectSelectionDone);
			constructionInfo = new MBBindingList<InformationElement>();
			productionInfo = new MBBindingList<InformationElement>();
			GameTexts.SetVariable("VILLAGE_NAME", data.Settlement.Name);
			villageData = this.data.VillageData;
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			projects.RefreshValues();

			ConstructionInfo.Clear();
			ProductionInfo.Clear();
			ConstructionInfo.Add(new InformationElement("Construction:", villageData.Construction + " (Daily)",
			   "How much the local population can progress with construction projects, on a daily basis"));
			ConstructionInfo.Add(new InformationElement("Current Progress:", villageData.IsCurrentlyBuilding ? FormatValue(villageData.CurrentBuilding.BuildingProgress / 
				villageData.CurrentBuilding.GetConstructionCost()) : "Daily project (endless)",
			   "Amount of completed work in the current project"));
			ConstructionInfo.Add(new InformationElement("Days to complete:", villageData.IsCurrentlyBuilding ?
				FormatDays((villageData.CurrentBuilding.GetConstructionCost() - villageData.CurrentBuilding.BuildingProgress) /
				villageData.Construction) + " Days" : "Daily project (endless)",
			   "Remaining days for the current project to be built"));

			BKVillageProductionModel model = new BKVillageProductionModel();
			float productionQuantity = 0f;
			StringBuilder sb = new StringBuilder();
			foreach ((ItemObject, float) production in BannerKingsConfig.Instance.PopulationManager.GetProductions(villageData))
            {
				sb.Append(production.Item1.Name + ", ");
				productionQuantity += model.CalculateDailyProductionAmount(villageData.Village, production.Item1);
			}
			sb.Remove(sb.Length - 2, 1);
			string productionString = sb.ToString();
			ProductionInfo.Add(new InformationElement("Goods Production:", productionQuantity + " (Daily)",
			   "Sum of goods produced on a daily basis, including all the types produced here"));
			ProductionInfo.Add(new InformationElement("Items Produced:", productionString,
			   "The types of outputs produced in this village"));
		}

		private void OnProjectSelectionDone()
		{	
			if (villageData != null)
            {
				villageData.BuildingsInProgress.Clear();
				List<Building> localDevelopmentList = Projects.LocalDevelopmentList;
				Building? building = Projects.CurrentDailyDefault?.Building;
				if (localDevelopmentList != null && localDevelopmentList.Count > 0)
                {
					foreach (VillageBuilding building2 in localDevelopmentList)
						if (!building2.BuildingType.IsDefaultProject)
							villageData.BuildingsInProgress.Enqueue(building2);
				}

				if (building != null && building.BuildingType.BuildingLocation == BuildingLocation.Daily)
					foreach (VillageBuilding b in villageData.Buildings)
                    {
						if (b.BuildingType.StringId == building.BuildingType.StringId)
							b.IsCurrentlyDefault = true;
						else b.IsCurrentlyDefault = false;
                    }
			}
		}

		[DataSourceProperty]
		public string Title => new TextObject("{=!}Projects at {VILLAGE_NAME}").ToString();

        [DataSourceProperty]
		public SettlementProjectSelectionVM Projects
		{
			get => projects;

			set
			{
				if (value != projects)
				{
					projects = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> ProductionInfo
		{
			get => productionInfo;
			set
			{
				if (value != productionInfo)
				{
					productionInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> ConstructionInfo
		{
			get => constructionInfo;
			set
			{
				if (value != constructionInfo)
				{
					constructionInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		public void ExecuteClose()
		{
			UIManager.Instance.CloseUI();
		}
	}
}
