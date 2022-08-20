using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Skills;

public class BKSkills
{
    public SkillObject Scholarship { get; private set; }

    public SkillObject Theology { get; private set; }

    public SkillObject Lordship { get; private set; }

    public static BKSkills Instance => ConfigHolder.CONFIG;

    public void Initialize()
    {
        Scholarship = Game.Current.ObjectManager.RegisterPresumedObject(new SkillObject("Scholarship"));
        Scholarship.Initialize(new TextObject("{=!}Scholarship"),
                new TextObject(
                    "{=!}Reading and writting competence as well as knowledge over literary and legal matters."),
                SkillObject.SkillTypeEnum.Personal)
            .SetAttribute(BKAttributes.Instance.Wisdom);

        Theology = Game.Current.ObjectManager.RegisterPresumedObject(new SkillObject("Theology"));
        Theology.Initialize(new TextObject("{=!}Theology"),
                new TextObject(
                    "{=!}Understanding over spiritual matters. Normally reserved for preachers and the most pious faithful."),
                SkillObject.SkillTypeEnum.Personal)
            .SetAttribute(BKAttributes.Instance.Wisdom);

        Lordship = Game.Current.ObjectManager.RegisterPresumedObject(new SkillObject("Lordship"));
        Lordship.Initialize(new TextObject("{=!}Lordship"),
                new TextObject("{=!}Ability to deal with legal administration of titles and feudal contracts."),
                SkillObject.SkillTypeEnum.Personal)
            .SetAttribute(BKAttributes.Instance.Wisdom);
    }

    internal struct ConfigHolder
    {
        public static BKSkills CONFIG = new();
    }
}