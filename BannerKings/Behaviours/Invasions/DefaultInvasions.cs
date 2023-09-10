using System.Collections.Generic;

namespace BannerKings.Behaviors.Invasions
{
    public class DefaultInvasions : DefaultTypeInitializer<DefaultInvasions, Invasion>
    {
        public override IEnumerable<Invasion> All
        {
            get
            {
                foreach (Invasion item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
        }
    }
}
