using BannerKings.Behaviours.Diplomacy.Wars;
using System.Collections.Generic;
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
    }
}
