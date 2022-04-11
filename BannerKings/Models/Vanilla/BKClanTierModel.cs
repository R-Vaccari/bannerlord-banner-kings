using BannerKings.Managers.Court;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models.Vanilla
{
    public class BKClanTierModel : DefaultClanTierModel
    {

        public override int GetPartyLimitForTier(Clan clan, int clanTierToCheck)
        {
            int result = base.GetPartyLimitForTier(clan, clanTierToCheck);
            if (BannerKingsConfig.Instance.TitleManager != null && BannerKingsConfig.Instance.CourtManager != null)
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                if (title != null)
                    result += 5 - (int)title.type;
                
            }

            return result;
        }

        public override int GetCompanionLimit(Clan clan)
        {
            int result = base.GetCompanionLimit(clan);

            result += BannerKingsConfig.Instance.CourtManager.GetCouncilEffectInteger(clan.Leader, CouncilPosition.Chancellor, 4f);
            return result;
        }
    }
}
