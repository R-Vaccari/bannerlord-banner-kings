using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using BannerKings.Managers.Populations.Estates;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class GrowthModel
    {
        public abstract ExplainedNumber CalculateEffect(Settlement settlement, PopulationData data, bool descriptions = false);
        public abstract ExplainedNumber CalculateSettlementCap(Settlement settlement, PopulationData data, bool descriptions = false);
        public abstract ExplainedNumber CalculateEstateCap(Estate estate, bool descriptions = false);
        public abstract ExplainedNumber CalculatePopulationClassDemand(Settlement settlement, PopType type, bool explanations = false);
        public abstract ExplainedNumber CalculateSlavePrice(Settlement settlement, bool explanations = false);
    }
}
