using BannerKings.Behaviours.Criminality;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Schemes
{
    public class Secret : BannerKingsObject
    {
        public Secret(string stringId) : base(stringId)
        {
        }

        public Hero Hero { get; private set; }
        public bool Exposed { get; private set; }
        public List<Hero> DiscoveredBy { get; private set; }

        public Crime Crime { get; private set; }
    }
}
