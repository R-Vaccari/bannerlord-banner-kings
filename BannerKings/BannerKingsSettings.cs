using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using TaleWorlds.Localization;
using MCM.Abstractions.Settings.Base.Global;

namespace BannerKings
{
    public class BannerKingsSettings : AttributeGlobalSettings<BannerKingsSettings>
    {
        private int volunteersLimit = 12;
        public override string Id => "BannerKings";
        public override string DisplayName => new TextObject("{=!}Banner Kings").ToString();


        // Value is displayed as "X Denars"
        [SettingPropertyInteger("Volunteers Limit", 6, 20, "0 Volunteers",
            Order = 1, RequireRestart = true, HintText = "Setting explanation.")]
        [SettingPropertyGroup("{=!}Balancing")]
        public int VolunteersLimit
        {
            get => volunteersLimit;
            set
            {
                volunteersLimit = value;
                OnPropertyChanged();
            }
        }
    }
}
