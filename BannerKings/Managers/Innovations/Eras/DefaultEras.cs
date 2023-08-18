using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations.Eras
{
    public class DefaultEras : DefaultTypeInitializer<DefaultEras, Era>
    {
        public Era FirstEra { get; } = new Era("FirstEra");
        public Era SecondEra { get; } = new Era("SecondEra");

        public override IEnumerable<Era> All
        {
            get
            {
                yield return FirstEra;
                yield return SecondEra;
            }
        }

        public override void Initialize()
        {
            FirstEra.Initialize(new TextObject("{=!}Internecine Age"),
               new TextObject(""),
               null);

            SecondEra.Initialize(new TextObject("{=!} Age"),
               new TextObject(""),
               FirstEra);
        }
    }
}
