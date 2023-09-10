using System.Collections.Generic;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DefaultReligions : DefaultTypeInitializer<DefaultReligions, Religion>
    {

        public override IEnumerable<Religion> All
        {
            get
            {
                foreach (Religion item in ModAdditions)
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
