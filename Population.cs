using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Populations
{
    public static class Population
    {
        private static Dictionary<Settlement, PopulationData> SettlementPops = new Dictionary<Settlement, PopulationData>();

        public static bool IsSettlementPopulated(Settlement settlement) => SettlementPops.ContainsKey(settlement);

        public static PopulationData GetPopData(Settlement settlement) => SettlementPops[settlement];


        public static void InitializeSettlementPops(Settlement settlement)
        {
            int popQuantityRef = GetDesiredTotalPop(settlement);
            Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
            List<PopulationClass> classes = new List<PopulationClass>();

            int nobles = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Nobles][0], desiredTypes[PopType.Nobles][1]));
            int craftsmen = settlement.IsTown ? (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Nobles][0], desiredTypes[PopType.Nobles][1])) : 0;
            int serfs = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Serfs][0], desiredTypes[PopType.Serfs][1]));
            int slaves = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Slaves][0], desiredTypes[PopType.Slaves][1]));

            classes.Add(new PopulationClass(PopType.Nobles, nobles));
            classes.Add(new PopulationClass(PopType.Craftsmen, craftsmen));
            classes.Add(new PopulationClass(PopType.Serfs, serfs));
            classes.Add(new PopulationClass(PopType.Slaves, slaves));

            PopulationData data = new PopulationData(classes);
            SettlementPops.Add(settlement, data);
        }

        public static void GetHearthChange(Village village, ref ExplainedNumber baseResult)
        {
            if (village.Settlement != null && SettlementPops.ContainsKey(village.Settlement))
            {
                PopulationData data = SettlementPops[village.Settlement];
                int serfCount = data.GetTypeCount(PopType.Serfs);
                int slaveCount = data.GetTypeCount(PopType.Slaves);
                float serfToSlaveRatio = serfCount / slaveCount;
                float serfsFactor = data.GetTypeCount(PopType.Serfs) * (0.001f * serfToSlaveRatio);
                float noblesFactor = data.GetTypeCount(PopType.Nobles) * 0.001f;

                baseResult.AddFactor(serfsFactor + noblesFactor, null);
                data.UpdatePopulation(village.Settlement, MBRandom.RandomInt((int)baseResult.BaseNumber * 3, (int)baseResult.BaseNumber * 6));
            }
        }

        public static void UpdateSettlementPops(Settlement settlement)
        {
            if (!SettlementPops.ContainsKey(settlement))
                InitializeSettlementPops(settlement);
            else
            {

            }
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

        private static Dictionary<PopType, float[]> GetDesiredPopTypes(Settlement settlement)
        {
            if (settlement.IsCastle)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.04f, 0.08f} },
                    { PopType.Serfs, new float[] {0.75f, 0.8f} },
                    { PopType.Nobles, new float[] {0.15f, 0.30f} }
                };
            else if (settlement.IsVillage)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.01f, 0.02f} },
                    { PopType.Serfs, new float[] {0.5f, 0.7f} },
                    { PopType.Nobles, new float[] {0.5f, 0.6f} }
                };
            else if (settlement.IsTown)
                return new Dictionary<PopType, float[]>()
                {
                    { PopType.Nobles, new float[] {0.01f, 0.05f} },
                    { PopType.Craftsmen, new float[] {0.08f, 0.12f} },
                    { PopType.Serfs, new float[] {0.4f, 0.5f} },
                    { PopType.Nobles, new float[] {0.33f, 0.51f} }
                };
            else return null;
        }

        public class PopulationData
        {
            private List<PopulationClass> classes;
            private int TotalPop;

            public PopulationData(List<PopulationClass> classes)
            {
                this.classes = classes;
                classes.ForEach(popClass => TotalPop += popClass.count);
            }

            public void UpdatePopulation(Settlement settlement, int pops)
            {
                Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
                IEnumerable<PopType> types = new List<PopType>();
                if (settlement.IsTown) types.AddItem(PopType.Craftsmen);
                types.AddItem(PopType.Nobles);
                types.AddItem(PopType.Serfs);
                types.AddItem(PopType.Slaves);
                PopType targetType = MBRandom.ChooseWeighted<PopType>(types, delegate (PopType type)
                {
                    return MBRandom.RandomFloatRanged(desiredTypes[type][0], desiredTypes[type][1]);
                });
                UpdatePopType(targetType, pops);
            }

            public void UpdatePopType(PopType type, int count)
            {
                PopulationClass pops = classes.Find(popClass => popClass.type == type);
                pops.count += count;
                RefreshTotal();
            }

            public int GetTypeCount(PopType type) => classes.Find(popClass => popClass.type == type).count;

            public float GetCurrentTypeFraction(PopType type) => GetTypeCount(type) / TotalPop;

            private void RefreshTotal()
            {
                TotalPop = 0;
                classes.ForEach(popClass => TotalPop += popClass.count);
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
            Slaves
        }
    }
}
