using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.PolicyManager;

namespace BannerKings.Models
{
    public class MilitiaModel : DefaultSettlementMilitiaModel
    {
        public override void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate, out float rangedTroopRate)
        {
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                MilitiaPolicy policy = BannerKingsConfig.Instance.PolicyManager.GetMilitiaPolicy(settlement);
                if (policy == MilitiaPolicy.Melee)
                {
                    meleeTroopRate = 0.75f;
                    rangedTroopRate = 0.25f;
                } else if (policy == MilitiaPolicy.Ranged)
                {
                    meleeTroopRate = 0.25f;
                    rangedTroopRate = 0.75f;
                } else base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
            } else
                base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
        }

        public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateMilitiaChange(settlement, includeDescriptions);
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement)
                && settlement.Town != null)
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                int serfs = data.GetTypeCount(PopType.Serfs);
                float maxMilitia = GetMilitiaLimit(data, settlement.IsCastle);
                float filledCapacity = settlement.Town.Militia / maxMilitia;
                float baseGrowth = (float)serfs * 0.0025f;

                if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyType.CONSCRIPTION))
                    baseResult.Add(baseGrowth * (1f - 1f * filledCapacity), new TextObject("Conscription policy"));
                else if (filledCapacity > 1f)
                    baseResult.Add(baseGrowth * -1f * filledCapacity, new TextObject("Over supported limit"));
            }

            return baseResult;
        }

        public float GetMilitiaLimit(PopulationData data, bool isCastle)
        {
            if (isCastle)
                return (float)data.TotalPop * 0.1f + 200f;
            else return (float)data.TotalPop * 0.02f + 100f;
        }

        public override float CalculateEliteMilitiaSpawnChance(Settlement settlement)
        {
            float baseResult = base.CalculateEliteMilitiaSpawnChance(settlement) + (settlement.IsTown ? 0.12f : 0.20f);
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement) 
                && BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyType.SUBSIDIZE_MILITIA))
                baseResult += 0.12f;
            
            return baseResult;
        }
    }
}
