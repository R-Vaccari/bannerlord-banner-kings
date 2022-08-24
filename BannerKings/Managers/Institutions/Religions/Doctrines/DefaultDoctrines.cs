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
            Druidism = new Doctrine("druidism", new TextObject("{=NSnuOpbY3}Druidism"),
                new TextObject("{=Rp9nZVHRW}Clergy is considered part of the Druid caste, who represent the spiritual power, but are also involved in political, material affairs. In a way, druids are a form of lesser nobility and cannot be excluded from political affairs."),
                new TextObject("{=rZOWSThsK}Preachers provide noble troops\nNo religious council advisor causes daily influence loss"),
                new List<string>());

            Animism = new Doctrine("animism", new TextObject("{=pKcJT64Lc}Animism"),
                new TextObject("{=M3Osir9hf}Spirits inhabit everywhere in this world, hidden in plain sight. Under the earth, flowing along rivers or bound to animals or trees, spirits can be anywhere. It is the duty of the faithful to not harm the balance of the material world with the spiritual world, which are often one and the same."),
                new TextObject("{=PQWZO9SkB}Woodland acreage provides piety\nReduced baseline fervor"),
                new List<string>());

            CommunalFaith = new Doctrine("communal_faith", new TextObject("{=uE03id5KX}Communal Faith"),
                new TextObject("{=!}"),
                new TextObject("{=!}"),
                new List<string>());

            Legalism = new Doctrine("legalism", new TextObject("{=TpqK43z2y}Legalism"),
                new TextObject("{=g1m7a3qqM}Without laws, man is but beast. The wisdom of previous generations is preserved through law, which must be followed to the letter."),
                new TextObject("{=Pk8xB1t3O}Heathens can not fill council positions\n+1 to vassal limit for each personal virtue"),
                new List<string>());

            HeathenTax = new Doctrine("heathen_tax", new TextObject("{=BwvAebi6a}Heathen Taxation"),
                new TextObject("{=4EFCM8pT9}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=4AJUqWk2N}Heathen population yields extra tax\nReduced militarism in predominantly heathen settlements"),
                new List<string>());

            Pastoralism = new Doctrine("pastorialism", new TextObject("{=OEBXJRLBP}Pastorialism"),
                new TextObject("{=4EFCM8pT9}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=XKYNwE0A7}Pasture and farmland acreage are more productive\nReduced militia and drafting efficiency"),
                new List<string>());

            Childbirth = new Doctrine("childbirth", new TextObject("{=QuDORnnmm}Honored Childbirth"),
                new TextObject("{=LibqeU49r}Birth of children and the spread of the family are seen as a blessing. The more children we bear, more will defend and honor our ways of life in the future."),
                new TextObject("{=C5JnBqbQi}Clan renown is increased every time a child is born\nIncreased fertility"),
                new List<string>());

            Pacifism = new Doctrine("pacifism", new TextObject("{=AuiqXSmVR}Pacifism"),
                new TextObject("{=mMRu0rUh3}Peace is our most valued treasure. Unlike warmongering beasts, we cherish thriving through cooperation and hard work."),
                new TextObject("{=p6yvBgQ49}Increased stability in settlements\nPiety maluses while participating in wars and declaring wars"),
                new List<string>());

            Literalism = new Doctrine("literalism", new TextObject("{=9r5DBX0nb}Literalism"),
                new TextObject("{=F6cgDymPR}It is only through our holy texts that we can uphold our values and faith."),
                new TextObject("{=k9tf9EGFa}High scholarship provides piety, illiteracy reduces piety"),
                new List<string>());

            Sacrifice = new Doctrine("sacrifice", new TextObject("{=BSuN1VDOC}Human Sacrifices"),
                new TextObject("{=QFrdwLdUm}Worthy opponents are deserving of a better death than commoners in the battlefield. We can prove our devotion by ritually sacrificing them."),
                new TextObject("{=0jXB9RUVm}Allows the Human Sacrifice rite"),
                new List<string>());
        }
    }
}