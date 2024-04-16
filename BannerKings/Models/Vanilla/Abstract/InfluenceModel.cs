using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class InfluenceModel : DefaultClanPoliticsModel
    {
        public abstract ExplainedNumber CalculateInfluenceCap(Clan clan, bool includeDescriptions = false);
    }
}
