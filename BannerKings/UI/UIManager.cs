using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Models.Vanilla;
using BannerKings.Settings;
using BannerKings.UI.Notifications;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.Education;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Recruitment;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Policies;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using MathF = TaleWorlds.Library.MathF;

namespace BannerKings.UI
{
    internal class UIManager
    {
        private static UIManager instance;

        private BannerKingsMapView mapView;
        public BannerKingsScreen BKScreen { get; private set; }

        public void SetScreen(BannerKingsScreen screen) => BKScreen = screen;

        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UIManager();
                }

                return instance;
            }
            set => instance = value;
        }

        public void ShowWindow(string id)
        {
            if (mapView != null)
            {
                mapView.Close();
            }

            mapView = new BannerKingsMapView(id);
            mapView.Refresh();
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
        [HarmonyPatch(typeof(SettlementProjectSelectionVM), "Refresh")]
        internal class AvailableBuildingsPatch
        {
            private static void Postfix(SettlementProjectSelectionVM __instance)
            {
                InnovationData data = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(Settlement.CurrentSettlement.Culture);
                if (data != null)
                {
                    var buildings = data.GetAvailableBuildings(Settlement.CurrentSettlement);

                    var unwanted = new List<SettlementBuildingProjectVM>();
                    foreach (var available in __instance.AvailableProjects)
                    {
                        if (!buildings.Any(type => type.StringId == available.Building.BuildingType.StringId))
                            unwanted.Add(available);

                        if (!data.IsBuildingUpgradeAvailable(available.Building.BuildingType, available.Building.CurrentLevel + 1))
                            unwanted.Add(available);
                    }

                    foreach (var toRemove in unwanted)
                    {
                        __instance.AvailableProjects.Remove(toRemove);
                        __instance.LocalDevelopmentList.Remove(toRemove.Building);
                    }   
                }
            }
        }


        [HarmonyPatch(typeof(MapNotificationVM), "PopulateTypeDictionary")]
        internal class PopulateNotificationsPatch
        {
            private static void Postfix(MapNotificationVM __instance)
            {
                var dic = (Dictionary<Type, Type>) __instance.GetType().GetField("_itemConstructors",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(__instance);
                dic.Add(typeof(DemesneLimitNotification), typeof(DemesneLimitNotificationVM));
                dic.Add(typeof(UnlandedDemesneLimitNotification), typeof(DemesneLimitNotificationVM));
            }
        }

        [HarmonyPatch(typeof(KingdomManagementVM), "SetSelectedCategory")]
        internal class KingdomManagementRefreshPatch
        {
            private static void Postfix(KingdomManagementVM __instance)
            {
                __instance.RefreshValues();
            }
        }


        [HarmonyPatch(typeof(SettlementGovernorSelectionVM))]
        internal class AvailableGovernorsPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(MethodType.Constructor, typeof(Settlement), typeof(Action<Hero>))]
            internal static void ConstructorPostfix(SettlementGovernorSelectionVM __instance, Settlement settlement,
                Action<Hero> onDone)
            {
                if (settlement != null)
                {
                    foreach (var notable in settlement.Notables)
                    {
                        if (!notable.IsDisabled && !notable.IsDead)
                        {
                            __instance.AvailableGovernors.Add(new SettlementGovernorSelectionItemVM(notable,
                                delegate(SettlementGovernorSelectionItemVM x) { onDone.Invoke(x.Governor); }));
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Hero))]
        internal class HeroNamePatch
        {
            private static Dictionary<Hero, TextObject> names = new Dictionary<Hero, TextObject>();

            private static void AddName(Hero hero, TextObject name)
            {
                if (names.ContainsKey(hero))
                {
                    names[hero] = name;
                }
                else
                {
                    names.Add(hero, name); 
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Name", MethodType.Getter)]
            internal static void GetterPostfix(Hero __instance, ref TextObject __result)
            {
                if (names.ContainsKey(__instance) && MBRandom.RandomFloat > 0.1f)
                {
                    __result = names[__instance];
                    return;
                }

                var namingSetting = BannerKingsSettings.Instance.Naming.SelectedValue;
                if (__instance.IsLord && namingSetting != DefaultSettings.Instance.NamingNoTitles &&
                    BannerKingsConfig.Instance.TitleManager != null)
                {
                    var kingdom = __instance.Clan?.Kingdom;
                    var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(__instance);
                    if (title != null)
                    {
                        var honorary = Utils.TextHelper.GetTitleHonorary(title.TitleType, __instance.IsFemale,
                            kingdom != null ? kingdom.Culture : __instance.Culture);
                        var name = (TextObject) __instance.GetType()
                            .GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic)
                            .GetValue(__instance);

                        if (namingSetting.Equals(DefaultSettings.Instance.NamingFullTitlesSuffixed))
                        {
                            __result = new TextObject("{=PyG7eif0}{NAME}, {TITLE} of {SETTLEMENT}")
                                .SetTextVariable("TITLE", honorary)
                                .SetTextVariable("NAME", name)
                                .SetTextVariable("SETTLEMENT", title.shortName);
                            AddName(__instance, __result);
                        }
                        else if (namingSetting.Equals(DefaultSettings.Instance.NamingFullTitles))
                        {
                            __result = new TextObject("{=mMTUt5kV}{TITLE} {NAME} of {SETTLEMENT}")
                                .SetTextVariable("TITLE", honorary)
                                .SetTextVariable("NAME", name)
                                .SetTextVariable("SETTLEMENT", title.shortName);
                            AddName(__instance, __result);
                        }
                        else if (namingSetting.Equals(DefaultSettings.Instance.NamingTitlePrefix))
                        {
                            __result = new TextObject("{=SkfVh2Sp}{TITLE} {NAME}")
                                .SetTextVariable("TITLE", honorary)
                                .SetTextVariable("NAME", name);
                            AddName(__instance, __result);
                        }
                    }
                    else if (__instance.Clan != null && __instance.Clan.Leader != __instance && BannerKingsSettings.Instance.CloseRelativesNaming)
                    {
                        var leader = __instance.Clan.Leader;
                        var leaderTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);
                        if (leaderTitle != null)
                        {
                            var name = (TextObject)__instance.GetType()
                                .GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic)
                                .GetValue(__instance);

                            if (leader == __instance.Spouse)
                            {
                                var honorary = Utils.TextHelper.GetTitleHonorary(leaderTitle.TitleType, __instance.IsFemale,
                                    kingdom != null ? kingdom.Culture : __instance.Culture);

                                __result = new TextObject("{=SkfVh2Sp}{TITLE} {NAME}")
                                    .SetTextVariable("TITLE", honorary)
                                    .SetTextVariable("NAME", name);
                                AddName(__instance, __result);
                            }
                            else if (leaderTitle.Contract.Government != DefaultGovernments.Instance.Republic && leaderTitle.IsSovereignLevel && 
                                (leader.Children.Contains(__instance) || leader.Siblings.Contains(__instance)))
                            { 
                                var honorary = Utils.TextHelper.GetPrinceTitles(__instance.IsFemale,
                                    kingdom != null ? kingdom.Culture : __instance.Culture);

                                __result = new TextObject("{=SkfVh2Sp}{TITLE} {NAME}")
                                    .SetTextVariable("TITLE", honorary)
                                    .SetTextVariable("NAME", name);
                                AddName(__instance, __result);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SkillVM), MethodType.Constructor, typeof(SkillObject), typeof(CharacterVM),
            typeof(Action<PerkVM>))]
        internal class SkillVMConstructorPatch
        {
            private static void Postfix(SkillVM __instance, SkillObject skill, CharacterVM developerVM,
                Action<PerkVM> onStartPerkSelection)
            {
                var explainedNumber = BannerKingsConfig.Instance.LearningModel.CalculateLearningLimit(developerVM.Hero,
                    developerVM.GetCurrentAttributePoint(skill.CharacterAttribute),
                    __instance.CurrentFocusLevel,
                    skill.CharacterAttribute.Name,
                    true);
                __instance.LearningLimitTooltip = new BasicTooltipViewModel(() =>
                    CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult(
                        new TextObject("{=fTKqtNxB}Learning Limit").ToString(), explainedNumber.ResultNumber,
                        ref explainedNumber));
            }
        }

        [HarmonyPatch(typeof(CharacterVM), "RefreshValues")]
        internal class CharacterVMRefreshPatch
        {
            private static bool Prefix(CharacterVM __instance)
            {
                var focus = __instance.GetType()
                    .GetProperty("OrgUnspentFocusPoints", BindingFlags.Instance | BindingFlags.Public);
                var value = __instance.GetCharacterDeveloper().UnspentFocusPoints;
                focus.SetValue(__instance, value);
                __instance.UnspentCharacterPoints = value;
                return true;
            }
        }

        [HarmonyPatch(typeof(RecruitVolunteerTroopVM), "ExecuteBeginHint")]
        internal class RecruitVolunteerTroopVMHintPatch
        {
            private static bool Prefix(RecruitVolunteerTroopVM __instance)
            {
                if (__instance.Character != null && !__instance.PlayerHasEnoughRelation)
                {
                   
                    InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[]
                    {
                        UIHelper.GetRecruitToolTip(__instance.Character, __instance.Owner.OwnerHero, 
                            Hero.MainHero.GetRelation(__instance.Owner.OwnerHero), false)
                    });

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SkillIconVisualWidget), "SkillId", MethodType.Setter)]
        internal class SkillIconOnLateUpdatePatch
        {
            private static bool Prefix(SkillIconVisualWidget __instance, string value)
            {
                var text = value switch
                {
                    "Lordship" => "leadership",
                    "Scholarship" => "Steward",
                    "Theology" => "charm",
                    _ => value
                };

                var skillId = __instance.GetType().GetField("_skillId", BindingFlags.Instance | BindingFlags.NonPublic);
                if (skillId != null)
                {
                    skillId.SetValue(__instance, text);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(KingdomPoliciesVM), "RefreshPolicyList")]
        internal class RefreshPolicyListPatch
        {
            private static KingdomPoliciesVM instance;

            private static void Postfix(KingdomPoliciesVM __instance)
            {
                if (BannerKingsConfig.Instance.TitleManager == null)
                {
                    return;
                }

                instance = __instance;

                var title =
                    BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Hero.MainHero.MapFaction as Kingdom);
                if (title?.Contract == null)
                {
                    return;
                }

                var active = __instance.GetType()
                    .GetMethod("IsPolicyActive", BindingFlags.Instance | BindingFlags.NonPublic);
                var select = __instance.GetType()
                    .GetMethod("OnPolicySelect", BindingFlags.Instance | BindingFlags.NonPublic);


                if (title.Contract == null)
                {
                    return;
                }

                var list = title.Contract.Government.ProhibitedPolicies;
                __instance.OtherPolicies.Clear();
                foreach (var policy2 in from p in PolicyObject.All
                         where !(bool) active.Invoke(__instance, new object[] {p}) && !list.Contains(p)
                         select p)
                {
                    __instance.OtherPolicies.Add(new KingdomPolicyItemVM(policy2,
                        delegate(KingdomPolicyItemVM x) { select.Invoke(__instance, new object[] {x}); },
                        IsPolicyActive));
                }
            }

            private static bool IsPolicyActive(PolicyObject policy)
            {
                var active = instance.GetType()
                    .GetMethod("IsPolicyActive", BindingFlags.Instance | BindingFlags.NonPublic);
                return (bool) active.Invoke(instance, new object[] {policy});
            }
        }

        [HarmonyPatch(typeof(SettlementProjectVM))]
        internal class SettlementProjectVMPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Building", MethodType.Setter)]
            internal static void SetterPostfix(SettlementProjectVM __instance, Building value)
            {
                var code = value != null ? value.BuildingType.StringId.ToLower() : "";
                code = code switch
                {
                    "bannerkings_palisade" => "building_fortifications",
                    "bk_building_daily_assimilation" => "building_settlement_fairgrounds",
                    "bannerkings_trainning" => "building_settlement_militia_barracks",
                    "bannerkings_manor" or "bk_building_courthouse" => "building_castle_castallans_office",
                    "bannerkings_bakery" or "bannerkings_butter" or "bannerkings_daily_pasture" =>
                        "building_settlement_granary",
                    "bannerkings_mining" or "building_town_mines" or "building_castle_mines" or "bannerkings_mines" => "building_siege_workshop",
                    "bannerkings_farming" or "bannerkings_daily_farm" => "building_settlement_lime_kilns",
                    "bannerkings_sawmill" or "bannerkings_tannery" or "bannerkings_blacksmith" or "bannerkings_daily_production" =>
                        "building_castle_workshops",
                    "bannerkings_daily_woods" or "bannerkings_fishing" => "building_irrigation",
                    "bannerkings_warehouse" or "bk_building_armory" or "building_castle_retinue" => "building_settlement_garrison_barracks",
                    "bk_building_theater" => "building_settlement_forum",
                    "bannerkings_marketplace" => "building_settlement_marketplace",
                    "bannerkings_taxoffice" or "bannerkings_courier" => "building_castle_lime_kilns",
                    _ => code
                };

                __instance.VisualCode = code;
            }
        }


        [HarmonyPatch(typeof(RecruitmentVM), "OnDone")]
        internal class RecruitmentOnDonePatch
        {
            private static bool Prefix(RecruitmentVM __instance)
            {
                var settlement = Settlement.CurrentSettlement;
                if (BannerKingsConfig.Instance.PopulationManager != null &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    var refresh = __instance.GetType().GetMethod("RefreshPartyProperties",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    refresh.Invoke(__instance, null);
                    var num = __instance.TroopsInCart.Sum(t => t.Cost);

                    foreach (var recruitVolunteerTroopVM in __instance.TroopsInCart)
                    {
                        recruitVolunteerTroopVM.Owner.OwnerHero.VolunteerTypes[recruitVolunteerTroopVM.Index] = null;
                        MobileParty.MainParty.MemberRoster.AddToCounts(recruitVolunteerTroopVM.Character, 1);
                        CampaignEventDispatcher.Instance.OnUnitRecruited(recruitVolunteerTroopVM.Character, 1);
                        data.MilitaryData.DeduceManpower(data, 1, recruitVolunteerTroopVM.Character,
                            recruitVolunteerTroopVM.Owner.OwnerHero);
                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, recruitVolunteerTroopVM.Owner.OwnerHero,
                            recruitVolunteerTroopVM.Cost, true);
                    }

                    if (num > 0)
                    {
                        MBTextManager.SetTextVariable("GOLD_AMOUNT", MathF.Abs(num));
                        InformationManager.DisplayMessage(new InformationMessage(
                            GameTexts.FindText("str_gold_removed_with_icon").ToString(),
                            "event:/ui/notification/coins_negative"));
                    }

                    __instance.Deactivate();
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterCreationGainedPropertiesVM))]
        internal class CharacterCreationGainedPropertiesVMPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch(MethodType.Constructor, typeof(CharacterCreation), typeof(int))]
            private static void Prefix1(CharacterCreationGainedPropertiesVM __instance,
                CharacterCreation characterCreation, int currentIndex)
            {
                var _characterCreation = __instance
                    .GetType()
                    .GetField("_characterCreation",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                _characterCreation.SetValue(__instance, characterCreation);

                var _currentIndex = __instance
                    .GetType()
                    .GetField("_currentIndex",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                _currentIndex.SetValue(__instance, currentIndex);

                var _affectedAttributesMap = __instance
                    .GetType()
                    .GetField("_affectedAttributesMap",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                _affectedAttributesMap.SetValue(__instance, new Dictionary<CharacterAttribute, Tuple<int, int>>());

                var _affectedSkillMap = __instance
                    .GetType()
                    .GetField("_affectedSkillMap",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                _affectedSkillMap.SetValue(__instance, new Dictionary<SkillObject, Tuple<int, int>>());


                __instance.GainGroups = new MBBindingList<CharacterCreationGainGroupItemVM>();
                __instance.GainedTraits = new MBBindingList<EncyclopediaTraitItemVM>();
                foreach (var attributeObj in BKAttributes.AllAttributes)
                {
                    __instance.GainGroups.Add(
                        new CharacterCreationGainGroupItemVM(attributeObj, characterCreation, currentIndex));
                }

                __instance.UpdateValues();
            }

            [HarmonyPostfix]
            [HarmonyPatch("PopulateInitialValues")]
            private static void Postfix1(CharacterCreationGainedPropertiesVM __instance)
            {
                var characterAttribute = BKAttributes.Instance.Wisdom;
                var attributeValue = Hero.MainHero.GetAttributeValue(characterAttribute);

                var _affectedAttributesMap = (Dictionary<CharacterAttribute, Tuple<int, int>>) __instance
                    .GetType()
                    .GetField("_affectedAttributesMap",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                if (_affectedAttributesMap.ContainsKey(characterAttribute))
                {
                    var tuple2 = _affectedAttributesMap[characterAttribute];
                    _affectedAttributesMap[characterAttribute] = new Tuple<int, int>(tuple2.Item1 + attributeValue, 0);
                }
                else
                {
                    _affectedAttributesMap.Add(characterAttribute, new Tuple<int, int>(attributeValue, 0));
                }
            }
        }


        [HarmonyPatch(typeof(EducationGainedPropertiesVM), MethodType.Constructor, typeof(Hero), typeof(int))]
        internal class EducationGainedPropertiesVMConstructorPatch
        {
            private static void Postfix(EducationGainedPropertiesVM __instance, Hero child, int pageCount)
            {
                __instance.GainGroups.Clear();
                foreach (var attributeObj in BKAttributes.AllAttributes)
                {
                    __instance.GainGroups.Add(new EducationGainGroupItemVM(attributeObj));
                }
            }
        }

        [HarmonyPatch(typeof(CharacterVM), "InitializeCharacter")]
        internal class InitializeCharacterPatch
        {
            private static bool Prefix(CharacterVM __instance)
            {
                var inspectAttr = __instance.GetType()
                    .GetMethod("OnInspectAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
                var addAttr = __instance.GetType()
                    .GetMethod("OnAddAttributePoint", BindingFlags.Instance | BindingFlags.NonPublic);
                var startSelection = __instance.GetType()
                    .GetMethod("OnStartPerkSelection", BindingFlags.Instance | BindingFlags.NonPublic);

                __instance.HeroCharacter = new HeroViewModel();
                __instance.Skills = new MBBindingList<SkillVM>();
                __instance.Traits = new MBBindingList<EncyclopediaTraitItemVM>();
                __instance.Attributes.Clear();
                __instance.HeroCharacter.FillFrom(__instance.Hero);
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default);
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.HorseHarness, default);
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default);

                foreach (var characterAttribute in BKAttributes.AllAttributes)
                {
                    var item = new CharacterAttributeItemVM(__instance.Hero,
                        characterAttribute,
                        __instance,
                        delegate(CharacterAttributeItemVM x) { inspectAttr.Invoke(__instance, new object[] {x}); },
                        delegate(CharacterAttributeItemVM x) { addAttr.Invoke(__instance, new object[] {x}); });

                    __instance.Attributes.Add(item);

                    foreach (var skill2 in characterAttribute.Skills)
                    {
                        __instance.Skills.Add(new SkillVM(skill2, __instance,
                            delegate(PerkVM x) { startSelection.Invoke(__instance, new object[] {x}); }));
                    }
                }

                using (var enumerator3 = Skills.All.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        var skill = enumerator3.Current;
                        if (__instance.Skills.All(s => s.Skill != skill))
                        {
                            __instance.Skills.Add(new SkillVM(skill, __instance,
                                delegate(PerkVM x) { startSelection.Invoke(__instance, new object[] {x}); }));
                        }
                    }
                }

                foreach (var skillVM in __instance.Skills)
                {
                    skillVM.RefreshWithCurrentValues();
                }

                foreach (var characterAttributeItemVM in __instance.Attributes)
                {
                    characterAttributeItemVM.RefreshWithCurrentValues();
                }

                __instance.SetCurrentSkill(__instance.Skills[0]);
                __instance.RefreshCharacterValues();
                __instance.CharacterStats = new MBBindingList<StringPairItemVM>();
                if (__instance.Hero.GovernorOf != null)
                {
                    GameTexts.SetVariable("SETTLEMENT_NAME", __instance.Hero.GovernorOf.Name.ToString());
                    __instance.CharacterStats.Add(
                        new StringPairItemVM(GameTexts.FindText("str_governor_of_label").ToString(), ""));
                }

                if (MobileParty.MainParty.GetHeroPerkRole(__instance.Hero) != SkillEffect.PerkRole.None)
                {
                    __instance.CharacterStats.Add(
                        new StringPairItemVM(CampaignUIHelper.GetHeroClanRoleText(__instance.Hero, Clan.PlayerClan),
                            ""));
                }

                foreach (var traitObject in CampaignUIHelper.GetHeroTraits())
                {
                    if (__instance.Hero.GetTraitLevel(traitObject) != 0)
                    {
                        __instance.Traits.Add(new EncyclopediaTraitItemVM(traitObject, __instance.Hero));
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(SettlementProjectVM), "RefreshValues")]
        internal class SettlementProjectVMRefreshPatch
        {
            private static bool Prefix()
            {
                var settlement = Settlement.CurrentSettlement;
                if (!settlement.IsVillage)
                {
                    return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ArmyManagementVM))]
        internal class ArmyManagementVMPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnRefresh", MethodType.Normal)]
            static void Postfix(ArmyManagementVM __instance)
            {
                __instance.CanCreateArmy = (float)__instance.TotalCost <= Hero.MainHero.Clan.Influence && __instance.PartiesInCart.Count > 1 &&
                    new BKArmyManagementModel().CanCreateArmy(Hero.MainHero);
            }

            [HarmonyPrefix]
            [HarmonyPatch("ExecuteDone", MethodType.Normal)]
            static bool Prefix(ArmyManagementVM __instance)
            {
                bool canCreate = BannerKingsConfig.Instance.ArmyManagementModel.CanCreateArmy(Hero.MainHero);
                if (!canCreate)
                {
                    var rulingClan = Clan.PlayerClan.Kingdom.RulingClan;
                    var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(rulingClan);
                    var marshal = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Marshal);
                    var legate = council.GetCouncilPosition(DefaultCouncilPositions.Instance.LegionCommander1);

                    var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom);
                    TextObject reason = null;
                    if (title != null)
                    {
                        if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion))
                        {
                            reason = new TextObject("{=xE7gruuP}You must be faction leader, {MARSHAL} or a {LEGATE} for the {CLAN} to raise a legion.")
                                .SetTextVariable("MARSHAL", marshal.GetCulturalName())
                                .SetTextVariable("LEGATE", legate.GetCulturalName())
                                .SetTextVariable("CLAN", rulingClan.Name);
                        }
                        else
                        {
                            reason = new TextObject("{=9ap6ssvZ}You must be faction leader, {MARSHAL} for the {CLAN} or have a title superior to Lordship level.")
                                                                           .SetTextVariable("MARSHAL", marshal.Name)
                                                                           .SetTextVariable("CLAN", rulingClan.Name);

                            if (Clan.PlayerClan.Kingdom.HasPolicy(BKPolicies.Instance.LimitedArmyPrivilege))
                            {
                                reason = new TextObject("{=0Yoz051M}You must be faction leader, {MARSHAL} for the {CLAN} or have a title superior to County level.")
                                                        .SetTextVariable("MARSHAL", marshal.Name)
                                                        .SetTextVariable("CLAN", rulingClan.Name);
                            }
                        }
                    }      

                    MBInformationManager.AddQuickInformation(reason, 0);
                }

              
                return canCreate;
            }
        }

    }
}