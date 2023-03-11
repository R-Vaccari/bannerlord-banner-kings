using BannerKings.Managers.Items;
using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Patches
{
    internal class FixesPatches
    {
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
                        result = settlement.Prosperity / 400f * (float)num;
                    }
                    else if (aftermathType == SiegeAftermathAction.SiegeAftermath.ShowMercy && attackerParty.MapFaction.Culture != settlement.Culture)
                    {
                        result = settlement.Prosperity / 400f * (float)num2;
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
                int hideCount = equipmentElement.Item.HorseComponent.HideCount;

                if (meatCount == 0 || hideCount == 0)
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
            [HarmonyPatch("CreateTownOrder")]
            private static bool CreateTownOrderPrefix(Hero orderOwner, int orderSlot)
            {
                if (orderOwner == null || orderOwner.CurrentSettlement == null || !orderOwner.CurrentSettlement.IsTown)
                {
                    return false;
                }
                return true;
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
        }
    }
}
