
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using BannerKings.UI.Windows;
using HarmonyLib;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDecision;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

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

        [HarmonyPatch(typeof(KingdomPoliciesVM), "RefreshPolicyList")]
        class RefreshPolicyListPatch
        {
            private static KingdomPoliciesVM instance;
            static void Postfix(KingdomPoliciesVM __instance)
            {
                if (BannerKingsConfig.Instance.TitleManager == null) return;
                instance = __instance;

                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Hero.MainHero.MapFaction as Kingdom);
                if (title == null || title.contract == null) return;

                MethodInfo active = __instance.GetType().GetMethod("IsPolicyActive", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo select = __instance.GetType().GetMethod("OnPolicySelect", BindingFlags.Instance | BindingFlags.NonPublic);


                if (title.contract == null) return;
                List<PolicyObject> list = PolicyHelper.GetForbiddenGovernmentPolicies(title.contract.government);
                __instance.OtherPolicies.Clear();
                foreach (PolicyObject policy2 in from p in PolicyObject.All
                            where !(bool)active.Invoke(__instance, new object[] { p }) && !list.Contains(p)
                            select p)
                {
                    __instance.OtherPolicies.Add(new KingdomPolicyItemVM(policy2,
                        new Action<KingdomPolicyItemVM>(delegate (KingdomPolicyItemVM x) { select.Invoke(__instance, new object[] { x }); }),
                        new Func<PolicyObject, bool>(IsPolicyActive)));
                }
            }

            static bool IsPolicyActive(PolicyObject policy)
            {
                MethodInfo active = instance.GetType().GetMethod("IsPolicyActive", BindingFlags.Instance | BindingFlags.NonPublic);
                return (bool)active.Invoke(instance, new object[] { policy });
            }

        }

        /*
         * [HarmonyPatch(typeof(SettlementProjectVM))]
        internal class CharacterCreationCultureStagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Building", MethodType.Setter)]
            internal static bool SetterPrefix(SettlementProjectVM __instance, Building value)
            {
                FieldInfo _building = __instance.GetType().GetField("_building", BindingFlags.Instance | BindingFlags.NonPublic);
                _building.SetValue(__instance, value);
                __instance.Name = ((value != null) ? value.Name.ToString() : "");
                __instance.Explanation = ((value != null) ? value.Explanation.ToString() : "");
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

                int constructionCost = __instance.Building.GetConstructionCost();
                TextObject textObject;
                if (constructionCost > 0)
                {
                    textObject = new TextObject("{=tAwRIPiy}Construction Cost: {COST}", null);
                    textObject.SetTextVariable("COST", constructionCost);
                }
                else
                {
                    textObject = TextObject.Empty;
                }
                __instance.ProductionCostText = ((value != null) ? textObject.ToString() : "");
                __instance.CurrentPositiveEffectText = ((value != null) ? value.GetBonusExplanation().ToString() : "");

                __instance.VisualCode = code;

                return false;
            }
        }
         */



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

        

        [HarmonyPatch(typeof(RecruitmentVM), "OnDone")]
        class RecruitmentOnDonePatch
        {
            static bool Prefix(RecruitmentVM __instance)
            {
                Settlement settlement = Settlement.CurrentSettlement;
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    MethodInfo refresh = __instance.GetType().GetMethod("RefreshPartyProperties", BindingFlags.Instance | BindingFlags.NonPublic);
                    refresh.Invoke(__instance, null);
                    int num = __instance.TroopsInCart.Sum((RecruitVolunteerTroopVM t) => t.Cost);
 
                    foreach (RecruitVolunteerTroopVM recruitVolunteerTroopVM in __instance.TroopsInCart)
                    {
                        recruitVolunteerTroopVM.Owner.OwnerHero.VolunteerTypes[recruitVolunteerTroopVM.Index] = null;
                        MobileParty.MainParty.MemberRoster.AddToCounts(recruitVolunteerTroopVM.Character, 1, false, 0, 0, true, -1);
                        CampaignEventDispatcher.Instance.OnUnitRecruited(recruitVolunteerTroopVM.Character, 1);
                        data.MilitaryData.DeduceManpower(data, 1, recruitVolunteerTroopVM.Character);
                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, recruitVolunteerTroopVM.Owner.OwnerHero, recruitVolunteerTroopVM.Cost, true);
                    }
                    
                    if (num > 0)
                    {
                        MBTextManager.SetTextVariable("GOLD_AMOUNT", MathF.Abs(num));
                        InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("str_gold_removed_with_icon", null).ToString(), "event:/ui/notification/coins_negative"));
                    }
                    __instance.Deactivate();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SettlementProjectVM), "RefreshValues")]
        class SettlementProjectVMPRefreshPatch
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

        [HarmonyPatch(typeof(SettlementProjectSelectionVM), "OnCurrentProjectSelection")]
        class OnCurrentProjectSelectionPatch
        {
            static bool Prefix(SettlementProjectSelectionVM __instance, SettlementProjectVM selectedItem, bool isSetAsActiveDevelopment)
            {
                if (!selectedItem.IsDaily)
                {
                    if (isSetAsActiveDevelopment)
                    {
                        __instance.LocalDevelopmentList.Clear();
                        __instance.LocalDevelopmentList.Add(selectedItem.Building);
                    }
                    else if (__instance.LocalDevelopmentList.Exists((Building d) => d == selectedItem.Building))
                    {
                        __instance.LocalDevelopmentList.Remove(selectedItem.Building);
                    }
                    else
                    {
                        __instance.LocalDevelopmentList.Add(selectedItem.Building);
                    }
                }
                else
                {
                    __instance.CurrentDailyDefault.IsDefault = false;
                    __instance.CurrentDailyDefault = (selectedItem as SettlementDailyProjectVM);
                    (selectedItem as SettlementDailyProjectVM).IsDefault = true;
                }
                MethodInfo refresh = __instance.GetType().GetMethod("RefreshDevelopmentsQueueIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                refresh.Invoke(__instance, null);
                if (__instance.LocalDevelopmentList.Count == 0)
                {
                    __instance.CurrentSelectedProject = __instance.CurrentDailyDefault;
                }
                else if (selectedItem != __instance.CurrentSelectedProject)
                {
                    __instance.CurrentSelectedProject = selectedItem;
                }
                FieldInfo fi = __instance.GetType().GetField("_onAnyChangeInQueue", BindingFlags.Instance | BindingFlags.NonPublic);
                Action onAnyChangeInQueue = (Action)fi.GetValue(__instance);
                if (onAnyChangeInQueue != null)
                    onAnyChangeInQueue.Invoke();

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

                        List<VillageBuilding> buildings = new List<VillageBuilding>(villageData.Buildings);
                        List<VillageBuilding> toAdd = new List<VillageBuilding>();
                        foreach (Building building in buildings)
                        {
                            if (building.BuildingType.Explanation == null || building.BuildingType.Name == null)
                            {
                                string id = building.BuildingType.StringId;
                                BuildingType type = DefaultVillageBuildings.Instance.All().FirstOrDefault(x => x.StringId == id);
                                if (type != null)
                                    toAdd.Add(new VillageBuilding(type, villageData.Village.MarketTown, villageData.Village, 
                                        building.BuildingProgress, building.CurrentLevel));
                            }
                        }

                        if (toAdd.Count > 0)
                        {
                            villageData.Buildings.Clear();
                            foreach (VillageBuilding building in toAdd)
                                villageData.Buildings.Add(building);
                        }
                        


                        foreach (VillageBuilding building in villageData.Buildings)
                        {
                            BuildingLocation location = building.BuildingType.BuildingLocation;
                            if (location != BuildingLocation.Daily)
                            {
                                SettlementBuildingProjectVM vm = new SettlementBuildingProjectVM(
                                new Action<SettlementProjectVM, bool>(delegate (SettlementProjectVM x, bool y) { selection.Invoke(__instance, new object[] { x, y }); }),
                                new Action<SettlementProjectVM>(delegate (SettlementProjectVM x) { set.Invoke(__instance, new object[] { x }); }),
                                new Action(delegate {  }),
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

                                if (building.BuildingType.Explanation != null && __instance.CurrentDailyDefault == null ||
                                    __instance.CurrentDailyDefault.Building == null)
                                    __instance.CurrentDailyDefault = settlementDailyProjectVM;
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
