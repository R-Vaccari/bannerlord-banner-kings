using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models
{
    public interface IBannerKingsModel
    {
        public abstract ExplainedNumber CalculateEffect(Settlement settlement);
    }
}