using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            CampaignEvents.OnGovernorChangedEvent.AddNonSerializedListener(this, new Action<Town, Hero, Hero>(OnGovernorChanged));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnGovernorChanged(Town town, Hero oldGovernor, Hero newGovernor)
        {
            if (oldGovernor == null || !oldGovernor.IsNotable) return;

            Hero owner = town.OwnerClan.Leader;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(owner, oldGovernor, -10, true);
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement.Town == null || settlement.OwnerClan == null) return;

            Hero governor = settlement.Town.Governor;
            if (governor == null || !governor.IsNotable) return;

            if (MBRandom.RandomInt(1, 100) < 5)
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.Town.OwnerClan.Leader, governor, 1, true);
        }
    }

    namespace Patches
    {

        [HarmonyPatch(typeof(SettlementHelper), "SpawnNotablesIfNeeded")]
        class SpawnNotablesIfNeededPatch
        {
            static bool Prefix(Settlement settlement)
            {
                List<Occupation> list = new List<Occupation>();
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
                float randomFloat = MBRandom.RandomFloat;
                int num = 0;
                foreach (Occupation occupation in list)
                    num += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation);

                int count = settlement.Notables.Count;
                float num2 = settlement.Notables.Any<Hero>() ? ((float)(num - settlement.Notables.Count) / (float)num) : 1f;
                num2 *= MathF.Pow(num2, 0.36f);
                if (randomFloat <= num2 && count < num)
                {
                    List<Occupation> list2 = new List<Occupation>();
                    foreach (Occupation occupation2 in list)
                    {
                        int num3 = 0;
                        using (List<Hero>.Enumerator enumerator2 = settlement.Notables.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                                if (enumerator2.Current.CharacterObject.Occupation == occupation2)
                                    num3++;
                        }
                        int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation2);
                        if (num3 < targetNotableCountForSettlement)
                            list2.Add(occupation2);

                    }
                    if (list2.Count > 0)
                        EnterSettlementAction.ApplyForCharacterOnly(HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement<Occupation>(), settlement), settlement);

                }

                return false;
            }
        }


        // Fix perk crash due to notable not having a Clan.
        [HarmonyPatch(typeof(GovernorCampaignBehavior), "DailyTickSettlement")]
        class DailyTickSettlementPatch
        {
            static bool Prefix(Settlement settlement)
            {
                if ((settlement.IsTown || settlement.IsCastle) && settlement.Town.Governor != null)
                {
                    Hero governor = settlement.Town.Governor;
                    if (governor.IsNotable || governor.Clan == null)
                    {

                        if (governor.GetPerkValue(DefaultPerks.Charm.MeaningfulFavors) && MBRandom.RandomFloat < 0.02f)
                            foreach (Hero hero in settlement.Notables)
                                if (hero.Power >= 200f)
                                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.OwnerClan.Leader, hero, (int)DefaultPerks.Charm.MeaningfulFavors.SecondaryBonus, true);

                        SkillLevelingManager.OnSettlementGoverned(governor, settlement);
                        return false;
                    }
                }
                
                return true;
            }
        }

        // Fix perk crash due to notable not having a Clan.
        [HarmonyPatch(typeof(Town), "DailyTick")]
        class TownDailyTicktPatch
        {
            static bool Prefix(Town __instance)
            {
                if (__instance.Governor != null && __instance.Governor.IsNotable)
                {
                    __instance.Loyalty += __instance.LoyaltyChange;
                    __instance.Security += __instance.SecurityChange;
                    __instance.FoodStocks += __instance.FoodChange;
                    if (__instance.FoodStocks < 0f)
                    {
                        __instance.FoodStocks = 0f;
                        __instance.Owner.RemainingFoodPercentage = -100;
                    }
                    else __instance.Owner.RemainingFoodPercentage = 0;
                    
                    if (__instance.FoodStocks > (float)__instance.FoodStocksUpperLimit())
                        __instance.FoodStocks = (float)__instance.FoodStocksUpperLimit();
                    
                    if (!__instance.CurrentBuilding.BuildingType.IsDefaultProject)
                        __instance.GetType().GetMethod("TickCurrentBuilding", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(__instance, null);
                    
                    else if (__instance.Governor != null && __instance.Governor.GetPerkValue(DefaultPerks.Charm.Virile) && MBRandom.RandomFloat < 0.1f)
                    {
                        Hero randomElement = __instance.Settlement.Notables.GetRandomElement<Hero>();
                        if (randomElement != null)
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.OwnerClan.Leader, randomElement, MathF.Round(DefaultPerks.Charm.Virile.SecondaryBonus), false);  
                    }
                    if (__instance.Governor != null)
                    {
                        if (__instance.Governor.GetPerkValue(DefaultPerks.Roguery.WhiteLies) && MBRandom.RandomFloat < 0.02f)
                        {
                            Hero randomElement2 = __instance.Settlement.Notables.GetRandomElement<Hero>();
                            if (randomElement2 != null)
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.Governor, randomElement2, MathF.Round(DefaultPerks.Roguery.WhiteLies.SecondaryBonus), true);
                            
                        }
                        if (__instance.Governor.GetPerkValue(DefaultPerks.Roguery.Scarface) && MBRandom.RandomFloat < 0.05f)
                        {
                            Hero randomElementWithPredicate = __instance.Settlement.Notables.GetRandomElementWithPredicate((Hero x) => x.IsGangLeader);
                            if (randomElementWithPredicate != null)
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.Governor, randomElementWithPredicate, MathF.Round(DefaultPerks.Roguery.Scarface.SecondaryBonus), true);
                            
                        }
                    }
                    __instance.Owner.Settlement.Prosperity += __instance.ProsperityChange;
                    if (__instance.Owner.Settlement.Prosperity < 0f)
                    {
                        __instance.Owner.Settlement.Prosperity = 0f;
                    }

                    __instance.GetType().GetMethod("HandleMilitiaAndGarrisonOfSettlementDaily", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(__instance, null);
                    __instance.GetType().GetMethod("RepairWallsOfSettlementDaily", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(__instance, null);
                }

                return true;
            }
        }
    }
}
