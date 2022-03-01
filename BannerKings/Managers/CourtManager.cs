using BannerKings.Managers.Court;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers
{
    public class CourtManager
    {
        private Dictionary<Hero, Council> COUNCILS { get; set; }

        public CourtManager(Dictionary<Hero, Council> councils)
        {
            this.COUNCILS = councils;
        }

        public Council GetCouncil(Hero hero)
        {
            if (this.COUNCILS.ContainsKey(hero))
                return this.COUNCILS[hero];
            else
            {
                Council council = new Council(hero);
                this.COUNCILS.Add(hero, council);
                return council;
            }
        }
    }
}
