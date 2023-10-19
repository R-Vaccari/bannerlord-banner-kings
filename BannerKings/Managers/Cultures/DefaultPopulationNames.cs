using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Cultures
{
    public class DefaultPopulationNames : DefaultTypeInitializer<DefaultPopulationNames, CulturalPopulationName>
    {
        public CulturalPopulationName DefaultNobles { get; private set; }
        public CulturalPopulationName DefaultCraftsmen { get; private set; }
        public CulturalPopulationName DefaultTenants { get; private set; }
        public CulturalPopulationName DefaultSerfs { get; private set; }
        public CulturalPopulationName DefaultSlaves { get; private set; }

        public override IEnumerable<CulturalPopulationName> All
        {
            get
            {
                yield return DefaultNobles;
                yield return DefaultCraftsmen;
                yield return DefaultTenants;
                yield return DefaultSerfs;
                yield return DefaultSlaves;
                foreach (var item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public CulturalPopulationName GetPopulationName(CultureObject culture, PopType popType)
        {
            CulturalPopulationName name = All
                .FirstOrDefault(x => x.Culture != null && x.Culture.StringId == culture.StringId && popType == x.PopType);
            return name != null ? name : All.First(x => x.Culture == null && popType == x.PopType);
        }

        public override void Initialize()
        {
            DefaultNobles = CulturalPopulationName.CreateNobles("DefaultNobles",
                null,
                new TextObject("{=lj2vnf00}Nobles"));

            DefaultCraftsmen = CulturalPopulationName.CreateCraftsmen("DefaultCraftsmen",
                null,
                new TextObject("{=xLu9Wont}Craftsmen"));

            DefaultTenants = CulturalPopulationName.CreateTenants("DefaultTenants",
                null,
                new TextObject("{=EOgnTiuT}Tenants"));

            DefaultSerfs = CulturalPopulationName.CreateSerfs("DefaultSerfs",
                null,
                new TextObject("{=ZvgIyIA1}Serfs"));

            DefaultSlaves = CulturalPopulationName.CreateSlaves("DefaultSlaves",
                null,
                new TextObject("{=Cu5aCpzU}Slaves"));
        }
    }
}
