using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Cultures
{
    public class CulturalPopulationName : BannerKingsObject
    {
        public CulturalPopulationName(string stringId) : base(stringId)
        {
        }

        public CultureObject Culture { get; private set; }
        public PopType PopType { get; private set; }

        public static CulturalPopulationName CreateNobles(string id, CultureObject culture, TextObject name, TextObject description = null)
        {
            CulturalPopulationName result = InitPopulation(id,
                culture, 
                name, 
                description != null ? description : new TextObject("{=NvDPFtJ3}The {CLASS} represent the free, wealthy and influential members of society. They pay very high taxes and increase your influence as a lord or lady"));
            result.PopType = PopType.Nobles;
            return result;
        }

        public static CulturalPopulationName CreateTenants(string id, CultureObject culture, TextObject name, TextObject description = null)
        {
            CulturalPopulationName result = InitPopulation(id,
                culture,
                name,
                description != null ? description : new TextObject("{=SY5K6vfd}The {CLASS} are a step above the serfs. These peasants are free to move and often have rights to protect themselves. Though less taxable than serfs, {CLASS} are more prone to stable and prosperous fiefs due to such rights and the ability to accumulate more wealth."));
            result.PopType = PopType.Tenants;
            return result;
        }

        public static CulturalPopulationName CreateCraftsmen(string id, CultureObject culture, TextObject name, TextObject description = null)
        {
            CulturalPopulationName result = InitPopulation(id,
                culture,
                name,
                description != null ? description : new TextObject("{=y1LusnZS}The {CLASS} are free people of trade, such as merchants, engineers and blacksmiths. Somewhat wealthy, free but not high status people. Craftsmen pay a significant amount of taxes and their presence boosts economical development. Their skills can also be hired to significantly boost construction projects."));
            result.PopType = PopType.Craftsmen;
            return result;
        }

        public static CulturalPopulationName CreateSerfs(string id, CultureObject culture, TextObject name, TextObject description = null)
        {
            CulturalPopulationName result = InitPopulation(id,
                culture,
                name,
                description != null ? description : new TextObject("{=QwugjxJo}The {CLASS} are the lowest class that possess some sort of freedom. Unable to attain specialized skills such as those of craftsmen, these people represent the agricultural workforce. They also pay tax over the profit of their production excess."));
            result.PopType = PopType.Serfs;
            return result;
        }

        public static CulturalPopulationName CreateSlaves(string id, CultureObject culture, TextObject name, TextObject description = null)
        {
            CulturalPopulationName result = InitPopulation(id,
                culture,
                name,
                description != null ? description : new TextObject("{=t6Ez6fZm}The {CLASS} are those destituted: criminals, prisioners unworthy of a ransom, and those unlucky to be born into slavery. Slaves do the hard manual labor across settlements, such as building and mining. They themselves pay no tax as they are unable to have posessions, but their labor generates income gathered as tax from their masters."));
            result.PopType = PopType.Slaves;
            return result;
        }

        private static CulturalPopulationName InitPopulation(string id, CultureObject culture, TextObject name, TextObject description)
        {
            CulturalPopulationName culturalName = new CulturalPopulationName(id);
            culturalName.Culture = culture;
            culturalName.Initialize(name, description.SetTextVariable("CLASS", name));
            return culturalName;
        }
    }
}
