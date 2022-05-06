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

        private Doctrine druidism, animism;

        public Doctrine Druidism => druidism;
        public Doctrine Animism => animism;

        public void Initialize()
        {
            druidism = new Doctrine("druidism", new TextObject("{=!}Druidism"), 
                new TextObject("{=!}Clergy is considered part of the Druid caste, who represent the spiritual power, but are also involved in political, material affairs. In a way, druids are a form of lesser nobility and cannot be excluded from political affairs."), 
                new TextObject("{=!}Druids will act as notables in settlements\nNo religious council advisor causes daily influence loss"),
                new List<string>());

            animism = new Doctrine("animism", new TextObject("{=!}Animism"), 
                new TextObject("{=!}Spirits inhabit everywhere in this world, hidden in plain sight. Under the earth, flowing along rivers or bound to animals or trees, spirits can be anywhere. It is the duty of the faithful to not harm the balance of the material world with the spiritual world, which are often one and the same."),
                new TextObject("{=!}Virtuous lords have increased battle morale"),
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
