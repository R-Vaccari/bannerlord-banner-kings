using HarmonyLib;
using System.Collections.Generic;
using static BannerKings.Managers.PopulationManager;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using BannerKings.Managers.Policies;
using System.Linq;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using BannerKings.Managers.Populations;
using BannerKings.Extensions;

namespace BannerKings.Patches
{
    internal class SettlementPatches
    {
        [HarmonyPatch(typeof(SellPrisonersAction))]
        internal class ApplyAllPrisionersPatch
        {
            private static void SendOffPrisoners(TroopRoster prisoners, Settlement currentSettlement)
            {
                if (currentSettlement == null) return;
                var policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                switch (policy.Policy)
                {
                    case BKCriminalPolicy.CriminalPolicy.Enslavement:
                        {
                            PopulationData data = currentSettlement.PopulationData();
                            data?.UpdatePopType(PopType.Slaves, Utils.Helpers.GetRosterCount(prisoners));
                            break;
                        }
                    case BKCriminalPolicy.CriminalPolicy.Forgiveness:
                        {
                            var dic = new Dictionary<CultureObject, int>();
                            foreach (var element in prisoners.GetTroopRoster())
                            {
                                if (element.Character.Occupation == Occupation.Bandit)
                                {
                                    continue;
                                }

                                var culture = element.Character.Culture;
                                if (culture == null || culture.IsBandit)
                                {
                                    continue;
                                }

                                if (dic.ContainsKey(culture))
                                {
                                    dic[culture] += element.Number;
                                }
                                else
                                {
                                    dic.Add(culture, element.Number);
                                }
                            }

                            foreach (var pair in dic)
                            {
                                if (!Settlement.All.Any(x => x.Culture == pair.Key))
                                {
                                    continue;
                                }
                                else 
                                {
                                    var random = Settlement.All.FirstOrDefault(x => x.Culture == pair.Key);
                                
                                    if (random != null)
                                    {
                                        PopulationData data = random.PopulationData();
                                        if (data != null)
                                            data.UpdatePopType(PopType.Tenants, pair.Value);
                                    }
                                }
                            }

                            break;
                        }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("ApplyForAllPrisoners", MethodType.Normal)]
            private static bool Prefix(PartyBase sellerParty, PartyBase buyerParty)
            {
                SendOffPrisoners(sellerParty.PrisonRoster.CloneRosterData(), sellerParty.Settlement ?? ((buyerParty != null) ? buyerParty.Settlement : null));
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("ApplyForSelectedPrisoners", MethodType.Normal)]
            private static bool Prefix(PartyBase sellerParty, PartyBase buyerParty, TroopRoster prisoners)
            {
                SendOffPrisoners(prisoners, sellerParty.Settlement ?? ((buyerParty != null) ? buyerParty.Settlement : null));
                return true;
            }
        }

        [HarmonyPatch(typeof(Town), "FoodStocksUpperLimit")]
        internal class FoodStockPatch
        {
            private static bool Prefix(ref Town __instance, ref int __result)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(__instance.Settlement);
                if (data == null) return true;

                float total = data.TotalPop;
                float result = total / 3.5f;

                if (__instance.IsCastle) result += TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementFoodModel.CastleFoodStockUpperLimitBonus;
                else result += TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementFoodModel.FoodStocksUpperLimit;

                foreach (var building in __instance.Buildings)
                {
                    if (building.CurrentLevel > 0 && building.BuildingType == DefaultBuildingTypes.CastleGranary || building.BuildingType == DefaultBuildingTypes.SettlementGranary)
                    {
                        result += 1000f * building.CurrentLevel;
                    }
                }

                __result = (int)result;
                return false;

            }
        }
    }
}
