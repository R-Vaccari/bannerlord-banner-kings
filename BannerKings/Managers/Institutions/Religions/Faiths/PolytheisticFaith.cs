using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class PolytheisticFaith : Faith
    {
        protected List<Divinity> pantheon;

        public void Initialize(Divinity mainGod, List<Divinity> pantheon, Dictionary<TraitObject, bool> traits,
          FaithGroup faithGroup, Dictionary<int, CharacterObject> presets)
        {
            base.Initialize(mainGod, traits, faithGroup, presets);
            this.pantheon = pantheon;
        }
    }
}
