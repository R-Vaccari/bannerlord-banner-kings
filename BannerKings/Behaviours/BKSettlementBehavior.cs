using BannerKings.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using HarmonyLib;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Components;
using BannerKings.Models;

namespace BannerKings.Behaviors
{
    public class BKSettlementBehavior : CampaignBehaviorBase
    {
        private PopulationManager populationManager;
        private PolicyManager policyManager;
        private TitleManager titleManager;
        private CourtManager courtManager;
        private ReligionsManager religionsManager;
        private EducationManager educationsManager = null;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSiegeAftermathAppliedEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement, SiegeAftermathCampaignBehavior.SiegeAftermath, Clan, Dictionary<MobileParty, float>>(OnSiegeAftermath));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameCreated));
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving)
            {
                populationManager = BannerKingsConfig.Instance.PopulationManager;
                policyManager = BannerKingsConfig.Instance.PolicyManager;
                titleManager = BannerKingsConfig.Instance.TitleManager;
                courtManager = BannerKingsConfig.Instance.CourtManager;
                religionsManager = BannerKingsConfig.Instance.ReligionsManager;
                educationsManager = BannerKingsConfig.Instance.EducationManager;
            }

            if (BannerKingsConfig.Instance.wipeData)
            {
                populationManager = null;
                policyManager = null;
                titleManager = null;
                courtManager = null;
                religionsManager = null;
                educationsManager = null;
            }

            dataStore.SyncData("bannerkings-populations", ref populationManager);
            dataStore.SyncData("bannerkings-titles", ref titleManager);
            dataStore.SyncData("bannerkings-courts", ref courtManager);
            dataStore.SyncData("bannerkings-policies", ref policyManager);
            dataStore.SyncData("bannerkings-religions", ref religionsManager);
            dataStore.SyncData("bannerkings-educations", ref educationsManager);

            if (dataStore.IsLoading)
            {
                if (populationManager == null && policyManager == null && titleManager == null && courtManager == null)
                    BannerKingsConfig.Instance.InitManagers();

                else BannerKingsConfig.Instance.InitManagers(populationManager, policyManager,
                    titleManager, courtManager, religionsManager, educationsManager);
            }
        }

        private void TickSettlementData(Settlement settlement)
        {
            UpdateSettlementPops(settlement);
            BannerKingsConfig.Instance.PolicyManager.InitializeSettlement(settlement);
        }

        private void OnSiegeAftermath(MobileParty attackerParty, Settlement settlement, SiegeAftermathCampaignBehavior.SiegeAftermath aftermathType, Clan previousSettlementOwner, Dictionary<MobileParty, float> partyContributions)
        {
            if (aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.ShowMercy || settlement == null || settlement.Town == null ||
                BannerKingsConfig.Instance.PopulationManager == null ||!BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement)) 
                return;

            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float shareToKill;
            if (aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.Pillage)
                shareToKill = MBRandom.RandomFloatRanged(0.1f, 0.16f);
            else shareToKill = MBRandom.RandomFloatRanged(0.16f, 0.24f);
            int killTotal = (int)((float)data.TotalPop * shareToKill);
            int lognum = killTotal;
            List<ValueTuple<PopType, float>> weights = new List<(PopType, float)>();
            foreach (KeyValuePair<PopType, float[]> pair in PopulationManager.GetDesiredPopTypes(settlement))
                weights.Add(new (pair.Key, pair.Value[0]));

            if (killTotal <= 0) return;

            while (killTotal > 0)
            {
                int random = MBRandom.RandomInt(10, 20);
                PopType target = MBRandom.ChooseWeighted(weights);
                int finalNum = MBMath.ClampInt(random, 0, data.GetTypeCount(target));
                data.UpdatePopType(target, -finalNum);
                killTotal -= finalNum;
            }

            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{NUMBER} people have been killed in the siege aftermath of {SETTLEMENT}.")
                .SetTextVariable("NUMBER", lognum)
                .SetTextVariable("SETTLEMENT", settlement.Name)
                .ToString()));
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party != null && party.IsLordParty && target.OwnerClan != null && party.LeaderHero == target.OwnerClan.Leader)
                if ((!target.IsVillage && target.Town.Governor == null) || (target.IsVillage && target.Village.MarketTown.Governor == null))
                    BannerKingsConfig.Instance.AI.SettlementManagement(target);
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement == null || settlement.StringId.Contains("tutorial") || settlement.StringId.Contains("Ruin")) return;

            TickSettlementData(settlement);
            TickRotting(settlement);

            BannerKingsConfig.Instance.AI.SettlementManagement(settlement);

            if (settlement.Town != null)
            {
                Town town = settlement.Town;
                BKWorkshopModel wkModel = (BKWorkshopModel)Campaign.Current.Models.WorkshopModel;
                foreach (Workshop wk in town.Workshops)
                    if (wk.IsRunning && wk.Owner.IsNotable)
                    {
                        int gold = Campaign.Current.Models.ClanFinanceModel.CalculateOwnerIncomeFromWorkshop(wk);
                        gold -= (int)(wkModel.CalculateWorkshopTax(wk.Settlement).ResultNumber * gold);
                        wk.Owner.ChangeHeroGold(gold);
                        wk.ChangeGold(-gold);
                    }

                LandData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).LandData;

                if (data.WorkforceSaturation > 1f)
                {
                    float workers = data.AvailableWorkForce * (data.WorkforceSaturation - 1f);
                    HashSet<ItemObject> items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                        foreach (Village vil in town.Villages)
                        {
                            VillageData vilData = BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement).VillageData;
                            foreach ((ItemObject, float) tuple in BannerKingsConfig.Instance.PopulationManager.GetProductions(vilData))
                                if (tuple.Item1.IsTradeGood && !tuple.Item1.IsFood) items.Add(tuple.Item1);
                        }

                    if (items.Count > 0)
                    {
                        ItemObject random = items.GetRandomElementInefficiently();
                        int itemCount = (int)(workers * 0.01f);
                        BuyOutput(town, random, itemCount, town.GetItemPrice(random));
                    }
                }


                if (town.FoodStocks >= town.FoodStocksUpperLimit() - 10)
                {
                    HashSet<ItemObject> items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                        foreach (Village vil in town.Villages)
                        {
                            VillageData vilData = BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement).VillageData;
                            foreach ((ItemObject, float) tuple in BannerKingsConfig.Instance.PopulationManager.GetProductions(vilData))
                                items.Add(tuple.Item1);
                        }
                    BKFoodModel foodModel = (BKFoodModel)Campaign.Current.Models.SettlementFoodModel;
                    PopulationData popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    float excess = foodModel.GetPopulationFoodProduction(popData, town).ResultNumber - 10
                        - foodModel.GetPopulationFoodConsumption(popData).ResultNumber;
                    //float pasturePorportion = data.Pastureland / data.Acreage;

                    float farmFood = MBMath.ClampFloat(data.Farmland * data.GetAcreOutput("farmland"), 0f, excess);
                    if (town.IsCastle) farmFood *= 0.1f;
                    while (farmFood > 1f)
                        foreach (ItemObject item in items)
                        {
                            if (!item.IsFood) continue;
                            int count = farmFood > 10f ? (int)MBMath.ClampFloat(farmFood * MBRandom.RandomFloat, 0f, farmFood) : (int)farmFood;
                            if (count == 0) break;
                            BuyOutput(town, item, count, town.GetItemPrice(item));
                            farmFood -= count;
                        }
                }
            }


            if (settlement.IsCastle)
            {
                //SetNotables(settlement);
                //UpdateVolunteers(settlement);
                if (settlement.Town != null && settlement.Town.GarrisonParty != null)
                {
                    foreach (Building castleBuilding in settlement.Town.Buildings)
                        if (Utils.Helpers._buildingCastleRetinue != null && castleBuilding.BuildingType == Utils.Helpers._buildingCastleRetinue)
                        {
                            MobileParty garrison = settlement.Town.GarrisonParty;
                            if (garrison.MemberRoster != null && garrison.MemberRoster.Count > 0)
                            {
                                List<TroopRosterElement> elements = garrison.MemberRoster.GetTroopRoster();
                                int currentRetinue = 0;
                                foreach (TroopRosterElement soldierElement in elements)
                                    if (Utils.Helpers.IsRetinueTroop(soldierElement.Character, settlement.Culture))
                                        currentRetinue += soldierElement.Number;

                                int maxRetinue = castleBuilding.CurrentLevel == 1 ? 20 : (castleBuilding.CurrentLevel == 2 ? 40 : 60);
                                if (currentRetinue < maxRetinue)
                                    if (garrison.MemberRoster.Count < garrison.Party.PartySizeLimit)
                                        garrison.MemberRoster.AddToCounts(settlement.Culture.EliteBasicTroop, 1);
                            }
                        }


                    if (settlement.Town.FoodStocks <= (float)settlement.Town.FoodStocksUpperLimit() * 0.05f && 
                        settlement.Town.Settlement.Stash != null)
                        ConsumeStash(settlement);
                }
            } else if (settlement.IsVillage)
            {
                VillageData villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).VillageData;
                if (villageData != null)
                {
                    float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Manor);
                    if (manor > 0)
                    {
                        MBReadOnlyList<MobileParty> retinues = BannerKingsConfig.Instance.PopulationManager.AllParties;
                        MobileParty retinue = null;
                        if (retinues.Count > 0) retinue = retinues.FirstOrDefault(x => x.StringId.Contains(string.Format("bk_retinue_{0}", settlement.Name.ToString())));
                        if (retinue == null) retinue = RetinueComponent.CreateRetinue(settlement);
                        
                        (retinue.PartyComponent as RetinueComponent).DailyTick(manor);
                    } 
                }
            }    
        }

        private void BuyOutput(Town town, ItemObject item, int count, int price)
        {
            int itemFinalPrice = (int)((float)price * (float)count);
            if (town.IsTown)
            {
                town.Owner.ItemRoster.AddToCounts(item, count);
                town.ChangeGold(-itemFinalPrice);
            } else
            {
                town.Settlement.Stash.AddToCounts(item, count);
                town.OwnerClan.Leader.ChangeHeroGold(-itemFinalPrice);
                if (town.OwnerClan.Leader == Hero.MainHero)
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("You have been charged {GOLD} for the excess production of {ITEM}, now in your stash at {CASTLE}.")
                        .SetTextVariable("GOLD", itemFinalPrice)
                        .SetTextVariable("ITEM", item.Name)
                        .SetTextVariable("CASTLE", town.Name)
                        .ToString()));
            }
        }

        private void TickRotting(Settlement settlement)
        {
            PartyBase party = settlement.Party;
            if (party == null) return;

            ItemRoster roster = party.ItemRoster;
            if (roster == null) return;

            float maxStorage = 1000f;
            if (settlement.Town != null)
                foreach (Building b in settlement.Town.Buildings)
                    if (b.BuildingType == DefaultBuildingTypes.CastleGranary || b.BuildingType == DefaultBuildingTypes.SettlementGranary)
                        maxStorage += b.CurrentLevel * 5000f;
            RotRosterFood(roster, maxStorage);
            if (settlement.Stash != null) RotRosterFood(settlement.Stash, settlement.IsCastle ? maxStorage : 1000f);
        }

        private void RotRosterFood(ItemRoster roster, float maxStorage)
        {
            if (roster.TotalFood > maxStorage)
            {
                int toRot = (int)(roster.TotalFood * 0.01f);
                foreach (ItemRosterElement element in roster.ToList().FindAll(x => x.EquipmentElement.Item != null &&
                        x.EquipmentElement.Item.ItemCategory.Properties == ItemCategory.Property.BonusToFoodStores))
                {
                    if (toRot <= 0) break;
                    int result = (int)MathF.Min(MBRandom.RandomFloatRanged(10f, toRot), (float)element.Amount);
                    roster.AddToCounts(element.EquipmentElement, -result);
                    toRot -= result;
                }
            }
        }

        private void ConsumeStash(Settlement settlement)
        {
            List<ItemRosterElement> elements = new List<ItemRosterElement>();
            foreach (ItemRosterElement element in settlement.Stash)
                if (element.EquipmentElement.Item != null && element.EquipmentElement.Item.ItemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
                    elements.Add(element);

            int food = 0;
            foreach (ItemRosterElement element in elements)
            {
                food += element.Amount;
                settlement.Stash.Remove(element);
            }

            if (food > 0) settlement.Town.FoodStocks += food;
        }


        private void OnGameCreated(CampaignGameStarter campaignGameStarter)
        {
            BannerKingsConfig.Instance.InitManagers();
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            if (BannerKingsConfig.Instance.PolicyManager == null || BannerKingsConfig.Instance.TitleManager == null)
                BannerKingsConfig.Instance.InitManagers();

            foreach (Settlement settlement in Settlement.All)
                if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    settlement.Culture = data.CultureData.DominantCulture;
                }

            BuildingType retinueType = MBObjectManager.Instance.GetObjectTypeList<BuildingType>().FirstOrDefault(x => x == Utils.Helpers._buildingCastleRetinue);
            if (retinueType == null)
            {
                Utils.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks", null), new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level).", null), new int[]
                {
                     1000,
                     1500,
                     2000
                }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                }, 0);
            }
        }
    }

    namespace Patches
    {


        [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior), "SpawnNotablesIfNeeded")]
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
                } else if (settlement.IsCastle)
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

        [HarmonyPatch(typeof(CaravansCampaignBehavior), "SpawnCaravan")]
        class SpawnCaravanPatch
        {
            static bool Prefix(Hero hero, bool initialSpawn = false)
            {
                if (hero.CurrentSettlement != null && hero.CurrentSettlement.IsTown)
                    return true;
                
                return false;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForAllPrisoners")]
        class ApplyAllPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement, bool applyGoldChange = true)
            {
                if (currentSettlement != null && (currentSettlement.IsCastle || currentSettlement.IsTown) && BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                        return true;

                    BKCriminalPolicy policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                    if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Enslavement)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                        if (data != null)
                            data.UpdatePopType(PopType.Slaves, Utils.Helpers.GetRosterCount(prisoners));
                    }
                        
                    else if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Forgiveness)
                    {
                        Dictionary<CultureObject, int> dic = new Dictionary<CultureObject, int>();
                        foreach (TroopRosterElement element in prisoners.GetTroopRoster())
                        {
                            if (element.Character.Occupation == Occupation.Bandit) continue;
                            CultureObject culture = element.Character.Culture;
                            if (culture == null || culture.IsBandit) continue;
                            if (dic.ContainsKey(culture))
                                dic[culture] += element.Number;
                            else dic.Add(culture, element.Number);
                        }

                        foreach (KeyValuePair<CultureObject, int> pair in dic)
                        {
                            if (Settlement.All.Any(x => x.Culture == pair.Key))
                            {
                                Settlement random = Settlement.All.FirstOrDefault(x => x.Culture == pair.Key);
                                if (random != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(random))
                                    BannerKingsConfig.Instance.PopulationManager.GetPopData(random)
                                        .UpdatePopType(PopType.Serfs, pair.Value);
                            } 
                        }
                    }
                    else return false;
                }
                    
                return true;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForSelectedPrisoners")]
        class ApplySelectedPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement)
            {
                if (currentSettlement != null && (currentSettlement.IsCastle || currentSettlement.IsTown) & BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                        return true;

                    BKCriminalPolicy policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                    if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Enslavement)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                        if (data != null)
                            data.UpdatePopType(PopType.Slaves, Utils.Helpers.GetRosterCount(prisoners));
                    }
                    else if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Forgiveness)
                    {
                        Dictionary<CultureObject, int> dic = new Dictionary<CultureObject, int>();
                        foreach (TroopRosterElement element in prisoners.GetTroopRoster())
                        {
                            if (element.Character.Occupation == Occupation.Bandit) continue;
                            CultureObject culture = element.Character.Culture;
                            if (culture == null) continue;
                            if (dic.ContainsKey(culture))
                                dic[culture] += element.Number;
                            else dic.Add(culture, element.Number);
                        }

                        foreach (KeyValuePair<CultureObject, int> pair in dic)
                        {
                            if (Settlement.All.Any(x => x.Culture == pair.Key))
                            {
                                Settlement random = Settlement.All.FirstOrDefault(x => x.Culture == pair.Key);
                                if (random != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(random))
                                {
                                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                                    if (data != null)
                                        data.UpdatePopType(PopType.Serfs, pair.Value);
                                }
                            }  
                        }
                    }
                    else return false;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(Town), "FoodStocksUpperLimit")]
        class FoodStockPatch
        {
            static bool Prefix(ref Town __instance, ref int __result)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(__instance.Settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(__instance.Settlement);
                    int total = data.TotalPop;
                    int result = (int)((float)total / 6.5f);

                    __result = (int)((float)(Campaign.Current.Models.SettlementFoodModel.FoodStocksUpperLimit +
                        (__instance.IsCastle ? Campaign.Current.Models.SettlementFoodModel.CastleFoodStockUpperLimitBonus : 0)) +
                        __instance.GetEffectOfBuildings(BuildingEffectEnum.Foodstock) +
                        result);
                    return false;
                }
                else return true;
            }
        }

        [HarmonyPatch(typeof(DefaultBuildingTypes), "InitializeAll")]
        class InitializeBuildingsPatch
        {
            static void Postfix()
            {
                Utils.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks", null), new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level).", null), new int[]
                {
                     800,
                     1200,
                     1500
                }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                }, 0);
            }
        }
    }
}
