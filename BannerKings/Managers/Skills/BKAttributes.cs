using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Skills
{
    public class BKAttributes
    {
        public void Initialize()
        {

            wisdom = Game.Current.ObjectManager.RegisterPresumedObject<CharacterAttribute>(new CharacterAttribute("wisdom"));
            wisdom.Initialize(new TextObject("{=!}Wisdom", null), 
                new TextObject("{=!}Wisdom represents your world knowledge an competence to deal with skills that require deep learning.", null),
                new TextObject("{=!}WIS", null));
        }

        public CharacterAttribute Wisdom => wisdom;

        private CharacterAttribute wisdom;

        public static BKAttributes Instance => ConfigHolder.CONFIG;
        internal struct ConfigHolder
        {
            public static BKAttributes CONFIG = new BKAttributes();
        }
    }
}
