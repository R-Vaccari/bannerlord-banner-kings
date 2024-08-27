using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Dynasties
{
    public class Dynasty : BannerKingsObject
    {
        public Dynasty(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, 
            TextObject description, 
            Hero founder = null,
            List<Clan> starterClans = null, 
            List<Legacy> starterLegacies = null)
        {
            Initialize(name, description);
            Founder = founder;
            if (starterClans == null) starterClans = new List<Clan>();
            Clans = starterClans;

            if (starterLegacies == null) starterLegacies = new List<Legacy>();
            Legacies = starterLegacies;
        }

        public bool HasLegacy(Legacy legacy) => Legacies.Any(x => x.StringId == legacy.StringId);

        public float Renown {  get; private set; }
        public Hero Founder { get; private set; }
        public Hero Head { get; private set; }
        public List<Clan> Clans { get; private set; }
        public List<Legacy> Legacies { get; private set; }
    }
}
