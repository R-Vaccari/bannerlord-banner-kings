using TaleWorlds.CampaignSystem;

namespace BannerKings.Models
{
    internal interface IReligionModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Hero hero, bool descriptions = false);
    }
}