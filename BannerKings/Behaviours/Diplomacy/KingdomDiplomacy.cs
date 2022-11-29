using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Diplomacy
{
    public class KingdomDiplomacy
    {
        public Kingdom Kingdom { get; }
        public List<Kingdom> TradePacts { get; private set; }
    }
}
