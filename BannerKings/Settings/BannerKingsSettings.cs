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

        [SettingProperty("{=4pJUkbew}AI Dismiss Parties", RequireRestart = false, HintText = "{=FhNxraXd}AI clan leaders will dismiss parties from their family members during peace in order to save more money. Default: true.")]
        [SettingPropertyGroup("{=2oJQ4Snn}Economy")]
        public bool DismissParties { get; set; } = true;

        [SettingPropertyFloatingInteger("{=D0UNUOgy}Base Wages", minValue: 0.5f, maxValue: 2f, "#0%", RequireRestart = false, HintText = "{=Jabak5nx}Modifier for base wages of soldiers, changing their daily upkeep but also their recruitment cost, calculated on their base wage. May SEVERELY impact AI and it's ability to recruit/keep troops. Default: 100%.")]
        [SettingPropertyGroup("{=2oJQ4Snn}Economy")]
        public float BaseWage { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=Kjb25TsU}Loot Scale", minValue: 0.2f, maxValue: 1f, "#0%", RequireRestart = false, HintText = "{=g9Sa4rmA}The scale of loot dropped by troops. When under 100%, items will randomly be eliminated from the loot pool. Vanilla is 100%. Default: 50%.")]
        [SettingPropertyGroup("{=2oJQ4Snn}Economy")]
        public float LootScale { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=smqPaUHR}Clan Renown Scaling", minValue: 1f, maxValue: 10f, "#0%", RequireRestart = false, HintText = "{=G08wtvco}The scale of renown points required for clans to level up in tier. Vanilla is 100%. Default: 300%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float ClanRenown { get; set; } = 3f;

        [SettingPropertyFloatingInteger("{=mSLQa207}Party Size Scaling", minValue: 1f, maxValue: 3f, "#0%", RequireRestart = false, HintText = "{=RszZwN4X}The scale of party sizes on the map. Applies in half to parties of heroes not leading their clan. Vanilla is 100%. Default: 200%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float PartySizes { get; set; } = 2f;

        [SettingProperty("{=QtuWPLe1}Alternative Leveling", RequireRestart = false, HintText = "{=LEE0ixHk}An alternate skill leveling progression. Skills will require a higher base xp since level 1, but will scale less aggressively. The result should be more consitent xp across all levels, slower at the beginning but faster near the end. Default: true.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public bool AlternateLeveling { get; set; } = true;

        [SettingPropertyFloatingInteger("{=iZcJtDkH}World Companions Limit", minValue: 0.5f, maxValue: 1f, "#0%", RequireRestart = false, HintText = "{=6m4y9ujC}The max limit of wanderers in the world, available at taverns. The limit is relative to all existing cities in the map. Vanilla is 60%, and amounts to less than 1 wanderer per town, BK amounts to 1 per town. Default: 100%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float WorldCompanions { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=hpWaDjNM}Army Cohesion Boost", minValue: 0f, maxValue: 0.8f, "#0%", RequireRestart = false, HintText = "{=AW5mYHB5}Cohesion boost to armies to they last longer, to counter balance the presence of more parties. Vanilla is 0%, 50% decreases cohesion loss by half. Default: 50%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float CohesionBoost { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=2yDhJfgh}Troop Upgrade Xp", minValue: 1f, maxValue: 20f, "#0%", RequireRestart = false, HintText = "{=xvNKsFbW}How much Xp troops need to upgrade. Vanilla is 100%. Default: 200%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float TroopUpgradeXp { get; set; } = 2f;

        [SettingPropertyFloatingInteger("{=OohdenyR}Slower Parties", minValue: 0f, maxValue: 0.75f, "#0%", RequireRestart = false, HintText = "{=5MR7XH9E}Slows all parties down related to their original speed. 0% is the original speed. Intended to better reflect a realistic map scale. Default: 40%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float SlowerParties { get; set; } = 0.4f;

        [SettingPropertyFloatingInteger("{=FtWk1Jm0}Longer Sieges", minValue: 0f, maxValue: 0.75f, "#0%", RequireRestart = false, HintText = "{=0ctG0FEp}Decreases siege camp build speed. 0% is the original speed. Intended to make sieges harder and more impactful, and prevent multiple sequential sieges of same settlement. Default: 50%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float LongerSieges { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=gxcgWiwh}Knight Clan Creation Speed", minValue: 0f, maxValue: 5f, "#0%", RequireRestart = false, HintText = "{=yTkU621N}Knight AI heroes can eventually form their own clans, even those in the player clan. Increasing this setting increases their speed of doing so. Setting it to 0% will stop knights from creating clans altogether. Default: 100%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float KnightClanCreationSpeed { get; set; } = 1f;

        [SettingProperty("{=DZyyJXRn}Crafting Waiting Time", RequireRestart = false, HintText = "{=pSX0rWGt}When doing any type of work in the smithy, you'll be forced to wait an amount of time correspondent to how much energy was used, as well as pay for that time. Represents a more realistic approach to crafting. Default: true.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public bool CraftingWaitingTime { get; set; } = true;

        [SettingPropertyFloatingInteger("{=iBLGdG1Y}Party Supplies", minValue: 0f, maxValue: 2f, "#0%", RequireRestart = false, 
            HintText = "{=uURHROGF}Affects the party supplies requirement factor. 0% means the feature is functionally disabled. 100% is the standard rate of items consumption, 200% means doube the rate, resulting in more expensive parties. May affect AI party limit sizes. Default: 100%.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public float PartySuppliesFactor { get; set; } = 0.5f;

        [SettingProperty("{=iBLGdG1Y}Reset Party Supplies Demand", RequireRestart = false, HintText = "{=uURHROGF}Party supply demands stack each day. Enabling this setting forgets the old demands of the party.")]
        [SettingPropertyGroup("{=P8UecnYf}Balancing")]
        public bool ResetPartySupplyDemand { get; set; } = false;

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

        [SettingProperty("{=CHVW1U24}De Re Militari Bandits", 
            RequireRestart = false, 
            HintText = "{=bAt2AWmj}If you have De Re Militari (DRM) mod, enabling this allows bandit heroes and parties to use DRM's new bandits. Enabling it without DRM will break your game. Default: false.")]
        [SettingPropertyGroup("{=k2Vw7iNm}Bandits")]
        public bool DRMBandits { get; set; } = false;

        [SettingPropertyInteger("{=tx1tsrx0}Hieout Spotting Difficulty", 1, 10, "{=rn4pdZCU}Difficulty 0",
            Order = 1,
            RequireRestart = false,
            HintText = "{=gsnBrnud}Despite being supposed to be hidden, 'Hideouts' can be spotted past the player's actual seeing range, and are not difficult at all to find. Spotting range is divided by the difficulty factor here: 1 is vanilla, 10 means a spotting range 10 times shorter.")]
        [SettingPropertyGroup("{=k2Vw7iNm}Bandits")]
        public int HideoutSpotDifficulty { get; set; } = 5;

        [SettingPropertyInteger("{=00bG9Ube}Bandit Parties Limit", 150, 600, "{=4tnEdJNu}0 Parties ",
            Order = 1,
            RequireRestart = false,
            HintText = "{=gDK2LRib}Maximum number of bandit parties in the world map. Vanilla is 150. Default: 150.")]
        [SettingPropertyGroup("{=k2Vw7iNm}Bandits")]
        public int BanditPartiesLimit { get; set; } = 150;

        [SettingPropertyBool("Enable Useful Perks", Order =0,  HintText = "Enable perks to scale up with skill level Default: True.")]
        [SettingPropertyGroup("Useful Skills and Perks")]
        public bool EnableUsefulPerks { get; set; } = true;       

        [SettingPropertyBool("Enable Perks From All Party Members", Order = 1, HintText = "Enable perks to become effective from all party members with a reduced ratio. Default setting is True.")]
        [SettingPropertyGroup("Useful Skills and Perks")]
        public bool EnableUsefulPerksFromAllPartyMembers { get; set; } = true;


        [SettingPropertyBool("Enable Skills From All Party Members", Order = 1, HintText = "Enable skills effects to become effective from all party members with a reduced ratio. Default setting is True.")]
        [SettingPropertyGroup("Useful Skills and Perks")]
        public bool EnableUsefulSkillsFromAllPartyMembers { get; set; } = true;

        [SettingPropertyBool("Enable Governor Perks From Settlement Owner", Order = 2, HintText = "Enable governor perks to become effective from settlement owners with a reduced ratio. Default setting is True.")]
        [SettingPropertyGroup("Useful Skills and Perks")]
        public bool EnableUsefulGovernorPerksFromSettlementOwner { get; set; } = true;


        [SettingPropertyBool("Enable Useful Steward Skills", Order = 3, HintText = "Enable steward skills effects to scale up with skill level Default: True.")]
        [SettingPropertyGroup("Useful Skills and Perks")]
        public bool EnableUsefulStewardSkills { get; set; } = true;


        [SettingPropertyBool("Enable Useful Steward Perks", Order = 3, HintText = "Enable steward perks to scale up with skill level Default: True.")]
        [SettingPropertyGroup("Useful Skills and Perks")]
        public bool EnableUsefulStewardPerks { get; set; } = true;
    }
}
