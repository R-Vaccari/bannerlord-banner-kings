using TaleWorlds.Localization;

namespace BannerKings.Settings
{
    public class SettingsOption : BannerKingsObject
    {

        public SettingsOption(string id, TextObject name) : base(id)
        {
            Initialize(name, null);
        }

        public override string ToString() => Name.ToString();
        
    }
}
