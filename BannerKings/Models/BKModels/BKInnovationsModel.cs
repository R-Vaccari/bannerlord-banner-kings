using BannerKings.Managers;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
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
                bool boost = data.TitleData.Title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.NoblesLaxDuties);
                result.Add(nobles / (boost ? 90000f : 100000f), new TextObject("{=pJAF5pzO}Nobles"));
            }

            if (craftsmen > 0)
            {
                result.Add(craftsmen / 150000f, new TextObject("{=d0YJZ6Z1}Craftsmen"));
            }

            if (settlement.Owner != null)
            {
                if (settlement.Owner.GetPerkValue(BKPerks.Instance.ScholarshipPeerReview))
                {
                    result.AddFactor(0.2f, BKPerks.Instance.ScholarshipPeerReview.Name);
                }

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.Owner,
                    Managers.Court.CouncilPosition.Philosopher, 0.05f, true);
            }

            return result;
        }
    }
}