using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class WorkshopModel : DefaultWorkshopModel
    {
        public abstract ExplainedNumber GetProductionQuality(Workshop workshop, bool explanations = false);
        public abstract ExplainedNumber GetDailyExpense(Workshop workshop, bool includeDescriptions = false);
        public abstract ExplainedNumber GetProductionEfficiency(Workshop workshop, bool explanations = false);
        public abstract ExplainedNumber CalculateWorkshopTax(Settlement settlement, Hero payer);
        public abstract int GetInventoryCost(Workshop workshop);
        public abstract ExplainedNumber GetBuyingCostExplained(Workshop workshop, Hero buyer, bool descriptions = false);
        public abstract int GetUpgradeCost(Workshop workshop);
    }
}
