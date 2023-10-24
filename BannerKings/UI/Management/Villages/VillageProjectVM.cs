using System.Text;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Management.Villages
{
    public class VillageProjectVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> constructionInfo;
        private MBBindingList<InformationElement> productionInfo;
        private VillageProjectSelectionVM projects;
        private readonly VillageData villageData;

        public VillageProjectVM(PopulationData data) : base(data, true)
        {
            projects = new VillageProjectSelectionVM(data);
            constructionInfo = new MBBindingList<InformationElement>();
            productionInfo = new MBBindingList<InformationElement>();
            GameTexts.SetVariable("VILLAGE_NAME", data.Settlement.Name);
            villageData = this.data.VillageData;
        }

        [DataSourceProperty]
        public string Title => new TextObject("{=y5BMxhJv}Projects at {VILLAGE_NAME}").ToString();
        [DataSourceProperty]
        public string AvailableProjects => new TextObject("{=hqJtVfPx}Available Projects").ToString();

        [DataSourceProperty]
        public VillageProjectSelectionVM Projects
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
            ConstructionInfo.Add(new InformationElement(new TextObject("{=KbTvcQko}Construction:").ToString(),
                new TextObject("{=mbUwoU0h}{POINTS} (Daily)")
                    .SetTextVariable("POINTS", villageData.Construction.ToString("0.00")).ToString(),
                new TextObject("{=pVCiG95C}How much the local population can progress with construction projects, on a daily basis").ToString()));

            ConstructionInfo.Add(new InformationElement(new TextObject("{=fvaNp0we}Current Progress:").ToString(),
                villageData.IsCurrentlyBuilding
                    ? FormatValue(villageData.CurrentBuilding.BuildingProgress /
                                  villageData.CurrentBuilding.GetConstructionCost())
                    : new TextObject("{=ZhWtAYrh}Daily project (endless)").ToString(),
                new TextObject("{=uUQpLvpq}Amount of completed work in the current project").ToString()));

            ConstructionInfo.Add(new InformationElement(new TextObject("{=CT7CW9ZL}Days to complete:").ToString(),
                villageData.IsCurrentlyBuilding
                    ? FormatDays((villageData.CurrentBuilding.GetConstructionCost() -
                                  villageData.CurrentBuilding.BuildingProgress) /
                                 villageData.Construction) + " Days"
                    : new TextObject("{=ZhWtAYrh}Daily project (endless)").ToString(),
                new TextObject("{=f26MsNsU}Remaining days for the current project to be built").ToString()));

            var sb = new StringBuilder();
            foreach (var production in BannerKingsConfig.Instance.PopulationManager.GetProductions(data))
            {
                sb.Append(production.Item1.Name + ", ");
            }

            sb.Remove(sb.Length - 2, 1);
            var productionString = sb.ToString();
            var productionExplained = villageData.ProductionsExplained;
            ProductionInfo.Add(new InformationElement(new TextObject("{=Fin3KXMP}Goods Production:").ToString(),
                new TextObject("{=mbUwoU0h}{POINTS} (Daily)")
                .SetTextVariable("POINTS", productionExplained.ResultNumber.ToString("0.00"))
                .ToString(),
                new TextObject("{=CS_explain}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=g480uUyC}Sum of goods produced on a daily basis, including all the types produced here."))
                    .SetTextVariable("EXPLANATIONS", productionExplained.GetExplanations())
                    .ToString()));

            ProductionInfo.Add(new InformationElement(new TextObject("{=e8Wk3tKw}Items Produced:").ToString(),
                productionString,
                new TextObject("{=0RAPEDaT}Goods locally produced by the population.").ToString()));
        }

        private void OnProjectSelectionDone()
        {
            if (villageData != null)
            {
                villageData.BuildingsInProgress.Clear();
                var localDevelopmentList = Projects.LocalDevelopmentList;
                var building = Projects.CurrentDailyDefault?.Building;
                if (localDevelopmentList is { Count: > 0 })
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