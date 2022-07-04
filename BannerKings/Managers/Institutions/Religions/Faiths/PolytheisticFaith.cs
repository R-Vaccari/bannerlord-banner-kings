using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class PolytheisticFaith : Faith
    {
        protected List<Divinity> pantheon;

        public PolytheisticFaith() : base()
        {

        }

        public void Initialize(Divinity mainGod, List<Divinity> pantheon, Dictionary<TraitObject, bool> traits,
          FaithGroup faithGroup, List<Rite> rites = null)
        {
            base.Initialize(mainGod, traits, faithGroup);
            this.pantheon = pantheon;
        }
    }
}
