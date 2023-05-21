using System.Collections.Generic;

namespace BannerKings.Behaviours.Events
{
    public class DefaultBannerKingsEvents : DefaultTypeInitializer<DefaultBannerKingsEvents, BannerKingsEvent>
    {
        public override IEnumerable<BannerKingsEvent> All
        {
            get
            {
                yield break;
            }
        }

        public override void Initialize()
        {
        }
    }
}
