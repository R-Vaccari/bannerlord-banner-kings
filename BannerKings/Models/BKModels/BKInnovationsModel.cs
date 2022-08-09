using BannerKings.Managers.Skills;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKInnovationsModel
    {
        public ExplainedNumber CalculateSettlementResearch(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data != null)
            {
                int nobles = data.GetTypeCount(Managers.PopulationManager.PopType.Nobles);
                int craftsmen = data.GetTypeCount(Managers.PopulationManager.PopType.Craftsmen);

                if (nobles > 0) result.Add(nobles / 100000f, new TextObject("{=!}Nobles"));
                if (craftsmen > 0) result.Add(craftsmen / 150000f, new TextObject("{=!}Craftsmen"));

                if (settlement.Owner.GetPerkValue(BKPerks.Instance.ScholarshipPeerReview))
                    result.AddFactor(0.2f, BKPerks.Instance.ScholarshipPeerReview.Name);
            }

            return result;
        }
    }
}
