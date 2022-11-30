using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy
{
    public class KingdomDiplomacy
    {
        public Kingdom Kingdom { get; }
        public List<Kingdom> TradePacts { get; private set; }

        public bool HasTradePact(Kingdom kingdom) => TradePacts.Contains(kingdom);

        public void DissolveTradePact(Kingdom kingdom, TextObject reason)
        {
            if (HasTradePact(kingdom))
            {
                TradePacts.Remove(kingdom);
                if (kingdom.MapFaction == Hero.MainHero.MapFaction)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}The trade pact with {KINGDOM} has ended. {REASON}")
                        .SetTextVariable("REASON", reason),
                        0,
                        null,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }
    }
}
