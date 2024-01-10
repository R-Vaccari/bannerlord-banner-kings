using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Campaign.Economy.Markets
{
    public class MarketGroup : BannerKingsObject
    {
        public MarketGroup(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, CultureObject culture, Dictionary<CultureObject, float> spawns)
        {
            Initialize(name, description);
            Culture = culture;
            Spawns = spawns;
        }

        public CultureObject Culture { get; private set; }
        public Dictionary<CultureObject, float> Spawns { get; private set; }

        public float GetSpawn(CultureObject culture)
        {
            float result = 0f;
            if (Spawns.ContainsKey(culture)) result = Spawns[culture];

            return result;
        }
    }
}
