using System.Collections.Generic;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Societies
{
    public class DefaultSocieties : DefaultTypeInitializer<DefaultSocieties, Society>
    {
        public override IEnumerable<Society> All
        {
            get
            {
                foreach (Society item in ModAdditions)
                    yield return item;
            }
        }

        public override void Initialize()
        {
        }
    }
}
