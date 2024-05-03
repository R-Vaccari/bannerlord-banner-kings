using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class InfluenceModel : DefaultClanPoliticsModel
    {
        public abstract ExplainedNumber CalculateInfluenceCap(Clan clan, bool includeDescriptions = false);
        public abstract ExplainedNumber GetBequeathPeerageCost(Kingdom kingdom, bool explanations = false);
        public abstract ExplainedNumber GetMinimumPeersQuantity(Kingdom kingdom, bool explanations = false);
        public abstract float GetRejectKnighthoodCost(Clan clan);
        public abstract ExplainedNumber CalculateSettlementInfluence(Settlement settlement, PopulationData data,
            bool includeDescriptions = false);
    }
}
