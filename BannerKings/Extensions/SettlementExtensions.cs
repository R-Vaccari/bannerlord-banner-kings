using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Extensions
{
    public static class SettlementExtensions
    {
        public static PopulationData PopulationData(this Settlement settlement) => BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
    }
}

