using System;
using System.Collections.Generic;
using System.Linq;
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
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                if (title != null)
                {
                    result += 5 - (int)title.type;
                }
            }

            return result;
        }
    }
}
