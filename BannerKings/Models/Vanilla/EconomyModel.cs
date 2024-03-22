using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public abstract class EconomyModel : DefaultSettlementEconomyModel
    {
        public abstract int GetSettlementMarketGoldLimit(Settlement settlememt);
        public abstract int GetNotableCaravanLimit(Hero notable);
        public abstract ExplainedNumber CalculateTradePower(Settlement settlement, bool descriptions = false);
        public abstract ExplainedNumber CalculateProductionQuality(Settlement settlement);
        public abstract ExplainedNumber CalculateMercantilism(Settlement settlement);
        public abstract ExplainedNumber CalculateProductionEfficiency(Settlement settlement, bool explanations = false, PopulationData data = null);
    }
}
