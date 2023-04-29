using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Criminality
{
    public class DefaultCrimes : DefaultTypeInitializer<DefaultCrimes, Crime>
    {
        public Crime Banditry { get; } = new Crime("banditry");
        public override IEnumerable<Crime> All
        {
            get
            {
                yield return Banditry;
            }
        }

        public override void Initialize()
        {
            Banditry.Initialize(new TextObject("{=!}Bandity"),
                new TextObject("{=!}"));

        }
    }
}
