using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static Populations.PopulationManager;
using static Populations.PolicyManager;

namespace Populations.Models
{
    public class MilitiaModel : DefaultSettlementMilitiaModel
    {
        public override void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate, out float rangedTroopRate)
        {
            if (PopulationConfig.Instance.PolicyManager != null)
            {
                MilitiaPolicy policy = PopulationConfig.Instance.PolicyManager.GetMilitiaPolicy(settlement);
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
            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement) && PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyType.CONSCRIPTION))
            {
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
                int serfs = data.GetTypeCount(PopType.Serfs);
                baseResult.Add((float)serfs * 0.005f, new TextObject("Conscripted serfs"));
            }

            return baseResult;
        }

        public override float CalculateEliteMilitiaSpawnChance(Settlement settlement)
        {
            float baseResult = base.CalculateEliteMilitiaSpawnChance(settlement);
            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement) 
                && PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyType.SUBSIDIZE_MILITIA))
                baseResult += 0.2f;
            
            return baseResult;
        }
    }
}
