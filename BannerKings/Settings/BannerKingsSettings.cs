using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using TaleWorlds.Localization;
using MCM.Abstractions.Settings.Base.Global;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.Attributes.v1;

namespace BannerKings.Settings
{
    public class BannerKingsSettings : AttributeGlobalSettings<BannerKingsSettings>
    {
        private int volunteersLimit = 10;


        public override string Id => "BannerKings";
        public override string DisplayName => new TextObject("{=!}Banner Kings").ToString();




        [SettingProperty("Close Relatives Honorifics", RequireRestart = false, HintText = "{=!}Apply title honorifcs for close relatives of title holders. This only takes effect if the 'Title Honorifcs' option is any other than 'No Titles'. Different rules apply but in general, spouses of will have an equivalent, gendered title (ie, 'Queen FemaleName' the spouse of 'King MaleName'), while children of Kings and Emperors may be Princes.")]
        public bool CloseRelativesNaming { get; set; } = true;

        [SettingPropertyDropdown("{=!}Title Honorifics", Order = 5, RequireRestart = false, HintText = "{=!}How lords with titles are named. Full titles Suffixed (Default): 'Firstname, Prince of TitleName'; Full titles: 'Prince FirstName of TitleName';  Title Prefixes: 'Prince TitleName'.")]
        public DropdownDefault<SettingsOption> Naming { get; set; } = new DropdownDefault<SettingsOption>(new SettingsOption[]
        {
            DefaultSettings.Instance.NamingFullTitlesSuffixed,
            DefaultSettings.Instance.NamingFullTitles,
            DefaultSettings.Instance.NamingTitlePrefix,
            DefaultSettings.Instance.NamingNoTitles
        }, selectedIndex: 0);


        // Value is displayed as "X Denars"
        [SettingPropertyInteger("{=!}Volunteers Limit", 6, 20, "0 Volunteers",
            Order = 1, RequireRestart = false, HintText = "{=!}The number of volunteers that notables may have. Requires reloading. Vanilla is 6, default for BK is 10. The recruitable amount is calculated on percentages and thus is always balanced. Recruits will be lost when changing to a smaller limit. Limits can be changed at any point during campaigns.")]
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
