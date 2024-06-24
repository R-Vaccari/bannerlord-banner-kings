using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace BannerKings.CampaignContent.Economy.Markets
{
    public class DefaultMarketGroups : DefaultTypeInitializer<DefaultMarketGroups, MarketGroup>
    {
        public override IEnumerable<MarketGroup> All
        {
            get
            {
                foreach (var group in ModAdditions)
                {
                    yield return group;
                }
            }
        }

        public MarketGroup GetMarket(CultureObject culture) => All.FirstOrDefault(x => x.Culture.StringId == culture.StringId);

        public override void Initialize()
        {
        }
    }
}
