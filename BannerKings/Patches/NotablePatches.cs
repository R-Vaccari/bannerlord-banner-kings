using BannerKings.Utils;
using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using System.Reflection;

namespace BannerKings.Patches
{
    internal class NotablePatches
    {
        [HarmonyPatch(typeof(HeroCreator), "CreateHeroAtOccupation")]
        internal class CreateHeroAtOccupationPatch
        {
            private static bool Prefix(Occupation neededOccupation, ref Hero __result, Settlement forcedHomeSettlement = null)
            {
                Settlement settlement = forcedHomeSettlement ?? SettlementHelper.GetRandomTown(null);
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                IEnumerable<CharacterObject> enumerable;
                if (data != null && data.CultureData != null)
                {
                    enumerable = from x in data.CultureData.GetRandomCulture().NotableAndWandererTemplates
                                   where x.Occupation == neededOccupation
                                   select x;
                }
                else enumerable = from x in settlement.Culture.NotableAndWandererTemplates
                                  where x.Occupation == neededOccupation
                                  select x;

                int num = 0;
                foreach (CharacterObject characterObject in enumerable)
                {
                    int num2 = characterObject.GetTraitLevel(DefaultTraits.Frequency) * 10;
                    num += ((num2 > 0) ? num2 : 100);
                }
                if (!enumerable.Any())
                {
                    __result = null;
                    return false;
                }

                CharacterObject template = null;
                int num3 = settlement.RandomIntWithSeed((uint)settlement.Notables.Count, 1, num);
                foreach (CharacterObject characterObject2 in enumerable)
                {
                    int num4 = characterObject2.GetTraitLevel(DefaultTraits.Frequency) * 10;
                    num3 -= ((num4 > 0) ? num4 : 100);
                    if (num3 < 0)
                    {
                        template = characterObject2;
                        break;
                    }
                }
                Hero hero = HeroCreator.CreateSpecialHero(template, settlement, null, null, -1);
                if (hero.HomeSettlement.IsVillage && hero.HomeSettlement.Village.Bound != null && hero.HomeSettlement.Village.Bound.IsCastle)
                {
                    float value = MBRandom.RandomFloat * 20f;
                    hero.AddPower(value);
                }
                if (neededOccupation != Occupation.Wanderer)
                {
                    hero.ChangeState(Hero.CharacterStates.Active);
                }
                if (neededOccupation != Occupation.Wanderer)
                {
                    EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                }
                if (neededOccupation != Occupation.Wanderer)
                {
                    int amount = 10000;
                    GiveGoldAction.ApplyBetweenCharacters(null, hero, amount, true);
                }
                CharacterObject template2 = hero.Template;
                if (((template2 != null) ? template2.HeroObject : null) != null && hero.Template.HeroObject.Clan != null && hero.Template.HeroObject.Clan.IsMinorFaction)
                {
                    hero.SupporterOf = hero.Template.HeroObject.Clan;
                }
                else
                {
                    hero.SupporterOf = HeroHelper.GetRandomClanForNotable(hero);
                }
                if (neededOccupation != Occupation.Wanderer)
                {
                    typeof(HeroCreator)
                        .GetMethod("AddRandomVarianceToTraits", BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { hero });
                }

                __result = hero;
                return false;
            }
        }


        [HarmonyPatch(typeof(HeroHelper), "GetVolunteerTroopsOfHeroForRecruitment")]
        internal class GetVolunteerTroopsOfHeroForRecruitmentPatch
        {
            private static bool Prefix(Hero hero, ref List<CharacterObject> __result)
            {
                List<CharacterObject> list = new List<CharacterObject>();
                for (int i = 0; i < hero.VolunteerTypes.Length; i++)
                {
                    list.Add(hero.VolunteerTypes[i]);
                }
                __result = list;

                return false;
            }
        }

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
                    num += TaleWorlds.CampaignSystem.Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
                        occupation);
                }

                var count = settlement.Notables.Count;
                var num2 = settlement.Notables.Any() ? (num - settlement.Notables.Count) / (float)num : 1f;
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
                            TaleWorlds.CampaignSystem.Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
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
                                        hero, (int)DefaultPerks.Charm.MeaningfulFavors.SecondaryBonus);
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

        [HarmonyPatch(typeof(Village), "DailyTick")]
        internal class VillageDailyTicktPatch
        {
            private static bool Prefix(Village __instance)
            {
                int hearthLevel = __instance.GetHearthLevel();
                __instance.Hearth += __instance.HearthChange;
                if (hearthLevel != __instance.GetHearthLevel())
                {
                    __instance.Settlement.Party.Visuals.RefreshLevelMask(__instance.Settlement.Party);
                }
                if (__instance.Hearth < 10f)
                {
                    __instance.Hearth = 10f;
                }

                __instance.Owner.Settlement.Militia += __instance.MilitiaChange;
                return false;
            }
        }

        // Fix perk crash due to notable not having a Clan.
        [HarmonyPatch(typeof(Town), "DailyTick")]
        internal class TownDailyTicktPatch
        {
            private static bool Prefix(Town __instance)
            {
                var result = true;
                ExceptionUtils.TryCatch(() =>
                {
                    if (__instance.Governor != null && __instance.Governor is { IsNotable: true } && __instance.OwnerClan != null &&
                        __instance.OwnerClan.Leader != null)
                    {
                        result = false;
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

                        if (!__instance.CurrentBuilding.BuildingType.IsDefaultProject)
                        {
                            __instance.GetType().GetMethod("TickCurrentBuilding",
                                    BindingFlags.Instance | BindingFlags.NonPublic)
                                .Invoke(__instance, null);
                        }

                        else if (__instance.Governor.GetPerkValue(DefaultPerks.Charm.Virile) && MBRandom.RandomFloat < 0.1f)
                        {
                            var randomElement = __instance.Settlement.Notables.GetRandomElement();
                            if (randomElement != null)
                            {
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.OwnerClan.Leader,
                                    randomElement, MathF.Round(DefaultPerks.Charm.Virile.SecondaryBonus), false);
                            }
                        }

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
                },
                "TownDailyTicktPatch",
                false);


                return result;
            }

            private static System.Exception Finalize()
            {
                return null;
            }
        }
    }
}
