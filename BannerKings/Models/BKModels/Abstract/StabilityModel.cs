using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class StabilityModel
    {
        public abstract ExplainedNumber CalculateStabilityTarget(Settlement settlement, bool descriptions = false);
        public abstract ExplainedNumber CalculateAutonomyEffect(Settlement settlement, float stability, float autonomy);
        public abstract ExplainedNumber CalculateDemesneLimit(Hero hero, bool descriptions = false);
        public abstract ExplainedNumber CalculateVassalLimit(Hero hero);
    }
}
