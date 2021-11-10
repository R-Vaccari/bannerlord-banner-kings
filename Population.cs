using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Populations
{
    public static class Population
    {
        private static  Dictionary<Settlement, PopulationData> SettlementPops = new Dictionary<Settlement, PopulationData>();

        public static bool IsSettlementPopulated(Settlement settlement) => SettlementPops.ContainsKey(settlement);
        

        public static void InitializeSettlementPops(Settlement settlement)
        {
            int popQuantityRef = GetDesiredTotalPop(settlement);
            Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);

            int nobles = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Nobles][0], desiredTypes[PopType.Nobles][1]));
            int craftsmen = settlement.IsTown ? (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Nobles][0], desiredTypes[PopType.Nobles][1])) : 0;
            int serfs = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Serfs][0], desiredTypes[PopType.Serfs][1]));
            int slaves = (int)(popQuantityRef * MBRandom.RandomFloatRanged(desiredTypes[PopType.Slaves][0], desiredTypes[PopType.Slaves][1]));

            PopulationData data = new PopulationData(nobles, craftsmen, serfs, slaves);
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

        class PopulationData
        {
            private int NobleCount, CraftsmenCount, SerfCount, SlaveCount, TotalPop;

            public PopulationData(int NobleCount, int CraftsmenCount, int SerfCount, int SlaveCount)
            {
                this.NobleCount = NobleCount;
                this.CraftsmenCount = CraftsmenCount;
                this.SerfCount = SerfCount;
                this.SlaveCount = SlaveCount;
                TotalPop = NobleCount + CraftsmenCount + SerfCount + SlaveCount;
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
                int currentCount;
                if (type == PopType.Nobles)
                    currentCount = NobleCount;
                else if (type == PopType.Craftsmen)
                    currentCount = CraftsmenCount;
                else if (type == PopType.Serfs)
                    currentCount = SerfCount;
                else currentCount = SlaveCount;

                int diff = count - currentCount;

                if (type == PopType.Nobles)
                    NobleCount += diff;
                else if (type == PopType.Craftsmen)
                    CraftsmenCount += diff;
                else if (type == PopType.Serfs)
                    SerfCount += diff;
                else SlaveCount += diff;

                TotalPop += diff;
            }

            public int GetTypeCount(PopType type) 
            {
                if (type == PopType.Nobles)
                    return NobleCount;
                else if (type == PopType.Craftsmen)
                    return CraftsmenCount;
                else if (type == PopType.Serfs)
                    return SerfCount;
                else return SlaveCount;
            }
            public float GetCurrentTypeFraction(PopType type)
            {
                if (type == PopType.Nobles)
                    return NobleCount / TotalPop;
                else if (type == PopType.Craftsmen)
                    return CraftsmenCount / TotalPop;
                else if (type == PopType.Serfs)
                    return SerfCount / TotalPop;
                else return SlaveCount / TotalPop;
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
