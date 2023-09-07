using System.Collections.Generic;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class DefaultFaiths : DefaultTypeInitializer<DefaultFaiths, Faith>
    {
        public override IEnumerable<Faith> All
        {
            get
            {
                foreach (Faith item in ModAdditions)
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
