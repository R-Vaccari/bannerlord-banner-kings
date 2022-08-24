using System.Text;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Models.Vanilla;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Management
{
    public class VillageProjectVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> constructionInfo;
        private MBBindingList<InformationElement> productionInfo;
        private SettlementProjectSelectionVM projects;
        private readonly VillageData villageData;

        public VillageProjectVM(PopulationData data) : base(data, true)
        {
            projects = new SettlementProjectSelectionVM(data.Settlement, OnProjectSelectionDone);
            constructionInfo = new MBBindingList<InformationElement>();
            productionInfo = new MBBindingList<InformationElement>();
            GameTexts.SetVariable("VILLAGE_NAME", data.Settlement.Name);
            villageData = this.data.VillageData;
        }

        [DataSourceProperty] public string Title => new TextObject("{=y5BMxhJv}Projects at {VILLAGE_NAME}").ToString();

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

        public override void RefreshValues()
        {
            base.RefreshValues();
            projects.RefreshValues();

            ConstructionInfo.Clear();
            ProductionInfo.Clear();
            ConstructionInfo.Add(new InformationElement("Construction:", villageData.Construction + " (Daily)",
                "How much the local population can progress with construction projects, on a daily basis"));
            ConstructionInfo.Add(new InformationElement("Current Progress:", villageData.IsCurrentlyBuilding
                    ? FormatValue(villageData.CurrentBuilding.BuildingProgress /
                                  villageData.CurrentBuilding.GetConstructionCost())
                    : "Daily project (endless)",
                "Amount of completed work in the current project"));
            ConstructionInfo.Add(new InformationElement("Days to complete:", villageData.IsCurrentlyBuilding
                    ? FormatDays((villageData.CurrentBuilding.GetConstructionCost() -
                                  villageData.CurrentBuilding.BuildingProgress) /
                                 villageData.Construction) + " Days"
                    : "Daily project (endless)",
                "Remaining days for the current project to be built"));

            var model = new BKVillageProductionModel();
            var productionQuantity = 0f;
            var sb = new StringBuilder();
            foreach (var production in BannerKingsConfig.Instance.PopulationManager.GetProductions(villageData))
            {
                sb.Append(production.Item1.Name + ", ");
                productionQuantity += model.CalculateDailyProductionAmount(villageData.Village, production.Item1);
            }

            sb.Remove(sb.Length - 2, 1);
            var productionString = sb.ToString();
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
                var localDevelopmentList = Projects.LocalDevelopmentList;
                var building = Projects.CurrentDailyDefault?.Building;
                if (localDevelopmentList is {Count: > 0})
                {
                    foreach (VillageBuilding building2 in localDevelopmentList)
                    {
                        if (!building2.BuildingType.IsDefaultProject)
                        {
                            villageData.BuildingsInProgress.Enqueue(building2);
                        }
                    }
                }

                if (building != null && building.BuildingType.BuildingLocation == BuildingLocation.Daily)
                {
                    foreach (var b in villageData.Buildings)
                    {
                        if (b.BuildingType.StringId == building.BuildingType.StringId)
                        {
                            b.IsCurrentlyDefault = true;
                        }
                        else
                        {
                            b.IsCurrentlyDefault = false;
                        }
                    }
                }
            }
        }

        public new void ExecuteClose()
        {
            UIManager.Instance.CloseUI();
        }
    }
}