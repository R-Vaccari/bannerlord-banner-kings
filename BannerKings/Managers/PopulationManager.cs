using BannerKings.Components;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class PopulationManager
    {
        [SaveableProperty(1)]
        private Dictionary<Settlement, PopulationData> Populations { get; set; }

        [SaveableProperty(2)]
        private List<MobileParty> Caravans { get; set; }

        public PopulationManager(Dictionary<Settlement, PopulationData> pops, List<MobileParty> caravans)
        {
            this.Populations = pops;
            this.Caravans = caravans;
        }

        public bool IsSettlementPopulated(Settlement settlement)
        {
            if (Populations != null)
            {
                if (settlement.StringId.Contains("Ruin") || settlement.StringId.Contains("tutorial"))
                    return false;

                else return Populations.ContainsKey(settlement);
            }
            else return false;
        }

        public PopulationData GetPopData(Settlement settlement) 
        {
            try
            {
                if (Populations.ContainsKey(settlement)) return Populations[settlement];
                InitializeSettlementPops(settlement);
                return Populations[settlement];
            } catch (Exception ex)
            {
                string cause = "Exception in Banner Kings GetPopData method. ";
                string objInfo = null;
                if (settlement != null)
                    objInfo = string.Format("Name [{0}], Id [{1}], Culture [{2}].", settlement.Name, settlement.StringId, settlement.Culture);
                else objInfo = "Null settlement.";

                throw new BannerKingsException(cause + objInfo, ex);
            }
            return null;
        }

        public void AddSettlementData(Settlement settlement, PopulationData data) => Populations.Add(settlement, data);
        public bool IsPopulationParty(MobileParty party) => Caravans.Contains(party);
        public void AddParty(MobileParty party) => Caravans.Add(party);
        public void RemoveCaravan(MobileParty party)
        {
            if (Caravans.Contains(party))
                Caravans.Remove(party);
        }

        public List<MobileParty> GetClanMilitias(Clan clan)
        {
            List<MobileParty> list = new List<MobileParty>();
            foreach (MobileParty party in Caravans)
                if (party.PartyComponent is RetinueComponent && party.Owner.Clan == clan)
                    list.Add(party);
            
            return list;
        }

        public List<MobileParty> GetParties(Type type) => this.Caravans.FindAll(x => x.GetType() == type);

        public List<(ItemObject, float)> GetProductions(VillageData villageData)
        {
            List<(ItemObject, float)> productions = new List<(ItemObject, float)>(villageData.Village.VillageType.Productions);

            float tannery = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Tannery);
            if (tannery > 0)
            {
                /*ItemObject randomItem = this.GetRandomItem(production.Outputs[i].Item1, town);
                if (randomItem != null)
                {
                    list.Add(new ValueTuple<ItemObject, int>(randomItem, item));
                    num3 += town.GetItemPrice(randomItem, null, true) * item;
                } WorkshopCampaignBehavior for reference how to add arms to production    */
                productions.Add(new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>("leather"), tannery * 0.5f));
            }

            float smith = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Blacksmith);
            if (smith > 0)
                productions.Add(new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>("tools"), smith * 0.5f));

            return productions;
        }

        public void ApplyProductionBuildingEffect(ref ExplainedNumber explainedNumber, BuildingType type, VillageData data)
        {
            int level = data.GetBuildingLevel(type);
            if (level > 0) explainedNumber.AddFactor(level * 0.05f);
        }

        public void ReInitBuildings()
        {
            foreach (PopulationData data in this.Populations.Values)
                if (data.VillageData != null)
                    data.VillageData.ReInitializeBuildings();
        }

        public static void InitializeSettlementPops(Settlement settlement)
        {
            if (settlement.StringId.Contains("Ruin") || settlement.StringId.Contains("tutorial"))
                return;

            int popQuantityRef = GetDesiredTotalPop(settlement);
            Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
            List<PopulationClass> classes = new List<PopulationClass>();

            int nobles = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Nobles][0], desiredTypes[PopType.Nobles][1]));
            int craftsmen = !settlement.IsVillage ? (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Craftsmen][0], desiredTypes[PopType.Craftsmen][1])) : 0;
            int serfs = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Serfs][0], desiredTypes[PopType.Serfs][1]));
            int slaves = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Slaves][0], desiredTypes[PopType.Slaves][1]));

            classes.Add(new PopulationClass(PopType.Nobles, nobles));
            if (craftsmen > 0) classes.Add(new PopulationClass(PopType.Craftsmen, craftsmen));
            classes.Add(new PopulationClass(PopType.Serfs, serfs));
            classes.Add(new PopulationClass(PopType.Slaves, slaves));

            float assimilation = settlement.Culture == settlement.OwnerClan.Culture ? 1f : 0f;
            PopulationData data = new PopulationData(classes, settlement, assimilation);
            BannerKingsConfig.Instance.PopulationManager.AddSettlementData(settlement, data);
        }

        public bool PopSurplusExists(Settlement settlement, PopType type, bool maxSurplus = false)
        {
            PopulationData data = GetPopData(settlement);
            int pops = data.GetTypeCount(type);
            Dictionary<PopType, float[]> popRatios = GetDesiredPopTypes(settlement);
            double ratio;
            if (maxSurplus) ratio = popRatios[type][1];
            else ratio = MBRandom.RandomFloatRanged(popRatios[type][0], popRatios[type][1]);
            double currentRatio = (double)pops / (double)data.TotalPop;
            return currentRatio > ratio;
        }

        public static void UpdateSettlementPops(Settlement settlement)
        {
            if ((settlement.IsCastle || (settlement.IsTown && settlement.Town != null)
                || (settlement.IsVillage && settlement.Village != null)) && settlement.OwnerClan != null)
            {
                if (!BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    InitializeSettlementPops(settlement);
                else
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    data.Update(null);
                }  
            }
        }

        public 

         static int GetDesiredTotalPop(Settlement settlement)
        {
            if (settlement.IsCastle)
            {
                float prosperityFactor = (0.0001f * settlement.Prosperity) + 1f;
                return MBRandom.RandomInt((int)(2000 * prosperityFactor), (int)(3000 * prosperityFactor));
            }
            else if (settlement.IsVillage)
                return MBRandom.RandomInt((int)settlement.Village.Hearth * 4, (int)settlement.Village.Hearth * 6);
            else if (settlement.IsTown)
            {
                float prosperityFactor = (0.0001f * settlement.Prosperity) + 1f;
                if (settlement.Owner != null && settlement.Owner.IsFactionLeader)
                    prosperityFactor *= 1.2f;
                if (!IsMetropolis(settlement)) return MBRandom.RandomInt((int)(8000 * prosperityFactor), (int)(15000 * prosperityFactor));
                else return MBRandom.RandomInt((int)(20000 * prosperityFactor), (int)(25000 * prosperityFactor));
            }
            else return 0;
        }

        private static bool IsMetropolis(Settlement settlement) => settlement.Name.ToString() == "Liberartis" ||
            settlement.Name.ToString() == "Pravend" || settlement.Name.ToString() == "Lycaron" || settlement.Name.ToString() == "Quyaz" ||
            settlement.Name.ToString() == "Kapudere" || settlement.Name.ToString() == "Qasira" || settlement.Name.ToString() == "Epicrotea"
            || settlement.Name.ToString() == "Argoron";

        public int GetPopCountOverLimit(Settlement settlement, PopType type)
        {
            Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
            PopulationData data = GetPopData(settlement);
            int max = (int)((float)data.TotalPop * desiredTypes[type][1]);
            int current = data.GetTypeCount(type);
            return current - max;
        }

        public static Dictionary<PopType, float[]> GetDesiredPopTypes(Settlement settlement)
        {
            float nobleFactor = 1f;
            float slaveFactor = 1f;
            Kingdom faction = settlement.OwnerClan.Kingdom;
            if (faction != null)
            {
                if (faction.ActivePolicies.Contains(DefaultPolicies.Serfdom))
                    slaveFactor -= 0.3f;

                if (faction.ActivePolicies.Contains(DefaultPolicies.ForgivenessOfDebts))
                    slaveFactor -= 0.1f;

                if (faction.ActivePolicies.Contains(DefaultPolicies.Citizenship))
                    nobleFactor += 0.1f;
            }

            if (settlement.IsCastle)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.07f * nobleFactor, 0.09f * nobleFactor } },
                    { PopType.Craftsmen, new float[] {0.03f, 0.05f} },
                    { PopType.Serfs, new float[] {0.75f, 0.8f} },
                    { PopType.Slaves, new float[] {0.1f * slaveFactor, 0.15f * slaveFactor } }
                };
            else if (settlement.IsVillage)
            {
                if (IsVillageProducingFood(settlement.Village))
                    return new Dictionary<PopType, float[]>()
                    {
                        { PopType.Nobles, new float[] {0.035f * nobleFactor, 0.055f * nobleFactor } },
                        { PopType.Craftsmen, new float[] { 0.01f, 0.02f} },
                        { PopType.Serfs, new float[] {0.7f, 0.8f} },
                        { PopType.Slaves, new float[] {0.1f * slaveFactor, 0.2f * slaveFactor } }
                    };
                else if (IsVillageAMine(settlement.Village))
                    return new Dictionary<PopType, float[]>()
                    {
                        { PopType.Nobles, new float[] {0.02f * nobleFactor, 0.04f * nobleFactor } },
                        { PopType.Craftsmen, new float[] {0.01f, 0.02f} },
                        { PopType.Serfs, new float[] {0.3f, 0.4f} },
                        { PopType.Slaves, new float[] {0.6f * slaveFactor, 0.7f * slaveFactor } }
                    };
                else
                    return new Dictionary<PopType, float[]>()
                    {
                        { PopType.Nobles, new float[] {0.025f * nobleFactor, 0.045f * nobleFactor } },
                        { PopType.Craftsmen, new float[] { 0.01f, 0.02f } },
                        { PopType.Serfs, new float[] {0.5f, 0.7f} },
                        { PopType.Slaves, new float[] {0.4f * slaveFactor, 0.5f * slaveFactor } }
                    };
            }
            else if (settlement.IsTown)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.01f * nobleFactor, 0.03f * nobleFactor } },
                    { PopType.Craftsmen, new float[] {0.06f, 0.08f} },
                    { PopType.Serfs, new float[] {0.6f, 0.7f} },
                    { PopType.Slaves, new float[] {0.1f * slaveFactor, 0.2f * slaveFactor } }
                };
            else return null;
        }

        public static bool IsVillageProducingFood(Village village) => village.VillageType == DefaultVillageTypes.CattleRange || village.VillageType == DefaultVillageTypes.DateFarm ||
                village.VillageType == DefaultVillageTypes.Fisherman || village.VillageType == DefaultVillageTypes.OliveTrees ||
                village.VillageType == DefaultVillageTypes.VineYard || village.VillageType == DefaultVillageTypes.WheatFarm;

        public static bool IsVillageAMine(Village village) => village.VillageType == DefaultVillageTypes.SilverMine || village.VillageType == DefaultVillageTypes.IronMine ||
                village.VillageType == DefaultVillageTypes.SaltMine || village.VillageType == DefaultVillageTypes.ClayMine;

        public enum PopType
        {
            Nobles,
            Craftsmen,
            Serfs,
            Slaves,
            None
        }

        public enum ConsumptionType
        {
            Luxury,
            Industrial,
            General,
            Food,
            None
        }
    }
}
