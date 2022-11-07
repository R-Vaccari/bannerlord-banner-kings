using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Extensions;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Policies;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using static TaleWorlds.CampaignSystem.Election.KingSelectionKingdomDecision;
using static TaleWorlds.CampaignSystem.Issues.CaravanAmbushIssueBehavior;
using static TaleWorlds.CampaignSystem.Issues.EscortMerchantCaravanIssueBehavior;
using static TaleWorlds.CampaignSystem.Issues.LandLordNeedsManualLaborersIssueBehavior;
using static TaleWorlds.CampaignSystem.Issues.VillageNeedsToolsIssueBehavior;

namespace BannerKings
{
    namespace Patches
    {
        namespace Recruitment
        {
            [HarmonyPatch(typeof(RecruitmentCampaignBehavior))]
            internal class RecruitmentApplyInternalPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch("ApplyInternal", MethodType.Normal)]
                private static void ApplyInternalPostfix(MobileParty side1Party, Settlement settlement, Hero individual,
                    CharacterObject troop, int number, int bitCode, RecruitmentCampaignBehavior.RecruitingDetail detail)
                {
                    if (settlement == null)
                    {
                        return;
                    }

                    if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {
                        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                        data.MilitaryData.DeduceManpower(data, number, troop);
                    }
                }

