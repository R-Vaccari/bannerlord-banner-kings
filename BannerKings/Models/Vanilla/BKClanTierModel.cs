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

        public override int GetPartyLimitForTier(Clan clan, int clanTierToCheck)
        {
            var result = base.GetPartyLimitForTier(clan, clanTierToCheck);
            if (BannerKingsConfig.Instance.TitleManager != null && BannerKingsConfig.Instance.CourtManager != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                if (title != null)
                {
                    result += 5 - (int)title.TitleType;
                }
            }

            return result;
        }

        public override int CalculateTier(Clan clan)
        {
            int result = MinClanTier;
            for (int i = MinClanTier + 1; i <= MaxClanTier; i++)
            {
                if (clan.Renown >= GetRequiredRenownForTier(i))
                {
                    result = i;
                }
            }
            return result;
        }

        public override int GetRequiredRenownForTier(int tier) => (int) (base.GetRequiredRenownForTier(tier) * BannerKingsSettings.Instance.ClanRenown);      
    }
}