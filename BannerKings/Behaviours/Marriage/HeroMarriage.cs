using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Marriage
{
    public class HeroMarriage
    {

        public HeroMarriage()
        {
            Partners = new List<Hero>();
        }

        [SaveableProperty(1)] public Hero PrimarySpouse { get; set; }
        [SaveableProperty(2)] public List<Hero> Partners { get; private set; }
    }
}
