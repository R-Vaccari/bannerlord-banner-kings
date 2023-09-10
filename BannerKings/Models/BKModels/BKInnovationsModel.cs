using BannerKings.Managers;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKInnovationsModel
    {
        public ExplainedNumber CalculateSettlementResearch(Settlement settlement, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
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
                if (data.TitleData != null && data.TitleData.Title != null)
                {
                    bool boost = data.TitleData.Title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.NoblesLaxDuties);
                    result.Add(nobles / (boost ? 7000f : 9000f), new TextObject("{=pJAF5pzO}Nobles"));
                }
            }

            if (craftsmen > 0)
            {
                result.Add(craftsmen / 15000f, new TextObject("{=d0YJZ6Z1}Craftsmen"));
            }

            result.AddFactor(data.Stability - 0.75f, new TextObject("{=!}Stability"));

            if (settlement.Owner != null)
            {
                if (settlement.Owner.GetPerkValue(BKPerks.Instance.ScholarshipPeerReview))
                {
                    result.AddFactor(0.2f, BKPerks.Instance.ScholarshipPeerReview.Name);
                }

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.Owner,
                    DefaultCouncilPositions.Instance.Philosopher,
                    DefaultCouncilTasks.Instance.EncourageMilitarism,
                    0.05f, true);
            }

            return result;
        }

        public ExplainedNumber CalculateCultureResearch(CultureObject culture, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);

            float castles = 0f;
            float villages = 0f;
            float towns = 0f;
            int castleCount = 0;
            int townCount = 0;
            int villageCount = 0;
            foreach (var settlement in Settlement.All)
            {
                if (settlement.Culture != culture)
                {
                    continue;
                }

                float research = CalculateSettlementResearch(settlement).ResultNumber;
                if (settlement.IsTown)
                {
                    towns += research;
                    townCount++;
                }
                else if (settlement.IsCastle) 
                {
                    castles += research;
                    castleCount++;
                } 
                else if (settlement.IsVillage)
                {
                    villages += research;
                    villageCount++;
                }
            }

            result.Add(towns, new TextObject("{=!}Towns (x{COUNT})")
                .SetTextVariable("COUNT", townCount));

            result.Add(castles, new TextObject("{=!}Castles (x{COUNT})")
                .SetTextVariable("COUNT", castleCount));

            result.Add(villages, new TextObject("{=!}Villages (x{COUNT})")
                .SetTextVariable("COUNT", villageCount));

            return result;
        }
    }
}