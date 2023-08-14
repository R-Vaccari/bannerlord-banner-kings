using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations.Eras
{
    public class DefaultEras : DefaultTypeInitializer<DefaultEras, Era>
    {
        public Era FirstEra { get; } = new Era("first_era");
        public Era SecondEra { get; } = new Era("second_era");
        public Era ThirdEra { get; } = new Era("third_era");

        public override IEnumerable<Era> All
        {
            get
            {
                yield return FirstEra;
                yield return SecondEra;
                yield return ThirdEra;
            }
        }

        public override void Initialize()
        {
            FirstEra.Initialize(new TextObject("{=!}Calradoi Age"),
                new TextObject(""));

            SecondEra.Initialize(new TextObject("{=!}Internecine Age"),
               new TextObject(""),
               FirstEra);

            ThirdEra.Initialize(new TextObject("{=!}Internecine Age"),
               new TextObject(""),
               SecondEra);
        }
    }
}
