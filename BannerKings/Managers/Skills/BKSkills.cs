using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Skills
{
    public class BKSkills
    {
        public void Initialize()
        {
            scholarship = Game.Current.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject("scholarship"));
            scholarship.Initialize(new TextObject("{=!}Scholarship", null), 
                new TextObject("{=!}Reading and writting competence as well as knowledge over literary and legal matters.", null), 
                SkillObject.SkillTypeEnum.Personal)
                .SetAttribute(BKAttributes.Instance.Wisdom);

            scholarship = Game.Current.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject("theology"));
            scholarship.Initialize(new TextObject("{=!}Theology", null),
                new TextObject("{=!}Understanding over spiritual matters. Normally reserved for preachers and the most pious faithful.", null),
                SkillObject.SkillTypeEnum.Personal)
                .SetAttribute(BKAttributes.Instance.Wisdom);

            scholarship = Game.Current.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject("lordship"));
            scholarship.Initialize(new TextObject("{=!}Lordship", null),
                new TextObject("{=!}Ability to deal with legal administration of titles and feudal contracts.", null),
                SkillObject.SkillTypeEnum.Personal)
                .SetAttribute(BKAttributes.Instance.Wisdom);
        }

        private SkillObject scholarship, lordship, theology;

        public SkillObject Scholarship => scholarship;
        public SkillObject Theology => theology;
        public SkillObject Lordship => lordship;

        public static BKSkills Instance => ConfigHolder.CONFIG;
        internal struct ConfigHolder
        {
            public static BKSkills CONFIG = new BKSkills();
        }
    }
}
