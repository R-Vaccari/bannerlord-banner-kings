using BannerKings.Managers.Helpers;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Models;
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
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.Education;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDecision;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

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
                if (mapView == null) mapView = new BannerKingsMapView(id);
                else if (mapView.id != id) mapView = new BannerKingsMapView(id);
                mapView.Refresh();
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
        [HarmonyPatch(typeof(SettlementGovernorSelectionVM))]
        internal class AvailableGovernorsPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Settlement), typeof(Action<Hero>) } )]
            internal static void ConstructorPostfix(SettlementGovernorSelectionVM __instance, Settlement settlement, Action<Hero> onDone)
            {
                if (settlement != null)
                    foreach (Hero notable in settlement.Notables)
                        if (!notable.IsDisabled && !notable.IsDead)
                            __instance.AvailableGovernors.Add(new SettlementGovernorSelectionItemVM(notable, 
                                new Action<SettlementGovernorSelectionItemVM>(delegate (SettlementGovernorSelectionItemVM x) { onDone.Invoke(x.Governor); })));
            }
        }

        [HarmonyPatch(typeof(Hero))]
        internal class HeroNamePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Name", MethodType.Getter)]
            internal static void GetterPostfix(Hero __instance, ref TextObject __result)
            {
                if (__instance.IsNoble && BannerKingsConfig.Instance.TitleManager != null)
                {
                    Kingdom kingdom = __instance.Clan != null ? __instance.Clan.Kingdom : null;
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(__instance);
                    if (title != null)
                    {
                        GovernmentType government = GovernmentType.Feudal;
                        if (title.contract != null) government = title.contract.Government;
                        string honorary = Utils.Helpers.GetTitleHonorary(title.type, government, __instance.IsFemale, kingdom != null ? kingdom.Culture : __instance.Culture);
                        TextObject name = (TextObject)__instance.GetType()
                            .GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic)
                            .GetValue(__instance);
                        __result = new TextObject(name + ", " + 
                            string.Format("{0} of {1}", honorary, title.shortName));
                    }     
                }
            }
        }

        [HarmonyPatch(typeof(SkillVM), MethodType.Constructor)]
        class SkillVMConstructorPatch
        {
            static void Postfix(SkillVM __instance, SkillObject skill, CharacterVM developerVM, Action<PerkVM> onStartPerkSelection)
            {
                ExplainedNumber explainedNumber = BannerKingsConfig.Instance.LearningModel.CalculateLearningLimit(developerVM.Hero, 
                    developerVM.GetCurrentAttributePoint(skill.CharacterAttribute), 
                    __instance.CurrentFocusLevel, 
                    skill.CharacterAttribute.Name, 
                    true);
                __instance.LearningLimitTooltip = new BasicTooltipViewModel(() => CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult(new TextObject("{=YT9giTet}Learning Limit", null).ToString(), explainedNumber.ResultNumber, ref explainedNumber));
            }
        }

        [HarmonyPatch(typeof(SkillIconVisualWidget), "SkillId", MethodType.Setter)]
        class SkillIconOnLateUpdatePatch
        {
            static bool Prefix(SkillIconVisualWidget __instance, string value)
            {
                string text = value;
                if (value == "Lordship") text = "leadership";
                else if (value == "Scholarship") text = "Steward";
                else if (value == "Theology") text = "charm";

                FieldInfo skillId = __instance.GetType().GetField("_skillId", BindingFlags.Instance | BindingFlags.NonPublic);
                if (skillId != null) skillId.SetValue(__instance, text);
                return false;
            }
        } 

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
                List<PolicyObject> list = PolicyHelper.GetForbiddenGovernmentPolicies(title.contract.Government);
                __instance.OtherPolicies.Clear();
                foreach (PolicyObject policy2 in from p in PolicyObject.All
                            where !(bool)active.Invoke(__instance, new object[] { p }) && !list.Contains(p)
                            select p)
                {
                    __instance.OtherPolicies.Add(new KingdomPolicyItemVM(policy2,
                        delegate (KingdomPolicyItemVM x) { select.Invoke(__instance, new object[] { x }); },
                        IsPolicyActive));
                }
            }

            static bool IsPolicyActive(PolicyObject policy)
            {
                MethodInfo active = instance.GetType().GetMethod("IsPolicyActive", BindingFlags.Instance | BindingFlags.NonPublic);
                return (bool)active.Invoke(instance, new object[] { policy });
            }

        }

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
                    int num = __instance.TroopsInCart.Sum(t => t.Cost);
 
                    foreach (RecruitVolunteerTroopVM recruitVolunteerTroopVM in __instance.TroopsInCart)
                    {
                        recruitVolunteerTroopVM.Owner.OwnerHero.VolunteerTypes[recruitVolunteerTroopVM.Index] = null;
                        MobileParty.MainParty.MemberRoster.AddToCounts(recruitVolunteerTroopVM.Character, 1);
                        CampaignEventDispatcher.Instance.OnUnitRecruited(recruitVolunteerTroopVM.Character, 1);
                        data.MilitaryData.DeduceManpower(data, 1, recruitVolunteerTroopVM.Character);
                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, recruitVolunteerTroopVM.Owner.OwnerHero, recruitVolunteerTroopVM.Cost, true);
                    }
                    
                    if (num > 0)
                    {
                        MBTextManager.SetTextVariable("GOLD_AMOUNT", MathF.Abs(num));
                        InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("str_gold_removed_with_icon").ToString(), "event:/ui/notification/coins_negative"));
                    }
                    __instance.Deactivate();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterCreationGainedPropertiesVM))]
        class CharacterCreationGainedPropertiesVMPatches
        {

            [HarmonyPrefix]
            [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(CharacterCreation), typeof(int)})]
            static void Prefix1(CharacterCreationGainedPropertiesVM __instance, CharacterCreation characterCreation, int currentIndex)
            {
                FieldInfo _characterCreation = __instance
                    .GetType()
                    .GetField("_characterCreation",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _characterCreation.SetValue(__instance, characterCreation);

                FieldInfo _currentIndex = __instance
                    .GetType()
                    .GetField("_currentIndex",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _currentIndex.SetValue(__instance, currentIndex);

                FieldInfo _affectedAttributesMap = __instance
                    .GetType()
                    .GetField("_affectedAttributesMap",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _affectedAttributesMap.SetValue(__instance, new Dictionary<CharacterAttribute, Tuple<int, int>>());

                FieldInfo _affectedSkillMap = __instance
                    .GetType()
                    .GetField("_affectedSkillMap",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _affectedSkillMap.SetValue(__instance, new Dictionary<SkillObject, Tuple<int, int>>());

                PropertyInfo GainGroups = __instance
                    .GetType()
                    .GetProperty("GainGroups",
                    BindingFlags.Instance | BindingFlags.Public);
                GainGroups.SetValue(__instance, new MBBindingList<CharacterCreationGainGroupItemVM>());


                foreach (CharacterAttribute attributeObj in BKAttributes.AllAttributes)
                {
                    __instance.GainGroups.Add(new CharacterCreationGainGroupItemVM(attributeObj, characterCreation, currentIndex));
                }
                __instance.UpdateValues();
            }

            [HarmonyPostfix]
            [HarmonyPatch("PopulateInitialValues")]
            static void Postfix1(CharacterCreationGainedPropertiesVM __instance)
            {
                CharacterAttribute characterAttribute = BKAttributes.Instance.Wisdom;
                int attributeValue = Hero.MainHero.GetAttributeValue(characterAttribute);

                Dictionary<CharacterAttribute, Tuple<int, int>> _affectedAttributesMap = (Dictionary<CharacterAttribute, Tuple<int, int>>)__instance
                    .GetType()
                    .GetField("_affectedAttributesMap",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                if (_affectedAttributesMap.ContainsKey(characterAttribute))
                {
                    Tuple<int, int> tuple2 = _affectedAttributesMap[characterAttribute];
                    _affectedAttributesMap[characterAttribute] = new Tuple<int, int>(tuple2.Item1 + attributeValue, 0);
                }
                else  _affectedAttributesMap.Add(characterAttribute, new Tuple<int, int>(attributeValue, 0));
                
            }
        }


        [HarmonyPatch(typeof(EducationGainedPropertiesVM), MethodType.Constructor, new Type[] { typeof(Hero), typeof(int) })]
        class EducationGainedPropertiesVMConstructorPatch
        {
            static void Postfix(EducationGainedPropertiesVM __instance, Hero child, int pageCount)
            {
                __instance.GainGroups.Clear();
                foreach (CharacterAttribute attributeObj in BKAttributes.AllAttributes)
                    __instance.GainGroups.Add(new EducationGainGroupItemVM(attributeObj));

            }
        }

        [HarmonyPatch(typeof(CharacterVM), "InitializeCharacter")]
        class InitializeCharacterPatch
        {
            static bool Prefix(CharacterVM __instance)
            {
                MethodInfo inspectAttr = __instance.GetType().GetMethod("OnInspectAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo addAttr = __instance.GetType().GetMethod("OnAddAttributePoint", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo startSelection = __instance.GetType().GetMethod("OnStartPerkSelection", BindingFlags.Instance | BindingFlags.NonPublic);

                __instance.HeroCharacter = new HeroViewModel(CharacterViewModel.StanceTypes.None);
                __instance.Skills = new MBBindingList<SkillVM>();
                __instance.Traits = new MBBindingList<EncyclopediaTraitItemVM>();
                __instance.Attributes.Clear();
                __instance.HeroCharacter.FillFrom(__instance.Hero, -1, false, false);
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default(EquipmentElement));
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.HorseHarness, default(EquipmentElement));
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default(EquipmentElement));

                foreach (CharacterAttribute characterAttribute in BKAttributes.AllAttributes)
                {
                    CharacterAttributeItemVM item = new CharacterAttributeItemVM(__instance.Hero,
                        characterAttribute,
                        __instance,
                        delegate (CharacterAttributeItemVM x) { inspectAttr.Invoke(__instance, new object[] { x }); },
                        delegate (CharacterAttributeItemVM x) { addAttr.Invoke(__instance, new object[] { x }); });

                    __instance.Attributes.Add(item);

                    foreach (SkillObject skill2 in characterAttribute.Skills)
                    {
                        __instance.Skills.Add(new SkillVM(skill2, __instance,
                            delegate (PerkVM x) { startSelection.Invoke(__instance, new object[] { x }); }));
                    }
                }

                using (List<SkillObject>.Enumerator enumerator3 = Skills.All.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        SkillObject skill = enumerator3.Current;
                        if (__instance.Skills.All((SkillVM s) => s.Skill != skill))
                            __instance.Skills.Add(new SkillVM(skill, __instance, 
                                delegate (PerkVM x) { startSelection.Invoke(__instance, new object[] { x }); }));
                        
                    }
                }
                foreach (SkillVM skillVM in __instance.Skills) skillVM.RefreshWithCurrentValues();
                
                foreach (CharacterAttributeItemVM characterAttributeItemVM in __instance.Attributes) characterAttributeItemVM.RefreshWithCurrentValues();

                __instance.SetCurrentSkill(__instance.Skills[0]);
                __instance.RefreshCharacterValues();
                __instance.CharacterStats = new MBBindingList<StringPairItemVM>();
                if (__instance.Hero.GovernorOf != null)
                {
                    GameTexts.SetVariable("SETTLEMENT_NAME", __instance.Hero.GovernorOf.Name.ToString());
                    __instance.CharacterStats.Add(new StringPairItemVM(GameTexts.FindText("str_governor_of_label", null).ToString(), "", null));
                }
                if (MobileParty.MainParty.GetHeroPerkRole(__instance.Hero) != SkillEffect.PerkRole.None)
                {
                    __instance.CharacterStats.Add(new StringPairItemVM(CampaignUIHelper.GetHeroClanRoleText(__instance.Hero, Clan.PlayerClan), "", null));
                }
                foreach (TraitObject traitObject in CampaignUIHelper.GetHeroTraits())
                {
                    if (__instance.Hero.GetTraitLevel(traitObject) != 0)
                        __instance.Traits.Add(new EncyclopediaTraitItemVM(traitObject, __instance.Hero));
                    
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(SettlementProjectVM), "RefreshValues")]
        class SettlementProjectVMRefreshPatch
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
                    else if (__instance.LocalDevelopmentList.Exists(d => d == selectedItem.Building))
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

                        foreach (VillageBuilding building in villageData.Buildings)
                        {
                            BuildingLocation location = building.BuildingType.BuildingLocation;
                            if (location != BuildingLocation.Daily)
                            {
                                SettlementBuildingProjectVM vm = new SettlementBuildingProjectVM(
                                delegate (SettlementProjectVM x, bool y) { selection.Invoke(__instance, new object[] { x, y }); },
                                delegate (SettlementProjectVM x) { set.Invoke(__instance, new object[] { x }); },
                                delegate {
                                    __instance.CurrentSelectedProject = (__instance.LocalDevelopmentList.Count > 0) ?
                                        __instance.AvailableProjects.First(p => p.Building.BuildingType.StringId == __instance.LocalDevelopmentList[0].BuildingType.StringId) : __instance.CurrentDailyDefault;
                                }, building);

                                __instance.AvailableProjects.Add(vm);
                                if (building.BuildingType.StringId == villageData.CurrentBuilding.BuildingType.StringId)
                                    __instance.CurrentSelectedProject = vm;
                            } else
                            {
                                SettlementDailyProjectVM settlementDailyProjectVM = new SettlementDailyProjectVM(
                                delegate (SettlementProjectVM x, bool y) { selection.Invoke(__instance, new object[] { x, y }); },
                                delegate (SettlementProjectVM x) { set.Invoke(__instance, new object[] { x }); },
                                delegate {   __instance.CurrentSelectedProject = (__instance.LocalDevelopmentList.Count > 0) ?
                                    __instance.AvailableProjects.First(p => p.Building.BuildingType.StringId == __instance.LocalDevelopmentList[0].BuildingType.StringId) : __instance.CurrentDailyDefault; },
                                building);
                                __instance.DailyDefaultList.Add(settlementDailyProjectVM);
                                if (building.BuildingType.StringId == villageData.CurrentDefault.BuildingType.StringId)
                                {
                                    __instance.CurrentDailyDefault = settlementDailyProjectVM;
                                    __instance.CurrentDailyDefault.IsDefault = true;
                                    settlementDailyProjectVM.IsDefault = true;
                                }
                            }
                        }

                        foreach (VillageBuilding item in villageData.BuildingsInProgress)
                            __instance.LocalDevelopmentList.Add(item);

                        if (__instance.LocalDevelopmentList.Count > 0)
                        {
                            Building first = __instance.LocalDevelopmentList[0];
                            selection.Invoke(__instance, new object[] { __instance.AvailableProjects.First(x => x.Building.BuildingType.StringId == first.BuildingType.StringId), true });
                            foreach (VillageBuilding b in villageData.BuildingsInProgress)
                                if (b.BuildingType.StringId != first.BuildingType.StringId)
                                    selection.Invoke(__instance, new object[] { __instance.AvailableProjects.First(x => x.Building.BuildingType.StringId == b.BuildingType.StringId), false });
                        }

                        MethodInfo refreshQueue = __instance.GetType().GetMethod("RefreshDevelopmentsQueueIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                        refreshQueue.Invoke(__instance, null);
                    }
                }
            }
        }
    }
}
