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
            base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
        }

        public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateMilitiaChange(settlement, includeDescriptions);
            if (IsSettlementPopulated(settlement) && IsPolicyEnacted(settlement, PolicyType.CONSCRIPTION))
            {
                PopulationData data = GetPopData(settlement);
                int serfs = data.GetTypeCount(PopType.Serfs);
                baseResult.Add((float)serfs * 0.005f, new TextObject("Conscripted serfs"));
            }

            return baseResult;
        }

        public override float CalculateEliteMilitiaSpawnChance(Settlement settlement)
        {
            float baseResult = base.CalculateEliteMilitiaSpawnChance(settlement);
            if (IsSettlementPopulated(settlement) && IsPolicyEnacted(settlement, PolicyType.SUBSIDIZE_MILITIA))
            {
                if (baseResult == 0f)
                    baseResult = 0.15f;
                else baseResult *= 1.2f;
            }
            return baseResult;
        }
    }
}
