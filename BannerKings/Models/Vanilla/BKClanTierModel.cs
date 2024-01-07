using BannerKings.Managers.Court.Members;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKClanTierModel : DefaultClanTierModel
    {
        public override int GetCompanionLimit(Clan clan)
        {
            var result = base.GetCompanionLimit(clan);

            result += BannerKingsConfig.Instance.CourtManager.GetCouncilEffectInteger(clan.Leader,
                DefaultCouncilPositions.Instance.Chancellor, 4f);
            return result;
        }

        public override int GetRequiredRenownForTier(int tier) => (int) (base.GetRequiredRenownForTier(tier) * BannerKingsSettings.Instance.ClanRenown);      
    }
}