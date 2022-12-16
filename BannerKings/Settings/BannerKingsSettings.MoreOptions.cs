using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using TaleWorlds.Localization;
using MCM.Abstractions.Attributes.v1;
using MCM.Common;
using MCM.Abstractions.Base.Global;
using TaleWorlds.SaveSystem;


namespace BannerKings.Settings
{
    public partial class BannerKingsSettings
    {
        [SettingPropertyGroup("Smithing")]
        [SettingPropertyFloatingInteger("Cost per hour", 0, 200,
            HintText = "Gold Cost per hour.  50 = default",
            RequireRestart = false,
            Order = 1)]        
        public float SmithingGoldCostPerHour { get; set; } = 10f;

        [SettingPropertyGroup("Smithing")]
        [SettingPropertyFloatingInteger("Hour per modifier", 0f, 5f, HintText = "Modifiers how long it takes to craft per stamina used. Lower is faster. Default 1", Order = 2)]
        public float SmithingStaminaPerHourModifier { get; set; } = 0.25f;
    }
}
