
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class LoyaltyModel : DefaultSettlementLoyaltyModel
    {
        private static readonly float SLAVE_LOYALTY = -0.0005f;
        private static readonly float NOBLE_EXEMPTION_LOYALTY = 0.004f;
        private static readonly float TAX_POLICY_LOYALTY = 0.0001f;

        public override ExplainedNumber CalculateLoyaltyChange(Town town, bool includeDescriptions = false)
        {

            ExplainedNumber baseResult = base.CalculateLoyaltyChange(town, includeDescriptions);
            if (IsSettlementPopulated(town.Settlement))
            {
                //InformationManager.DisplayMessage(new InformationMessage("Loyalty model running..."));
                PopulationData data = GetPopData(town.Settlement);
                int slaves = data.GetTypeCount(PopType.Slaves);
                bool surplusExists = PopSurplusExists(town.Settlement, PopType.Slaves, true);
                baseResult.Add((float)slaves * SLAVE_LOYALTY * (surplusExists ? 1.2f : 1f), new TextObject("Slave population"));

                if (IsPolicyEnacted(town.Settlement, PolicyType.EXEMPTION))
                {
                    int nobles = data.GetTypeCount(PopType.Nobles);
                    baseResult.Add((float)nobles * NOBLE_EXEMPTION_LOYALTY, new TextObject("Nobles exemption policy"));
                }

                if (GetSettlementTax(town.Settlement) == TaxType.Low)
                {
                    int totalPops = data.TotalPop;
                    baseResult.Add((float)(totalPops - slaves) * TAX_POLICY_LOYALTY * -1f, new TextObject("Low tax policy"));
                }
                else if (GetSettlementTax(town.Settlement) == TaxType.High)
                {
                    int totalPops = data.TotalPop;
                    baseResult.Add((float)(totalPops - slaves) * TAX_POLICY_LOYALTY, new TextObject("High tax policy"));
                }
            }

            return baseResult;
        }
    }
}
