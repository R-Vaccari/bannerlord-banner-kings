using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Groups;

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
