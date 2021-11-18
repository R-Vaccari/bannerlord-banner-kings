
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class LoyaltyModel : DefaultSettlementLoyaltyModel
    {

        public override ExplainedNumber CalculateLoyaltyChange(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateLoyaltyChange(town, includeDescriptions);
            if (IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = GetPopData(town.Settlement);
                int slaves = data.GetTypeCount(PopType.Slaves);
                bool surplusExists = PopSurplusExists(town.Settlement, PopType.Slaves, true);
                baseResult.Add((float)slaves * SLAVE_LOYALTY * (surplusExists ? 1.2f : 1f), new TextObject("Slave population"));

                if (IsPolicyEnacted(town.Settlement, PolicyType.EXEMPTION))
                {
                    int nobles = data.GetTypeCount(PopType.Nobles);
                    baseResult.Add((float)nobles * NOBLE_EXEMPTION_LOYALTY, new TextObject("Nobles exemption policy"));
                }

                if (GetSettlementTax(town.Settlement) == TaxType.LOW)
                {
                    int totalPops = data.TotalPop;
                    baseResult.Add((float)(totalPops - slaves) * TAX_POLICY_LOYALTY * -1f, new TextObject("Low tax policy"));
                }
                else if (GetSettlementTax(town.Settlement) == TaxType.HIGH)
                {
                    int totalPops = data.TotalPop;
                    baseResult.Add((float)(totalPops - slaves) * TAX_POLICY_LOYALTY, new TextObject("High tax policy"));
                }
            }

            return baseResult;
        }
    }
}
