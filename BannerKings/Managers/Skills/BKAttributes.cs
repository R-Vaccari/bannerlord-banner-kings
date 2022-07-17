using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Skills
{
    public class BKAttributes
    {
        public void Initialize()
        {

            wisdom = Game.Current.ObjectManager.RegisterPresumedObject(new CharacterAttribute("wisdom"));
            wisdom.Initialize(new TextObject("{=!}Wisdom", null), 
                new TextObject("{=!}Wisdom represents your world knowledge and competence to deal with skills that require deep learning.", null),
                new TextObject("{=!}WIS", null));
        }

        public static MBReadOnlyList<CharacterAttribute> AllAttributes 
        { 
            get
            {
                PropertyInfo allAttrs = Campaign.Current.GetType().GetProperty("AllCharacterAttributes", BindingFlags.Instance | BindingFlags.NonPublic);
                return (MBReadOnlyList<CharacterAttribute>)allAttrs.GetValue(Campaign.Current);
            }
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
