using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Dynasties
{
    public class DefaultLegacies : DefaultTypeInitializer<DefaultLegacies, Legacy>
    {
        public Legacy Warfare1 { get; } = new Legacy("Warfare1");
        public Legacy Warfare2 { get; } = new Legacy("Warfare2");
        public Legacy Warfare3 { get; } = new Legacy("Warfare3");

        public Legacy Rule1 { get; } = new Legacy("Rule1");
        public Legacy Rule2 { get; } = new Legacy("Rule2");
        public Legacy Rule3 { get; } = new Legacy("Rule3");

        public Legacy Mercantile1 { get; } = new Legacy("Mercantile1");
        public Legacy Mercantile2 { get; } = new Legacy("Mercantile2");
        public Legacy Mercantile3 { get; } = new Legacy("Mercantile3");

        public Legacy Slaver1 { get; } = new Legacy("Slaver1");
        public Legacy Slaver2 { get; } = new Legacy("Slaver2");
        public Legacy Slaver3 { get; } = new Legacy("Slaver3");

        public Legacy Stewardship1 { get; } = new Legacy("Stewardship1");
        public Legacy Stewardship2 { get; } = new Legacy("Stewardship2");
        public Legacy Stewardship3 { get; } = new Legacy("Stewardship3");
        public override IEnumerable<Legacy> All => throw new NotImplementedException();

        public override void Initialize()
        {
            Warfare1.Initialize(new TextObject("{=!}Household Guard"),
                null,
                new TextObject("{=!}Ability to customize & recruit troops (court location)"),
                DefaultLegacyTypes.Instance.Warfare,
                300);

            Warfare2.Initialize(new TextObject("{=!}Retainers"),
               null,
               new TextObject("{=!}Increased clan party sizes by 15%"),
               DefaultLegacyTypes.Instance.Warfare,
               300,
               Warfare1);

            Warfare3.Initialize(new TextObject("{=!}Knights Gallant"),
               null,
               new TextObject("{=!}Increased clan part limit by 2"),
               DefaultLegacyTypes.Instance.Warfare,
               300,
               Warfare2);
        }
    }
}
