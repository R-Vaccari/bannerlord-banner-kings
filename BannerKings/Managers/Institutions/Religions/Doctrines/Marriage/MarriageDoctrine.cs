using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines.Marriage
{
    public class MarriageDoctrine : Doctrine
    {
        public MarriageDoctrine(string id, TextObject name, TextObject description, TextObject effects, 
            List<Doctrine> incompatibleDoctrines, int concubines, bool homosexual) : 
            base(id, name, description, effects, incompatibleDoctrines, false)
        {
        }
    }
}
