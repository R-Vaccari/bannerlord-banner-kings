using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines
{
    public class DefaultDoctrines
    {

        public static DefaultDoctrines Instance => ConfigHolder.CONFIG;
        internal struct ConfigHolder
        {
            public static DefaultDoctrines CONFIG = new DefaultDoctrines();
        }

        private Doctrine druidism, animism, legalism, communalFaith, literalism, pastoralism, pacifism, heathenTax, childbirth, sacrifice;

        public Doctrine Druidism => druidism;
        public Doctrine Animism => animism;

        public void Initialize()
        {
            druidism = new Doctrine("druidism", new TextObject("{=!}Druidism"), 
                new TextObject("{=!}Clergy is considered part of the Druid caste, who represent the spiritual power, but are also involved in political, material affairs. In a way, druids are a form of lesser nobility and cannot be excluded from political affairs."), 
                new TextObject("{=!}Preachers provide noble troops\nNo religious council advisor causes daily influence loss"),
                new List<string>());

            animism = new Doctrine("animism", new TextObject("{=!}Animism"), 
                new TextObject("{=!}Spirits inhabit everywhere in this world, hidden in plain sight. Under the earth, flowing along rivers or bound to animals or trees, spirits can be anywhere. It is the duty of the faithful to not harm the balance of the material world with the spiritual world, which are often one and the same."),
                new TextObject("{=!}Woodland acreage provides piety\nReduced baseline fervor"),
                new List<string>());

            legalism = new Doctrine("legalism", new TextObject("{=!}Legalism"),
                new TextObject("{=!}Without laws, man is but beast. The wisdom of previous generations is preserved through law, which must be followed to the letter."),
                new TextObject("{=!}Heathens can not fill council positions\nHigh scholarship provides piety\n+1 to vassal limit for each personal virtue"),
                new List<string>());

            heathenTax = new Doctrine("heathen_tax", new TextObject("{=!}Heathen Taxation"),
                new TextObject("{=!}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=!}Heathen population yields extra tax\nReduced militarism in predominantly heathen settlements"),
                new List<string>());

            pastoralism = new Doctrine("pastorialism", new TextObject("{=!}Pastorialism"),
                new TextObject("{=!}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=!}Pasture and farmland acreage are more productive\nReduced militia and drafting efficiency"),
                new List<string>());

            childbirth = new Doctrine("childbirth", new TextObject("{=!}Honored Childbirth"),
                new TextObject("{=!}Birth of children and the spread of the family are seen as a blessing. The more children we bear, more will defend and honor our ways of life in the future."),
                new TextObject("{=!}Clan renown is increased everytime a child is born\nIncreased fertility"),
                new List<string>());

            pacifism = new Doctrine("pacifism", new TextObject("{=!}Pacifism"),
                new TextObject("{=!}Peace is our most valued treasure. Unlike warmongering beasts, we cherish thriving through cooperation and hard work."),
                new TextObject("{=!}Increased stability in settlements\nPiety maluses while participating in wars and declaring wars"),
                new List<string>());

            literalism = new Doctrine("literalism", new TextObject("{=!}Literalism"),
                new TextObject("{=!}It is only through our holy texts that we can uphold our values and faith."),
                new TextObject("{=!}High scholarship provides piety, illiteracy reduces piety"),
                new List<string>());

            sacrifice = new Doctrine("sacrifice", new TextObject("{=!}Human Sacrifices"),
                new TextObject("{=!}Worthy opponents are deserving of a better death than commoners in the battlefield. We can prove our devotion by ritually sacrificing them."),
                new TextObject("{=!}Allows the Human Sacrifice rite"),
                new List<string>());
        }

        public Doctrine GetById(string doctrineId)
        {
            if (doctrineId == "druidism")
                return druidism;
            else return animism;
        }
    }
}
