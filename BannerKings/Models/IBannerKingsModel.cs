using TaleWorlds.CampaignSystem;

namespace BannerKings.Models
{
    public interface IBannerKingsModel
    {
        public abstract ExplainedNumber CalculateEffect(Settlement settlement);
    }
}
