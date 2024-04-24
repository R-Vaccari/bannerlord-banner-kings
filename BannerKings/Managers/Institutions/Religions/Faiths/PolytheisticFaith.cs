using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Doctrines.Marriage;
using BannerKings.Managers.Institutions.Religions.Doctrines.War;
using BannerKings.Managers.Institutions.Religions.Faiths.Groups;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using static BannerKings.Behaviours.Feasts.Feast;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class PolytheisticFaith : Faith
    {
        public void Initialize(Divinity mainGod, 
            List<Divinity> pantheon, 
            Dictionary<TraitObject, bool> traits, 
            FaithGroup faithGroup,
            List<Doctrine> doctrines,
            MarriageDoctrine marriageDoctrine,
            WarDoctrine warDoctrine,
            List<Rite> rites = null, 
            FeastType feastType = FeastType.None)
        {
            Initialize(mainGod, traits, faithGroup, doctrines, marriageDoctrine, warDoctrine, rites, feastType);
            this.pantheon = pantheon;
        }
    }
}