using BannerKings.Managers.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Library;

namespace BannerKings.UI.Management.Villages
{
    public class VillageProjectSelectionVM : ViewModel
    {
        private VillageData villageData;
        public VillageProjectSelectionVM(PopulationData data)
        {
            villageData = data?.VillageData;
        }

        public List<Building> LocalDevelopmentList { get; private set; }

        [DataSourceProperty]
        public string ProjectsText { get; set; }

        [DataSourceProperty]
        public string QueueText { get; set; }

        [DataSourceProperty]
        public string DailyDefaultsText { get; set; }

        public override void RefreshValues()
        {
            AvailableProjects = new MBBindingList<VillageBuildingProjectVM>();
            DailyDefaultList = new MBBindingList<VillageBuildingDailyProjectVM>();
            LocalDevelopmentList = new List<Building>();
            CurrentDevelopmentQueue = new MBBindingList<VillageBuildingProjectVM>();
            AvailableProjects.Clear();

            if (villageData == null)
            {
                return;
            }

            List<Building> buildings = new List<Building>();
            Settlement currentSettlement = Settlement.CurrentSettlement;

            foreach (var b in villageData.Buildings)
            {
                buildings.Add(b);
            }

            foreach (Building building in from x in buildings
                                          where x.BuildingType.BuildingLocation != BuildingLocation.Daily
                                          select x)
            {
                VillageBuildingProjectVM VillageBuildingProjectVM = new VillageBuildingProjectVM(
                    new Action<SettlementProjectVM, bool>(OnCurrentProjectSelection),
                    new Action<SettlementProjectVM>(OnCurrentProjectSet),
                    new Action(OnResetCurrentProject),
                    building);
                AvailableProjects.Add(VillageBuildingProjectVM);
                if (VillageBuildingProjectVM.Building.BuildingType.StringId == villageData.CurrentBuilding.BuildingType.StringId)
                {
                    CurrentSelectedProject = VillageBuildingProjectVM;
                }
            }
            if (Settlement.CurrentSettlement != null)
            {
                foreach (Building building2 in from x in buildings
                                               where x.BuildingType.BuildingLocation == BuildingLocation.Daily
                                               select x)
                {
                    VillageBuildingDailyProjectVM VillageBuildingDailyProjectVM = new VillageBuildingDailyProjectVM(
                        new Action<SettlementProjectVM, bool>(OnCurrentProjectSelection),
                        new Action<SettlementProjectVM>(OnCurrentProjectSet),
                        new Action(OnResetCurrentProject),
                        building2);
                    DailyDefaultList.Add(VillageBuildingDailyProjectVM);
                    if (VillageBuildingDailyProjectVM.Building.BuildingType.StringId ==
                        villageData.CurrentDefault.BuildingType.StringId)
                    {
                        CurrentDailyDefault = VillageBuildingDailyProjectVM;
                        CurrentDailyDefault.IsDefault = true;
                        VillageBuildingDailyProjectVM.IsDefault = true;
                    }
                }
            }
            foreach (Building item in villageData.BuildingsInProgress)
            {
                LocalDevelopmentList.Add(item);
            }

            RefreshDevelopmentsQueueIndex();
        }

        private void OnCurrentProjectSet(SettlementProjectVM selectedItem)
        {
            if (selectedItem != CurrentSelectedProject)
            {
                CurrentSelectedProject = selectedItem;
                CurrentSelectedProject.RefreshProductionText();
            }
        }

        private void OnResetCurrentProject()
        {
            CurrentSelectedProject = LocalDevelopmentList.Count > 0 ? AvailableProjects.First((VillageBuildingProjectVM p) => p.Building == LocalDevelopmentList[0]) : CurrentDailyDefault;
            CurrentSelectedProject.RefreshProductionText();
        }

        private void OnCurrentProjectSelection(SettlementProjectVM selectedItem, bool isSetAsActiveDevelopment)
        {
            if (!selectedItem.IsDaily)
            {
                if (isSetAsActiveDevelopment)
                {
                    LocalDevelopmentList.Clear();
                    LocalDevelopmentList.Add(selectedItem.Building);
                }
                else if (LocalDevelopmentList.Exists((d) => d == selectedItem.Building))
                {
                    LocalDevelopmentList.Remove(selectedItem.Building);
                }
                else
                {
                    LocalDevelopmentList.Add(selectedItem.Building);
                }
            }
            else
            {
                CurrentDailyDefault.IsDefault = false;
                CurrentDailyDefault = selectedItem as VillageBuildingDailyProjectVM;
                (selectedItem as VillageBuildingDailyProjectVM).IsDefault = true;
            }
            RefreshDevelopmentsQueueIndex();
            if (LocalDevelopmentList.Count == 0)
            {
                CurrentSelectedProject = CurrentDailyDefault;
            }
            else if (selectedItem != CurrentSelectedProject)
            {
                CurrentSelectedProject = selectedItem;
            }

            OnQueueChange();
        }

