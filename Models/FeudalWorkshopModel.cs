using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Library;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class FeudalWorkshopModel : DefaultWorkshopModel
    {
        private static readonly float CRAFTSMEN_EFFECT_CAP = 0.4f;
        public override float GetPolicyEffectToProduction(Town town)
        {
            float baseResult = base.GetPolicyEffectToProduction(town);
            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                float craftsmen = data.GetTypeCount(PopType.Craftsmen);
                baseResult  += MathF.Min((craftsmen / 250f) * 0.020f, CRAFTSMEN_EFFECT_CAP);

                if (PopulationConfig.Instance.PolicyManager.GetSettlementWork(town.Settlement) == PolicyManager.WorkforcePolicy.Martial_Law)
                    baseResult -= 0.25f;
            }
            return baseResult;
        }
    }
}
