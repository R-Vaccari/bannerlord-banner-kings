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
            Druidism = new Doctrine("druidism", new TextObject("{=CSeHfjWPB}Druidism"),
                new TextObject("{=o8rLQoJ9Y}Clergy is considered part of the Druid caste, who represent the spiritual power, but are also involved in political, material affairs. In a way, druids are a form of lesser nobility and cannot be excluded from political affairs."),
                new TextObject("{=ixRwH6oWx}Preachers provide noble troops\nNo religious council advisor causes daily influence loss"),
                new List<string>());

            Animism = new Doctrine("animism", new TextObject("{=1p093bUJQ}Animism"),
                new TextObject("{=wMgfuetCh}Spirits inhabit everywhere in this world, hidden in plain sight. Under the earth, flowing along rivers or bound to animals or trees, spirits can be anywhere. It is the duty of the faithful to not harm the balance of the material world with the spiritual world, which are often one and the same."),
                new TextObject("{=BR3CoGmyJ}Woodland acreage provides piety\nReduced baseline fervor"),
                new List<string>());

            CommunalFaith = new Doctrine("communal_faith", new TextObject("{=VddeOt4pT}Communal Faith"),
                new TextObject("{=!}"),
                new TextObject("{=!}"),
                new List<string>());

            Legalism = new Doctrine("legalism", new TextObject("{=mMWr5tKYr}Legalism"),
                new TextObject("{=f1sWPrJ1e}Without laws, man is but beast. The wisdom of previous generations is preserved through law, which must be followed to the letter."),
                new TextObject("{=aLx7eMG0o}Heathens can not fill council positions\n+1 to vassal limit for each personal virtue"),
                new List<string>());

            HeathenTax = new Doctrine("heathen_tax", new TextObject("{=yUCu5vXUz}Heathen Taxation"),
                new TextObject("{=KvogdoWdQ}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=JdOXyqOz8}Heathen population yields extra tax\nReduced militarism in predominantly heathen settlements"),
                new List<string>());

            Pastoralism = new Doctrine("pastorialism", new TextObject("{=AgBBtgdCP}Pastorialism"),
                new TextObject("{=KvogdoWdQ}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=LfTz52mRm}Pasture and farmland acreage are more productive\nReduced militia and drafting efficiency"),
                new List<string>());

            Childbirth = new Doctrine("childbirth", new TextObject("{=XqTmtbVzu}Honored Childbirth"),
                new TextObject("{=QC52oAgTh}Birth of children and the spread of the family are seen as a blessing. The more children we bear, more will defend and honor our ways of life in the future."),
                new TextObject("{=AsQ6yT9cx}Clan renown is increased every time a child is born\nIncreased fertility"),
                new List<string>());

            Pacifism = new Doctrine("pacifism", new TextObject("{=ZzGrbtRCX}Pacifism"),
                new TextObject("{=KwqT4XvyV}Peace is our most valued treasure. Unlike warmongering beasts, we cherish thriving through cooperation and hard work."),
                new TextObject("{=MZBvLEOP3}Increased stability in settlements\nPiety maluses while participating in wars and declaring wars"),
                new List<string>());

            Literalism = new Doctrine("literalism", new TextObject("{=XzAMNth0M}Literalism"),
                new TextObject("{=dwE3MmHWe}It is only through our holy texts that we can uphold our values and faith."),
                new TextObject("{=mHasUXZ87}High scholarship provides piety, illiteracy reduces piety"),
                new List<string>());

            Sacrifice = new Doctrine("sacrifice", new TextObject("{=Bbm3RaVQ0}Human Sacrifices"),
                new TextObject("{=5Ukux68jE}Worthy opponents are deserving of a better death than commoners in the battlefield. We can prove our devotion by ritually sacrificing them."),
                new TextObject("{=y38zm5mA5}Allows the Human Sacrifice rite"),
                new List<string>());
        }
    }
}