using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace Models
{
    public class ProsperityModel : DefaultSettlementProsperityModel
    {
        public override ExplainedNumber CalculateHearthChange(Village village, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateHearthChange(village, includeDescriptions);
            float result = baseResult.BaseNumber * 0.1f;
            float diff = result - baseResult.BaseNumber;
            baseResult.AddFactor(diff, new TaleWorlds.Localization.TextObject("Population effect."));

            return baseResult;
        }

        public override ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateProsperityChange(fortification, includeDescriptions);
            float result = baseResult.BaseNumber * 0.1f;
            float diff = result - baseResult.BaseNumber;
            baseResult.AddFactor(diff, new TaleWorlds.Localization.TextObject("Population effect."));

            return baseResult;
        }
    }
}
