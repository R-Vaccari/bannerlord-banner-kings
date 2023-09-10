using BannerKings.Managers.Institutions.Religions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.BKModels
{
    public interface IReligionModel
    {
        public ExplainedNumber GetAppointInfluence(Hero appointer, ReligionData data, bool descriptions = false);
        public ExplainedNumber GetAppointCost(Hero appointer, ReligionData data, bool descriptions = false);
        public ExplainedNumber GetRemoveInfluence(Hero appointer, Hero removed, ReligionData data, bool descriptions = false);
        public ExplainedNumber GetRemoveCost(Hero appointer, Hero removed, ReligionData data, bool descriptions = false);
        public ExplainedNumber GetRemoveLoyaltyCost(Hero appointer, Hero removed, ReligionData data, bool descriptions = false);
        public ExplainedNumber GetConversionLikelihood(Hero converter, Hero converted);
        public ExplainedNumber GetConversionInfluenceCost(Hero notable, Hero converter);
        public ExplainedNumber GetConversionPietyCost(Hero converted, Hero converter);
        public ExplainedNumber CalculateTensionTarget(ReligionData data);
        public ExplainedNumber CalculateFervor(Religion religion);
        public ExplainedNumber CalculateReligionWeight(Religion religion, Settlement settlement);
        public float GetNotableFactor(Hero notable, Settlement settlement);
        public ExplainedNumber CalculatePietyChange(Hero hero, bool descriptions = false);
    }
}
