using System.Collections.Generic;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class DefaultFaithGroups : DefaultTypeInitializer<DefaultFaithGroups, FaithGroup>
    {
        public override IEnumerable<FaithGroup> All
        {
            get
            {
                foreach (FaithGroup item in ModAdditions)
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
