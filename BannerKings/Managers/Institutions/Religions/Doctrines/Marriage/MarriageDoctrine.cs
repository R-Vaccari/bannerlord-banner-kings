using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines.Marriage
{
    public class MarriageDoctrine : Doctrine
    {
        public MarriageDoctrine(string id, TextObject name, TextObject description, TextObject effects, 
            List<Doctrine> incompatibleDoctrines, 
            int concubines, 
            int consanguinity,
            bool acceptsUntolerated,
            bool isConcubinage = false) : 
            base(id, name, description, effects, incompatibleDoctrines, false)
        {
            Consorts = concubines;
            Consanguinity = consanguinity;
            AcceptsUntolerated = acceptsUntolerated;
            IsConcubinage = isConcubinage;
        }

        public int Consorts { get; private set; }
        public int Consanguinity { get; private set; }
        public bool AcceptsUntolerated { get; private set; }
        public bool IsConcubinage { get; private set; }

        public TextObject ConsortsExplanation
        {
            get
            {
                if (Consorts == 0) return new TextObject("{=!}Additional spouses or concubines/consorts are not tolerated by the faith.");
                else if (IsConcubinage) return new TextObject("{=!}Up to {COUNT} concubines/consorts are allowed in addition to your primary spouse. Concubines can be forced into concubinage and do not yield diplomatic bindings.")
                    .SetTextVariable("COUNT", Consorts);
                return new TextObject("{=!}Up to {COUNT} secondary spouses are allowed in addition to your primary spouse. Spouses may not be forcefully married and allow for diplomatic bindings such as alliances.")
                    .SetTextVariable("COUNT", Consorts);
            }
        }

        public TextObject ConsanguinityExplanation
        {
            get
            {
                if (Consanguinity == 0) return new TextObject("{=!}Spouses are allowed to have any level of consanguinity.");
                return new TextObject("{=!}Spouses are not allowed to have any blood relatives up to {COUNT} generation(s).")
                    .SetTextVariable("COUNT", Consanguinity);
            }
        }

        public TextObject UntoleratedExplanation
        {
            get
            {
                if (AcceptsUntolerated) return new TextObject("{=!}Spouses may be of faith groups considered untolerated, but not hostile.");
                return new TextObject("{=!}Spouse faiths must be of the same faith group, but not necessarily the same faith. Untolerated or hostile faiths are disallowed.");
            }
        }
    }
}
