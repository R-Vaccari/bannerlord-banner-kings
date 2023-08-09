using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations.Eras
{
    public class DefaultEras : DefaultTypeInitializer<DefaultEras, Era>
    {
        public Era FirstEra { get; } = new Era("first_era");
        public Era SecondEra { get; } = new Era("second_era");

        public override IEnumerable<Era> All => throw new NotImplementedException();

        public override void Initialize()
        {
            FirstEra.Initialize(new TextObject("{=!}Internecine Age"),
                new TextObject(""));
        }
    }
}
