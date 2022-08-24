using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines
{
    public class DefaultDoctrines : DefaultTypeInitializer<DefaultDoctrines, Doctrine>
    {
        public Doctrine Druidism { get; private set; }

        public Doctrine Animism { get; private set; }

        public Doctrine Legalism { get; private set; }

        public Doctrine CommunalFaith { get; private set; }

        public Doctrine Literalism { get; private set; }

        public Doctrine Pastoralism { get; private set; }

        public Doctrine Pacifism { get; private set; }

        public Doctrine HeathenTax { get; private set; }

        public Doctrine Childbirth { get; private set; }

        public Doctrine Sacrifice { get; private set; }

        public override IEnumerable<Doctrine> All
        {
            get
            {
                yield return Druidism;
                yield return Animism;
                yield return Legalism;
                yield return CommunalFaith;
                yield return Literalism;
                yield return Pastoralism;
                yield return Pacifism;
                yield return HeathenTax;
                yield return Childbirth;
                yield return Sacrifice;
            }
        }

        public override void Initialize()
        {
            Druidism = new Doctrine("druidism", new TextObject("{=5nWO51SA}Druidism"),
                new TextObject("{=jGBY3HOW}Clergy is considered part of the Druid caste, who represent the spiritual power, but are also involved in political, material affairs. In a way, druids are a form of lesser nobility and cannot be excluded from political affairs."),
                new TextObject("{=f9FjtWfK}Preachers provide noble troops\nNo religious council advisor causes daily influence loss"),
                new List<string>());

            Animism = new Doctrine("animism", new TextObject("{=bFYdz2SF}Animism"),
                new TextObject("{=gEVbZNLR}Spirits inhabit everywhere in this world, hidden in plain sight. Under the earth, flowing along rivers or bound to animals or trees, spirits can be anywhere. It is the duty of the faithful to not harm the balance of the material world with the spiritual world, which are often one and the same."),
                new TextObject("{=HHWuN1Dr}Woodland acreage provides piety\nReduced baseline fervor"),
                new List<string>());

            CommunalFaith = new Doctrine("communal_faith", new TextObject("{=DNL1iDfQ}Communal Faith"),
                new TextObject("{=!}"),
                new TextObject("{=!}"),
                new List<string>());

            Legalism = new Doctrine("legalism", new TextObject("{=adw0cyLs}Legalism"),
                new TextObject("{=y62ywx7D}Without laws, man is but beast. The wisdom of previous generations is preserved through law, which must be followed to the letter."),
                new TextObject("{=ceMkfGDw}Heathens can not fill council positions\n+1 to vassal limit for each personal virtue"),
                new List<string>());

            HeathenTax = new Doctrine("heathen_tax", new TextObject("{=Rxanrbgd}Heathen Taxation"),
                new TextObject("{=jFA6bUyA}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=MbR3x9yR}Heathen population yields extra tax\nReduced militarism in predominantly heathen settlements"),
                new List<string>());

            Pastoralism = new Doctrine("pastorialism", new TextObject("{=QHZE14uU}Pastorialism"),
                new TextObject("{=jFA6bUyA}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=Z9jpYhBX}Pasture and farmland acreage are more productive\nReduced militia and drafting efficiency"),
                new List<string>());

            Childbirth = new Doctrine("childbirth", new TextObject("{=aZPrwBmH}Honored Childbirth"),
                new TextObject("{=qMZDzL9R}Birth of children and the spread of the family are seen as a blessing. The more children we bear, more will defend and honor our ways of life in the future."),
                new TextObject("{=9JXiCbmp}Clan renown is increased every time a child is born\nIncreased fertility"),
                new List<string>());

            Pacifism = new Doctrine("pacifism", new TextObject("{=VrhbvJmo}Pacifism"),
                new TextObject("{=a4cYvGfP}Peace is our most valued treasure. Unlike warmongering beasts, we cherish thriving through cooperation and hard work."),
                new TextObject("{=fRZpfqeO}Increased stability in settlements\nPiety maluses while participating in wars and declaring wars"),
                new List<string>());

            Literalism = new Doctrine("literalism", new TextObject("{=ASqPLcV8}Literalism"),
                new TextObject("{=xkWEdLWg}It is only through our holy texts that we can uphold our values and faith."),
                new TextObject("{=jvo4skg2}High scholarship provides piety, illiteracy reduces piety"),
                new List<string>());

            Sacrifice = new Doctrine("sacrifice", new TextObject("{=oHDvC6Kp}Human Sacrifices"),
                new TextObject("{=MTbddWd8}Worthy opponents are deserving of a better death than commoners in the battlefield. We can prove our devotion by ritually sacrificing them."),
                new TextObject("{=jemKvbvV}Allows the Human Sacrifice rite"),
                new List<string>());
        }
    }
}