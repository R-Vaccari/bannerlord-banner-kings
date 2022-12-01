using System;
using System.Collections.Generic;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class DefaultCasusBelli : DefaultTypeInitializer<DefaultCasusBelli, CasusBelli>
    {
        public CasusBelli ImperialSuperiority { get; } = new CasusBelli("imperial_superiority");
        public override IEnumerable<CasusBelli> All => throw new NotImplementedException();

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
