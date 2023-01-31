using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class MonotheisticFaith : Faith
    {
        public void Initialize(Divinity mainGod, List<Divinity> pantheon, Dictionary<TraitObject, bool> traits,
            FaithGroup faithGroup, List<ContextualRite> rites = null)
        {
            Initialize(mainGod, traits, faithGroup, rites);
            this.pantheon = pantheon;
        }
    }
}