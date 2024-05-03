using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths.Societies;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class ReligionModel
    {
        public abstract ExplainedNumber GetJoinSocietyCost(Hero hero, Society society, bool descriptions = false);
        public abstract ExplainedNumber GetAppointInfluence(Hero appointer, ReligionData data, bool descriptions = false);
        public abstract ExplainedNumber GetAppointCost(Hero appointer, ReligionData data, bool descriptions = false);
        public abstract ExplainedNumber GetRemoveInfluence(Hero appointer, Hero removed, ReligionData data, bool descriptions = false);
        public abstract ExplainedNumber GetRemoveCost(Hero appointer, Hero removed, ReligionData data, bool descriptions = false);
        public abstract ExplainedNumber GetRemoveLoyaltyCost(Hero appointer, Hero removed, ReligionData data, bool descriptions = false);
        public abstract ExplainedNumber GetConversionLikelihood(Hero converter, Hero converted);
        public abstract ExplainedNumber GetConversionInfluenceCost(Hero notable, Hero converter);
        public abstract ExplainedNumber GetConversionPietyCost(Hero converted, Hero converter, Religion religion);
        public abstract ExplainedNumber CalculateTensionTarget(ReligionData data);
        public abstract ExplainedNumber CalculateFervor(Religion religion);
        public abstract ExplainedNumber CalculateReligionWeight(Religion religion, Settlement settlement);
        public abstract float GetNotableFactor(Hero notable, Settlement settlement);
        public abstract ExplainedNumber CalculatePietyChange(Hero hero, bool descriptions = false);
        public abstract ExplainedNumber CreateFaithLeaderCost(Religion religion, Hero creator, Hero leader, bool descriptions = false);
    }
}
