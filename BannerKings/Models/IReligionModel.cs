using TaleWorlds.CampaignSystem;

namespace BannerKings.Models
{
    interface IReligionModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Hero hero);
    }
}
