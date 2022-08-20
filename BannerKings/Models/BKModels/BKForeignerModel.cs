using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.Populations
{
    public class BKForeignerModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            float baseValue = settlement.IsTown ? 0.06f : (settlement.IsCastle ? 0.03f : 0.01f);
            result.Add(baseValue, new TextObject("Base"));

            if (settlement.IsTown)
            {
                float prosp = settlement.Town.Prosperity;
                result.Add((prosp - 5000f) / 75000f, new TextObject("Prosperity"));
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_foreigner_ban"))
                return new ExplainedNumber(0f);

            return result;
        }
    }
}
