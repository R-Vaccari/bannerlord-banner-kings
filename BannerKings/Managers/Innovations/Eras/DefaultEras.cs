using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations.Eras
{
    public class DefaultEras : DefaultTypeInitializer<DefaultEras, Era>
    {
        public Era FirstEra { get; } = new Era("FirstEra");
        public Era SecondEra { get; } = new Era("SecondEra");
        public Era ThirdEra { get; } = new Era("ThirdEra");

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
               new TextObject("{=!}The era of the Calradoi was the period in which the Imperium reigned unmatched. "),
               null);

            SecondEra.Initialize(new TextObject("{=!}Internecine Age"),
               new TextObject("{=!}With the advent of the battle of Pendraic and death of emperor Arenicos Pethros, the former Imperium has shattered into civil war - an Internecine."),
               FirstEra);

            ThirdEra.Initialize(new TextObject("{=!}Dark Age"),
               new TextObject("{=!}Little is known about the period after the Internecine... However, scholars all agree that, within the 2 centuries, the empire was completely destroyed. Hordes have pillaged and razed cities to the ground, faiths and cultures ceased to exist and new kingdoms arose from the darkness. Yet, none ever shone a light so strong as did the Imperium."),
               SecondEra);
        }
    }
}
