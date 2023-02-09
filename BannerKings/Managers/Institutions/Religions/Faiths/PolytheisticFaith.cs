using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using static BannerKings.Behaviours.Feasts.Feast;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class PolytheisticFaith : Faith
    {
        public void Initialize(Divinity mainGod, List<Divinity> pantheon, Dictionary<TraitObject, bool> traits, 
            FaithGroup faithGroup, List<Rite> rites = null, FeastType feastType = FeastType.None)
        {
            base.Initialize(mainGod, traits, faithGroup, rites);
            this.pantheon = pantheon;
        }
    }
}