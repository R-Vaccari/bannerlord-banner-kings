using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class PolytheisticFaith : Faith
    {
        protected List<Divinity> pantheon;

        public PolytheisticFaith() : base()
        {

        }

        public void Initialize(Divinity mainGod, List<Divinity> pantheon, Dictionary<TraitObject, bool> traits,
          FaithGroup faithGroup)
        {
            base.Initialize(mainGod, traits, faithGroup);
            this.pantheon = pantheon;
        }
    }
}
