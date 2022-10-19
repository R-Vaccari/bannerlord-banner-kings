using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Settings
{
    public class DefaultSettings : DefaultTypeInitializer<DefaultSettings, SettingsOption>
    {

        public SettingsOption NamingFullTitlesSuffixed => new SettingsOption("settings_naming_full_title_suffixed",
            new TextObject("{=!}Full Titles Suffixed"));

        public SettingsOption NamingFullTitles => new SettingsOption("settings_naming_full_title",
            new TextObject("{=!}Full Titles"));

        public SettingsOption NamingTitlePrefix => new SettingsOption("settings_naming_prefix_title",
            new TextObject("{=!}Title Prefixed"));

        public SettingsOption NamingNoTitles => new SettingsOption("settings_naming_no_title",
            new TextObject("{=!}No Titles"));


        public override IEnumerable<SettingsOption> All => throw new NotImplementedException();

        public override void Initialize()
        {

        }
    }
}
