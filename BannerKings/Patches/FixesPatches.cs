using BannerKings.Managers.Items;
using HarmonyLib;
using Helpers;
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
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.CampaignSystem.Issues.EscortMerchantCaravanIssueBehavior;

namespace BannerKings.Patches
{
    internal class FixesPatches
    {
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
