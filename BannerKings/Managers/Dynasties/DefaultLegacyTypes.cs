using System;
using System.Collections.Generic;

namespace BannerKings.Managers.Dynasties
{
    public class DefaultLegacyTypes : DefaultTypeInitializer<DefaultLegacyTypes, LegacyType>
    {
        public LegacyType Warfare { get; } = new LegacyType("Warfare");
        public LegacyType Mercantile { get; } = new LegacyType("Mercantile");
        public LegacyType Slaver { get; } = new LegacyType("Slaver");
        public LegacyType Stewardship { get; } = new LegacyType("Stewardship");
        public LegacyType Rule { get; } = new LegacyType("Rule");

        public override IEnumerable<LegacyType> All
        {
            get
            {
                yield return Warfare;
                yield return Mercantile;
                yield return Stewardship;
                yield return Rule;
                yield return Slaver;
                foreach (var legacyType in ModAdditions)
                    yield return legacyType;
                
            }
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
