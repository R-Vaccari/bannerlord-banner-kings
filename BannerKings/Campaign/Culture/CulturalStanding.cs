using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Campaign.Culture
{
    public class CulturalStanding : BannerKingsObject
    {
        public CultureObject Culture { get; private set; }
        private Dictionary<CultureObject, int> Standings { get; set; }

        public CulturalStanding(string id, CultureObject culture) : base(id)
        { 
            Culture = culture;
            Standings = new Dictionary<CultureObject, int>(5);
        }

        public void AddStanding(CultureObject culture, int relation)
        {
            if (culture.StringId != Culture.StringId)
                Standings[culture] = relation;
        }
        
        public int GetStanding(CultureObject culture)
        {
            int result = 0;
            if (culture.StringId == Culture.StringId) result = 5;
            else
            {
                var standing = Standings.FirstOrDefault(x => x.Key.StringId == culture.StringId);
                if (standing.Key != null) result = standing.Value;
            }

            return result;
        }
    }
}
