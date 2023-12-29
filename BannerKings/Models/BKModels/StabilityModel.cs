using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels
{
    public abstract class StabilityModel
    {
        public abstract ExplainedNumber CalculateStabilityTarget(Settlement settlement, bool descriptions = false);
        public abstract ExplainedNumber CalculateAutonomyEffect(Settlement settlement, float stability, float autonomy);
    }
}
