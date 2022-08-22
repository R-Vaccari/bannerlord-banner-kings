using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Behaviours
{
    public class BKNotableBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGovernorChangedEvent.AddNonSerializedListener(this, OnGovernorChanged);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnGovernorChanged(Town town, Hero oldGovernor, Hero newGovernor)
        {
            if (oldGovernor == null || !oldGovernor.IsNotable)
            {
                return;
            }

            var owner = town.OwnerClan.Leader;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(owner, oldGovernor, -10);
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement.Town == null || settlement.OwnerClan == null)
            {
                return;
            }

            if (settlement.IsCastle)
            {
                SettlementHelper.SpawnNotablesIfNeeded(settlement);
                UpdateVolunteers(settlement);
            }

            var governor = settlement.Town.Governor;
            if (governor == null || !governor.IsNotable)
            {
                return;
            }

            if (MBRandom.RandomInt(1, 100) < 5)
            {
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.Town.OwnerClan.Leader, governor, 1);
            }
        }

        private void UpdateVolunteers(Settlement settlement)
        {
            if (settlement.Notables.Count == 0 || settlement.Notables[0].IsDead)
            {
                return;
            }

            var hero = settlement.Notables[0];
            if (!hero.CanHaveRecruits)
            {
                return;
            }

            var flag = false;
            var basicVolunteer = Campaign.Current.Models.VolunteerModel.GetBasicVolunteer(hero);
            for (var i = 0; i < 6; i++)
            {
                if (!(MBRandom.RandomFloat <
                      Campaign.Current.Models.VolunteerModel.GetDailyVolunteerProductionProbability(hero, i,
                          settlement)))
                {
                    continue;
                }

                var characterObject = hero.VolunteerTypes[i];
                if (characterObject == null)
                {
                    hero.VolunteerTypes[i] = basicVolunteer;
                    flag = true;
                }
                else if (characterObject.UpgradeTargets.Length != 0 && characterObject.Tier <= 3)
                {
                    var num = MathF.Log(hero.Power / (float)characterObject.Tier, 2f) * 0.01f;
                    if (!(MBRandom.RandomFloat < num))
                    {
                        continue;
                    }

                    hero.VolunteerTypes[i] = characterObject.UpgradeTargets[MBRandom.RandomInt(characterObject.UpgradeTargets.Length)];
                    flag = true;
                }
            }

            if (!flag)
            {
                return;
            }

            var volunteerTypes = hero.VolunteerTypes;
            for (var j = 1; j < 6; j++)
            {
                var characterObject2 = volunteerTypes[j];
                if (characterObject2 == null)
                {
                    continue;
                }

                var num2 = 0;
                var num3 = j - 1;
                var characterObject3 = volunteerTypes[num3];
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

    namespace Patches
    {
        [HarmonyPatch(typeof(SettlementHelper), "SpawnNotablesIfNeeded")]
        internal class SpawnNotablesIfNeededPatch
        {
            private static bool Prefix(Settlement settlement)
            {
                var list = new List<Occupation>();
                if (settlement.IsTown)
                {
                    list = new List<Occupation>
                    {
                        Occupation.GangLeader,
                        Occupation.Artisan,
                        Occupation.Merchant
                    };
                }
                else if (settlement.IsVillage)
                {
                    list = new List<Occupation>
                    {
                        Occupation.RuralNotable,
                        Occupation.Headman
                    };
                }
                else if (settlement.IsCastle)
                {
                    list = new List<Occupation>
                    {
                        Occupation.Headman
                    };
                }

                var randomFloat = MBRandom.RandomFloat;
                var num = 0;
                foreach (var occupation in list)
                {
                    num += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
                        occupation);
                }

                var count = settlement.Notables.Count;
                var num2 = settlement.Notables.Any() ? (num - settlement.Notables.Count) / (float) num : 1f;
                num2 *= MathF.Pow(num2, 0.36f);
                if (randomFloat <= num2 && count < num)
                {
                    var list2 = new List<Occupation>();
                    foreach (var occupation2 in list)
                    {
                        var num3 = 0;
                        using (var enumerator2 = settlement.Notables.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (enumerator2.Current.CharacterObject.Occupation == occupation2)
                                {
                                    num3++;
                                }
                            }
                        }

                        var targetNotableCountForSettlement =
                            Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
                                occupation2);
                        if (num3 < targetNotableCountForSettlement)
                        {
                            list2.Add(occupation2);
                        }
                    }

                    if (list2.Count > 0)
                    {
                        EnterSettlementAction.ApplyForCharacterOnly(
                            HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement(), settlement), settlement);
                    }
                }

                return false;
            }
        }


        // Fix perk crash due to notable not having a Clan.
        [HarmonyPatch(typeof(GovernorCampaignBehavior), "DailyTickSettlement")]
        internal class DailyTickSettlementPatch
        {
            private static bool Prefix(Settlement settlement)
            {
                if ((settlement.IsTown || settlement.IsCastle) && settlement.Town.Governor != null)
                {
                    var governor = settlement.Town.Governor;
                    if (governor.IsNotable || governor.Clan == null)
                    {
                        if (governor.GetPerkValue(DefaultPerks.Charm.MeaningfulFavors) && MBRandom.RandomFloat < 0.02f)
                        {
                            foreach (var hero in settlement.Notables)
                            {
                                if (hero.Power >= 200f)
                                {
                                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.OwnerClan.Leader,
                                        hero, (int) DefaultPerks.Charm.MeaningfulFavors.SecondaryBonus);
                                }
                            }
                        }

                        SkillLevelingManager.OnSettlementGoverned(governor, settlement);
                        return false;
                    }
                }

                return true;
            }
        }

        // Fix perk crash due to notable not having a Clan.
        [HarmonyPatch(typeof(Town), "DailyTick")]
        internal class TownDailyTicktPatch
        {
            private static bool Prefix(Town __instance)
            {
                if (__instance.Governor is {IsNotable: true})
                {
                    __instance.Loyalty += __instance.LoyaltyChange;
                    __instance.Security += __instance.SecurityChange;
                    __instance.FoodStocks += __instance.FoodChange;
                    if (__instance.FoodStocks < 0f)
                    {
                        __instance.FoodStocks = 0f;
                        __instance.Owner.RemainingFoodPercentage = -100;
                    }
                    else
                    {
                        __instance.Owner.RemainingFoodPercentage = 0;
                    }

                    if (__instance.FoodStocks > __instance.FoodStocksUpperLimit())
                    {
                        __instance.FoodStocks = __instance.FoodStocksUpperLimit();
                    }

                    if (!__instance.CurrentBuilding.BuildingType.IsDefaultProject)
                    {
                        __instance.GetType().GetMethod("TickCurrentBuilding",
                                BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(__instance, null);
                    }

                    else if (__instance.Governor != null &&
                             __instance.Governor.GetPerkValue(DefaultPerks.Charm.Virile) && MBRandom.RandomFloat < 0.1f)
                    {
                        var randomElement = __instance.Settlement.Notables.GetRandomElement();
                        if (randomElement != null)
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.OwnerClan.Leader,
                                randomElement, MathF.Round(DefaultPerks.Charm.Virile.SecondaryBonus), false);
                        }
                    }

                    if (__instance.Governor != null)
                    {
                        if (__instance.Governor.GetPerkValue(DefaultPerks.Roguery.WhiteLies) &&
                            MBRandom.RandomFloat < 0.02f)
                        {
                            var randomElement2 = __instance.Settlement.Notables.GetRandomElement();
                            if (randomElement2 != null)
                            {
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.Governor,
                                    randomElement2, MathF.Round(DefaultPerks.Roguery.WhiteLies.SecondaryBonus));
                            }
                        }

                        if (__instance.Governor.GetPerkValue(DefaultPerks.Roguery.Scarface) &&
                            MBRandom.RandomFloat < 0.05f)
                        {
                            var randomElementWithPredicate =
                                __instance.Settlement.Notables.GetRandomElementWithPredicate(x => x.IsGangLeader);
                            if (randomElementWithPredicate != null)
                            {
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.Governor,
                                    randomElementWithPredicate,
                                    MathF.Round(DefaultPerks.Roguery.Scarface.SecondaryBonus));
                            }
                        }
                    }

                    __instance.Owner.Settlement.Prosperity += __instance.ProsperityChange;
                    if (__instance.Owner.Settlement.Prosperity < 0f)
                    {
                        __instance.Owner.Settlement.Prosperity = 0f;
                    }

                    __instance.GetType().GetMethod("HandleMilitiaAndGarrisonOfSettlementDaily",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(__instance, null);
                    __instance.GetType().GetMethod("RepairWallsOfSettlementDaily",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(__instance, null);
                }

                return true;
            }
        }
    }
}