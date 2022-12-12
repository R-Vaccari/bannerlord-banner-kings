using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using TaleWorlds.Localization;
using MCM.Abstractions.Attributes.v1;
using MCM.Common;
using MCM.Abstractions.Base.Global;
using TaleWorlds.SaveSystem;

namespace BannerKings.Settings
{
    public partial class BannerKingsSettings : AttributeGlobalSettings<BannerKingsSettings>
    {
        private int volunteersLimit = 10;
        public override string Id => "BannerKings";
        public override string DisplayName => new TextObject("Banner Kings").ToString();

        [SettingProperty("{=o394sPDk}Close Relatives Honorifics", RequireRestart = false, HintText = "{=AinXkzz7}Apply title honorifcs for close relatives of title holders. This only takes effect if the 'Title Honorifcs' option is any other than 'No Titles'. Different rules apply but in general, spouses of will have an equivalent, gendered title (ie, 'Queen FemaleName' the spouse of 'King MaleName'), while children of Kings and Emperors may be Princes.")]
        public bool CloseRelativesNaming { get; set; } = true;

        [SettingPropertyDropdown("{=2MqbeBCb}Title Honorifics", Order = 5, RequireRestart = false, HintText = "{=LuLE14V8}How lords with titles are named. Full titles Suffixed (Default): 'Firstname, Prince of TitleName'; Full titles: 'Prince FirstName of TitleName';  Title Prefixes: 'Prince TitleName'.")]
        public Dropdown<SettingsOption> Naming { get; set; } = new Dropdown<SettingsOption>(new SettingsOption[]
        {
            DefaultSettings.Instance.NamingFullTitlesSuffixed,
            DefaultSettings.Instance.NamingFullTitles,
            DefaultSettings.Instance.NamingTitlePrefix,
            DefaultSettings.Instance.NamingNoTitles
        }, selectedIndex: 0);


        [SettingProperty("{=0GVNKkGr}Village Tax Reserves", RequireRestart = false, HintText = "{=y9fQuceM}Leave a fifth of villages' production out of player income. This keeps villages in the player's income summary and so lets them always have an idea of how much villages and estates are producing. These can be manually collected through settlement menus. When disabled, all the income is removed and thus villages and estates have nothing to report most of the time, because their income depends on villagers bringing profit back from towns. Default: false.")]
        [SettingPropertyGroup("Economy")]
        public bool VillageTaxReserves { get; set; } = false;

        [SettingProperty("{=vWcskVBm}Realistic Caravan Income", RequireRestart = false, HintText = "{=1it7E8tF}Caravans pose a major risk factor not represented in the game: carrying your profits. With this setting, caravan profits will only be added when they enter a settlement owned by their owner, or where they are situated (ie, notables). Default: true.")]
        [SettingPropertyGroup("Economy")]
        public bool RealisticCaravanIncome { get; set; } = true;


        [SettingProperty("{=OohdenyR}Slower Parties", RequireRestart = false, HintText = "{=F8nfgxrt}Slows all parties by 30%. Intended to better reflect a realistic map scale. Default: true.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public bool SlowerParties { get; set; } = true;

        [SettingProperty("{=FtWk1Jm0}Longer Sieges", RequireRestart = false, HintText = "{=RaBG5ArJ}Decreases siege camp build speed by 50%. Intended to make sieges harder and more impactful, and prevent multiple sequential sieges of same settlement. Default: true.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public bool LongerSieges { get; set; } = true;

        // Value is displayed as "X Denars"
        [SettingPropertyInteger("{=iLmmsgFE}Volunteers Limit", 6, 20, "0 Volunteers",
            Order = 1, RequireRestart = false, HintText = "{=2AsFpOok}The number of volunteers that notables may have. Requires reloading. Vanilla is 6, default for BK is 10. The recruitable amount is calculated on percentages and thus is always balanced. Recruits will be lost when changing to a smaller limit. Limits can be changed at any point during campaigns.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
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
