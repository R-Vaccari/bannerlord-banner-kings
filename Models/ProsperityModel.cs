using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static Populations.PopulationManager;

namespace Populations.Models
{
    public class ProsperityModel : DefaultSettlementProsperityModel
    {
        public override ExplainedNumber CalculateHearthChange(Village village, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateHearthChange(village);
            if (IsSettlementPopulated(village.Settlement)) 
            { 
                PopulationData data = GetPopData(village.Settlement);
                int growthFactor = GetDataGrowthFactor(data);
                float hearths = MBRandom.RandomFloatRanged(growthFactor / 3, growthFactor / 6);
                data.UpdatePopulation(village.Settlement, (int)MBRandom.RandomFloatRanged(hearths * 3f, hearths * 6f), PopType.None);
                baseResult.Add(hearths, null);
            }

            return baseResult;
        }

        public override ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateProsperityChange(fortification, includeDescriptions);
            if (IsSettlementPopulated(fortification.Settlement))
            {
                PopulationData data = GetPopData(fortification.Settlement);
                int craftsmen = data.GetTypeCount(PopType.Craftsmen);
                baseResult.Add((float)craftsmen * 0.0005f, new TextObject("Craftsmen output"));
                int slaves = data.GetTypeCount(PopType.Slaves);
                baseResult.Add((float)slaves * -0.0001f, new TextObject("Slave population"));

                if (SlaveSurplusExists(fortification.Settlement, true))
                    baseResult.Add((float)slaves * -0.0003f, new TextObject("Slave surplus"));
                
                if (PolicyManager.IsPolicyEnacted(fortification.Settlement, PolicyManager.PolicyType.SELF_INVEST)) 
                {
                    ExplainedNumber income = new TaxModel().CalculateTownTax(fortification);
                    float tax = income.ResultNumber;
                    if (tax > 0)
                        baseResult.Add(tax * 0.0005f, new TextObject("Self-investment policy"));
                }
            }
            return baseResult;
        }
    }
}
