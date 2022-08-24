using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Skills
{
    public class BKSkills : DefaultTypeInitializer<BKSkills, SkillObject>
    {
        public override IEnumerable<SkillObject> All => Game.Current.ObjectManager.GetObjectTypeList<SkillObject>();

        public SkillObject Scholarship { get; private set; }

        public SkillObject Theology { get; private set; }

        public SkillObject Lordship { get; private set; }

        public static BKSkills Instance => ConfigHolder.CONFIG;

        public override void Initialize()
        {
            Scholarship = Game.Current.ObjectManager.RegisterPresumedObject(new SkillObject("Scholarship"));
            Scholarship.Initialize(new TextObject("{=vfp8kzB4k}Scholarship"),
                    new TextObject("{=AK6XaECvO}Reading and writting competence as well as knowledge over literary and legal matters."),
                    SkillObject.SkillTypeEnum.Personal)
                .SetAttribute(BKAttributes.Instance.Wisdom);

            Theology = Game.Current.ObjectManager.RegisterPresumedObject(new SkillObject("Theology"));
            Theology.Initialize(new TextObject("{=5XOq4J0wh}Theology"),
                    new TextObject("{=pqAxqa0Jt}Understanding over spiritual matters. Normally reserved for preachers and the most pious faithful."),
                    SkillObject.SkillTypeEnum.Personal)
                .SetAttribute(BKAttributes.Instance.Wisdom);

            Lordship = Game.Current.ObjectManager.RegisterPresumedObject(new SkillObject("Lordship"));
            Lordship.Initialize(new TextObject("{=fM8vJDS0J}Lordship"),
                    new TextObject("{=XmwkJiRZD}Ability to deal with legal administration of titles and feudal contracts."),
                    SkillObject.SkillTypeEnum.Personal)
                .SetAttribute(BKAttributes.Instance.Wisdom);
        }

        private struct ConfigHolder
        {
            public static BKSkills CONFIG = new();
        }
    }
}