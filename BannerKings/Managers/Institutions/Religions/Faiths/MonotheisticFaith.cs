using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class MonotheisticFaith : Faith
    {
        public void Initialize(Divinity mainGod, Dictionary<TraitObject, bool> traits,
          FaithGroup faithGroup, Dictionary<int, CharacterObject> presets)
        {
            base.Initialize(mainGod, traits, faithGroup, presets);
        }
    }
}
