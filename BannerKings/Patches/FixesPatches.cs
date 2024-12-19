using BannerKings.Behaviours;
using BannerKings.Managers.Items;
using BannerKings.Settings;
using HarmonyLib;
using Helpers;
using SandBox;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Armies;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.CampaignSystem.Issues.EscortMerchantCaravanIssueBehavior;

namespace BannerKings.Patches
{
    internal class FixesPatches
    {
        [HarmonyPatch(typeof(KingdomArmyVM))]
        internal class KingdomArmyVMPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("RefreshCanManageArmy")]
            private static bool RefreshCanManageArmyPrefix(KingdomArmyVM __instance)
            {
                TextObject hintText;
                bool mapScreenActionIsEnabledWithReason = CampaignUIHelper.GetMapScreenActionIsEnabledWithReason(out hintText);
                __instance.PlayerHasArmy = (MobileParty.MainParty.Army != null);
                Kingdom kingdom = (Kingdom)__instance.GetType()
                    .GetField("_kingdom", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .GetValue(__instance);

                bool flag = kingdom != null;
                bool flag2 = __instance.PlayerHasArmy && MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty;
                __instance.CanCreateArmy = (mapScreenActionIsEnabledWithReason && flag && !__instance.PlayerHasArmy);
                if (!flag)
                    __instance.CreateArmyHint.HintText = new TextObject("{=XSQ0Y9gy}You need to be a part of a kingdom to create an army.", null);

                if (__instance.PlayerHasArmy && !flag2)
                    __instance.CreateArmyHint.HintText = new TextObject("{=NAA4pajB}You need to leave your current army to create a new one.", null);

                if (!mapScreenActionIsEnabledWithReason)
                    __instance.CreateArmyHint.HintText = hintText;

                __instance.CreateArmyHint.HintText = TextObject.Empty;

                return false;
            }
        }
        

