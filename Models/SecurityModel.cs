using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Localization;

namespace Populations.Models
{
    class SecurityModel : DefaultSettlementSecurityModel
    {

        public override ExplainedNumber CalculateSecurityChange(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateSecurityChange(town, includeDescriptions);

            if (town.IsCastle)
                baseResult.Add(0.5f, new TextObject("Castle security"));

            if (PopulationConfig.Instance.PolicyManager != null)
                if (PopulationConfig.Instance.PolicyManager.GetSettlementWork(town.Settlement) == PolicyManager.WorkforcePolicy.Martial_Law)
                {
                    float militia = town.Militia;
                    baseResult.Add(militia * 0.01f, new TextObject("Martial Law policy"));
                }

            return baseResult;
        }

    }
}
