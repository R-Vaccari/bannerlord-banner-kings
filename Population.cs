using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Populations
{
    public static class Population
    {
        public static readonly Dictionary<Settlement, PopulationData> POPS = new Dictionary<Settlement, PopulationData>();
        public static readonly Dictionary<MobileParty, Settlement> CARAVANS = new Dictionary<MobileParty, Settlement>();
        private static readonly float POP_GROWTH_FACTOR = 0.001f;
        private static readonly float SLAVE_GROWTH_FACTOR = 0.0005f;


        public static bool IsSettlementPopulated(Settlement settlement) => POPS.ContainsKey(settlement);
        public static PopulationData GetPopData(Settlement settlement) => POPS[settlement];

        public static void InitializeSettlementPops(Settlement settlement)
        {
            int popQuantityRef = GetDesiredTotalPop(settlement);
            Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
            List<PopulationClass> classes = new List<PopulationClass>();

            int nobles = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Nobles][0], desiredTypes[PopType.Nobles][1]));
            int craftsmen = settlement.IsTown ? (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Craftsmen][0], desiredTypes[PopType.Craftsmen][1])) : 0;
            int serfs = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Serfs][0], desiredTypes[PopType.Serfs][1]));
            int slaves = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Slaves][0], desiredTypes[PopType.Slaves][1]));

            classes.Add(new PopulationClass(PopType.Nobles, nobles));
            classes.Add(new PopulationClass(PopType.Craftsmen, craftsmen));
            classes.Add(new PopulationClass(PopType.Serfs, serfs));
            classes.Add(new PopulationClass(PopType.Slaves, slaves));

            PopulationData data = new PopulationData(classes);
            POPS.Add(settlement, data);
        }

        public static bool SlaveSurplusExists(Settlement settlement, bool maxSurplus = false)
        {
            PopulationData data = GetPopData(settlement);
            int slaves = data.GetTypeCount(PopType.Slaves);
            Dictionary<PopType, float[]> popRatios = GetDesiredPopTypes(settlement);
            double ratio;
            if (maxSurplus) ratio = popRatios[PopType.Slaves][1];
            else ratio = MBRandom.RandomFloatRanged(popRatios[PopType.Slaves][0], popRatios[PopType.Slaves][1]);
            double currentRatio = slaves / (double)data.TotalPop;
            return currentRatio > ratio;
        }

        public static void UpdateSettlementPops(Settlement settlement)
        {  
            if ((settlement.IsCastle || (settlement.IsTown && settlement.Town != null) 
                || (settlement.IsVillage && settlement.Village !=null)) && settlement.OwnerClan != null)
            {
                if (!POPS.ContainsKey(settlement))
                    InitializeSettlementPops(settlement);
                else
                {
                    PopulationData data = POPS[settlement];
                    int growthFactor = GetDataGrowthFactor(data);
                    data.UpdatePopulation(settlement, growthFactor, PopType.None);
                    POPS[settlement] = data;
                }
            }
        }

        public static int GetDataGrowthFactor(PopulationData data)
        {
            int growthFactor = 0;
            data.Classes.ForEach(popClass =>
            {
                if (popClass.type != PopType.Slaves)
                    growthFactor += (int)(popClass.count * POP_GROWTH_FACTOR);
                else growthFactor -= (int)(popClass.count * SLAVE_GROWTH_FACTOR);
            });
            return growthFactor;
        }

    private static int GetDesiredTotalPop(Settlement settlement)
        {
            if (settlement.IsCastle)
            {
                float prosperityFactor = (0.0001f * settlement.Prosperity) + 1f;
                return MBRandom.RandomInt((int)(400 * prosperityFactor), (int)(1200 * prosperityFactor));
            }
            else if (settlement.IsVillage)
                return MBRandom.RandomInt((int)settlement.Village.Hearth * 3, (int)settlement.Village.Hearth * 6);
            else if (settlement.IsTown)
            {
                float prosperityFactor = (0.0001f * settlement.Prosperity) + 1f;
                if (settlement.Owner != null && settlement.Owner.IsFactionLeader)
                    prosperityFactor *= 1.2f;
                return MBRandom.RandomInt((int)(8000 * prosperityFactor), (int)(25000 * prosperityFactor));
            }
            else return 0;
        }

        public static Dictionary<PopType, float[]> GetDesiredPopTypes(Settlement settlement)
        {
            if (settlement.IsCastle)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.04f, 0.08f} },
                    { PopType.Serfs, new float[] {0.75f, 0.8f} },
                    { PopType.Slaves, new float[] {0.15f, 0.25f} }
                };
            else if (settlement.IsVillage)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.01f, 0.02f} },
                    { PopType.Serfs, new float[] {0.5f, 0.7f} },
                    { PopType.Slaves, new float[] {0.4f, 0.5f} }
                };
            else if (settlement.IsTown)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.01f, 0.03f} },
                    { PopType.Craftsmen, new float[] {0.09f, 0.12f} },
                    { PopType.Serfs, new float[] {0.4f, 0.5f} },
                    { PopType.Slaves, new float[] {0.33f, 0.45f} }
                };
            else return null;
        }

        public class PopulationData
        {
            private List<PopulationClass> classes;            
            private int totalPop;

            public PopulationData(List<PopulationClass> classes)
            {
                this.classes = classes;
                classes.ForEach(popClass => TotalPop += popClass.count);
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

            public void UpdatePopulation(Settlement settlement, int pops, PopType target)
            {
                if (target != PopType.None)
                {
                    Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
                    IEnumerable<PopType> types = new List<PopType>();
                    classes.ForEach(popClass => types.AddItem(popClass.type));
                    PopType targetType = MBRandom.ChooseWeighted(types, delegate (PopType type)
                    {
                        return MBRandom.RandomFloatRanged(desiredTypes[type][0], desiredTypes[type][1]);
                    });
                    UpdatePopType(targetType, pops);
                }
                else UpdatePopType(target, pops);
            }

            public void UpdatePopType(PopType type, int count)
            {
                if (type != PopType.None)
                {
                    PopulationClass pops = classes.Find(popClass => popClass.type == type);
                    if (pops == null)
                        pops = new PopulationClass(type, 0);
                 
                    pops.count += count;
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
                return GetTypeCount(type) / TotalPop;
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
            public PopType type;
            public int count;

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
    }
}