        [HarmonyPatch(typeof(Workshop))]
        internal class WorkshopPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ChangeOwnerOfWorkshop")]
            private static bool ChangeOwnerOfWorkshopPrefix(Workshop __instance, Hero newOwner, WorkshopType type, int capital)
            {
                if (__instance.Owner != null) return true;

                AccessTools.Field(__instance.GetType(), "_owner").SetValue(__instance, newOwner);
                __instance.Owner.AddOwnedWorkshop(__instance);
                AccessTools.Field(__instance.GetType(), "Capital").SetValue(__instance, capital);
                if (type != __instance.WorkshopType)
                    __instance.ChangeWorkshopProduction(type);
                
                return false;
            }
        }
        
        [HarmonyPatch(typeof(BuildingHelper))]
        internal class BuildingHelperPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetProgressOfBuilding")]
            private static bool GetProgressOfBuildingPrefix(Building building, Town town, ref float __result)
            {
                __result = building.BuildingProgress / (float)building.GetConstructionCost();
                return false;
            }
        }

        [HarmonyPatch(typeof(MobilePartyHelper))]
        internal class MobilePartyHelperPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("CanTroopGainXp")]
            private static bool CanTroopGainXpPrefix(PartyBase owner, CharacterObject character, ref bool __result, out int gainableMaxXp)
            {
                gainableMaxXp = 0;
                if (character.UpgradeTargets == null)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GarrisonTroopsCampaignBehavior))]
        internal class GarrisonTroopsCampaignBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetGarrisonLeaveOrTakeDataOfParty")]
            private static bool GetGarrisonLeaveOrTakeDataOfPartyPrefix(MobileParty mobileParty, ref ValueTuple<int, int> __result)
            {
                Settlement currentSettlement = mobileParty.CurrentSettlement;
                int num = TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementGarrisonModel
                    .FindNumberOfTroopsToLeaveToGarrison(mobileParty, currentSettlement);
                int item = 0;
                if (mobileParty.LeaderHero != null && currentSettlement != null)
                {
                    if (num <= 0 && mobileParty.LeaderHero.Clan == currentSettlement.OwnerClan && !mobileParty.IsWageLimitExceeded())
                    {
                        item = TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementGarrisonModel
                            .FindNumberOfTroopsToTakeFromGarrison(mobileParty, mobileParty.CurrentSettlement, 0f);
                    }
                }
                
                __result = new ValueTuple<int, int>(num, item);
                return false;
            }
        }

        [HarmonyPatch(typeof(CompanionsCampaignBehavior))]
        internal class CompanionsCampaignBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("_desiredTotalCompanionCount", MethodType.Getter)]
            private static bool DesiredTotalPrefix(ref float __result)
            {
                __result = Town.AllTowns.Count * BannerKingsSettings.Instance.WorldCompanions;
                return false;
            }
        }

        [HarmonyPatch(typeof(GangLeaderNeedsToOffloadStolenGoodsIssueBehavior))]
        internal class GangLeaderNeedsToOffloadStolenGoodsIssueBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ConditionsHold")]
            private static bool ConditionsHoldPrefix(Hero issueGiver, ref Settlement selectedHideout, ref bool __result)
            {
                if (issueGiver.CurrentSettlement == null || issueGiver.CurrentSettlement.IsVillage)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(MapScreen))]
        internal class MapScreenPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnExitToMainMenu")]
            private static bool OnExitToMainMenu()
            {
                BKManagerBehavior behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKManagerBehavior>();
                behavior.NullManagers();
                return true;
            }
        }

        [HarmonyPatch(typeof(MapScene))]
        internal class MapScenePatches
        {

            [HarmonyPrefix]
            [HarmonyPatch("GetAccessiblePointNearPosition")]
            private static bool GetAccessiblePointNearPosition(MapScene __instance, Vec2 position, float radius, ref Vec2 __result)
            {
                Vec2 vector = MBMapScene.GetAccessiblePointNearPosition(__instance.Scene, position, radius);
                if (!PartyBase.IsPositionOkForTraveling(vector))
                {
                    vector = MBMapScene.GetAccessiblePointNearPosition(__instance.Scene, position, 1f);
                }
                __result = vector;
                return true;
            }
        }

        [HarmonyPatch(typeof(EscortMerchantCaravanIssueBehavior))]
        internal class EscortMerchantCaravanIssueBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ConditionsHold")]
            private static bool ConditionsHoldPrefix(Hero issueGiver, ref bool __result)
            {
                if (issueGiver.CurrentSettlement == null || issueGiver.CurrentSettlement.IsVillage)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(NameGenerator))]
        internal class NameGeneratorPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GenerateHeroFullName")]
            private static bool GenerateHeroFullNamePrefix(ref TextObject __result, Hero hero, TextObject heroFirstName,
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

        [HarmonyPatch(typeof(EscortMerchantCaravanIssueQuest))]
        internal class EscortMerchantCaravanIssueQuestPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ThinkAboutSpawningBanditParty")]
            private static bool ThinkAboutSpawningBanditPartyPrefix()
            {
                Settlement closestHideout = SettlementHelper.FindNearestHideout((Settlement x) => x.IsActive, null);
                Clan clan = Clan.BanditFactions.FirstOrDefault((Clan t) => t.Culture == closestHideout.Culture);

                return clan != null;
            }
        }

        [HarmonyPatch(typeof(ChangeRelationAction))]
        internal class ChangeRelationActionPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ApplyInternal")]
            private static bool ApplyInternalPrefix(Hero originalHero, Hero originalGainedRelationWith, int relationChange, bool showQuickNotification, ChangeRelationAction.ChangeRelationDetail detail)
            {
                if (originalHero == null || originalGainedRelationWith == null)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(FactionHelper))]
        internal class FactionHelperPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("CanPlayerOfferMercenaryService")]
            private static bool CanPlayerOfferMercenaryServicePrefix(Kingdom offerKingdom, out List<IFaction> playerWars, out List<IFaction> warsOfFactionToJoin, ref bool __result)
            {
                playerWars = new List<IFaction>();
                warsOfFactionToJoin = new List<IFaction>();
                float strengthThresholdForNonMutualWarsToBeIgnoredToJoinKingdom = TaleWorlds.CampaignSystem.Campaign.Current.Models.DiplomacyModel.GetStrengthThresholdForNonMutualWarsToBeIgnoredToJoinKingdom(offerKingdom);
                foreach (Kingdom kingdom in Kingdom.All)
                {
                    if (Clan.PlayerClan.MapFaction.IsAtWarWith(kingdom) && kingdom.TotalStrength > strengthThresholdForNonMutualWarsToBeIgnoredToJoinKingdom)
                    {
                        playerWars.Add(kingdom);
                    }
                }
                foreach (Kingdom kingdom2 in Kingdom.All)
                {
                    if (offerKingdom.IsAtWarWith(kingdom2))
                    {
                        warsOfFactionToJoin.Add(kingdom2);
                    }
                }

                if (offerKingdom != null && !offerKingdom.IsEliminated)
                {
                    __result = Clan.PlayerClan.Kingdom == null && !Clan.PlayerClan.IsAtWarWith(offerKingdom) && Clan.PlayerClan.Tier >= TaleWorlds.CampaignSystem.Campaign.Current.Models.ClanTierModel.MercenaryEligibleTier && offerKingdom.Leader.GetRelationWithPlayer()
                   >= (float)TaleWorlds.CampaignSystem.Campaign.Current.Models.DiplomacyModel.MinimumRelationWithConversationCharacterToJoinKingdom
                   && warsOfFactionToJoin.Intersect(playerWars).Count<IFaction>() == playerWars.Count && Clan.PlayerClan.Settlements.IsEmpty<Settlement>();
                }
                else __result = false;

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemRoster))]
        internal class ItemRosterPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetItemNumber")]
            private static bool GetItemNumberPrefix(ItemRoster __instance, ItemObject item, ref int __result)
            {
                int count = 0;
                foreach (ItemRosterElement element in __instance.ToList())
                {
                    if (element.EquipmentElement.Item == item) count += element.Amount;
                }
                __result = count;
                return false;
            }
        }     

        [HarmonyPatch(typeof(MobileParty))]
        internal class MobilePartyPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnRemoveParty")]
            private static bool OnRemovePartyPrefix(MobileParty __instance)
            {
                PartyComponent partyComponent = __instance.PartyComponent;
                if (partyComponent != null && partyComponent.MobileParty == null)
                {
                    AccessTools.Method((partyComponent as PartyComponent).GetType(), "SetMobilePartyInternal")
                        .Invoke(partyComponent, new object[] { __instance });
                }

                return true;
            }

            /*[HarmonyPrefix]
            [HarmonyPatch("FillPartyStacks")]
            private static bool FillPartyStacksPrefix(MobileParty __instance, PartyTemplateObject pt, int troopNumberLimit = -1)
            {
                if (__instance.ActualClan != null && __instance.ActualClan.IsClanTypeMercenary)
                {
                    Console.Write("");
                }

                if (__instance.IsBandit)
                {
                    float playerProgress = Campaign.Current.PlayerProgress;
                    float num = 0.4f + 0.8f * playerProgress;
                    int num2 = MBRandom.RandomInt(2);
                    float num3 = (num2 == 0) ? MBRandom.RandomFloat : (MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * 4f);
                    float num4 = (num2 == 0) ? (num3 * 0.8f + 0.2f) : (1f + num3);
                    float randomFloat = MBRandom.RandomFloat;
                    float randomFloat2 = MBRandom.RandomFloat;
                    float randomFloat3 = MBRandom.RandomFloat;
                    float f = (pt.Stacks.Count > 0) ? ((float)pt.Stacks[0].MinValue + num * num4 * randomFloat * (float)(pt.Stacks[0].MaxValue - pt.Stacks[0].MinValue)) : 0f;
                    float f2 = (pt.Stacks.Count > 1) ? ((float)pt.Stacks[1].MinValue + num * num4 * randomFloat2 * (float)(pt.Stacks[1].MaxValue - pt.Stacks[1].MinValue)) : 0f;
                    float f3 = (pt.Stacks.Count > 2) ? ((float)pt.Stacks[2].MinValue + num * num4 * randomFloat3 * (float)(pt.Stacks[2].MaxValue - pt.Stacks[2].MinValue)) : 0f;
                    __instance.AddElementToMemberRoster(pt.Stacks[0].Character, MBRandom.RoundRandomized(f), false);
                    if (pt.Stacks.Count > 1)
                    {
                        __instance.AddElementToMemberRoster(pt.Stacks[1].Character, MBRandom.RoundRandomized(f2), false);
                    }
                    if (pt.Stacks.Count > 2)
                    {
                        __instance.AddElementToMemberRoster(pt.Stacks[2].Character, MBRandom.RoundRandomized(f3), false);
                        return false;
                    }
                }
                else
                {
                    if (troopNumberLimit < 0)
                    {
                        float playerProgress2 = Campaign.Current.PlayerProgress;
                        for (int i = 0; i < pt.Stacks.Count; i++)
                        {
                            int numberToAdd = (int)(playerProgress2 * (float)(pt.Stacks[i].MaxValue - pt.Stacks[i].MinValue)) + pt.Stacks[i].MinValue;
                            CharacterObject character = pt.Stacks[i].Character;
                            __instance.AddElementToMemberRoster(character, numberToAdd, false);
                        }
                        return false;
                    }
                    for (int j = 0; j < troopNumberLimit; j++)
                    {
                        int num5 = -1;
                        float num6 = 0f;
                        for (int k = 0; k < pt.Stacks.Count; k++)
                        {
                            num6 += ((__instance.IsGarrison && pt.Stacks[k].Character.IsRanged) ? 6f : ((__instance.IsGarrison && !pt.Stacks[k].Character.IsMounted) ? 2f : 1f)) * ((float)(pt.Stacks[k].MaxValue + pt.Stacks[k].MinValue) / 2f);
                        }
                        float num7 = MBRandom.RandomFloat * num6;
                        for (int l = 0; l < pt.Stacks.Count; l++)
                        {
                            num7 -= ((__instance.IsGarrison && pt.Stacks[l].Character.IsRanged) ? 6f : ((__instance.IsGarrison && !pt.Stacks[l].Character.IsMounted) ? 2f : 1f)) * ((float)(pt.Stacks[l].MaxValue + pt.Stacks[l].MinValue) / 2f);
                            if (num7 < 0f)
                            {
                                num5 = l;
                                break;
                            }
                        }
                        if (num5 < 0)
                        {
                            num5 = 0;
                        }
                        CharacterObject character2 = pt.Stacks[num5].Character;
                        __instance.AddElementToMemberRoster(character2, 1, false);
                    }
                    bool isVillager = __instance.IsVillager;
                }

                return false;
            }*/
        }

        [HarmonyPatch(typeof(LordPartyComponent))]
        internal class LordPartyComponentPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("InitializeLordPartyProperties")]
            private static bool InitializeLordPartyProperties(LordPartyComponent __instance, MobileParty mobileParty, Vec2 position, float spawnRadius, Settlement spawnSettlement)
            {
                Hero Owner = __instance.Owner;
                mobileParty.AddElementToMemberRoster(Owner.CharacterObject, 1, insertAtFront: true);
                mobileParty.ActualClan = Owner.Clan;
                int troopNumberLimit = 0;
                if (Owner != Hero.MainHero && Owner.Clan != Clan.PlayerClan)
                {
                    float factor = (Owner.Clan.IsRebelClan ? BannerKingsSettings.Instance.RebelSpawnSize
                        : (Owner.Clan.IsClanTypeMercenary ? BannerKingsSettings.Instance.MercenarySpawnSize
                            : BannerKingsSettings.Instance.NobleSpawnSize));
                    if (BannerKingsSettings.Instance.SpawnSizeWar &&
                        mobileParty.MapFaction.IsKingdomFaction &&
                        FactionManager.GetEnemyKingdoms((Kingdom)mobileParty.MapFaction).Count() > 0)
                        factor *= 0.5f;
                    troopNumberLimit = (int)(mobileParty.Party.PartySizeLimit * factor);
                }

                if (!Campaign.Current.GameStarted)
                {
                    float randomFloat = MBRandom.RandomFloat;
                    float num = MathF.Sqrt(MBRandom.RandomFloat);
                    float num2 = 1f - randomFloat * num;
                    troopNumberLimit = (int)((float)mobileParty.Party.PartySizeLimit * num2);
                }
                mobileParty.InitializeMobilePartyAroundPosition(Owner.Clan.DefaultPartyTemplate, position, spawnRadius, 0f, troopNumberLimit);
                mobileParty.Party.SetVisualAsDirty();
                if (spawnSettlement != null)
                {
                    mobileParty.Ai.SetMoveGoToSettlement(spawnSettlement);
                }
                mobileParty.Aggressiveness = 0.9f + 0.1f * (float)Owner.GetTraitLevel(DefaultTraits.Valor) - 0.05f * (float)Owner.GetTraitLevel(DefaultTraits.Mercy);
                mobileParty.ItemRoster.Add(new ItemRosterElement(DefaultItems.Grain, MBRandom.RandomInt(15, 30)));
                Owner.PassedTimeAtHomeSettlement = (int)(MBRandom.RandomFloat * 100f);
                if (spawnSettlement != null)
                {
                    mobileParty.Ai.SetMoveGoToSettlement(spawnSettlement);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(SiegeAftermathCampaignBehavior))]
        internal class SiegeAftermathCampaignBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetSiegeAftermathInfluenceCost")]
            private static bool GetSiegeAftermathInfluenceCostPrefix(MobileParty attackerParty, Settlement settlement, 
                SiegeAftermathAction.SiegeAftermath aftermathType, ref float __result)
            {
                float result = 0f;
                if (attackerParty.Army != null && aftermathType != SiegeAftermathAction.SiegeAftermath.Pillage)
                {
                    int num = attackerParty.Army.Parties.Count((MobileParty t) =>
                    {
                        if (t.LeaderHero != null)
                        {
                            return t.LeaderHero.GetTraitLevel(DefaultTraits.Mercy) > 0;
                        }

                        return false;
                    });
                    int num2 = attackerParty.Army.Parties.Count((MobileParty t) => 
                    {
                        if (t.LeaderHero != null)
                        {
                            return t.LeaderHero.GetTraitLevel(DefaultTraits.Mercy) > 0;
                        }

                        return false;
                    });
                    if (aftermathType == SiegeAftermathAction.SiegeAftermath.Devastate)
                    {
                        result = settlement.Town.Prosperity / 400f * (float)num;
                    }
                    else if (aftermathType == SiegeAftermathAction.SiegeAftermath.ShowMercy && attackerParty.MapFaction.Culture != settlement.Culture)
                    {
                        result = settlement.Town.Prosperity / 400f * (float)num2;
                    }
                }
                __result = result;

                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryLogic))]
        internal class InventoryLogicPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("SlaughterItem")]
            private static bool SlaughterItemPrefix(ItemRosterElement itemRosterElement)
            {
                EquipmentElement equipmentElement = itemRosterElement.EquipmentElement;
                int meatCount = equipmentElement.Item.HorseComponent.MeatCount;
                if (meatCount == 0)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(DefaultItems))]
        internal class RegisterItemsAndCategories
        {
            [HarmonyPostfix]
            [HarmonyPatch("InitializeAll")]
            private static void InitializeAllPostfix()
            {
                BKItemCategories.Instance.Initialize();
                BKItems.Instance.Initialize();
            }
        }

        [HarmonyPatch(typeof(CraftingCampaignBehavior))]
        internal class CraftingCampaignBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("DailyTickSettlement")]
            private static bool CreateTownOrderPrefix(CraftingCampaignBehavior __instance, Settlement settlement)
            {
                if (settlement.IsTown && __instance.CraftingOrders[settlement.Town].IsThereAvailableSlot())
                {
                    List<Hero> list = new List<Hero>();
                    foreach (Hero hero in settlement.Notables)
                    {
                        if (hero.CurrentSettlement == settlement && hero != Hero.MainHero && MBRandom.RandomFloat <= 0.05f)
                        {
                            int availableSlot = __instance.CraftingOrders[settlement.Town].GetAvailableSlot();
                            if (availableSlot > -1)
                            {
                                __instance.CreateTownOrder(hero, availableSlot);
                            }
                        }
                    }
                    list = null;
                }

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetMaxHeroCraftingStamina")]
            private static bool GetMaxHeroCraftingStaminaPrefix(Hero hero, ref int __result)
            {
                __result = 50 + MathF.Round((float)hero.GetSkillValue(DefaultSkills.Crafting) * 1f);
                return false;
            }
        }

        [HarmonyPatch(typeof(FoodConsumptionBehavior))]
        internal class FoodConsumptionBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("MakeFoodConsumption")]
            private static bool MakeFoodConsumptionPrefix(MobileParty party, ref int partyRemainingFoodPercentage)
            {
                ItemRoster itemRoster = party.ItemRoster;
                int num = 0;
                for (int i = 0; i < itemRoster.Count; i++)
                {
                    if (itemRoster.GetItemAtIndex(i).IsFood)
                    {
                        num++;
                    }
                }
                bool flag = false;
                int count = 0;
                while (num > 0 && partyRemainingFoodPercentage < 0)
                {
                    count++;
                    if (count > 5000)
                        break;
                    int num2 = MBRandom.RandomInt(num);
                    bool flag2 = false;
                    int num3 = 0;
                    for (int i = itemRoster.Count - 1; i >= 0 && !flag2; i--)
                    {
                        if (itemRoster.GetItemAtIndex(i).IsFood)
                        {
                            int elementNumber = itemRoster.GetElementNumber(i);
                            if (elementNumber > 0)
                            {
                                num3++;
                                if (num2 < num3)
                                {
                                    itemRoster.AddToCounts(itemRoster.GetItemAtIndex(i), -1);
                                    partyRemainingFoodPercentage += 100;
                                    if (elementNumber == 1)
                                    {
                                        num--;
                                    }
                                    flag2 = true;
                                    flag = true;
                                }
                            }
                        }
                    }
                    if (flag)
                    {
                        party.Party.OnConsumedFood();
                    }
                }
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SlaughterLivestock")]
            private static bool SlaughterLivestockPrefix(MobileParty party, int partyRemainingFoodPercentage, ref bool __result)
            {
                int num = 0;
                ItemRoster itemRoster = party.ItemRoster;
                foreach (var element in itemRoster)
                {
                    ItemObject itemAtIndex = element.EquipmentElement.Item;
                    HorseComponent horseComponent = itemAtIndex.HorseComponent;
                    if (horseComponent != null && horseComponent.IsLiveStock)
                    {
                        while (num * 100 < -partyRemainingFoodPercentage)
                        {
                            itemRoster.AddToCounts(itemAtIndex, -1);
                            num += itemAtIndex.HorseComponent.MeatCount;
                            if (itemRoster.FindIndexOfItem(itemAtIndex) == -1)
                            {
                                break;
                            }
                        }
                    }
                }

                if (num > 0)
                {
                    itemRoster.AddToCounts(DefaultItems.Meat, num);
                    __result = true;
                }
                else __result = false;

                return false;
            }
        }
    }
}
