using System.Collections.Generic;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DefaultDivinities : DefaultTypeInitializer<DefaultDivinities, Divinity>
    {
        public override IEnumerable<Divinity> All
        {
            get
            {
                foreach (Divinity item in ModAdditions)
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