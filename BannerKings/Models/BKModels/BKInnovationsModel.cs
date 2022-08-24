using BannerKings.Managers;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKInnovationsModel
    {
        public ExplainedNumber CalculateSettlementResearch(Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data == null)
            {
                return result;
            }

            var nobles = data.GetTypeCount(PopulationManager.PopType.Nobles);
            var craftsmen = data.GetTypeCount(PopulationManager.PopType.Craftsmen);

            if (nobles > 0)
            {
                result.Add(nobles / 100000f, new TextObject("{=UUtWsyMb}Nobles"));
            }

            if (craftsmen > 0)
            {
                result.Add(craftsmen / 150000f, new TextObject("{=iC5RFbuT}Craftsmen"));
            }

            if (settlement.Owner.GetPerkValue(BKPerks.Instance.ScholarshipPeerReview))
            {
                result.AddFactor(0.2f, BKPerks.Instance.ScholarshipPeerReview.Name);
            }

            return result;
        }
    }
}