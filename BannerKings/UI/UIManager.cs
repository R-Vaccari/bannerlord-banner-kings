
using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using BannerKings.UI.Windows;
using HarmonyLib;
using SandBox.View.Map;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;

namespace BannerKings.UI
{
    class UIManager
    {
        private static UIManager instance;

        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new UIManager();
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private BannerKingsMapView mapView;

        public void ShowWindow(string id)
        {
            if (MapScreen.Instance != null)
            {
                if (this.mapView == null) this.mapView = new BannerKingsMapView(id);
                else if (this.mapView.id != id) this.mapView = new BannerKingsMapView(id);
                this.mapView.Refresh();
            }
        }

        public void CloseUI()
        {
            if (mapView != null)
            {
                mapView.Close();
                mapView = null;
            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(SettlementProjectVM))]
        internal class CharacterCreationCultureStagePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Building", MethodType.Setter)]
            internal static void SetterPostfix(SettlementProjectVM __instance, Building value)
            {
                string code = ((value != null) ? value.BuildingType.StringId.ToLower() : "");
                if (code == "bannerkings_palisade")
                    code = "building_fortifications";
                else if (code == "bannerkings_trainning")
                    code = "building_settlement_militia_barracks";
                else if (code == "bannerkings_manor")
                    code = "building_castle_castallans_office";
                else if (code == "bannerkings_bakery" || code == "bannerkings_butter" || code == "bannerkings_daily_pasture")
                    code = "building_settlement_granary";
                else if (code == "bannerkings_mining")
                    code = "building_siege_workshop";
                else if (code == "bannerkings_farming" || code == "bannerkings_daily_farm")
                    code = "building_settlement_lime_kilns";
                else if (code == "bannerkings_sawmill" || code == "bannerkings_tannery" || code == "bannerkings_blacksmith")
                    code = "building_castle_workshops";
                else if (code == "bannerkings_daily_woods" || code == "bannerkings_fishing")
                    code = "building_irrigation";
                else if (code == "bannerkings_warehouse")
                    code = "building_settlement_garrison_barracks";
                else if (code == "bannerkings_courier")
                    code = "building_castle_lime_kilns";

                __instance.VisualCode = code;
            }
        }

        [HarmonyPatch(typeof(SettlementProjectVM), "RefreshValues")]
        class SettlementProjectVMPRefreshatch
        {
            static bool Prefix()
            {
                Settlement settlement = Settlement.CurrentSettlement;
                if (!settlement.IsVillage) return true;
                return false;
            }
        }

        [HarmonyPatch(typeof(SettlementBuildingProjectVM), "RefreshProductionText")]
        class BuildingProjectVMRefreshPatch
        {
            static bool Prefix()
            {
                Settlement settlement = Settlement.CurrentSettlement;
                if (!settlement.IsVillage) return true;
                return false;
            }
        }

        [HarmonyPatch(typeof(SettlementProjectSelectionVM), "Refresh")]
        class ProjectSelectionRefreshPatch
        {
            static void Postfix(SettlementProjectSelectionVM __instance)
            {
                Settlement settlement = (Settlement)__instance.GetType().GetField("_settlement", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                if (settlement.IsVillage)
                {
                    VillageData villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).VillageData;
                    if (villageData != null)
                    {

                        MethodInfo selection = __instance.GetType().GetMethod("OnCurrentProjectSelection", BindingFlags.Instance | BindingFlags.NonPublic);
                        MethodInfo set = __instance.GetType().GetMethod("OnCurrentProjectSet", BindingFlags.Instance | BindingFlags.NonPublic);
                        MethodInfo reset = __instance.GetType().GetMethod("OnResetCurrentProject", BindingFlags.Instance | BindingFlags.NonPublic);
                        foreach (VillageBuilding building in villageData.Buildings)
                        {
                            BuildingLocation location = building.BuildingType.BuildingLocation;
                            if (location != BuildingLocation.Daily)
                            {
                                SettlementBuildingProjectVM vm = new SettlementBuildingProjectVM(
                                new Action<SettlementProjectVM, bool>(delegate (SettlementProjectVM x, bool y) { selection.Invoke(__instance, new object[] { x, y }); }),
                                new Action<SettlementProjectVM>(delegate (SettlementProjectVM x) { set.Invoke(__instance, new object[] { x }); }),
                                new Action(delegate { reset.Invoke(__instance, null); }),
                                building
                            );
                                __instance.AvailableProjects.Add(vm);
                                if (building == villageData.CurrentBuilding)
                                    __instance.CurrentSelectedProject = vm;
                            } else
                            {
                                SettlementDailyProjectVM settlementDailyProjectVM = new SettlementDailyProjectVM(
                                new Action<SettlementProjectVM, bool>(delegate (SettlementProjectVM x, bool y) { selection.Invoke(__instance, new object[] { x, y }); }),
                                new Action<SettlementProjectVM>(delegate (SettlementProjectVM x) { set.Invoke(__instance, new object[] { x }); }),
                                new Action(delegate { reset.Invoke(__instance, null); }),
                                building);
                                __instance.DailyDefaultList.Add(settlementDailyProjectVM);
                                if (building == villageData.CurrentDefault)
                                {
                                    __instance.CurrentDailyDefault = settlementDailyProjectVM;
                                    __instance.CurrentDailyDefault.IsDefault = true;
                                    settlementDailyProjectVM.IsDefault = true;
                                }
                            }
                        }

                        foreach (VillageBuilding item in villageData.BuildingsInProgress)
                            __instance.LocalDevelopmentList.Add(item);

                        MethodInfo refreshQueue = __instance.GetType().GetMethod("RefreshDevelopmentsQueueIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                        refreshQueue.Invoke(__instance, null);
                    }
                }
            }
        }
    }
}
