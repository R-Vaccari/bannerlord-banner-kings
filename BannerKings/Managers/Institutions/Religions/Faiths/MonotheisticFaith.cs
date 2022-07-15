using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class MonotheisticFaith : Faith
    {
        public MonotheisticFaith() : base()
        {
            
        }

        public void Initialize(Divinity mainGod, List<Divinity> pantheon, Dictionary<TraitObject, bool> traits,
          FaithGroup faithGroup)
        {
            Initialize(mainGod, traits, faithGroup);
            this.pantheon = pantheon;
        }
    }
}
