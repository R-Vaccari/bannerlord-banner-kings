using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKForeignerModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            var result = new ExplainedNumber();
            var baseValue = settlement.IsTown ? 0.06f : settlement.IsCastle ? 0.03f : 0.01f;
            result.Add(baseValue, new TextObject("{=AaNeOd9n}Base"));

            if (settlement.IsTown)
            {
                var prosp = settlement.Town.Prosperity;
                result.Add((prosp - 5000f) / 75000f, new TextObject("{=mgK8aZuj}Prosperity"));
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_foreigner_ban"))
            {
                return new ExplainedNumber(0f);
            }

            return result;
        }
    }
}