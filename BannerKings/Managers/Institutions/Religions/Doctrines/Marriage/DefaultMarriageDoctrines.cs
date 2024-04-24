using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines.Marriage
{
    public class DefaultMarriageDoctrines : DefaultTypeInitializer<DefaultMarriageDoctrines, MarriageDoctrine>
    {
        public MarriageDoctrine Monogamy { get; set; }
        public MarriageDoctrine AvunculateMonogamy { get; set; }
        public MarriageDoctrine Concubinage { get; set; }
        public MarriageDoctrine OpenConcubinage { get; set; }
        public MarriageDoctrine AvunculateConcubinage { get; set; }
        public MarriageDoctrine Polygamy { get; set; }
        public MarriageDoctrine AvunculatePolygamy { get; set; }
        public override IEnumerable<MarriageDoctrine> All
        {
            get
            {
                yield return Monogamy;
                yield return Concubinage;
                yield return Polygamy;
                yield return OpenConcubinage;
                yield return AvunculateConcubinage;
                yield return AvunculatePolygamy;
                yield return AvunculateMonogamy;
            }
        }

        public override void Initialize()
        {
            Monogamy = new MarriageDoctrine("Monogamy",
                new TextObject("{=!}Monogamy"),
                new TextObject("{=!}Monogamy is a marriage system in which two spouses, male and female, are equally bound to each other. The marriage is always diplomatically binding. It does not accept any close degree of blood relation."),
                new TextObject(),
                new List<Doctrine>(),
                0,
                2,
                true);

            AvunculateMonogamy = new MarriageDoctrine("AvunculateMonogamy",
                new TextObject("{=!}Monogamy"),
                new TextObject("{=!}Avunculate Monogamy is a marriage system in which two spouses, male and female, are equally bound to each other. The marriage is always diplomatically binding. Spouses may not be close relatives."),
                new TextObject(),
                new List<Doctrine>(),
                0,
                1,
                true);

            AvunculateConcubinage = new MarriageDoctrine("AvunculateConcubinage",
                new TextObject("{=!}Avunculate Concubinage"),
                new TextObject("{=!}Avunculate Concubinage doctrine allows for clan leaders to take up to 3 concubines/consorts. Concubines may be forced into concubinage when imprisoned. Concubines yield no diplomatic bindings such as alliances. Spouses may not be close relatives."),
                new TextObject(),
                new List<Doctrine>(),
                3,
                1,
                true,
                true);

            OpenConcubinage = new MarriageDoctrine("OpenConcubinage",
                new TextObject("{=!}Open Concubinage"),
                new TextObject("{=!}Open Concubinage doctrine allows for clan leaders to take up to 3 concubines/consorts. Concubines may be forced into concubinage when imprisoned. Concubines yield no diplomatic bindings such as alliances. There are no restrictions towards consanguinity: close family may marry."),
                new TextObject(),
                new List<Doctrine>(),
                2,
                0,
                true,
                true);

            Concubinage = new MarriageDoctrine("Concubinage",
                new TextObject("{=!}Concubinage"),
                new TextObject("{=!}Concubinage doctrine allows for clan leaders to take up to 3 concubines/consorts. Concubines may be forced into concubinage when imprisoned. Concubines yield no diplomatic bindings such as alliances. It does not accept any close degree of blood relation."),
                new TextObject(),
                new List<Doctrine>(),
                3,
                2,
                true,
                true);

            Polygamy = new MarriageDoctrine("Polygamy",
                new TextObject("{=!}Polygamy"),
                new TextObject("{=!}Polygamy is a marriage system in which a lead spouse (a clan leader) may have one primary spouse, as weall as 3 secondary spouses. Secondary spouses may not be forced into marriage, and their marriages are also diplomatically binding, alongside the primary spouse. It does not accept any close degree of blood relation."),
                new TextObject(),
                new List<Doctrine>(),
                3,
                2,
                true);

            AvunculatePolygamy = new MarriageDoctrine("AvunculatePolygamy",
                new TextObject("{=!}Polygamy"),
                new TextObject("{=!}Avunculate Polygamy is a marriage system in which a lead spouse (a clan leader) may have one primary spouse, as weall as 3 secondary spouses. Secondary spouses may not be forced into marriage, and their marriages are also diplomatically binding, alongside the primary spouse. "),
                new TextObject(),
                new List<Doctrine>(),
                3,
                1,
                true);
        }
    }
}
