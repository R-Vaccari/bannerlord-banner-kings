using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Marriage
{
    public class HeroMarriage
    {

        public HeroMarriage(Hero hero)
        {
            Hero = hero;
            Concubines = new List<Hero>();
            Spouses = new List<Hero>(); 
        }

        public Hero Hero { get; private set; }
        public List<Hero> Concubines { get; private set; }
        public List<Hero> Spouses { get; private set; }
    }
}
