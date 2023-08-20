using System.Collections.Generic;

namespace BannerKings.Managers.Cultures
{
    public class DefaultTitleNames : DefaultTypeInitializer<DefaultTitleNames, CulturalTitleName>
    {
        public override IEnumerable<CulturalTitleName> All
        {
            get
            {
                foreach (var item in ModAdditions)
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
