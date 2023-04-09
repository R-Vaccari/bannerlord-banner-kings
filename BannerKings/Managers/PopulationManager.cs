using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Extensions;
using BannerKings.Managers.Institutions.Guilds;
using BannerKings.Managers.Items;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Populations.Villages;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class PopulationManager
    {
        public enum ConsumptionType
        {
            Luxury,
            Industrial,
            General,
            Food,
            None
        }

        public enum PopType
        {
            Nobles,
            Craftsmen,
            Serfs,
            Slaves,
            None,
            Tenants
        }

        public PopulationManager(Dictionary<Settlement, PopulationData> pops, List<MobileParty> caravans)
        {
            Populations = pops;
            Caravans = caravans;
            Estates = new Dictionary<Hero, List<Estate>>();
        }

        [SaveableProperty(1)] private Dictionary<Settlement, PopulationData> Populations { get; set; }

        [SaveableProperty(2)] private List<MobileParty> Caravans { get; set; }

        [SaveableProperty(3)] private Dictionary<Hero, List<Estate>> Estates { get; set; }

        public MBReadOnlyList<MobileParty> AllParties => new MBReadOnlyList<MobileParty>(Caravans);

        public void PostInitialize()
        {
            if (Estates == null)
            {
                Estates = new Dictionary<Hero, List<Estate>>();
            }

            foreach (var data in Populations.Values)
            {
                data.VillageData?.ReInitializeBuildings();
            }
        }

        public bool IsSettlementPopulated(Settlement settlement)
        {
            if (Populations != null)
            {
                if (settlement.StringId.Contains("Ruin") || settlement.StringId.Contains("tutorial"))
                {
                    return false;
                }

                if (!settlement.IsVillage && !settlement.IsTown && !settlement.IsCastle)
                {
                    return false;
                }

                return Populations.ContainsKey(settlement);
            }

            return false;
        }

        public PopulationData GetPopData(Settlement settlement)
        {
            try
            {
                if (Populations.ContainsKey(settlement))
                {
                    return Populations[settlement];
                }

                if (settlement.StringId.Contains("Ruin") || settlement.StringId.Contains("tutorial"))
                {
                    return null;
                }

                if (!settlement.IsVillage && !settlement.IsTown && !settlement.IsCastle)
                {
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                const string cause = "Exception in Banner Kings GetPopData method. ";
                var objInfo = settlement != null ? $"Name [{settlement.Name}], Id [{settlement.StringId}], Culture [{settlement.Culture}]."
                    : "Null settlement.";

                throw new BannerKingsException(cause + objInfo, ex);
            }
        }

        public void AddSettlementData(Settlement settlement, PopulationData data)
        {
            if (!Populations.ContainsKey(settlement))
            {
                Populations.Add(settlement, data);
            }
        }

        public bool IsPopulationParty(MobileParty party)
        {
            return Caravans.Contains(party);
        }

        public void AddParty(MobileParty party)
        {
            Caravans.Add(party);
        }

        public void RemoveCaravan(MobileParty party)
        {
            if (Caravans.Contains(party))
            {
                Caravans.Remove(party);
            }
        }

        public void ChangeEstateOwner(Estate estate, Hero owner)
        {
            var currentOwner = estate.Owner;
            if (currentOwner != null && Estates.ContainsKey(currentOwner))
            {
                if (Estates[currentOwner].Contains(estate))
                {
                    Estates[currentOwner].Remove(estate);
                }
            }

            if (owner != null)
            {
                if (Estates.ContainsKey(owner))
                {
                    Estates[owner].Add(estate);
                }
                else
                {
                    Estates.Add(owner, new List<Estate>() { estate });
                }
            }
        }

        public void AddEstate(Estate estate) 
        {
            var currentOwner = estate.Owner;
            if (currentOwner != null) 
            {
                if (Estates.ContainsKey(currentOwner))
                {
                    if (!Estates[currentOwner].Contains(estate))
                    {
                        Estates[currentOwner].Add(estate);
                    }
                }
                else
                {
                    Estates.Add(currentOwner, new List<Estate>() { estate });
                }
            } 
        }

        public List<Estate> GetEstates(Hero owner)
        {
            var list = new List<Estate>();
            if (owner == null)
            {
                return list;
            }

            if (Estates.ContainsKey(owner))
            {
                list = Estates[owner];
            }

            return list;
        }

        public List<(ItemObject, float)> GetProductions(PopulationData data)
        {
            var villageData = data.VillageData;
            var type = villageData.Village.VillageType;
            var productions = new List<(ItemObject, float)>(type.Productions);

            float mines = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Mines);
            if ((mines > 0 || villageData.Village.IsMiningVillage()) && data.MineralData != null)
            {
                foreach (var pair in data.MineralData.GetLocalMinerals())
                {
                    if (pair.Item1 != null)
                    {
                        productions.Add(new(pair.Item1, 0.5f * mines * pair.Item2));
                    }
                }
            }

            float tannery = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Tannery);
            if (tannery > 0)
            {
                productions.Add(new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>("leather"),
                        tannery * 0.5f));
            }

            float smith = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Blacksmith);
            if (smith > 0)
            {
                productions.Add(new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>("tools"),
                    smith * 0.5f));
            }

            if (villageData.Village.IsFarmingVillage())
            {
                productions.Add(
                    new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>("chicken"), 1f));
                productions.Add(new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>("goose"),
                    1f));
            }

            if (type == DefaultVillageTypes.OliveTrees)
            {
                productions.Add(new ValueTuple<ItemObject, float>(BKItems.Instance.Orange, 2f));
            }
            else if (type == DefaultVillageTypes.WheatFarm)
            {
                productions.Add(new ValueTuple<ItemObject, float>(BKItems.Instance.Apple, 2f));
                productions.Add(new ValueTuple<ItemObject, float>(BKItems.Instance.Carrot, 2f));
            }
            else if (type == DefaultVillageTypes.Lumberjack || type.StringId == "trapper")
            {
                float honey = 0.5f;
                float skeps = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Skeps);
                if (skeps > 0)
                {
                    honey += 1f * skeps;
                }

                productions.Add(new ValueTuple<ItemObject, float>(BKItems.Instance.Honey, honey));
            }


            var grainTuple = productions.FirstOrDefault(x => x.Item1 == DefaultItems.Grain);
            if (grainTuple.Item1 != null)
            {
                var bread = grainTuple.Item2 * 0.1f;
                float bakery = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Bakery);
                if (bakery > 0)
                {
                    bread += grainTuple.Item2 * 0.15f;
                }

                productions.Add(new ValueTuple<ItemObject, float>(BKItems.Instance.Bread, bread));
            }
            

            return productions;
        }

        public void ApplyGuildEffect(ref ExplainedNumber result, Guild guild, float maxEffect, bool factor)
        {
            var effect = maxEffect * guild.Influence;
            if (effect != 0f)
            {
                if (!factor)
                {
                    result.Add(effect, guild.GuildType.Name);
                }
                else
                {
                    result.AddFactor(effect, guild.GuildType.Name);
                }
            }
        }

        public void ApplyProductionBuildingEffect(ref ExplainedNumber explainedNumber, ItemObject item, VillageData data)
        {
            BuildingType buildingType = null;
            if (item.IsAnimal)
            {
                buildingType = DefaultVillageBuildings.Instance.AnimalHousing;
            }
            else if (item.IsMineral())
            {
                buildingType = DefaultVillageBuildings.Instance.Mining;
            }
            else if (item.ItemCategory == DefaultItemCategories.Wood)
            {
                buildingType = DefaultVillageBuildings.Instance.Sawmill;
            }
            else if (item.ItemCategory == DefaultItemCategories.Fish)
            {
                buildingType = DefaultVillageBuildings.Instance.FishFarm;
            }
            else if (item.ItemCategory == DefaultItemCategories.Grain ||
                     item.ItemCategory == DefaultItemCategories.DateFruit ||
                     item.ItemCategory == DefaultItemCategories.Grape ||
                     item.ItemCategory == DefaultItemCategories.Olives || item.ItemCategory == DefaultItemCategories.Flax)
            {
                buildingType = DefaultVillageBuildings.Instance.Farming;
            }

            var level = data.GetBuildingLevel(buildingType);
            if (level > 0)
            {
                explainedNumber.AddFactor(level * 0.05f);
            }
        }

        public void InitializeSettlementPops(Settlement settlement)
        {
            if (settlement.StringId.Contains("Ruin") || settlement.StringId.Contains("tutorial") || Populations.ContainsKey(settlement))
            {
                return;
            }

            var popQuantityRef = GetDesiredTotalPop(settlement);
            var desiredTypes = GetDesiredPopTypes(settlement, true);
            var classes = new List<PopulationClass>();

            var nobles = (int) (popQuantityRef *
                                MBRandom.RandomFloatRanged(desiredTypes[PopType.Nobles][0],
                                    desiredTypes[PopType.Nobles][1]));
            var craftsmen = !settlement.IsVillage
                ? (int) (popQuantityRef *
                         MBRandom.RandomFloatRanged(desiredTypes[PopType.Craftsmen][0], desiredTypes[PopType.Craftsmen][1]))
                : 0;
            var serfs = (int) (popQuantityRef *
                               MBRandom.RandomFloatRanged(desiredTypes[PopType.Serfs][0], desiredTypes[PopType.Serfs][1]));
            var slaves = (int) (popQuantityRef *
                                MBRandom.RandomFloatRanged(desiredTypes[PopType.Slaves][0],
                                    desiredTypes[PopType.Slaves][1]));

            classes.Add(new PopulationClass(PopType.Nobles, nobles));
            if (craftsmen > 0)
            {
                classes.Add(new PopulationClass(PopType.Craftsmen, craftsmen));
            }

            classes.Add(new PopulationClass(PopType.Serfs, serfs));
            classes.Add(new PopulationClass(PopType.Slaves, slaves));

            var assimilation = settlement.Culture == settlement.OwnerClan.Culture ? 1f : 0f;
            var data = new PopulationData(classes, settlement, assimilation);
            BannerKingsConfig.Instance.PopulationManager.AddSettlementData(settlement, data);
        }

        public bool PopSurplusExists(Settlement settlement, PopType type, bool maxSurplus = false)
        {
            var data = GetPopData(settlement);
            var pops = data.GetTypeCount(type);
            var popRatios = GetDesiredPopTypes(settlement);
            double ratio;
            if (maxSurplus)
            {
                ratio = popRatios[type][1];
            }
            else
            {
                ratio = MBRandom.RandomFloatRanged(popRatios[type][0], popRatios[type][1]);
            }

            var currentRatio = pops / (double) data.TotalPop;
            return currentRatio > ratio;
        }

        public static void UpdateSettlementPops(Settlement settlement)
        {
            if ((settlement.IsCastle || (settlement.IsTown && settlement.Town != null)
                                     || (settlement.IsVillage && settlement.Village != null)) &&
                settlement.OwnerClan != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null)
                {
                    data.Update(null);
                }
            }
        }

        public static int GetDesiredTotalPop(Settlement settlement)
        {
            if (settlement.IsCastle)
            {
                var prosperityFactor = 0.0001f * settlement.Prosperity + 1f;
                return MBRandom.RandomInt((int) (2000 * prosperityFactor), (int) (3000 * prosperityFactor));
            }

            if (settlement.IsVillage)
            {
                return MBRandom.RandomInt((int) settlement.Village.Hearth * 4, (int) settlement.Village.Hearth * 6);
            }

            if (settlement.IsTown)
            {
                var prosperityFactor = 0.0001f * settlement.Prosperity + 1f;
                if (settlement.Owner is {IsFactionLeader: true})
                {
                    prosperityFactor *= 1.2f;
                }

                if (!IsMetropolis(settlement))
                {
                    return MBRandom.RandomInt((int) (8000 * prosperityFactor), (int) (15000 * prosperityFactor));
                }

                return MBRandom.RandomInt((int) (20000 * prosperityFactor), (int) (25000 * prosperityFactor));
            }

            return 0;
        }

        private static bool IsMetropolis(Settlement settlement)
        {
            return settlement.Name.ToString() == "Liberartis" ||
                   settlement.Name.ToString() == "Pravend" || settlement.Name.ToString() == "Lycaron" ||
                   settlement.Name.ToString() == "Quyaz" ||
                   settlement.Name.ToString() == "Kapudere" || settlement.Name.ToString() == "Qasira" ||
                   settlement.Name.ToString() == "Epicrotea"
                   || settlement.Name.ToString() == "Argoron";
        }

        public int GetPopCountOverLimit(Settlement settlement, PopType type)
        {
            var desiredTypes = GetDesiredPopTypes(settlement);
            var data = GetPopData(settlement);
            var max = (int) (data.TotalPop * desiredTypes[type][1]);
            var current = data.GetTypeCount(type);
            return current - max;
        }

        public static Dictionary<PopType, float[]> GetDesiredPopTypes(Settlement settlement, bool firstTime = false)
        {
            var nobleFactor = 1f;
            var slaveFactor = 1f;
            float serfFactor = 1f;
            float tenantFactor = 1f;
           
            if (!firstTime)
            {
                nobleFactor = BannerKingsConfig.Instance.GrowthModel.CalculatePopulationClassDemand(settlement, PopType.Nobles).ResultNumber;
                slaveFactor = BannerKingsConfig.Instance.GrowthModel.CalculatePopulationClassDemand(settlement, PopType.Slaves).ResultNumber;
                serfFactor = BannerKingsConfig.Instance.GrowthModel.CalculatePopulationClassDemand(settlement, PopType.Serfs).ResultNumber;
                tenantFactor = BannerKingsConfig.Instance.GrowthModel.CalculatePopulationClassDemand(settlement, PopType.Tenants).ResultNumber;
            }

            if (settlement.IsCastle)
            {
                return new Dictionary<PopType, float[]>
                {
                    {PopType.Nobles, new[] {0.07f * nobleFactor, 0.09f * nobleFactor}},
                    {PopType.Craftsmen, new[] {0.03f, 0.05f}},
                    {PopType.Serfs, new[] {0.75f * serfFactor, 0.8f * serfFactor}},
                    {PopType.Tenants, new[] {0.75f * tenantFactor, 0.8f * tenantFactor}},
                    {PopType.Slaves, new[] {0.1f * slaveFactor, 0.15f * slaveFactor}}
                };
            }

            if (settlement.IsVillage)
            {
                if (IsVillageProducingFood(settlement.Village))
                {
                    return new Dictionary<PopType, float[]>
                    {
                        {PopType.Nobles, new[] {0.035f * nobleFactor, 0.055f * nobleFactor}},
                        {PopType.Craftsmen, new[] {0.035f, 0.55f}},
                        {PopType.Serfs, new[] {0.7f * serfFactor, 0.8f * serfFactor}},
                        {PopType.Tenants, new[] {0.7f * tenantFactor, 0.8f * tenantFactor}},
                        {PopType.Slaves, new[] {0.1f * slaveFactor, 0.2f * slaveFactor}}
                    };
                }

                if (IsVillageAMine(settlement.Village))
                {
                    return new Dictionary<PopType, float[]>
                    {
                        {PopType.Nobles, new[] {0.02f * nobleFactor, 0.04f * nobleFactor}},
                        {PopType.Craftsmen, new[] {0.055f, 0.1f}},
                        {PopType.Serfs, new[] {0.3f * serfFactor, 0.4f * serfFactor }},
                        {PopType.Tenants, new[] {0.3f * tenantFactor, 0.4f * tenantFactor}},
                        {PopType.Slaves, new[] {0.6f * slaveFactor, 0.7f * slaveFactor}}
                    };
                }

                return new Dictionary<PopType, float[]>
                {
                    {PopType.Nobles, new[] {0.025f * nobleFactor, 0.045f * nobleFactor}},
                    {PopType.Craftsmen, new[] {0.45f, 0.65f}},
                    {PopType.Serfs, new[] {0.5f * serfFactor, 0.7f * serfFactor }},
                    {PopType.Tenants, new[] {0.5f * tenantFactor, 0.7f * tenantFactor}},
                    {PopType.Slaves, new[] {0.4f * slaveFactor, 0.5f * slaveFactor}}
                };
            }

            if (settlement.IsTown)
            {
                return new Dictionary<PopType, float[]>
                {
                    {PopType.Nobles, new[] {0.01f * nobleFactor, 0.03f * nobleFactor}},
                    {PopType.Craftsmen, new[] {0.06f, 0.08f}},
                    {PopType.Serfs, new[] {0.6f * serfFactor, 0.7f * serfFactor }},
                    {PopType.Tenants, new[] {0.6f * tenantFactor, 0.7f * tenantFactor}},
                    {PopType.Slaves, new[] {0.1f * slaveFactor, 0.2f * slaveFactor}}
                };
            }

            return null;
        }

        public static bool IsVillageProducingFood(Village village)
        {
            return village.VillageType == DefaultVillageTypes.CattleRange ||
                   village.VillageType == DefaultVillageTypes.DateFarm ||
                   village.VillageType == DefaultVillageTypes.Fisherman ||
                   village.VillageType == DefaultVillageTypes.OliveTrees ||
                   village.VillageType == DefaultVillageTypes.VineYard ||
                   village.VillageType == DefaultVillageTypes.WheatFarm;
        }

        public static bool IsVillageAMine(Village village)
        {
            return village.VillageType == DefaultVillageTypes.SilverMine ||
                   village.VillageType == DefaultVillageTypes.IronMine ||
                   village.VillageType == DefaultVillageTypes.SaltMine ||
                   village.VillageType == DefaultVillageTypes.ClayMine;
        }
    }
}