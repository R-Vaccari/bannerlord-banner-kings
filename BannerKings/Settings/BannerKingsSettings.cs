using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using TaleWorlds.Localization;
using MCM.Abstractions.Attributes.v1;
using MCM.Common;
using MCM.Abstractions.Base.Global;

namespace BannerKings.Settings
{
    public class BannerKingsSettings : AttributeGlobalSettings<BannerKingsSettings>
    {
        private int volunteersLimit = 10;
        public override string Id => "BannerKings";
        public override string DisplayName => new TextObject("{=WaBMVVH9}Banner Kings").ToString();
        public override string FolderName => "BannerKings"; 
        public override string FormatType => "json2";

        [SettingProperty("{=K8qLtDh3}Feasts", RequireRestart = true, HintText = "{=Ctj7k5TV}Enable the ability to trigger feasts for player and AI. Default: True.")]
        [SettingPropertyGroup("{=FnRzVf4Q}Performance")]
        public bool Feasts { get; set; } = true;

        [SettingProperty("{=W54KmZDR}AI Companions", RequireRestart = true, HintText = "{=juP6OXmp}Enable the ability for AI to generate companions. Will add a large amount of heroes to the world and may impact performance. Default: True.")]
        [SettingPropertyGroup("{=FnRzVf4Q}Performance")]
        public bool AICompanions { get; set; } = true;

        [SettingProperty("{=6Qs7booz}AI Knights", RequireRestart = true, HintText = "{=aNSCjnzn}Enable the ability for AI to generate knights. Will add a large amount of heroes to the world and may impact performance. Default: True.")]
        [SettingPropertyGroup("{=FnRzVf4Q}Performance")]
        public bool AIKnights { get; set; } = true;

        [SettingProperty("{=UgfQur9G}AI Management", RequireRestart = true, HintText = "{=3tCf6aOi}Enable the ability for AI to manage their domains and vassals by calculating when to give away titles. Default: True.")]
        [SettingPropertyGroup("{=FnRzVf4Q}Performance")]
        public bool AIManagement { get; set; } = true;

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


        [SettingPropertyFloatingInteger("{=9G8cJYQd}Tax Income", minValue: 0.2f, maxValue: 2f, "#0%", RequireRestart = false, HintText = "{=VFRd9aNe}Affects the volume of settlement taxes. May SEVERELY impact AI and it's ability to recruit/keep troops. Default: 100%.")]
        [SettingPropertyGroup("{=2oJQ4Snn}Economy")]
        public float TaxIncome { get; set; } = 1f;

        [SettingProperty("{=0GVNKkGr}Village Tax Reserves", RequireRestart = false, HintText = "{=y9fQuceM}Leave a fifth of villages' production out of player income. This keeps villages in the player's income summary and so lets them always have an idea of how much villages and estates are producing. These can be manually collected through settlement menus. When disabled, all the income is removed and thus villages and estates have nothing to report most of the time, because their income depends on villagers bringing profit back from towns. Default: false.")]
        [SettingPropertyGroup("{=2oJQ4Snn}Economy")]
        public bool VillageTaxReserves { get; set; } = false;

        [SettingProperty("{=vWcskVBm}Realistic Caravan Income", RequireRestart = false, HintText = "{=1it7E8tF}Caravans pose a major risk factor not represented in the game: carrying your profits. With this setting, caravan profits will only be added when they enter a settlement owned by their owner, or where they are situated (ie, notables). Default: true.")]
        [SettingPropertyGroup("{=2oJQ4Snn}Economy")]
        public bool RealisticCaravanIncome { get; set; } = true;


        [SettingPropertyFloatingInteger("{=2yDhJfgh}Troop Upgrade Xp", minValue: 1f, maxValue: 5f, "#0%", RequireRestart = false, HintText = "{=xvNKsFbW}How much Xp troops need to upgrade. Vanilla is 100%. Default: 200%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float TroopUpgradeXp { get; set; } = 2f;

        [SettingPropertyFloatingInteger("{=OohdenyR}Slower Parties", minValue: 0f, maxValue: 0.75f, "#0%", RequireRestart = false, HintText = "{=5MR7XH9E}Slows all parties down related to their original speed. 0% is the original speed. Intended to better reflect a realistic map scale. Default: 40%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float SlowerParties { get; set; } = 0.4f;

        [SettingPropertyFloatingInteger("{=FtWk1Jm0}Longer Sieges", minValue: 0f, maxValue: 0.75f, "#0%", RequireRestart = false, HintText = "{=0ctG0FEp}Decreases siege camp build speed. 0% is the original speed. Intended to make sieges harder and more impactful, and prevent multiple sequential sieges of same settlement. Default: 50%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float LongerSieges { get; set; } = 0.5f;

        [SettingProperty("{=DZyyJXRn}Crafting Waiting Time", RequireRestart = false, HintText = "{=pSX0rWGt}When doing any type of work in the smithy, you'll be forced to wait an amount of time correspondent to how much energy was used, as well as pay for that time. Represents a more realistic approach to crafting. Default: true.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public bool CraftingWaitingTime { get; set; } = true;

        // Value is displayed as "X Denars"
        [SettingPropertyInteger("{=iLmmsgFE}Volunteers Limit", 6, 20, "{=Bm4KO72P}0 Volunteers",
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