                [HarmonyPrefix]
                [HarmonyPatch("UpdateVolunteersOfNotablesInSettlement", MethodType.Normal)]
                private static bool UpdateVolunteersPrefix(Settlement settlement)
                {
                    if ((settlement.Town != null && !settlement.Town.InRebelliousState && settlement.Notables != null) || 
                        (settlement.IsVillage && !settlement.Village.Bound.Town.InRebelliousState))
                    {
                        foreach (Hero hero in settlement.Notables)
                        {
                            if (hero.CanHaveRecruits)
                            {
                                bool flag = false;
                                CharacterObject basicVolunteer = Campaign.Current.Models.VolunteerModel.GetBasicVolunteer(hero);
                                for (int i = 0; i < hero.VolunteerTypes.Length; i++)
                                {
                                    if (MBRandom.RandomFloat < Campaign.Current.Models.VolunteerModel.GetDailyVolunteerProductionProbability(hero, i, settlement))
                                    {
                                        CharacterObject characterObject = hero.VolunteerTypes[i];
                                        if (characterObject == null)
                                        {
                                            hero.VolunteerTypes[i] = basicVolunteer;
                                            flag = true;
                                        }
                                        else if (characterObject.UpgradeTargets.Length != 0 && characterObject.Tier <= 3)
                                        {
                                            float num = MathF.Log(hero.Power / (float)characterObject.Tier, 2f) * 0.01f;
                                            if (MBRandom.RandomFloat < num)
                                            {
                                                hero.VolunteerTypes[i] = characterObject.UpgradeTargets[MBRandom.RandomInt(characterObject.UpgradeTargets.Length)];
                                                flag = true;
                                            }
                                        }
                                    }
                                }
                                if (flag)
                                {
                                    CharacterObject[] volunteerTypes = hero.VolunteerTypes;
                                    for (int j = 1; j < volunteerTypes.Length; j++)
                                    {
                                        CharacterObject characterObject2 = volunteerTypes[j];
                                        if (characterObject2 != null)
                                        {
                                            int num2 = 0;
                                            int num3 = j - 1;
                                            CharacterObject characterObject3 = volunteerTypes[num3];
                                            while (num3 >= 0 && (characterObject3 == null || (float)characterObject2.Level + (characterObject2.IsMounted ? 0.5f : 0f) < (float)characterObject3.Level + (characterObject3.IsMounted ? 0.5f : 0f)))
                                            {
                                                if (characterObject3 == null)
                                                {
                                                    num3--;
                                                    num2++;
                                                    if (num3 >= 0)
                                                    {
                                                        characterObject3 = volunteerTypes[num3];
                                                    }
                                                }
                                                else
                                                {
                                                    volunteerTypes[num3 + 1 + num2] = characterObject3;
                                                    num3--;
                                                    num2 = 0;
                                                    if (num3 >= 0)
                                                    {
                                                        characterObject3 = volunteerTypes[num3];
                                                    }
                                                }
                                            }
                                            volunteerTypes[num3 + 1 + num2] = characterObject2;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
            }

            [HarmonyPatch(typeof(DefaultVolunteerModel), "GetDailyVolunteerProductionProbability")]
            internal class GetDailyVolunteerProductionProbabilityPatch
            {
                private static bool Prefix(Hero hero, int index, Settlement settlement, ref float __result)
                {
                    __result = BannerKingsConfig.Instance.VolunteerModel.GetDraftEfficiency(hero, index, settlement).ResultNumber;
                    return false;
                }
            }
        }

        namespace Peerage
        {
            [HarmonyPatch(typeof(KingdomDecision), "DetermineSupporters")]
            internal class DetermineSupportersPatch
            {
                private static bool Prefix(KingdomDecision __instance, ref IEnumerable<Supporter> __result)
                {
                    var list = new List<Supporter>();
                    foreach (Clan clan in __instance.Kingdom.Clans)
                    {
                        var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
                        if (council != null && council.Peerage != null)
                        {
                            if (council.Peerage.CanVote)
                            {
                                list.Add(new Supporter(clan));
                            }
                        }
                    }

                    __result = list;
                    return false;
                }
            }
        }

        namespace Perks
        {

            [HarmonyPatch(typeof(MapEventParty), "OnTroopKilled")]
            internal class NameGeneratorPatch
            {
                private static void Postfix(MapEventParty __instance, UniqueTroopDescriptor troopSeed)
                {
                    var leader = __instance.Party.LeaderHero;
                    if (leader == null)
                    {
                        return;
                    }

                    var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                    if (education.HasPerk(BKPerks.Instance.MercenaryRansacker) && 
                        MBRandom.RandomFloat < 0.1f)
                    {
                        var contribution = __instance.ContributionToBattle;
                        AccessTools.Field(__instance.GetType(), "_contributionToBattle").SetValue(__instance, contribution + 1);
                    }
                }
            }
        }

        namespace Fixes
        {
            // Fix crash on wanderer same gender child born
            [HarmonyPatch(typeof(NameGenerator), "GenerateHeroFullName")]
            internal class NameGeneratorPatch
            {
                private static bool Prefix(ref TextObject __result, Hero hero, TextObject heroFirstName,
                    bool useDeterministicValues = true)
                {
                    var parent = hero.IsFemale ? hero.Mother : hero.Father;
                    if (parent == null)
                    {
                        return true;
                    }

                    if (BannerKingsConfig.Instance.TitleManager.IsHeroKnighted(parent) && hero.IsWanderer)
                    {
                        var textObject = heroFirstName;
                        textObject.SetTextVariable("FEMALE", hero.IsFemale ? 1 : 0);
                        textObject.SetTextVariable("IMPERIAL", hero.Culture.StringId == "empire" ? 1 : 0);
                        textObject.SetTextVariable("COASTAL",
                            hero.Culture.StringId is "empire" or "vlandia" ? 1 : 0);
                        textObject.SetTextVariable("NORTHERN",
                            hero.Culture.StringId is "battania" or "sturgia" ? 1 : 0);
                        textObject.SetCharacterProperties("HERO", hero.CharacterObject);
                        textObject.SetTextVariable("FIRSTNAME", heroFirstName);
                        __result = textObject;
                        return false;
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(GauntletGamepadNavigationManager), "OnWidgetNavigationStatusChanged")]
            internal class NavigationPatch
            {
                private static bool Prefix(GauntletGamepadNavigationManager __instance, EventManager source, Widget widget)
                {

                    return false;
                }
            }

            [HarmonyPatch(typeof(TownMarketData))]
            internal class TownItemPricePatch
            {
                [HarmonyPrefix]
                [HarmonyPatch("GetPrice", typeof(EquipmentElement), typeof(MobileParty), typeof(bool),
                    typeof(PartyBase))]
                private static bool Prefix2(TownMarketData __instance, ref int __result,
                    EquipmentElement itemRosterElement, MobileParty tradingParty = null, bool isSelling = false,
                    PartyBase merchantParty = null)
                {
                    if (itemRosterElement.Item == null || itemRosterElement.Item.ItemCategory == null)
                    {
                        __result = 0;
                    }
                    else
                    {
                        Dictionary<ItemCategory, ItemData> dictionary = (Dictionary<ItemCategory, ItemData>)AccessTools
                            .Field(__instance.GetType(), "_itemDict")
                            .GetValue(__instance);
                        ItemData data;
                        try
                        {
                            if (dictionary.ContainsKey(itemRosterElement.Item.ItemCategory))
                            {
                                data = dictionary[itemRosterElement.Item.ItemCategory];
                            }
                            else
                            {
                                data = default(ItemData);
                            }

                        } catch (NullReferenceException e)
                        {
                            data = default(ItemData);
                        }

                        __result = Campaign.Current.Models.TradeItemPriceFactorModel.GetPrice(itemRosterElement,
                            tradingParty, merchantParty, isSelling, data.InStoreValue, data.Supply,
                            data.Demand);
                    }

                    return false;
                }
            }


            [HarmonyPatch(typeof(Hero), nameof(Hero.CanHaveQuestsOrIssues))]
            internal class CanHaveQuestsOrIssuesPatch
            {
                private static bool Prefix(Hero __instance, ref bool __result)
                {
                    if (__instance.Issue != null)
                    {
                        return false;
                    }

                    __result = __instance.IsActive && __instance.IsAlive;
                    CampaignEventDispatcher.Instance.CanHaveQuestsOrIssues(__instance, ref __result);

                    return false;
                }
            }

            [HarmonyPatch(typeof(VillageNeedsToolsIssue), "IssueStayAliveConditions")]
            internal class VillageIssueStayAliveConditionsPatch
            {
                private static bool Prefix(CaravanAmbushIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner != null)
                    {
                        if (__instance.IssueOwner.CurrentSettlement == null ||
                            !__instance.IssueOwner.CurrentSettlement.IsVillage)
                        {
                            __result = false;
                            return false;
                        }
                    }
                    else
                    {
                        __result = false;
                        return false;
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(CaravanAmbushIssue), "IssueStayAliveConditions")]
            internal class CaravanIssueStayAliveConditionsPatch
            {
                private static bool Prefix(CaravanAmbushIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner != null)
                    {
                        if (__instance.IssueOwner.OwnedCaravans == null || __instance.IssueOwner.MapFaction == null)
                        {
                            __result = false;
                            return false;
                        }
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(LandLordNeedsManualLaborersIssue), "IssueStayAliveConditions")]
            internal class LaborersIssueStayAliveConditionsPatch
            {
                private static bool Prefix(LandLordNeedsManualLaborersIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner != null)
                    {
                        if (__instance.IssueOwner.CurrentSettlement == null ||
                            !__instance.IssueOwner.CurrentSettlement.IsVillage)
                        {
                            __result = false;
                            return false;
                        }
                    }
                    else
                    {
                        __result = false;
                        return false;
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(EscortMerchantCaravanIssueBehavior), "ConditionsHold")]
            internal class EscortCaravanConditionsHoldPatch
            {
                private static bool Prefix(Hero issueGiver, ref bool __result)
                {
                    if (issueGiver.CurrentSettlement == null || issueGiver.CurrentSettlement.IsVillage)
                    {
                        __result = false;
                        return false;
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(EscortMerchantCaravanIssue), "IssueStayAliveConditions")]
            internal class EscortCaravanIssueStayAliveConditionsPatch
            {
                private static bool Prefix(EscortMerchantCaravanIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner.CurrentSettlement == null ||
                        __instance.IssueOwner.CurrentSettlement.IsVillage)
                    {
                        __result = false;
                        return false;
                    }

                    return true;
                }
            }


            [HarmonyPatch(typeof(DefaultPartyMoraleModel), "CalculateFoodVarietyMoraleBonus")]
            internal class CalculateFoodVarietyMoraleBonusPatch
            {
                private static bool Prefix(MobileParty party, ref ExplainedNumber result)
                {
                    var num = MBMath.ClampFloat(party.ItemRoster.FoodVariety - 5f, -5f, 5f);
                    if (num != 0f && (num >= 0f || party.LeaderHero == null ||
                                      !party.LeaderHero.GetPerkValue(DefaultPerks.Steward.Spartan)))
                    {
                        if (num > 0f && party.HasPerk(DefaultPerks.Steward.Gourmet))
                        {
                            result.Add(num * DefaultPerks.Steward.Gourmet.PrimaryBonus,
                                DefaultPerks.Steward.Gourmet.Name);
                            return false;
                        }

                        result.Add(num, GameTexts.FindText("str_food_bonus_morale"));
                    }

                    var totalModifiers = 0f;
                    var modifierRate = 0f;
                    foreach (var element in party.ItemRoster.ToList().FindAll(x => x.EquipmentElement.Item.IsFood))
                    {
                        var modifier = element.EquipmentElement.ItemModifier;
                        if (modifier != null)
                        {
                            totalModifiers++;
                            modifierRate += modifier.PriceMultiplier;
                        }
                    }

                    if (modifierRate != 0f)
                    {
                        result.Add(MBMath.ClampFloat(modifierRate / totalModifiers, -5f, 5f),
                            new TextObject("{=oy3mdLFG}Food quality"));
                    }

                    return false;
                }
            }
        }


        namespace Government
        {
            [HarmonyPatch(typeof(KingdomPolicyDecision), "IsAllowed")]
            internal class PolicyIsAllowedPatch
            {
                private static bool Prefix(ref bool __result, KingdomPolicyDecision __instance)
                {
                    if (BannerKingsConfig.Instance.TitleManager != null)
                    {
                        var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                        if (sovereign != null)
                        {
                            __result = !PolicyHelper.GetForbiddenGovernmentPolicies(sovereign.contract.Government)
                                .Contains(__instance.Policy);
                            return false;
                        }
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(KingSelectionKingdomDecision), "ApplyChosenOutcome")]
            internal class ApplyChosenOutcomePatch
            {
                private static void Postfix(KingSelectionKingdomDecision __instance, DecisionOutcome chosenOutcome)
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                    if (title != null)
                    {
                        var deJure = title.deJure;
                        var king = ((KingSelectionDecisionOutcome) chosenOutcome).King;
                        if (deJure != king)
                        {
                            BannerKingsConfig.Instance.TitleManager.InheritTitle(deJure, king, title);
                        }
                    }
                }
            }
        }


        namespace Economy
        {
            [HarmonyPatch(typeof(NotablesCampaignBehavior), "BalanceGoldAndPowerOfNotable")]
            internal class BalanceGoldAndPowerOfNotablePatch
            {
                private static bool Prefix(Hero notable)
                {
                    if (notable.Gold > 10500)
                    {
                        var num = (notable.Gold - 10000) / 500;
                        GiveGoldAction.ApplyBetweenCharacters(notable, null, num * 500, true);
                        notable.AddPower(num);
                        return false;
                    }

                    return true;
                }
            }  

            [HarmonyPatch(typeof(WorkshopsCampaignBehavior), "ProduceOutput")]
            internal class WorkshopProduceOutputPatch
            {
                private static bool Prefix(EquipmentElement outputItem, Town town, Workshop workshop, int count,
                    bool doNotEffectCapital)
                {

                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                    var category = outputItem.Item.ItemCategory;

                    ItemModifierGroup modifierGroup = null;
                    if (outputItem.Item.ArmorComponent != null)
                    {
                        modifierGroup = outputItem.Item.ArmorComponent.ItemModifierGroup;
                    }
                    else if (outputItem.Item.WeaponComponent != null)
                    {
                        modifierGroup = outputItem.Item.WeaponComponent.ItemModifierGroup;
                    }
                    else if (outputItem.Item.IsFood)
                    {
                        modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("consumables");
                    }
                    else if (outputItem.Item.IsAnimal)
                    {
                        modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("animals");
                    }
                    else if (!outputItem.Item.HasHorseComponent && category != DefaultItemCategories.Iron)
                    {
                        modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("goods");
                    }


                    var result = data.EconomicData.ProductionQuality.ResultNumber;
                    if (workshop.Owner != null)
                    {
                        var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(workshop.Owner);
                        if (education.HasPerk(BKPerks.Instance.ArtisanCraftsman))
                        {
                            result += 0.05f;
                        }
                    }

                    if (workshop.WorkshopType.StringId == "artisans")
                    {
                        count = (int)MathF.Max(1f, count + (data.GetTypeCount(PopType.Craftsmen) / 450f));
                        if (outputItem.Item.HasArmorComponent || outputItem.Item.HasWeaponComponent || outputItem.Item.HasSaddleComponent)
                        {
                            count = (int)(count * 2f);
                        }
                    }


                    var itemPrice = town.GetItemPrice(new EquipmentElement(outputItem));
                    int totalValue = 0;
                    for (int i = 0; i < count; i++)
                    {
                        ItemModifier modifier = null;
                        if (modifierGroup != null)
                        {
                            modifier = modifierGroup.GetRandomModifierWithTarget(result, 0.2f);
                            if (modifier != null)
                            {
                                itemPrice = (int)(itemPrice * modifier.PriceMultiplier);
                            }
                        }
                        var element = new EquipmentElement(outputItem.Item, modifier);
                        totalValue += itemPrice;
                        town.Owner.ItemRoster.AddToCounts(element, 1);
                    }

                    if (Campaign.Current.GameStarted && !doNotEffectCapital && workshop.WorkshopType.StringId != "artisans")
                    {
                        var num = totalValue;
                        workshop.ChangeGold(num);
                        town.ChangeGold(-num);
                    }

                    CampaignEventDispatcher.Instance.OnItemProduced(outputItem.Item, town.Owner.Settlement, count);
                    return false;
                }
            }

            [HarmonyPatch(typeof(Workshop), "Expense", MethodType.Getter)]
            internal class WorkshopExpensePatch
            {
                private static bool Prefix(Workshop __instance, ref int __result)
                {
                    __result = (int)BannerKingsConfig.Instance.WorkshopModel.GetDailyExpense(__instance).ResultNumber;
                    return false;
                }
            }

            [HarmonyPatch(typeof(HeroHelper))]
            internal class StartRecruitingMoneyLimitPatch
            {
                [HarmonyPrefix]
                [HarmonyPatch("StartRecruitingMoneyLimit", MethodType.Normal)]
                private static bool Prefix1(Hero hero, ref float __result)
                {
                    __result = 50f;
                    if (hero.PartyBelongedTo != null)
                    {
                        __result += 1000f;
                    }

                    return false;
                }

                [HarmonyPrefix]
                [HarmonyPatch("GetVolunteerTroopsOfHeroForRecruitment", MethodType.Normal)]
                private static bool Prefix2(Hero hero, ref List<CharacterObject> __result)
                {
                    List<CharacterObject> list = new List<CharacterObject>();
                    for (int i = 0; i < BannerKingsSettings.Instance.VolunteersLimit; i++)
                    {
                        list.Add(hero.VolunteerTypes[i]);
                    }
                    __result = list;    
                    return false;
                }
            }

            [HarmonyPatch(typeof(CaravansCampaignBehavior), "GetTradeScoreForTown")]
            internal class GetTradeScoreForTownPatch
            {
                private static void Postfix(ref float __result, MobileParty caravanParty, Town town,
                    CampaignTime lastHomeVisitTimeOfCaravan,
                    float caravanFullness, bool distanceCut)
                {
                    if (BannerKingsConfig.Instance.PopulationManager != null &&
                        BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                    {
                        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                        __result *= data.EconomicData.CaravanAttraction.ResultNumber;
                    }
                }
            }


            [HarmonyPatch(typeof(SellGoodsForTradeAction), "ApplyInternal")]
            internal class SellGoodsPatch
            {
                private static bool Prefix(object[] __args)
                {
                    var settlement = (Settlement) __args[0];
                    var mobileParty = (MobileParty) __args[1];
                    if (settlement != null && mobileParty != null)
                    {
                        if (settlement.IsTown && mobileParty.IsVillager)
                        {
                            var component = settlement.Town;
                            var list = new List<ValueTuple<EquipmentElement, int>>();
                            var num = 10000;
                            ItemObject itemObject = null;
                            for (var i = 0; i < mobileParty.ItemRoster.Count; i++)
                            {
                                var elementCopyAtIndex = mobileParty.ItemRoster.GetElementCopyAtIndex(i);
                                if (elementCopyAtIndex.EquipmentElement.Item.ItemCategory ==
                                    DefaultItemCategories.PackAnimal &&
                                    elementCopyAtIndex.EquipmentElement.Item.Value < num)
                                {
                                    num = elementCopyAtIndex.EquipmentElement.Item.Value;
                                    itemObject = elementCopyAtIndex.EquipmentElement.Item;
                                }
                            }

                            for (var j = mobileParty.ItemRoster.Count - 1; j >= 0; j--)
                            {
                                var elementCopyAtIndex2 = mobileParty.ItemRoster.GetElementCopyAtIndex(j);
                                var itemPrice = component.GetItemPrice(elementCopyAtIndex2.EquipmentElement,
                                    mobileParty, true);
                                var num2 = mobileParty.ItemRoster.GetElementNumber(j);
                                if (elementCopyAtIndex2.EquipmentElement.Item == itemObject)
                                {
                                    var num3 = (int) (0.5f * mobileParty.MemberRoster.TotalManCount) + 2;
                                    num2 -= num3;

                                    if (itemObject.StringId == "mule")
                                    {
                                        num2 -= (int) (mobileParty.MemberRoster.TotalManCount * 0.1f);
                                    }
                                }

                                if (num2 > 0)
                                {
                                    var num4 = MathF.Min(num2, component.Gold / itemPrice);
                                    if (num4 > 0)
                                    {
                                        mobileParty.PartyTradeGold += num4 * itemPrice;
                                        var equipmentElement = elementCopyAtIndex2.EquipmentElement;
                                        component.ChangeGold(-num4 * itemPrice);
                                        settlement.ItemRoster.AddToCounts(equipmentElement, num4);
                                        mobileParty.ItemRoster.AddToCounts(equipmentElement, -num4);
                                        list.Add(new ValueTuple<EquipmentElement, int>(
                                            new EquipmentElement(equipmentElement.Item), num4));
                                    }
                                }
                            }

                            if (!list.IsEmpty())
                            {
                                CampaignEventDispatcher.Instance.OnCaravanTransactionCompleted(mobileParty, component,
                                    list);
                            }

                            return false;
                        }
                    }

                    return true;
                }
            }


            //Mules
            [HarmonyPatch(typeof(VillagerCampaignBehavior), "MoveItemsToVillagerParty")]
            internal class VillagerMoveItemsPatch
            {
                private static bool Prefix(Village village, MobileParty villagerParty)
                {
                    var mule = MBObjectManager.Instance.GetObject<ItemObject>(x => x.StringId == "mule");
                    var muleCount = (int) (villagerParty.MemberRoster.TotalManCount * 0.1f);
                    villagerParty.ItemRoster.AddToCounts(mule, muleCount);
                    var itemRoster = village.Settlement.ItemRoster;
                    var num = villagerParty.InventoryCapacity - villagerParty.ItemRoster.TotalWeight;
                    for (var i = 0; i < 4; i++)
                    {
                        foreach (var itemRosterElement in itemRoster)
                        {
                            var item = itemRosterElement.EquipmentElement.Item;
                            var num2 = MBRandom.RoundRandomized(itemRosterElement.Amount * 0.2f);
                            if (num2 > 0)
                            {
                                if (!item.HasHorseComponent && item.Weight * num2 > num)
                                {
                                    num2 = MathF.Ceiling(num / item.Weight);
                                    if (num2 <= 0)
                                    {
                                        continue;
                                    }
                                }

                                if (!item.HasHorseComponent)
                                {
                                    num -= num2 * item.Weight;
                                }

                                villagerParty.Party.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, num2);
                                itemRoster.AddToCounts(itemRosterElement.EquipmentElement, -num2);
                            }
                        }
                    }

                    return false;
                }
            }


            //Add gold to village and consume some of it, do not reset gold
            [HarmonyPatch(typeof(VillagerCampaignBehavior), "OnSettlementEntered")]
            internal class VillagerSettlementEnterPatch
            {
                private static bool Prefix(
                    ref Dictionary<MobileParty, List<Settlement>> ____previouslyChangedVillagerTargetsDueToEnemyOnWay,
                    MobileParty mobileParty, Settlement settlement, Hero hero)
                {
                    if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {
                        if (mobileParty is {IsActive: true, IsVillager: true})
                        {

                            if (____previouslyChangedVillagerTargetsDueToEnemyOnWay.ContainsKey(mobileParty))
                            {
                                ____previouslyChangedVillagerTargetsDueToEnemyOnWay[mobileParty].Clear();
                            }
                            
                            if (settlement.IsTown)
                            {
                                SellGoodsForTradeAction.ApplyByVillagerTrade(settlement, mobileParty);
                            }

                            if (settlement.IsVillage)
                            {
                                var tax = Campaign.Current.Models.SettlementTaxModel.CalculateVillageTaxFromIncome(
                                    mobileParty.HomeSettlement.Village, mobileParty.PartyTradeGold);
                                float remainder = mobileParty.PartyTradeGold - tax;
                                mobileParty.HomeSettlement.Village.ChangeGold((int) (remainder * 0.5f));
                                mobileParty.PartyTradeGold = 0;
                                mobileParty.HomeSettlement.Village.TradeTaxAccumulated += tax;
                            }

                            if (settlement.IsTown && settlement.Town.Governor != null &&
                                settlement.Town.Governor.GetPerkValue(DefaultPerks.Trade.DistributedGoods))
                            {
                                settlement.Town.TradeTaxAccumulated +=
                                    MathF.Round(DefaultPerks.Trade.DistributedGoods.SecondaryBonus);
                            }
                        }

                        return false;
                    }

                    return true;
                }
            }


            // Impact prosperity
            [HarmonyPatch(typeof(ChangeOwnerOfWorkshopAction), "ApplyInternal")]
            internal class BankruptcyPatch
            {
                private static void Postfix(Workshop workshop, Hero newOwner, WorkshopType workshopType, int capital,
                    bool upgradable, int cost, TextObject customName,
                    ChangeOwnerOfWorkshopAction.ChangeOwnerOfWorkshopDetail detail)
                {
                    var settlement = workshop.Settlement;
                    settlement.Prosperity -= 50f;
                }
            }

            // Added productions
            [HarmonyPatch(typeof(VillageGoodProductionCampaignBehavior), "TickGoodProduction")]
            internal class TickGoodProductionPatch
            {
                private static Dictionary<Village, Dictionary<ItemObject, float>> productionCache = new Dictionary<Village, Dictionary<ItemObject, float>>();

                private static bool Prefix(Village village, bool initialProductionForTowns)
                {
                    if (BannerKingsConfig.Instance.PopulationManager != null)
                    {
                        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);

                        if (data == null)
                        {
                            return true;
                        }

                        var productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(data);
                        foreach (var valueTuple in productions)
                        {
                            var item = valueTuple.Item1;
                            var result = Campaign.Current.Models.VillageProductionCalculatorModel.CalculateDailyProductionAmount(
                                    village, item);
                            var num = MathF.Floor(result);
                            var diff = result - num;
                            num += GetDifferential(village, item, diff);

                            if (num > 0)
                            {
                                if (!initialProductionForTowns)
                                {
                                    village.Owner.ItemRoster.AddToCounts(item, num);
                                    CampaignEventDispatcher.Instance.OnItemProduced(item, village.Owner.Settlement,
                                        num);
                                }
                                else
                                {
                                    village.Bound.ItemRoster.AddToCounts(item, num);
                                }
                            }
                        }

                        return false;
                    }

                    return true;
                }

                private static int GetDifferential(Village village, ItemObject item, float diff)
                {
                    int result = 0;
                    if (productionCache.ContainsKey(village))
                    {
                        var dic = productionCache[village];
                        if (dic.ContainsKey(item))
                        {
                            dic[item] += diff;
                            result = MathF.Floor(dic[item]);
                            if (result >= 1)
                            {
                                dic[item] -= result;
                            }
                        }
                        else
                        {
                            dic.Add(item, diff);
                        }
                    }
                    else
                    {
                        productionCache.Add(village, new Dictionary<ItemObject, float>() { { item, diff } });
                    }

                    return result;
                }
            }


            // Retain behavior of original while updating satisfaction parameters
            [HarmonyPatch(typeof(ItemConsumptionBehavior), "MakeConsumption")]
            internal class ItemConsumptionPatch
            {
                private static bool Prefix(Town town, Dictionary<ItemCategory, float> categoryDemand,
                    Dictionary<ItemCategory, int> saleLog)
                {
                    if (BannerKingsConfig.Instance.PopulationManager != null &&
                        BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                    {
                        saleLog.Clear();
                        var marketData = town.MarketData;
                        var itemRoster = town.Owner.ItemRoster;
                        var popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                        for (var i = itemRoster.Count - 1; i >= 0; i--)
                        {
                            var elementCopyAtIndex = itemRoster.GetElementCopyAtIndex(i);
                            var item = elementCopyAtIndex.EquipmentElement.Item;
                            var amount = elementCopyAtIndex.Amount;
                            var itemCategory = item.GetItemCategory();
                            if (!categoryDemand.ContainsKey(itemCategory))
                            {
                                continue;
                            }

                            var demand = categoryDemand[itemCategory];

                            var behaviors = Campaign.Current.GetCampaignBehaviors<ItemConsumptionBehavior>();
                            var dynMethod = behaviors.First().GetType().GetMethod("CalculateBudget",
                                BindingFlags.NonPublic | BindingFlags.Static);
                            var num = (float) dynMethod.Invoke(null, new object[] {town, demand, itemCategory});
                            if (item.HasArmorComponent || item.HasWeaponComponent)
                            {
                                num = (int)(num * 0.1f);
                            }

                            if (num > 0.01f)
                            {
                                var price = marketData.GetPrice(item);
                                var desiredAmount = num / price;
                                if (desiredAmount > amount)
                                {
                                    desiredAmount = amount;
                                }


                                if (item.IsFood && town.FoodStocks <= town.FoodStocksUpperLimit() * 0.1f)
                                {
                                    var requiredFood = town.FoodChange * -1f;
                                    if (amount > requiredFood)
                                    {
                                        desiredAmount += requiredFood + 1f;
                                    }
                                    else
                                    {
                                        desiredAmount += amount;
                                    }
                                }

                                var finalAmount = MBRandom.RoundRandomized(desiredAmount);
                                var type = Utils.Helpers.GetTradeGoodConsumptionType(item);
                                if (finalAmount > amount)
                                {
                                    finalAmount = amount;
                                    if (type != ConsumptionType.None)
                                    {
                                        popData.EconomicData.UpdateSatisfaction(type, -0.0015f);
                                    }
                                }
                                else if (type != ConsumptionType.None)
                                {
                                    popData.EconomicData.UpdateSatisfaction(type, 0.001f);
                                }

                                itemRoster.AddToCounts(elementCopyAtIndex.EquipmentElement, -finalAmount);
                                categoryDemand[itemCategory] = num - desiredAmount * price;
                                town.ChangeGold(finalAmount * price);
                                var num4 = 0;
                                saleLog.TryGetValue(itemCategory, out num4);
                                saleLog[itemCategory] = num4 + finalAmount;
                            }
                        }

                        if (town.FoodStocks <= town.FoodStocksUpperLimit() * 0.05f && town.Settlement.Stash != null)
                        {
                            var elements = new List<ItemRosterElement>();
                            foreach (var element in town.Settlement.Stash)
                            {
                                if (element.EquipmentElement.Item.CanBeConsumedAsFood())
                                {
                                    elements.Add(element);
                                }
                            }

                            foreach (var element in elements)
                            {
                                var item = element.EquipmentElement.Item;
                                var category = item.ItemCategory;
                                if (saleLog.ContainsKey(category))
                                {
                                    saleLog[category] += element.Amount;
                                }
                                else
                                {
                                    saleLog.Add(category, element.Amount);
                                }

                                town.Settlement.Stash.Remove(element);
                                if (item.HasHorseComponent)
                                {
                                    town.FoodStocks += item.GetItemFoodValue();
                                }
                            }
                        }


                        var list = new List<Town.SellLog>();
                        foreach (var keyValuePair in saleLog)
                        {
                            if (keyValuePair.Value > 0)
                            {
                                list.Add(new Town.SellLog(keyValuePair.Key, keyValuePair.Value));
                            }
                        }

                        town.SetSoldItems(list);
                        return false;
                    }

                    return true;
                }
            }
        }
    }
}