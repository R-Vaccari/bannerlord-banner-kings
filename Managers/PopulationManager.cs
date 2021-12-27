using Populations.Components;
using Populations.Models;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace Populations
{
    public class PopulationManager
    {
        [SaveableProperty(100)]
        private Dictionary<Settlement, PopulationData> POPS { get; set; }

        [SaveableProperty(101)]
        private List<MobileParty> CARAVANS { get; set; }

        public PopulationManager(Dictionary<Settlement, PopulationData> pops, List<MobileParty> caravans)
        {
            this.POPS = pops;
            this.CARAVANS = caravans;
        }

        public bool IsSettlementPopulated(Settlement settlement)
        {
            if (POPS != null) return POPS.ContainsKey(settlement);
            else return false;
        }
        public PopulationData GetPopData(Settlement settlement) => POPS[settlement];
        public void AddSettlementData(Settlement settlement, PopulationData data) => POPS.Add(settlement, data);
        public bool IsPopulationParty(MobileParty party) => CARAVANS.Contains(party);
        public void AddParty(MobileParty party) => CARAVANS.Add(party);
        public void RemoveCaravan(MobileParty party) => CARAVANS.Remove(party);

        public List<MobileParty> GetClanMilitias(Clan clan)
        {
            List<MobileParty> list = new List<MobileParty>();
            foreach (MobileParty party in CARAVANS)
                if (party.PartyComponent is MilitiaComponent && party.Owner.Clan == clan)
                    list.Add(party);
            
            return list;
        }

        public static void InitializeSettlementPops(Settlement settlement)
        {
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
            PopulationData data = new PopulationData(classes, assimilation);
            PopulationConfig.Instance.PopulationManager.AddSettlementData(settlement, data);
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
                if (!PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    InitializeSettlementPops(settlement);
                else
                {
                    new GrowthModel().CalculatePopulationGrowth(settlement);
                    new CultureModel().CalculateAssimilationChange(settlement);
                }  
            }
        }

        public 

         static int GetDesiredTotalPop(Settlement settlement)
        {
            if (settlement.IsCastle)
            {
                float prosperityFactor = (0.0001f * settlement.Prosperity) + 1f;
                return MBRandom.RandomInt((int)(900 * prosperityFactor), (int)(1200 * prosperityFactor));
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
            if (settlement.IsCastle)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.02f, 0.06f} },
                    { PopType.Craftsmen, new float[] {0.06f, 0.09f} },
                    { PopType.Serfs, new float[] {0.75f, 0.8f} },
                    { PopType.Slaves, new float[] {0.1f, 0.15f} }
                };
            else if (settlement.IsVillage)
            {
                if (IsVillageProducingFood(settlement.Village))
                    return new Dictionary<PopType, float[]>()
                    {
                        { PopType.Nobles, new float[] {0.025f, 0.045f} },
                        { PopType.Serfs, new float[] {0.7f, 0.8f} },
                        { PopType.Slaves, new float[] {0.1f, 0.2f} }
                    };
                else if (IsVillageAMine(settlement.Village))
                    return new Dictionary<PopType, float[]>()
                    {
                        { PopType.Nobles, new float[] {0.01f, 0.02f} },
                        { PopType.Serfs, new float[] {0.3f, 0.4f} },
                        { PopType.Slaves, new float[] {0.6f, 0.7f} }
                    };
                else
                    return new Dictionary<PopType, float[]>()
                    {
                        { PopType.Nobles, new float[] {0.01f, 0.02f} },
                        { PopType.Serfs, new float[] {0.5f, 0.7f} },
                        { PopType.Slaves, new float[] {0.4f, 0.5f} }
                    };
            }
            else if (settlement.IsTown)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.01f, 0.03f} },
                    { PopType.Craftsmen, new float[] {0.06f, 0.08f} },
                    { PopType.Serfs, new float[] {0.6f, 0.7f} },
                    { PopType.Slaves, new float[] {0.1f, 0.2f} }
                };
            else return null;
        }

        public static bool IsVillageProducingFood(Village village) => village.VillageType == DefaultVillageTypes.CattleRange || village.VillageType == DefaultVillageTypes.DateFarm ||
                village.VillageType == DefaultVillageTypes.Fisherman || village.VillageType == DefaultVillageTypes.OliveTrees ||
                village.VillageType == DefaultVillageTypes.VineYard || village.VillageType == DefaultVillageTypes.WheatFarm;


        public static bool IsVillageAMine(Village village) => village.VillageType == DefaultVillageTypes.SilverMine || village.VillageType == DefaultVillageTypes.IronMine ||
                village.VillageType == DefaultVillageTypes.SaltMine || village.VillageType == DefaultVillageTypes.ClayMine;


        public class PopulationData
        {
            [SaveableProperty(1)]
            private List<PopulationClass> classes { get; set; }

            [SaveableProperty(2)]
            private int totalPop { get; set; }

            [SaveableProperty(3)]
            private float assimilation { get; set; }

            private float[] satisfactions { get; set; }

            public PopulationData(List<PopulationClass> classes, float assimilation)
            {
                this.classes = classes;
                classes.ForEach(popClass => TotalPop += popClass.count);
                this.assimilation = assimilation;
                this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f, 0.5f};
            }

            public float Assimilation
            {
                get
                {
                    return assimilation;
                }
                set
                {
                    if (value != assimilation)
                        assimilation = value;
                }
            }

            public List<PopulationClass> Classes
            {
                get
                {
                    return classes;
                }
                set
                {
                    if (value != classes)
                        classes = value;
                }
            }

            public int TotalPop
            {
                get
                {
                    return totalPop;
                }
                set
                {
                    if (value != totalPop)
                        totalPop = value;
                }
            }

            public void UpdateSatisfaction(ConsumptionType type, float value)
            {
                if (this.satisfactions == null ) this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
                this.satisfactions[(int)type] += value;
            }

            public void UpdatePopulation(Settlement settlement, int pops, PopType target)
            {
                if (target == PopType.None)
                {
                    if (settlement.Owner == Hero.MainHero)
                        InformationManager.DisplayMessage(new InformationMessage());
                    bool divisibleNegative = ((float)pops * -1f) > 20;
                    if (pops > 20 || divisibleNegative)
                    {
                        int fractions = (int)((float)pops / (divisibleNegative ? -20f : 20f));
                        int reminder = pops % 20;
                        for (int i = 0; i < fractions; i++)
                        {
                            SelectAndUpdatePop(settlement, divisibleNegative ? -20 : 20);
                        }
                        SelectAndUpdatePop(settlement, divisibleNegative ? -reminder  : reminder);
                    }
                    else SelectAndUpdatePop(settlement, pops);
                }
                else UpdatePopType(target, pops);
            }

            private void SelectAndUpdatePop(Settlement settlement, int pops)
            {
                if (pops != 0)
                {
                    Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
                    List<ValueTuple<PopType, float>> typesList = new List<ValueTuple<PopType, float>>();
                    classes.ForEach(popClass =>
                    {
                        PopType type = popClass.type;
                        if (pops < 0 && popClass.count >= pops)
                        {
                            bool hasExcess = GetCurrentTypeFraction(type) > desiredTypes[type][1];
                            typesList.Add(new ValueTuple<PopType, float>(popClass.type, desiredTypes[type][0] * (hasExcess ? 2f : 1f)));
                        }
                        else if (pops > 0)
                        {
                            bool isLacking = GetCurrentTypeFraction(type) < desiredTypes[type][0];
                            typesList.Add(new ValueTuple<PopType, float>(popClass.type, desiredTypes[type][0] * (isLacking ? 2f : 1f)));
                        }
                    });

                    PopType targetType = MBRandom.ChooseWeighted(typesList);
                    UpdatePopType(targetType, pops);
                }
            }

            public void UpdatePopType(PopType type, int count)
            {
                if (type != PopType.None)
                {
                    PopulationClass pops = classes.Find(popClass => popClass.type == type);
                    if (pops == null)
                        pops = new PopulationClass(type, 0);

                    pops.count += count;
                    if (pops.count < 0) pops.count = 0;
                    RefreshTotal();
                }
            }

            public int GetTypeCount(PopType type)
            {
                PopulationClass targetClass = classes.Find(popClass => popClass.type == type);
                return targetClass != null ? targetClass.count : 0;
            }

            public float GetCurrentTypeFraction(PopType type)
            {
                RefreshTotal();
                return (float)GetTypeCount(type) / (float)TotalPop;
            }

            private void RefreshTotal()
            {
                int pops = 0;
                classes.ForEach(popClass => pops += popClass.count);
                TotalPop = pops;
            }
        }

        public class PopulationClass
        {
            [SaveableProperty(1)]
            public PopType type { get; set; }

            [SaveableProperty(2)]
            public int count { get; set; }

            public PopulationClass(PopType type, int count)
            {
                this.type = type;
                this.count = count;
            }
        }

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
            Food
        }
    }
}