        private void OnQueueChange()
        {
            OnProjectSelectionDone();
        }

        private void OnProjectSelectionDone()
        {
            List<Building> localDevelopmentList = LocalDevelopmentList;
            Building currentDefault = CurrentDailyDefault.Building;
            if (localDevelopmentList != null)
            {
                villageData.BuildingsInProgress = new Queue<Building>();
                foreach (Building b in LocalDevelopmentList)
                {
                    if (!b.BuildingType.IsDefaultProject)
                    {
                        villageData.BuildingsInProgress.Enqueue(b);
                    }
                }
            }

            if (currentDefault.BuildingType.StringId != villageData.CurrentDefault.BuildingType.StringId)
            {
                foreach (Building b in villageData.Buildings)
                {
                    b.IsCurrentlyDefault = false;
                    if (b.BuildingType.StringId == currentDefault.BuildingType.StringId)
                    {
                        currentDefault.IsCurrentlyDefault = true;
                    }
                }
            }
            //RefreshCurrentDevelopment();
        }

        private void RefreshDevelopmentsQueueIndex()
        {
            CurrentSelectedProject = null;
            CurrentDevelopmentQueue = new MBBindingList<VillageBuildingProjectVM>();
            using (IEnumerator<VillageBuildingProjectVM> enumerator = AvailableProjects.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    VillageBuildingProjectVM item = enumerator.Current;
                    item.DevelopmentQueueIndex = -1;
                    item.IsInQueue = LocalDevelopmentList.Any((d) => d.BuildingType == item.Building.BuildingType);
                    item.IsCurrentActiveProject = false;
                    if (item.IsInQueue)
                    {
                        int num = LocalDevelopmentList.IndexOf(item.Building);
                        item.DevelopmentQueueIndex = num;
                        if (num == 0)
                        {
                            CurrentSelectedProject = item;
                            item.IsCurrentActiveProject = true;
                        }
                        CurrentDevelopmentQueue.Add(item);
                    }
                    Comparer<VillageBuildingProjectVM> comparer = Comparer<VillageBuildingProjectVM>.Create((s1, s2) => s1.DevelopmentQueueIndex.CompareTo(s2.DevelopmentQueueIndex));
                    CurrentDevelopmentQueue.Sort(comparer);
                    item.RefreshProductionText();
                }
            }
        }

        private SettlementProjectVM currentProject;
        private VillageBuildingDailyProjectVM currentDefault;
        private MBBindingList<VillageBuildingProjectVM> availableProjects;
        private MBBindingList<VillageBuildingProjectVM> currentDevelopmentQueue;
        private MBBindingList<VillageBuildingDailyProjectVM> dailyProjectList;

        [DataSourceProperty]
        public SettlementProjectVM CurrentSelectedProject
        {
            get => currentProject;
            set
            {
                currentProject = value;
                OnPropertyChanged("CurrentSelectedProject");
            }
        }

        [DataSourceProperty]
        public VillageBuildingDailyProjectVM CurrentDailyDefault
        {
            get => currentDefault;
            set
            {
                currentDefault = value;
                OnPropertyChanged("CurrentDailyDefault");
            }
        }

        [DataSourceProperty]
        public MBBindingList<VillageBuildingProjectVM> AvailableProjects
        {
            get => availableProjects;
            set
            {
                availableProjects = value;
                OnPropertyChanged("AvailableProjects");
            }
        }

        [DataSourceProperty]
        public MBBindingList<VillageBuildingProjectVM> CurrentDevelopmentQueue
        {
            get => currentDevelopmentQueue;
            set
            {
                currentDevelopmentQueue = value;
                OnPropertyChanged("CurrentDevelopmentQueue");
            }
        }

        [DataSourceProperty]
        public MBBindingList<VillageBuildingDailyProjectVM> DailyDefaultList
        {
            get => dailyProjectList;
            set
            {
                dailyProjectList = value;
                OnPropertyChanged("DailyDefaultList");
            }
        }
    }
}
