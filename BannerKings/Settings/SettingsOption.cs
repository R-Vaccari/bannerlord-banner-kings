using TaleWorlds.Localization;

namespace BannerKings.Settings
{
    public class SettingsOption : BannerKingsObject
    {

        public SettingsOption(string id, TextObject name) : base(id)
        {
            Initialize(name, null);
        }

        public override bool Equals(object obj)
        {
            if (obj is SettingsOption)
            {
                return StringId == (obj as SettingsOption).StringId;
            }
            return base.Equals(obj);
        }
        public override string ToString() => Name.ToString();
        
    }
}
