using TaleWorlds.CampaignSystem;
using static TaleWorlds.CampaignSystem.SkillEffect;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Roster;
using System.Linq;
using System;
using System.Xml.Linq;

namespace BannerKings.Behaviours
{
    internal class BKTradeGoodsFixesBehavior : CampaignBehaviorBase
    {
        private static ItemRoster roster = new ItemRoster();

        public override void RegisterEvents()
        {
            CampaignEvents.OnPlayerTradeProfitEvent.AddNonSerializedListener(this, OnProfitMade);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnProfitMade(int profit)
        {
            var mainRoster = MobileParty.MainParty.ItemRoster;
            var settlement = Hero.MainHero.CurrentSettlement;
            if (settlement == null)
            {
                return;
            }

            foreach (var element in roster)
            {
                if (!mainRoster.Contains(element))
                {
                    var baseValue = element.EquipmentElement.GetBaseValue();
                    var value = settlement.IsVillage ? settlement.Village.GetItemPrice(element.EquipmentElement, MobileParty.MainParty, true) : 
                        settlement.Town.GetItemPrice(element.EquipmentElement, MobileParty.MainParty, true);
                    if (value > baseValue)
                    {
                        profit += value - baseValue;
                    }
                }
            }


            if (profit > 0)
            {
                float skillXp = (float)profit * 0.5f;
                var party = MobileParty.MainParty;
                Hero effectiveRoleHolder = party.GetEffectiveRoleHolder(PerkRole.PartyLeader);
                if (effectiveRoleHolder == null)
                {
                    return;
                }
                effectiveRoleHolder.AddSkillXp(DefaultSkills.Trade, skillXp);
            }
        }

        [HarmonyPatch(typeof(PlayerTownVisitCampaignBehavior), "game_menu_town_town_market_on_consequence")]
        internal class MarketPatch
        {
            private static void Postfix(MenuCallbackArgs args)
            {
                roster.Clear();
                foreach (var element in MobileParty.MainParty.ItemRoster)
                {
                    roster.Add(element);
                }
            }
        }


     
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(ItemRoster), "AddToCounts", new Type[] { typeof(ItemObject), typeof(int) })]
        internal class MarketPatch
        {
            private static bool Prefix(ItemRoster __instance, ItemObject item, int number, ref int __result)
            {
                if (number < 0)
                {

                    var unmodifiedElement = new EquipmentElement(item, null);
                    var index = __instance.FindIndexOfElement(unmodifiedElement);
                    if (index >= 0)
                    {
                        var itemRosterElement = __instance.GetElementCopyAtIndex(index);
                        var result = Math.Min(-number, itemRosterElement.Amount);
                        __instance.AddToCounts(unmodifiedElement, -result);
                        number += result;
                    }

                    var list = __instance.ToList().FindAll(x => x.EquipmentElement.Item == item);
                    list.Sort((x, y) =>
                    {
                        float xValue = 1f;
                        float yValue = 1f;
                        var xModifier = x.EquipmentElement.ItemModifier;
                        if (xModifier != null)
                        {
                            xValue = xModifier.PriceMultiplier;
                        }

                        var yModifer = y.EquipmentElement.ItemModifier;
                        if (yModifer != null)
                        {
                            yValue = yModifer.PriceMultiplier;
                        }


                        return xValue.CompareTo(yValue);
                    });

                    foreach (var element in list)
                    {
                        if (number == 0)
                        {
                            break;
                        }

                        var result = Math.Min(-number, element.Amount);
                        __instance.AddToCounts(element.EquipmentElement, -result);
                        number += result;
                    }

                    return false;
                }


                return true;
            }
        }
    }
}
