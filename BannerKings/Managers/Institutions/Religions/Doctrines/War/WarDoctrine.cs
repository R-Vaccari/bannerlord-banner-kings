using BannerKings.Behaviours.Diplomacy.Wars;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines.War
{
    public class WarDoctrine : Doctrine
    {
        public WarDoctrine(string id, 
            TextObject name, 
            TextObject description, 
            TextObject effects, 
            List<Doctrine> incompatibleDoctrines,
            Dictionary<CasusBelli, int> justifications,
            bool permanent = false) : base(id, name, description, effects, incompatibleDoctrines, permanent)
        {
            Justifications = justifications;
        }

        public Dictionary<CasusBelli, int> Justifications { get; private set; }
        public bool AcceptsJustification(CasusBelli belli) => Justifications.ContainsKey(belli);
        public bool HeroHasPiety(Hero hero, CasusBelli belli)
        {
            if (AcceptsJustification(belli))
            {
                float piety = BannerKingsConfig.Instance.ReligionsManager.GetPiety(hero);
                return piety >= Justifications[belli];
            }

            return false;
        }

        public int GetPietyCost(CasusBelli cb) => AcceptsJustification(cb) ? Justifications[cb] : 0;
    }
}
